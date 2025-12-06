using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace Movies_Project.Models
{
    public class Manager
    {
        public int ManagerID { get; set; }
        public string Name { get; set; }
        public string Adress { get; set; }
        public DateTime BirthDate { get; set; }
        public ICollection<Actor>? Actors { get; set; }
    }
}
