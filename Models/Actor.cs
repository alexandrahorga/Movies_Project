using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Movies_Project.Models
{
    public class Actor
    {
        public int ActorID { get; set; }
        public string Name { get; set; }

        public DateTime BirthDate { get; set; }
        public int? ManagerID { get; set; }
        public int? MovieID { get; set; }

        public Manager? Manager { get; set; }
        public Movie? Movie { get; set; }
    }
}
