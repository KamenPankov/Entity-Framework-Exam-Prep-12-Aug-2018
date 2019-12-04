namespace SoftJail.DataProcessor
{

    using Data;
    using Newtonsoft.Json;
    using SoftJail.DataProcessor.ExportDto;
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Serialization;

    public class Serializer
    {
        public static string ExportPrisonersByCells(SoftJailDbContext context, int[] ids)
        {
            var prisoners = context.Prisoners
                 .Where(p => ids.Any(id => id == p.Id))
                 .Select(p => new
                 {
                     Id = p.Id,
                     Name = p.FullName,
                     CellNumber = p.Cell.CellNumber,
                     Officers = p.PrisonerOfficers.Select(o => new
                     {
                         OfficerName = o.Officer.FullName,
                         Department = o.Officer.Department.Name
                     })
                     .OrderBy(o => o.OfficerName)
                     .ToArray(),
                     TotalOfficerSalary = p.PrisonerOfficers.Sum(po => po.Officer.Salary)
                 })
                 .OrderBy(p => p.Name)
                 .ThenBy(p => p.Id)
                 .ToArray();

            string jsonString = JsonConvert.SerializeObject(prisoners, Formatting.Indented);

            return jsonString;
        }

        public static string ExportPrisonersInbox(SoftJailDbContext context, string prisonersNames)
        {
            string[] prisonersNamesArray = prisonersNames.Split(",");

            PrisonerExportDto[] prisoners = context.Prisoners
                .Where(p => prisonersNamesArray.Any(n => n == p.FullName))
                .Select(p => new PrisonerExportDto()
                {
                    Id = p.Id,
                    Name = p.FullName,
                    IncarcerationDate = p.IncarcerationDate.ToString("yyyy-MM-dd"),
                    EncryptedMessages = p.Mails
                    .Select(m => new MailExportDto()
                    {
                        Descroption = ReverseString(m.Description)
                    })
                    .ToArray()
                })
                .OrderBy(p => p.Name)
                .ThenBy(p => p.Id)
                .ToArray();
            
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(PrisonerExportDto[]), 
                                                            new XmlRootAttribute("Prisoners"));

            XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
            namespaces.Add(string.Empty, string.Empty);

            StringBuilder stringBuilder = new StringBuilder();

            xmlSerializer.Serialize(new StringWriter(stringBuilder), prisoners, namespaces);

            return stringBuilder.ToString().TrimEnd();
        }

        private static string ReverseString(string stringToReverse)
        {
            StringBuilder stringBuilder = new StringBuilder();

            for (int i = stringToReverse.Length - 1; i >= 0 ; i--)
            {
                stringBuilder.Append(stringToReverse[i]);
            }

            return stringBuilder.ToString().TrimEnd();
        }
    }
}