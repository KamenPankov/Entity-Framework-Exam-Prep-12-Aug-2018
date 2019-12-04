namespace SoftJail.DataProcessor
{

    using Data;
    using SoftJail.Data.Models;
    using System;
    using System.Linq;

    public class Bonus
    {
        public static string ReleasePrisoner(SoftJailDbContext context, int prisonerId)
        {
            Prisoner prisonerToRelease = context.Prisoners.FirstOrDefault(p => p.Id == prisonerId);

            if (prisonerToRelease.ReleaseDate == null)
            {
                return $"Prisoner {prisonerToRelease.FullName} is sentenced to life";
            }
            else
            {
                prisonerToRelease.ReleaseDate = DateTime.Now;
                prisonerToRelease.CellId = null;

                context.SaveChanges();

                return $"Prisoner {prisonerToRelease.FullName} released";
            }
        }
    }
}
