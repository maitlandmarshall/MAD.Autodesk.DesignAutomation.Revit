using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.ExtensibleStorage;
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
                var type = element.GetType().Name;
                var p = this.GetEntityFromElement(element, type).ToList();

                yield return new DataReaderEntity
                {
                    Values = p.ToDictionary(y => y.Key, y => y.Value),
                    Type = type,
                    Id = element.Id.IntegerValue
                };
            }
        }

        public IEnumerable<KeyValuePair<string, object>> GetEntityFromElement(Element e, string type)
        {
            var elementParams = e.GetOrderedParameters();
            var paramNames = new HashSet<string>();

            yield return new KeyValuePair<string, object>("Id", e.Id.IntegerValue);
            yield return new KeyValuePair<string, object>("Name", e.Name);
            yield return new KeyValuePair<string, object>("CategoryId", e.Category?.Id?.IntegerValue);
            yield return new KeyValuePair<string, object>("CategoryName", e.Category?.Name);

            var elementType = e.Document.GetElement(e.GetTypeId()) as ElementType;

            if (elementType != null)
            {
                yield return new KeyValuePair<string, object>("FamilyName", elementType.FamilyName);
                yield return new KeyValuePair<string, object>("TypeName", elementType.Name);
                
                var typeParams = elementType.GetOrderedParameters();

                foreach (var p in typeParams)
                {
                    var value = p.AsValueString();
                    var key = $"Type_{p.Definition.ParameterGroup.ToString()}_{p.Definition.Name}";

                    yield return new KeyValuePair<string, object>(key, value);
                }
            }


            foreach (var p in elementParams)
            {
                var value = p.AsValueString();
                var key = $"{p.Definition.ParameterGroup.ToString()}_{p.Definition.Name}";

                yield return new KeyValuePair<string, object>(key, value);
            }
        }
    }
}

