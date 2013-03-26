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

        ///<summary>Creates a tile 256x256.</summary>
        ///
        ///<param name="activeView">An IActiveView interface</param>
        ///<param name="pathFileName">A System.String that the path and filename of the JPEG you want to create. Example: "C:\temp\hiResolutionTest.jpg"</param>
        /// 
        ///<returns>A System.Boolean indicating the success</returns>
        /// 
        ///<remarks></remarks>
        public System.Boolean CreateTileFromActiveView(ESRI.ArcGIS.Carto.IActiveView activeView, System.String pathFileName, int tileSizeX, int tileSizeY)
        {
            //parameter check
            if (activeView == null || !(pathFileName.EndsWith(".png")))
            {
                return false;
            }
            ESRI.ArcGIS.Output.IExport export = new ESRI.ArcGIS.Output.ExportPNGClass();
            export.ExportFileName = pathFileName;

            export.Resolution = 300;

            //ESRI.ArcGIS.Display.tagRECT exportRECT; // This is a structure
            ESRI.ArcGIS.esriSystem.tagRECT exportRECT;
            exportRECT.left = 0;
            exportRECT.top = 0;
            exportRECT.right = tileSizeX;
            exportRECT.bottom = tileSizeY;

            // Set up the PixelBounds envelope to match the exportRECT
            ESRI.ArcGIS.Geometry.IEnvelope envelope = new ESRI.ArcGIS.Geometry.EnvelopeClass();
            envelope.PutCoords(exportRECT.left, exportRECT.top, exportRECT.right, exportRECT.bottom);
            export.PixelBounds = envelope;

            System.Int32 hDC = export.StartExporting();

            activeView.Output(hDC, (System.Int16)export.Resolution, ref exportRECT, null, null); // Explicit Cast and 'ref' keyword needed 
            export.FinishExporting();
            export.Cleanup();

            return true;
        }

        protected override void OnClick()
        {            
            ESRI.ArcGIS.ArcMapUI.IMxDocument mxDocument = ArcMap.Application.Document as ESRI.ArcGIS.ArcMapUI.IMxDocument; // Dynamic Cast
            ESRI.ArcGIS.Carto.IActiveView activeView = mxDocument.ActiveView;
            ESRI.ArcGIS.Carto.IMap map = activeView.FocusMap;

            int tileSizeX = 256;
            int tileSizeY = 256;
            
            map.DelayDrawing(true);

            // Turn off all layers
            for (int i = 0; i < map.LayerCount; i++)
                map.get_Layer(i).Visible = false;

            for (int lyrnum = 0; lyrnum < map.LayerCount; lyrnum++)
            {
                // Turn on the layer of interest
                ESRI.ArcGIS.Carto.ILayer layer = map.get_Layer(lyrnum);
                layer.Visible = true;
   
                // Refresh and Zoom to the layer
                // Use FullExtent instead of Extent to make the extent independent of the activeView ratio
                activeView.FullExtent = layer.AreaOfInterest;
                //TODO slugify name
                CreateTileFromActiveView(activeView, "e:\\workspace\\test_addin\\" + layer.Name + ".png", tileSizeX, tileSizeY);

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
