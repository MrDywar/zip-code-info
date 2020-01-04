using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace ZCI.Domain.Models.Openweather
{
    public class OpenweatherCurrent
    {
        public Coordinates Coord { get; set; }
        public Main Main { get; set; }
        public string Name { get; set; }
    }
}
