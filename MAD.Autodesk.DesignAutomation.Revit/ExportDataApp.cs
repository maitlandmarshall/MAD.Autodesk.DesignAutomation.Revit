using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;
using DesignAutomationFramework;
using MAD.Autodesk.DesignAutomation.Revit.Services;
using Newtonsoft.Json;

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

            this.SaveEntitiesToOutput(groupByType);
        }

        private void SaveEntitiesToOutput(IEnumerable<IGrouping<string, DataReaderEntity>> entitiesGroupedByType)
        {
            var currentDir = Directory.GetCurrentDirectory();
            var outputFolder = Path.Combine(currentDir, "outputJson");

            if (Directory.Exists(outputFolder))
                Directory.Delete(outputFolder);

            var outputDir = Directory.CreateDirectory(outputFolder);
            
            foreach (var g in entitiesGroupedByType)
            {
                var json = JsonConvert.SerializeObject(g.ToList());
                var outputFile = Path.Combine(outputDir.FullName, $"{g.Key}.json");

                File.WriteAllText(outputFile, json);
            }
        }
    }
}
