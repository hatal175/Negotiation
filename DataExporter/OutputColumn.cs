using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Negotiation.Models;

namespace DataExporter
{
    public class OutputColumn
    {
        public String Name { get; set; }
        public Func<Game, String> Func { get; set; }

        public OutputColumn(String name, Func<Game, string> func)
        {
            Name = name;
            Func = func;
        }
    }
}
