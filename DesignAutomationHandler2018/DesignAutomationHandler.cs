using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DesignAutomationFramework;
using MAD.Autodesk.DesignAutomation.Revit;
using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace DesignAutomationHandler
{
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    class DesignAutomationHandlerApp : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var app = commandData.Application.Application;
            var dbApp = LoadDbApp(app);

            var doc = commandData.Application.ActiveUIDocument?.Document;
            HandleDAApplication(app, doc, dbApp);

            return Result.Succeeded;
        }

        public void HandleDAApplication(Autodesk.Revit.ApplicationServices.Application app, Document doc, ExportDataApp dbApp)
        {
            try
            {
                var filename = doc?.PathName;
                var currentdir = Directory.GetCurrentDirectory();
                var message = string.Empty;

                if (string.IsNullOrEmpty(filename))
                {
                    message = $"No input file.\nIf you have json file for parameters, now copy it under the current folder:\n{currentdir}";
                    MessageBox.Show(message, "DesignAutomationHandler");
                }

                var designAutomationResult = DesignAutomationBridge.SetDesignAutomationReady(app, filename);

                if (designAutomationResult)
                {
                    var resultFolder = string.IsNullOrEmpty(filename) ? currentdir : Path.GetDirectoryName(filename);
                    message = $"Succeed!\nFind the results at folder: {resultFolder}";
                }
                else
                {
                    message = $"Failed! You may debug the addin dll.";
                }

                MessageBox.Show(message, "DesignAutomationHandler");

            }
            catch (System.Exception e)
            {
                MessageBox.Show(e.ToString(), "DesignAutomationHandler");
            }
            finally
            {
                UnloadDbApp(app, dbApp);
            }
        }

        private void UnloadDbApp(Autodesk.Revit.ApplicationServices.Application app, ExportDataApp dba)
        {
            var flags = BindingFlags.NonPublic | BindingFlags.Instance;
            var controlledApp = Activator.CreateInstance(typeof(ControlledApplication), flags, null, new[] { app }, CultureInfo.InvariantCulture) as ControlledApplication;
            dba.OnShutdown(controlledApp);
        }

        private ExportDataApp LoadDbApp(Autodesk.Revit.ApplicationServices.Application app)
        {
            var instance = new ExportDataApp();
            var flags = BindingFlags.NonPublic | BindingFlags.Instance;
            var controlledApp = Activator.CreateInstance(typeof(ControlledApplication), flags, null, new[] { app }, CultureInfo.InvariantCulture) as ControlledApplication;

            instance.OnStartup(controlledApp);

            return instance;
        }
    }
}
