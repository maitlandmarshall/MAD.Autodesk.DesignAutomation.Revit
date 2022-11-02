using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Xml;
using ADSK.BIT.ModelChecker.API.DataModel;
using ADSK.BIT.ModelChecker.API.Services.Implementation;
using ADSK.BIT.ModelChecker.Revit.API.Services.Implementation;
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
            
            PerformModelCheckAndSaveResults(e);
            //ExtractAllEntities(e);
        }

        private void PerformModelCheckAndSaveResults(DesignAutomationReadyEventArgs e)
        {
            var checksetXml = new XmlDocument();
            checksetXml.Load(typeof(ExportDataApp).Assembly.GetManifestResourceStream("MAD.Autodesk.DesignAutomation.Revit.dash.xml"));

            var service = new CheckSetService();
            var checkSet = service.GetCheckSet(checksetXml);
            var checker = new DocumentCheckRunner(e.DesignAutomationData.RevitDoc, service);
            var reportRun = checker.RunChecks(true, checkSet);

            var exporter = new ResultExporterExcel();
            var options = new ExportOptions
            {
                ExportLocation = Path.Combine(this.GetExportDirectory(), "ReportRun.xlsx"),
                ExportLists = true
            };

            exporter.ExportReport(reportRun, options);
        }

        private void ExtractAllEntities(DesignAutomationReadyEventArgs e)
        {
            var dataReader = new DataReader(e.DesignAutomationData);
            var elementsToSave = dataReader.GetElements().ToList();
            var entitiesToSave = dataReader.GetEntitiesFromElements(elementsToSave).ToList();
            var groupByType = entitiesToSave.GroupBy(y => y.Type).ToList();

            this.SaveEntitiesToOutput(groupByType);
        }

        private void SaveEntitiesToOutput(IEnumerable<IGrouping<string, DataReaderEntity>> entitiesGroupedByType)
        {
            var outputDir = Directory.CreateDirectory(this.GetExportDirectory());
            
            foreach (var g in entitiesGroupedByType)
            {
                var json = JsonConvert.SerializeObject(g.ToList());
                var outputFile = Path.Combine(outputDir.FullName, $"{g.Key}.json");

                File.WriteAllText(outputFile, json);
            }
        }

        private string GetExportDirectory()
        {
            var currentDir = Directory.GetCurrentDirectory();
            var outputFolder = Path.Combine(currentDir, "output");

            return outputFolder;
        }
    }
}
