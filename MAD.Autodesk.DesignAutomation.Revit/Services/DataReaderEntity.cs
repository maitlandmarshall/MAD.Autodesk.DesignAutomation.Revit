using System.Collections.Generic;

namespace MAD.Autodesk.DesignAutomation.Revit.Services
{
    internal class DataReaderEntity
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public IDictionary<string, object> Values { get; set; }

        public override string ToString()
        {
            return $"{Type}:{Values.Count}";
        }
    }
}
