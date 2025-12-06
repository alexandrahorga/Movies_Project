using Microsoft.EntityFrameworkCore;
using Movies_Project.Models;

namespace Movies_Project.Data
{
    public static class DbInitializer
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new Movies_ProjectContext
           (serviceProvider.GetRequiredService
            <DbContextOptions<Movies_ProjectContext>>()))
            {
                if (context.Movie.Any())
                {
                    return; // BD a fost creata anterior
                }
                context.Movie.AddRange(
                new Movie
                {
                    Title = "Baltagul",
                    Director = "Mihail Sadoveanu", Budget=Decimal.Parse("22")},
               
                new Movie
                {
                    Title = "Enigma Otiliei",
                    Director = "George Calinescu",
                    Budget = Decimal.Parse("18")
                },
               
                new Movie
                {
                    Title = "Maytrei",
                    Director = "Mircea Eliade", Budget=Decimal.Parse("27")}
               
                );

                context.Genre.AddRange(
               new Genre { Name = "Roman" },
               new Genre { Name = "Nuvela" },
               new Genre { Name = "Poezie" }
                );
                context.Manager.AddRange(
                new Manager
                {
                    Name = "Popescu Marcela",
                    Adress = "Str. Plopilor, nr. 24",
                    BirthDate = DateTime.Parse("1979-09-01")
                },
                new Manager
                {
                    Name = "Mihailescu Cornel",
                    Adress = "Str. Bucuresti, nr.45,ap. 2",BirthDate=DateTime.Parse("1969 - 07 - 08")}
               
                );

                context.SaveChanges();
            }
        }

    }
}
