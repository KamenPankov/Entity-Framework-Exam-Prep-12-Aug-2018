using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace SoftJail.DataProcessor.ImportDto
{
    [XmlType("Officer")]
    public class OfficerImportDto
    {
        [XmlElement("Name")]
        public string Name { get; set; }

        [XmlElement("Money")]
        public decimal Money { get; set; }

        [XmlElement("Position")]
        public string Position { get; set; }

        [XmlElement("Weapon")]
        public string Weapon { get; set; }

        [XmlElement("DepartmentId")]
        public int DepartmentId { get; set; }

        [XmlArray("Prisoners")]
        public PrisonerXmlImportDto[] Prisoners { get; set; }
    }
}
