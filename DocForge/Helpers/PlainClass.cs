using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocForge.Helpers
{
    public class PlainClass
    {
        public string name { get; set; }

        public List<PlainClass> children { get; set; }

        //public bool primary { get; set; }

        public PlainClass()
        {
           // this.children = new List<PlainClass>();
        }
    }
}
