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

namespace ArcMapAddin1
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

            // Set up export object and pixel coordinates
            int tileSizeX = 256;
            int tileSizeY = 256;
            ESRI.ArcGIS.Output.IExport export = new ESRI.ArcGIS.Output.ExportPNGClass();
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

            for (int lyrnum = 0; lyrnum < map.LayerCount; lyrnum++)
            {
                // Turn on the layer of interest
                ESRI.ArcGIS.Carto.ILayer layer = map.get_Layer(lyrnum);
                layer.Visible = true;

                export.ExportFileName = "e:\\workspace\\test_addin\\" + layer.Name + ".png";
   
                // Refresh and Zoom to the layer
                // Use FullExtent instead of Extent to make the extent independent of the activeView ratio

                // TODO instead of AreaOfInterest, try a hardcoded extent
                //activeView.FullExtent = layer.AreaOfInterest;

                //ESRI.ArcGIS.Geometry.Envelope aoi = layer.AreaOfInterest;
                ESRI.ArcGIS.Geometry.IEnvelope aoi = new ESRI.ArcGIS.Geometry.EnvelopeClass();
                //aoi.PutCoords(exportRECT.left, exportRECT.top, exportRECT.right, exportRECT.bottom);
                aoi.PutCoords(-13832493.1523, 5909673.4917, -13232493.1523, 5309673.4917);
                aoi.SpatialReference = map.SpatialReference;
                // TODO aoi spatial reference
                activeView.FullExtent = aoi;
                
                // Export
                System.Int32 hDC = export.StartExporting();
                activeView.Output(hDC, (System.Int16)export.Resolution, ref exportRECT, null, null); // Explicit Cast and 'ref' keyword needed 
                export.FinishExporting();
                export.Cleanup();

                // Turn it off
                layer.Visible = false;
            }

            activeView.Refresh();
            map.DelayDrawing(false);  
        }

        protected override void OnUpdate()
        {
            Enabled = ArcMap.Application != null;
        }
    }

}
