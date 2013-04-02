/*
 * Author: Matthew Perry; mperry at ecotrust.org
 * 
Copyright (c) 2013, Ecotrust
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:
    * Redistributions of source code must retain the above copyright
      notice, this list of conditions and the following disclaimer.
    * Redistributions in binary form must reproduce the above copyright
      notice, this list of conditions and the following disclaimer in the
      documentation and/or other materials provided with the distribution.
    * Neither the name of the <organization> nor the
      names of its contributors may be used to endorse or promote products
      derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL <COPYRIGHT HOLDER> BE LIABLE FOR ANY
DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Drawing;

using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Desktop;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Output;


/*
 * TODO list
 * 
 * openlayers output
 * json output
 * sanity checking
 * check projection (dataframe, layers)
 * base url as user input for purposes of creating json fixture
 * avoid negative tile coords when zoomed to full earth extent
 * avoid outputing blank tiles?
 * time elapsed and estimated time remaining
 * logging
 * legends
 * user input for which layers to process (currently defaults to all)
 * optimization (indexes/preprocessing of data)
 * utfgrids
 * sluggify layer names
 * 
 */ 

namespace Ecotrust
{
    public class Button1 : ESRI.ArcGIS.Desktop.AddIns.Button
    {
        public Button1()
        {
        }

        protected override void OnClick()
        {
            // Get the min/max zoom from user input
            int minzoom = 0;
            int maxzoom = 6;
            Ecotrust.Form1 form1 = new Ecotrust.Form1();
            if (form1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                minzoom = (int)form1.numericUpDown1.Value;
                maxzoom = (int)form1.numericUpDown2.Value;
            }
            else
            {
                return; //TODO 
            }

            // Use the FolderBrowserDialog Class to choose export folder
            System.Windows.Forms.FolderBrowserDialog folderDialog = new System.Windows.Forms.FolderBrowserDialog();
            folderDialog.Description = "Select output folder for map tiles...";
            string exportDir = "";
            if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                // The returned string will be the full path, filename and file-extension for the chosen shapefile. Example: "C:\test\cities.shp"
                exportDir = folderDialog.SelectedPath;
                if (exportDir == "")
                    return;  // TODO raise error
            }
            else
            {
                return; //TODO 
            }

            ESRI.ArcGIS.ArcMapUI.IMxDocument mxDocument = ArcMap.Application.Document as ESRI.ArcGIS.ArcMapUI.IMxDocument; // Dynamic Cast
            ESRI.ArcGIS.Carto.IActiveView activeView = mxDocument.ActiveView;
            ESRI.ArcGIS.Carto.IMap map = activeView.FocusMap;

            ESRI.ArcGIS.Geometry.IEnvelope mapaoi = activeView.Extent;

            // Set up export object and tile pixel coordinates
            int tileSizeX = 256;
            int tileSizeY = 256;

            // set up exporter with transparent background
            ESRI.ArcGIS.Output.IExportPNG pngexport = new ESRI.ArcGIS.Output.ExportPNGClass();
            ESRI.ArcGIS.Display.IColor tcolor = new ESRI.ArcGIS.Display.RgbColorClass();
            // Warning: 254,254,254 will be set to transparent; don't use in any of map styling
            ((IRgbColor)tcolor).Red = 254;
            ((IRgbColor)tcolor).Green = 254;
            ((IRgbColor)tcolor).Blue = 254;
            ((ExportPNG)pngexport).BackgroundColor = tcolor;
            pngexport.TransparentColor = tcolor;
            ESRI.ArcGIS.Output.IExport export = (ESRI.ArcGIS.Output.IExport)pngexport;
            
            ESRI.ArcGIS.esriSystem.tagRECT exportRECT;
            exportRECT.left = 0;
            exportRECT.top = 0;
            exportRECT.right = tileSizeX;
            exportRECT.bottom = tileSizeY;
            ESRI.ArcGIS.Geometry.IEnvelope envelope = new ESRI.ArcGIS.Geometry.EnvelopeClass();
            envelope.PutCoords(exportRECT.left, exportRECT.top, exportRECT.right, exportRECT.bottom);
            export.PixelBounds = envelope;

            map.DelayDrawing(true);

            // Turn off all layers
            for (int i = 0; i < map.LayerCount; i++)
                map.get_Layer(i).Visible = false;
            
            // Calculate total number of tiles needed
            GlobalMercator mercator = new GlobalMercator();
            GlobalMercator.Coords tempmins;
            GlobalMercator.Coords tempmaxs;
            Double numTiles = 0;
            for (int tz = minzoom; tz <= maxzoom; tz++)
            {
                tempmins = mercator.MetersToTile(mapaoi.XMin, mapaoi.YMin, tz); 
                tempmaxs = mercator.MetersToTile(mapaoi.XMax, mapaoi.YMax, tz); 
                numTiles += ((tempmaxs.y - tempmins.y)+1) * ((tempmaxs.x - tempmins.x)+1);
            }
            numTiles *= map.LayerCount;

            ESRI.ArcGIS.esriSystem.IStatusBar statusBar = ArcMap.Application.StatusBar;
            statusBar.set_Message(0, "Rendering " + numTiles.ToString() + " tiles");

            // Create a CancelTracker
            ESRI.ArcGIS.esriSystem.ITrackCancel trackCancel = new ESRI.ArcGIS.Display.CancelTrackerClass();

            ESRI.ArcGIS.Framework.IProgressDialogFactory progressDialogFactory = new ESRI.ArcGIS.Framework.ProgressDialogFactoryClass();

            // Set the properties of the Step Progressor
            System.Int32 int32_hWnd = ArcMap.Application.hWnd;
            ESRI.ArcGIS.esriSystem.IStepProgressor stepProgressor = progressDialogFactory.Create(trackCancel, int32_hWnd);
            stepProgressor.MinRange = 0;
            stepProgressor.MaxRange = (int)numTiles;
            stepProgressor.StepValue = 1;
            stepProgressor.Message = "Calculating " + numTiles.ToString() + " tiles";

            // Create the ProgressDialog. This automatically displays the dialog
            ESRI.ArcGIS.Framework.IProgressDialog2 progressDialog2 = (ESRI.ArcGIS.Framework.IProgressDialog2)stepProgressor; // Explict Cast

            // Set the properties of the ProgressDialog
            progressDialog2.CancelEnabled = true;
            progressDialog2.Description = "Rendering " + numTiles.ToString() + " map tiles";
            progressDialog2.Title = "Creating map tiles...";
            progressDialog2.Animation = ESRI.ArcGIS.Framework.esriProgressAnimationTypes.esriDownloadFile;
            System.Boolean boolean_Continue = true;

            int tileCount = 0;

            for (int lyrnum = 0; lyrnum < map.LayerCount; lyrnum++)
            {
                // Turn on the layer of interest
                ESRI.ArcGIS.Carto.ILayer layer = map.get_Layer(lyrnum);
                layer.Visible = true;

                // Set extents
                //ESRI.ArcGIS.Geometry.IEnvelope layeraoi = layer.AreaOfInterest;
                ESRI.ArcGIS.Geometry.IEnvelope aoi = new ESRI.ArcGIS.Geometry.EnvelopeClass();

                // Create layer directory if it doesn't exist
                DirectoryInfo dir = new DirectoryInfo(exportDir  + "\\" + layer.Name);
                if (!dir.Exists)
                    dir.Create();

                DateTime startTime = DateTime.Now;

                // Loop through zoom levels, rows, cols 
                for (int tz = minzoom; tz <= maxzoom; tz++)
                {    
                    GlobalMercator.Coords mins = mercator.MetersToTile(mapaoi.XMin, mapaoi.YMin, tz);
                    GlobalMercator.Coords maxs = mercator.MetersToTile(mapaoi.XMax, mapaoi.YMax, tz);

                    // Create zoom directory if it doesn't exist
                    DirectoryInfo dir2 = new DirectoryInfo(dir.FullName + "\\" + tz);
                    if (!dir2.Exists)
                        dir2.Create();

                    for (int tx = (int)mins.x; tx <= (int)maxs.x; tx++) 
                    {
                        // Create X directory if it doesn't exist
                        DirectoryInfo dir3 = new DirectoryInfo(dir2.FullName + "\\" + tx);
                        if (!dir3.Exists)
                            dir3.Create();

                        for (int ty = (int)mins.y; ty <= (int)maxs.y; ty++)
                        {

                            // Flip y-axis for output tile name
                            int invertTy = (int) ((Math.Pow(2, tz) - 1) - ty);

                            tileCount += 1;

                            // TODO Calculate time and set new message
                            // TimeSpan timeElapsed = TimeSpan.FromTicks(DateTime.Now.Subtract(startTime).Ticks); // * ((double)tileCount - (numTiles + 1)) / (numTiles + 1));
                            // double timeRemaining = (timeElapsed.TotalSeconds / (tileCount / numTiles)) - timeElapsed.TotalSeconds;
                            //(" + ((int)timeRemaining).ToString() +" remaining)";

                            stepProgressor.Message = layer.Name + "\\" + tz + "\\" + tx + "\\" + invertTy +
                                ".png (" + tileCount + " of " + numTiles + ")"; 

                            
                            export.ExportFileName = dir3.FullName + "\\" + invertTy + ".png";

                            GlobalMercator.Bounds bnd = mercator.TileBounds(tx, ty, tz);
                            aoi.PutCoords(bnd.minx, bnd.miny, bnd.maxx, bnd.maxy);
                            aoi.SpatialReference = map.SpatialReference; // TODO aoi spatial reference == mercator?
                            // Use FullExtent instead of Extent to make the extent independent of the activeView ratio
                            activeView.FullExtent = aoi;                          

                            // Export
                            System.Int32 hDC = export.StartExporting();
                            activeView.Output(hDC, (System.Int16)export.Resolution, ref exportRECT, null, null); // Explicit Cast and 'ref' keyword needed 
                            export.FinishExporting();
                            export.Cleanup();

                            stepProgressor.Position = tileCount;

                            //Check if the cancel button was pressed. If so, break out of row
                            boolean_Continue = trackCancel.Continue();
                            if (!boolean_Continue)
                                break;
                        }
                        //Check if the cancel button was pressed. If so, break out of col
                        boolean_Continue = trackCancel.Continue();
                        if (!boolean_Continue)
                            break;                  
                    }
                    //Check if the cancel button was pressed. If so, break out of layers
                    boolean_Continue = trackCancel.Continue();
                    if (!boolean_Continue)
                        break;

                    // Write log                    
                    using (System.IO.StreamWriter file = new System.IO.StreamWriter( exportDir + "\\log.txt", true))
                    {
                        file.WriteLine(layer.Name + ", zoom " + tz + ", numtiles " + tileCount + ":" + 
                          mins.x + " " + mins.y + " " + maxs.x + " " + maxs.y);
                    }

                }
                // Turn it off
                layer.Visible = false;
            }

            map.DelayDrawing(false);

            // Turn ON all layers
            for (int i = 0; i < map.LayerCount; i++)
                map.get_Layer(i).Visible = true;

            // restore extent
            activeView.FullExtent = mapaoi; 
            activeView.Refresh();

            // Done
            trackCancel = null;
            stepProgressor = null;
            progressDialog2.HideDialog();
            progressDialog2 = null;

        }

        protected override void OnUpdate()
        {
            Enabled = ArcMap.Application != null;
        }
    }

}
