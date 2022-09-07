using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.Mechanical;
using DesignAutomationFramework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAD.Autodesk.DesignAutomation.Revit.Services
{
    internal class DataReader
    {
        private readonly DesignAutomationData designAutomationData;

        public DataReader(DesignAutomationData designAutomationData)
        {
            this.designAutomationData = designAutomationData;
        }

        public IEnumerable<Element> GetElements()
        {
            FilteredElementCollector col = new FilteredElementCollector(this.designAutomationData.RevitDoc);
            return col.WhereElementIsNotElementType();
        }

        public IEnumerable<DataReaderEntity> GetEntitiesFromElements(IEnumerable<Element> elements)
        {
            foreach (var element in elements)
            {
                // Convert the element's parameters into an entity
                var p = this.GetEntityFromElement(element).ToList();

                yield return new DataReaderEntity
                {
                    Values = p.ToDictionary(y => y.Key, y => y.Value),
                    Type = element.GetType().Name,
                    Id = element.Id.IntegerValue
                };
            }
        }

        public IEnumerable<KeyValuePair<string, object>> GetEntityFromElement(Element e)
        {
            var elementParams = e.GetOrderedParameters();
            var paramNames = new HashSet<string>();

            var m = e.GetAnalyticalModel();

            yield return new KeyValuePair<string, object>("Id", e.Id.IntegerValue);
            yield return new KeyValuePair<string, object>("Name", e.Name);
            yield return new KeyValuePair<string, object>("CategoryId", e.Category?.Id?.IntegerValue);
            yield return new KeyValuePair<string, object>("CategoryName", e.Category?.Name);

            foreach (var p in elementParams)
            {
                var value = p.AsValueString();
                var key = $"{p.Definition.Name}{p.Id}";

                if (!paramNames.Add(key))
                    continue;

                yield return new KeyValuePair<string, object>(key, value);
            }
        }
    }
}

