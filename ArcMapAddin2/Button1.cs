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
            if (activeView == null || !(pathFileName.EndsWith(".jpg")))
            {
                return false;
            }
            ESRI.ArcGIS.Output.IExport export = new ESRI.ArcGIS.Output.ExportJPEGClass();
            export.ExportFileName = pathFileName;

            export.Resolution = 300;

            //ESRI.ArcGIS.Display.tagRECT exportRECT; // This is a structure
            ESRI.ArcGIS.esriSystem.tagRECT exportRECT;
            exportRECT.left = 0;
            exportRECT.top = 0;
            exportRECT.right = tileSizeX; // activeView.ExportFrame.right * (outputResolution / screenResolution);
            exportRECT.bottom = tileSizeY; // activeView.ExportFrame.bottom * (outputResolution / screenResolution);

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

        ///<summary>Zooms to the selected layer in the TOC associated with the active view.</summary>
        /// 
        ///<param name="mxDocument">An IMxDocument interface</param>
        ///  
        ///<remarks></remarks>
        public void ZoomToActiveLayerInTOC(ESRI.ArcGIS.ArcMapUI.IMxDocument mxDocument)
        {
            if(mxDocument == null)
            {
            return;
            }
            ESRI.ArcGIS.Carto.IActiveView activeView = mxDocument.ActiveView; 

            // Get the TOC
            ESRI.ArcGIS.ArcMapUI.IContentsView IContentsView = mxDocument.CurrentContentsView;

            // Get the selected layer
            System.Object selectedItem = IContentsView.SelectedItem;
            if (!(selectedItem is ESRI.ArcGIS.Carto.ILayer))
            {
            return;
            }
            ESRI.ArcGIS.Carto.ILayer layer = selectedItem as ESRI.ArcGIS.Carto.ILayer; 


            // Zoom to the extent of the layer and refresh the map
            activeView.Extent = layer.AreaOfInterest;
            activeView.Refresh();
        }

        protected override void OnClick()
        {            
            ESRI.ArcGIS.ArcMapUI.IMxDocument mxDocument = ArcMap.Application.Document as ESRI.ArcGIS.ArcMapUI.IMxDocument; // Dynamic Cast
            ESRI.ArcGIS.Carto.IActiveView activeView = mxDocument.ActiveView;

            ZoomToActiveLayerInTOC(mxDocument as IMxDocument);

            ESRI.ArcGIS.Carto.IMap map = activeView.FocusMap;
            map.DelayDrawing(true);
            
            // activeView.ExportFrame
            
            // Turn off all layers
            for (int i = 0; i < map.LayerCount; i++)
                map.get_Layer(i).Visible = false;

            for (int lyrnum = 0; lyrnum < map.LayerCount; lyrnum++)
            {
                // Turn on the layer of interest
                map.get_Layer(lyrnum).Visible = true;
   
                // Refresh and Zoom to the layer
                //activeView.
                activeView.Extent = map.get_Layer(lyrnum).AreaOfInterest;
                activeView.Refresh();

                CreateTileFromActiveView(activeView, "e:\\workspace\\test_addin\\square_" + lyrnum + ".jpg", 256, 256);
                CreateTileFromActiveView(activeView, "e:\\workspace\\test_addin\\wide_" + lyrnum + ".jpg", 800, 256);

                CreateTileFromActiveView(activeView, "e:\\workspace\\test_addin\\tall_" + lyrnum + ".jpg", 256, 800);

                // Turn it off
                map.get_Layer(lyrnum).Visible = false;
            }

            map.DelayDrawing(false);  
            
            //ArcMap.Application.CurrentTool = null;
        }

        protected override void OnUpdate()
        {
            Enabled = ArcMap.Application != null;
        }
    }

}
