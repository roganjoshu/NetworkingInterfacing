using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace locationserver
{
    public class Person
    {
        public string name { get; set; }
        public string location { get; set; }

        public Person(string name, string location)
        {
            this.name = name;
            this.location = location;
        }
    }
}
