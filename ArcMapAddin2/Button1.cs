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

namespace Ecotrust
{
    public class Button1 : ESRI.ArcGIS.Desktop.AddIns.Button
    {
        public Button1()
        {
        }

        protected override void OnClick()
        {            
            ESRI.ArcGIS.ArcMapUI.IMxDocument mxDocument = ArcMap.Application.Document as ESRI.ArcGIS.ArcMapUI.IMxDocument; // Dynamic Cast
            ESRI.ArcGIS.Carto.IActiveView activeView = mxDocument.ActiveView;
            ESRI.ArcGIS.Carto.IMap map = activeView.FocusMap;

            // Set up export object and tile pixel coordinates
            int tileSizeX = 256;
            int tileSizeY = 256;

            /*
            ESRI.ArcGIS.Output.ExportPNG pngexport = new ESRI.ArcGIS.Output.ExportPNGClass();
            pngexport.BackgroundColor.NullColor = true;
            ESRI.ArcGIS.Output.IExport export = (ESRI.ArcGIS.Output.IExport)pngexport;
            */

            ESRI.ArcGIS.Output.ExportPNG pngexport = new ESRI.ArcGIS.Output.ExportPNGClass();
            ESRI.ArcGIS.Display.IColor tcolor = new ESRI.ArcGIS.Display.RgbColorClass();
            ((IRgbColor)tcolor).Red = 255;
            ((IRgbColor)tcolor).Green = 0;
            ((IRgbColor)tcolor).Blue = 0;
            ((IRgbColor)tcolor).Transparency = 0;  // Doesn't work?
            pngexport.BackgroundColor = tcolor;
            ((IExportPNG)pngexport).TransparentColor = tcolor;
            ESRI.ArcGIS.Output.IExport export = (ESRI.ArcGIS.Output.IExport)pngexport;
            
            
            // ***************************
            // TODO set transparency, 32 bit png
            // ESRI.ArcGIS.Carto.esriImageFormat.esriImagePNG32;            
            //ESRI.ArcGIS.Output.IExport export = new ESRI.ArcGIS.Output.ExportPNGClass();
            //ESRI.ArcGIS.Output.IExport export = (ESRI.ArcGIS.Output.IExport)xpk;
            //export.ImageType = (ESRI.ArcGIS.Output.esriExportImageType)ESRI.ArcGIS.Carto.esriImageFormat.esriImagePNG32;
            //
            //export.TransparentColor.Transparency = (byte)255;
            //export.BackgroundColor.Transparency = (byte)0;
            //export.BackgroundColor.

            /*
             * IExportPNG pngExport = new ExportPNGClass();
                    export = (IExport)pngExport;

             * 
             *             pngExport.TransparentColor.Transparency = 0;
            
            ESRI.ArcGIS.Output.IExport export = (ESRI.ArcGIS.Output.IExport)pngExport;break;
             */

            //export.ImageType = (ESRI.ArcGIS.Output.esriExportImageType)32;
            // ***************************
            ESRI.ArcGIS.esriSystem.tagRECT exportRECT;
            exportRECT.left = 0;
            exportRECT.top = 0;
            exportRECT.right = tileSizeX;
            exportRECT.bottom = tileSizeY;
            ESRI.ArcGIS.Geometry.IEnvelope envelope = new ESRI.ArcGIS.Geometry.EnvelopeClass();
            envelope.PutCoords(exportRECT.left, exportRECT.top, exportRECT.right, exportRECT.bottom);
            export.PixelBounds = envelope;

            map.DelayDrawing(true);

            // TODO make variables
            int minzoom = 0;
            int maxzoom = 4;

            // Turn off all layers
            for (int i = 0; i < map.LayerCount; i++)
                map.get_Layer(i).Visible = false;

            for (int lyrnum = 0; lyrnum < map.LayerCount; lyrnum++)
            {
                // Turn on the layer of interest
                ESRI.ArcGIS.Carto.ILayer layer = map.get_Layer(lyrnum);
                layer.Visible = true;

                // Set extents
                ESRI.ArcGIS.Geometry.IEnvelope layeraoi = layer.AreaOfInterest;
                ESRI.ArcGIS.Geometry.IEnvelope aoi = new ESRI.ArcGIS.Geometry.EnvelopeClass();

                /* TODO Loop through zoom levels, rows, cols 
                 * 
                 */
                for (int tz = minzoom; tz <= maxzoom; tz++)
                {
                    /* PYTHON
                     * 
                    print " * Processing Zoom Level %s" % tz
                    tminx, tminy = mercator.MetersToTile( bbox[0], bbox[2], tz)
                    tmaxx, tmaxy = mercator.MetersToTile( bbox[1], bbox[3], tz)
                    for ty in range(tminy, tmaxy+1):
                        for tx in range(tminx, tmaxx+1):
                     * 
                     */
                    GlobalMercator mercator = new GlobalMercator();
                    //GlobalMercator.Coords mins = mercator.MetersToTile(layeraoi.XMin, layeraoi.YMin, tz);
                    //GlobalMercator.Coords maxs = mercator.MetersToTile(layeraoi.XMax, layeraoi.YMax, tz); // extent needs to be mercator
                    GlobalMercator.Coords mins = mercator.MetersToTile(-14832493.1523, 4309673.4917, tz);
                    GlobalMercator.Coords maxs = mercator.MetersToTile(-12232493.1523, 6909673.4917, tz);
                    int tileCount = 0;
                    for (int ty = (int)mins.y; ty <= (int)maxs.y; ty++) 
                    {
                        for (int tx = (int)mins.x; tx <= (int)maxs.x; tx++)
                        {
                            tileCount += 1;
                            
                            export.ExportFileName = "e:\\workspace\\test_addin\\" + tz + "_" + tx + "_" + ty + "_" + layer.Name + ".png";
                            //aoi.PutCoords(exportRECT.left, exportRECT.top, exportRECT.right, exportRECT.bottom);

                            // TODO get tile coords from GlobalMercator.TileBounds
                            GlobalMercator.Bounds bnd = mercator.TileBounds(tx, ty, tz);
                            aoi.PutCoords(bnd.minx, bnd.miny, bnd.maxx, bnd.maxy);
                            //aoi.PutCoords(-13832493.1523, 5309673.4917, -13232493.1523, 5909673.4917);
                            aoi.SpatialReference = map.SpatialReference; // TODO aoi spatial reference == mercator?
                            // Use FullExtent instead of Extent to make the extent independent of the activeView ratio
                            activeView.FullExtent = aoi;

                            // Export
                            System.Int32 hDC = export.StartExporting();
                            activeView.Output(hDC, (System.Int16)export.Resolution, ref exportRECT, null, null); // Explicit Cast and 'ref' keyword needed 
                            export.FinishExporting();
                            export.Cleanup();
                            
                        }                    
                    }
                    using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"E:\\workspace\\test_addin\\test.txt", true))
                    {
                        file.WriteLine(layer.Name + ", zoom " + tz + ", numtiles " + tileCount + ":" + 
                          mins.x + " " + mins.y + " " + maxs.x + " " + maxs.y);
                    }

                    /*
                    export.ExportFileName = "e:\\workspace\\test_addin\\" + tz + "_" + layer.Name + ".png";
                    //aoi.PutCoords(exportRECT.left, exportRECT.top, exportRECT.right, exportRECT.bottom);
                    aoi.PutCoords(-13832493.1523, 5309673.4917, -13232493.1523, 5909673.4917);
                    aoi.SpatialReference = map.SpatialReference; // TODO aoi spatial reference == mercator?
                    // Use FullExtent instead of Extent to make the extent independent of the activeView ratio
                    activeView.FullExtent = aoi;

                    // Export
                    System.Int32 hDC = export.StartExporting();
                    activeView.Output(hDC, (System.Int16)export.Resolution, ref exportRECT, null, null); // Explicit Cast and 'ref' keyword needed 
                    export.FinishExporting();
                    export.Cleanup();
                     */
                }
                // Turn it off
                layer.Visible = false;
            }

            // Turn ON all layers
            for (int i = 0; i < map.LayerCount; i++)
                map.get_Layer(i).Visible = true;
            
            map.DelayDrawing(false);  
            activeView.Refresh();
        }

        protected override void OnUpdate()
        {
            Enabled = ArcMap.Application != null;
        }
    }

}
