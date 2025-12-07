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
                 //❌ Comentați SAU Ștergeți această condiție temporar
                 if (context.Movie.Any())
                {
                    return; // BD a fost creata anterior
                }

                if (context.Director.Any() || context.Manager.Any() || context.Genre.Any())
                {
                    // Dacă datele au fost introduse manual ulterior, dar filmele inițiale nu,
                    // ar putea fi o problemă. Cel mai sigur este să ștergeți baza de date
                    // o ultimă dată pentru a sincroniza DbInitializer.
                    // Dacă nu doriți să ștergeți DB-ul, ignorați această verificare, dar aveți grijă.
                    // Vom merge cu presupunerea că DB-ul este gol dacă Movie este gol.
                }

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

                var ChrisDirector = new Director { FirstName = "Chris", LastName = "Columbus" };
                var JohnDirector = new Director { FirstName = "John", LastName = "Glen" };
                var NickDirector = new Director { FirstName = "Nick", LastName = "Cassavetes" };

                context.Director.AddRange(
                    ChrisDirector,
                    JohnDirector,
                    NickDirector
                );

                context.Movie.AddRange(
                new Movie
                {
                    Title = "Home Alone",
                    Director = ChrisDirector,
                    Budget = Decimal.Parse("222,305"),
                    Genre = comedyGenre,
                },

                new Movie
                {
                    Title = "Agent 007",
                    Director = JohnDirector,
                    Budget = Decimal.Parse("180,000"),
                    Genre = adventureGenre,
                },

                new Movie
                {
                    Title = "The Notebook",
                    Director = NickDirector,
                    Budget = Decimal.Parse("27,898"),
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
