using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anno.EasyMod.Metadata
{
    public class Localized
    {
        public string? Chinese { get; set; }
        public string? English { get; set; }
        public string? French { get; set; }
        public string? German { get; set; }
        public string? Italian { get; set; }
        public string? Japanese { get; set; }
        public string? Korean { get; set; }
        public string? Polish { get; set; }
        public string? Russian { get; set; }
        public string? Spanish { get; set; }
        public string? Taiwanese { get; set; }

        public Localized() { }

        // keep most common languages on top
        public bool HasAny() =>
            English is not null || German is not null ||
            French is not null || Italian is not null || Polish is not null || Russian is not null || Spanish is not null ||
            Japanese is not null || Korean is not null || Taiwanese is not null;
    }
}
