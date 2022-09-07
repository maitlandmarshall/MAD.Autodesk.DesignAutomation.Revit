using System;
using System.IO;
using System.Linq;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;
using DesignAutomationFramework;
using MAD.Autodesk.DesignAutomation.Revit.Services;

namespace MAD.Autodesk.DesignAutomation.Revit
{
    [Regeneration(RegenerationOption.Manual)]
    [Transaction(TransactionMode.Manual)]
    public class ExportDataApp : IExternalDBApplication
    {
        public ExternalDBApplicationResult OnStartup(ControlledApplication app)
        {
            DesignAutomationBridge.DesignAutomationReadyEvent += this.HandleDesignAutomationReadyEvent;
            return ExternalDBApplicationResult.Succeeded;
        }

        public ExternalDBApplicationResult OnShutdown(ControlledApplication app)
        {
            DesignAutomationBridge.DesignAutomationReadyEvent -= this.HandleDesignAutomationReadyEvent;
            return ExternalDBApplicationResult.Succeeded;
        }

        public void HandleDesignAutomationReadyEvent(object sender, DesignAutomationReadyEventArgs e)
        {
            e.Succeeded = true;
            
            var dataReader = new DataReader(e.DesignAutomationData);
            var elementsToSave = dataReader.GetElements().ToList();
            var entitiesToSave = dataReader.GetEntitiesFromElements(elementsToSave).ToList();

            var groupByType = entitiesToSave.GroupBy(y => y.Type).ToList();
        }

        public static void DeleteAllWalls(DesignAutomationData data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            Application rvtApp = data.RevitApp;
            if (rvtApp == null) throw new InvalidDataException(nameof(rvtApp));

            string modelPath = data.FilePath;
            if (String.IsNullOrWhiteSpace(modelPath)) throw new InvalidDataException(nameof(modelPath));

            Document doc = data.RevitDoc;
            if (doc == null) throw new InvalidOperationException("Could not open document.");

            using (Transaction transaction = new Transaction(doc))
            {
                FilteredElementCollector col = new FilteredElementCollector(doc).OfClass(typeof(Wall));
                transaction.Start("Delete All Walls");
                doc.Delete(col.ToElementIds());
                transaction.Commit();
            }

            ModelPath path = ModelPathUtils.ConvertUserVisiblePathToModelPath("result.rvt");
            doc.SaveAs(path, new SaveAsOptions());
        }
    }
}
