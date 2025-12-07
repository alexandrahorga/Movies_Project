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
                // ❌ Comentați SAU Ștergeți această condiție temporar
                // if (context.Movie.Any())
                // {
                //     return; // BD a fost creata anterior
                // }

                // Asigurați-vă că ștergeți datele vechi înainte de a adăuga altele noi
                context.Movie.RemoveRange(context.Movie);
                context.Genre.RemoveRange(context.Genre);
                context.SaveChanges(); // Salvați ștergerile
                var comedyGenre = new Genre { Name = "Comedy" };
                var adventureGenre = new Genre { Name = "Adventure" };
                var romanceGenre = new Genre { Name = "Romance" };
                context.Genre.AddRange(
                   comedyGenre,
                   adventureGenre,
                   romanceGenre
                );
                context.Movie.AddRange(
                new Movie
                {
                    Title = "Home Alone",
                    Director = "Chris Columbus",
                    Budget = Decimal.Parse("222,305"),
                    Genre = comedyGenre,
                },

                new Movie
                {
                    Title = "Agent 007",
                    Director = "John Glen",
                    Budget = Decimal.Parse("180,000"),
                    Genre = adventureGenre,
                },
               
                new Movie
                {
                    Title = "The Notebook",
                    Director = "Nick Cassavetes", Budget=Decimal.Parse("27,898"),
                    Genre = romanceGenre,
                }
               
                );

               // context.Genre.AddRange(
               //new Genre { Name = "Roman" },
               //new Genre { Name = "Nuvela" },
               //new Genre { Name = "Poezie" }
               // );
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
