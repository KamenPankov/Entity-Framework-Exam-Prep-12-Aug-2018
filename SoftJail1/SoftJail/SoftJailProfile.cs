namespace SoftJail
{
    using AutoMapper;
    using SoftJail.Data.Models;
    using SoftJail.DataProcessor.ImportDto;
    using System;
    using System.Globalization;

    public class SoftJailProfile : Profile
    {
        // Configure your AutoMapper here if you wish to use it. If not, DO NOT DELETE THIS CLASS
        public SoftJailProfile()
        {
            this.CreateMap<CellImportDto, Cell>();

            this.CreateMap<DepartmentImportDto, Department>();

            this.CreateMap<MailImportDto, Mail>();

            this.CreateMap<PrisonerImportDto, Prisoner>()
                .ForMember(x => x.IncarcerationDate,
                y => y.MapFrom(x => DateTime.ParseExact(x.IncarcerationDate, "dd/MM/yyyy", CultureInfo.InvariantCulture)))
                .ForMember(x => x.ReleaseDate,
                y => y.MapFrom(x => !string.IsNullOrEmpty(x.ReleaseDate) 
                                    ? DateTime.ParseExact(x.ReleaseDate, "dd/MM/yyyy", CultureInfo.InvariantCulture) 
                                    : (DateTime?)null));


        }
    }
}
