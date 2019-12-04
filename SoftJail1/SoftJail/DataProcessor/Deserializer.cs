namespace SoftJail.DataProcessor
{
    using AutoMapper;
    using Data;
    using Newtonsoft.Json;
    using SoftJail.Data.Models;
    using SoftJail.Data.Models.Enums;
    using SoftJail.DataProcessor.ImportDto;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Serialization;
    using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

    public class Deserializer
    {
        public static string ImportDepartmentsCells(SoftJailDbContext context, string jsonString)
        {
            DepartmentImportDto[] departmentImportDtos = JsonConvert.DeserializeObject<DepartmentImportDto[]>(jsonString);

            StringBuilder stringBuilder = new StringBuilder();

            List<Department> departments = new List<Department>();

            foreach (DepartmentImportDto departmentImportDto in departmentImportDtos)
            {
                if (!IsValid(departmentImportDto))
                {
                    stringBuilder.AppendLine("Invalid Data");
                    continue;
                }

                bool isCellsImportDtoValid = departmentImportDto.Cells.All(c => IsValid(c));

                if (!isCellsImportDtoValid)
                {
                    stringBuilder.AppendLine("Invalid Data");
                    continue;
                }

                Department department = Mapper.Map<Department>(departmentImportDto);

                List<Cell> cells = new List<Cell>();

                foreach (CellImportDto cellImportDto in departmentImportDto.Cells)
                {
                    Cell cell = Mapper.Map<Cell>(cellImportDto);
                    cell.Department = department;

                    cells.Add(cell);
                }

                department.Cells = cells;

                departments.Add(department);

                stringBuilder.AppendLine($"Imported {department.Name} with {department.Cells.Count()} cells");
            }

            context.Departments.AddRange(departments);
            context.SaveChanges();

            //Console.WriteLine(stringBuilder.ToString().TrimEnd());

            return stringBuilder.ToString().TrimEnd();
        }

        public static string ImportPrisonersMails(SoftJailDbContext context, string jsonString)
        {
            PrisonerImportDto[] prisonerImportDtos = JsonConvert.DeserializeObject<PrisonerImportDto[]>(jsonString);

            StringBuilder stringBuilder = new StringBuilder();

            List<Prisoner> prisoners = new List<Prisoner>();

            //int[] cellsIdsValid = context.Cells.Select(c => c.Id).ToArray();

            foreach (PrisonerImportDto prisonerImportDto in prisonerImportDtos)
            {
                if (!IsValid(prisonerImportDto))
                {
                    stringBuilder.AppendLine("Invalid Data");
                    continue;
                }

                bool isIncarcerationDateValid = DateTime
                    .TryParseExact(prisonerImportDto.IncarcerationDate, "dd/MM/yyyy", 
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime incarcerationDate);

                bool isReleaseDateValid = true;
                if (!string.IsNullOrEmpty(prisonerImportDto.ReleaseDate))
                {
                    isReleaseDateValid = DateTime
                        .TryParseExact(prisonerImportDto.ReleaseDate, "dd/MM/yyyy",
                        CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime releaseDate);
                }

                //bool isprisonerImportDtoCellIdValid = true;
                //if (.CellId != null)
                //{
                //    isCellIdValid = cellsIdsValid.Any(id => id == prisonerImportDto.CellId);
                //}                

                bool isMailsDtoValid = prisonerImportDto.Mails.All(m => IsValid(m));

                if (!isIncarcerationDateValid || !isReleaseDateValid /*|| !isCellIdValid*/ || !isMailsDtoValid)
                {
                    stringBuilder.AppendLine("Invalid Data");
                    continue;
                }

                Prisoner prisoner = Mapper.Map <Prisoner>(prisonerImportDto);

                List<Mail> mails = new List<Mail>();

                foreach (MailImportDto mailImportDto in prisonerImportDto.Mails)
                {
                    Mail mail = Mapper.Map<Mail>(mailImportDto);

                    mail.Prisoner = prisoner;

                    mails.Add(mail);
                }

                prisoner.Mails = mails;

                prisoners.Add(prisoner);

                stringBuilder.AppendLine($"Imported {prisoner.FullName} {prisoner.Age} years old");
            }

            context.Prisoners.AddRange(prisoners);
            context.SaveChanges();

            //Console.WriteLine();
            //Console.WriteLine(stringBuilder.ToString().TrimEnd());

            return stringBuilder.ToString().TrimEnd();
        }

        public static string ImportOfficersPrisoners(SoftJailDbContext context, string xmlString)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(OfficerImportDto[]), new XmlRootAttribute("Officers"));

            OfficerImportDto[] officerImportDtos = (OfficerImportDto[])xmlSerializer.Deserialize(new StringReader(xmlString));

            StringBuilder stringBuilder = new StringBuilder();

            List<Officer> officers = new List<Officer>();

            int[] prisonersIdsValid = context.Prisoners.Select(p => p.Id).ToArray();
            int[] departmentsIdsValid = context.Departments.Select(d => d.Id).ToArray();

            foreach (OfficerImportDto officerImportDto in officerImportDtos)
            {
                bool isPositionValid = Enum.IsDefined(typeof(Position), officerImportDto.Position);
                bool isWeaponValid = Enum.IsDefined(typeof(Weapon), officerImportDto.Weapon);
                bool isDepartmentIdValid = departmentsIdsValid.Any(id => id == officerImportDto.DepartmentId);
                bool isPrisonerIdValid = officerImportDto.Prisoners.All(p => prisonersIdsValid.Any(id => id == p.Id));

                if (!isPositionValid || !isWeaponValid || !isDepartmentIdValid || !isPrisonerIdValid)
                {
                    stringBuilder.AppendLine("Invalid Data");
                    continue;
                }

                Officer officer = new Officer()
                {
                    FullName = officerImportDto.Name,
                    Salary = officerImportDto.Money,
                    Position = Enum.Parse<Position>(officerImportDto.Position),
                    Weapon = Enum.Parse<Weapon>(officerImportDto.Weapon),
                    DepartmentId = officerImportDto.DepartmentId,
                    Department = context.Departments.Single(d => d.Id == officerImportDto.DepartmentId)
                };

                if (!IsValid(officer))
                {
                    stringBuilder.AppendLine("Invalid Data");
                    continue;
                }

                List<OfficerPrisoner> officerPrisoners = new List<OfficerPrisoner>();

                foreach (PrisonerXmlImportDto prisonerXmlImportDto in officerImportDto.Prisoners)
                {
                    OfficerPrisoner officerPrisoner = new OfficerPrisoner()
                    {
                        Officer = officer,
                        PrisonerId = prisonerXmlImportDto.Id,
                        Prisoner = context.Prisoners.Single(p => p.Id == prisonerXmlImportDto.Id)
                    };

                    officerPrisoners.Add(officerPrisoner);
                }

                officer.OfficerPrisoners = officerPrisoners;

                officers.Add(officer);

                stringBuilder.AppendLine($"Imported {officer.FullName} ({officer.OfficerPrisoners.Count()} prisoners)");
            }

            context.Officers.AddRange(officers);
            context.SaveChanges();

            //Console.WriteLine();
            //Console.WriteLine(stringBuilder.ToString().TrimEnd());

            return stringBuilder.ToString().TrimEnd();
        }

        private static bool IsValid(object entity)
        {
            ValidationContext validationContext = new ValidationContext(entity);
            List<ValidationResult> validationResults = new List<ValidationResult>();

            bool result = Validator.TryValidateObject(entity, validationContext, validationResults, true);

            return result;
        }
    }
}