using System;
using System.Collections.Generic;
using System.Text;

namespace BlackApp.Models.data
{
    public class Patch
    {
        public List<TvProgram> Tvprograms { get; set; }
        public int Days { get; set; }

        public bool Update { get; set; }
    }
}
