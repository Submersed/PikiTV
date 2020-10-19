using System;
using System.Collections.Generic;
using System.Text;

namespace BlackApp.Models
{
    public class TvProgram
    {
        public int Id { get; set; }
        public string Url { get; set; }
        public string Img { get; set; }
        public string Title { get; set; }
        public bool Selected { get; set; }

    }
}
