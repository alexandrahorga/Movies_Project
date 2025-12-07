using System.Collections.Generic;
namespace Movies_Project.Models
{
    public class Director
    {
        public int ID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        // Proprietate de navigare pentru colecția de filme/cărți scrise de acest autor
        public ICollection<Movie>? Movies { get; set; }
    }
}
