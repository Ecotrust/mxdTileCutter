using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

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
 * projection (map, layers)
 * min/max zoom as user input
 * base url as user input
 * avoid negative tile coords when zoomed to full earth extent
 * transparency 
 * avoid outputing blank tiles?
 * confirm alignment and tile coordinates
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

            // Use the OpenFileDialog Class to choose export folder
            System.Windows.Forms.FolderBrowserDialog folderDialog = new System.Windows.Forms.FolderBrowserDialog();
            folderDialog.Description = "Select output folder for map tiles...";
            
            //folderDialog.RootFolder = @"C:\\"; // TODO
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

            // set up exporter 
            ESRI.ArcGIS.Output.ExportPNG pngexport = new ESRI.ArcGIS.Output.ExportPNGClass();
            ESRI.ArcGIS.Display.IColor tcolor = new ESRI.ArcGIS.Display.RgbColorClass();
            ((IRgbColor)tcolor).Red = 255;
            ((IRgbColor)tcolor).Green = 0;
            ((IRgbColor)tcolor).Blue = 0;
            // TODO set transparency, 32 bit png
            ((IRgbColor)tcolor).Transparency = 0;  // Doesn't work?
            pngexport.BackgroundColor = tcolor;
            ((IExportPNG)pngexport).TransparentColor = tcolor;
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
                tempmins = mercator.MetersToXYZTile(mapaoi.XMin, mapaoi.YMax, tz); // ymax bc of y coord flip
                tempmaxs = mercator.MetersToXYZTile(mapaoi.XMax, mapaoi.YMin, tz); 
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
            System.Boolean boolean_Continue = false;
            boolean_Continue = true;
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
                    
                // Loop through zoom levels, rows, cols 
                for (int tz = minzoom; tz <= maxzoom; tz++)
                {    
                    GlobalMercator.Coords mins = mercator.MetersToXYZTile(mapaoi.XMin, mapaoi.YMax, tz); // ymax bc of y coord flip
                    GlobalMercator.Coords maxs = mercator.MetersToXYZTile(mapaoi.XMax, mapaoi.YMin, tz); // map extent needs to be mercator

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
                            tileCount += 1;
                            stepProgressor.Message = layer.Name + "\\" + tz + "\\" + tx + "\\" + ty + ".png (" + tileCount + " of " + numTiles + ")";
                            
                            export.ExportFileName = dir3.FullName + "\\" + ty + ".png";

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
                    /*
                    using (System.IO.StreamWriter file = new System.IO.StreamWriter( exportDir + "\\log.txt", true))
                    {
                        file.WriteLine(layer.Name + ", zoom " + tz + ", numtiles " + tileCount + ":" + 
                          mins.x + " " + mins.y + " " + maxs.x + " " + maxs.y);
                    }
                     */

                }
                // Turn it off
                layer.Visible = false;
            }

            map.DelayDrawing(false);

            // Turn ON all layers
            for (int i = 0; i < map.LayerCount; i++)
                map.get_Layer(i).Visible = true;

            activeView.Extent = mapaoi; // restore extent
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
