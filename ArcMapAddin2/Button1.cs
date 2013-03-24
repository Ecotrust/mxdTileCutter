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

        ///<summary>Creates a .jpg (JPEG) file from IActiveView. Default values of 96 DPI are used for the image creation.</summary>
        ///
        ///<param name="activeView">An IActiveView interface</param>
        ///<param name="pathFileName">A System.String that the path and filename of the JPEG you want to create. Example: "C:\temp\test.jpg"</param>
        /// 
        ///<returns>A System.Boolean indicating the success</returns>
        /// 
        ///<remarks></remarks>
        public System.Boolean CreateJPEGFromActiveView(ESRI.ArcGIS.Carto.IActiveView activeView, System.String pathFileName)
        {
            //parameter check
            if (activeView == null || !(pathFileName.EndsWith(".jpg")))
            {
                return false;
            }
            ESRI.ArcGIS.Output.IExport export = new ESRI.ArcGIS.Output.ExportJPEGClass();
            export.ExportFileName = pathFileName;

            // Microsoft Windows default DPI resolution
            export.Resolution = 96;
            ESRI.ArcGIS.esriSystem.tagRECT exportRECT = activeView.ExportFrame;
            // um esri... THIS IS WRONG
            // ESRI.ArcGIS.Display.tagRECT exportRECT = activeView.ExportFrame;

            ESRI.ArcGIS.Geometry.IEnvelope envelope = new ESRI.ArcGIS.Geometry.EnvelopeClass();
            envelope.PutCoords(exportRECT.left, exportRECT.top, exportRECT.right, exportRECT.bottom);
            export.PixelBounds = envelope;
            System.Int32 hDC = export.StartExporting();
            activeView.Output(hDC, (System.Int16)export.Resolution, ref exportRECT, null, null);

            // Finish writing the export file and cleanup any intermediate files
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

            ZoomToActiveLayerInTOC(ArcMap.Application.Document as IMxDocument);
            
            ESRI.ArcGIS.ArcMapUI.IMxDocument mxDocument = ArcMap.Application.Document as ESRI.ArcGIS.ArcMapUI.IMxDocument; // Dynamic Cast
            ESRI.ArcGIS.Carto.IActiveView activeView = mxDocument.ActiveView;

            System.Boolean test = CreateJPEGFromActiveView(activeView, "e:\\workspace\\test.jpg");
            
            //ArcMap.Application.CurrentTool = null;
        }

        protected override void OnUpdate()
        {
            Enabled = ArcMap.Application != null;
        }
    }

}
