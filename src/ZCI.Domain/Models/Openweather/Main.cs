using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace ZCI.Domain.Models.Openweather
{
    public class Main
    {
        public double Temp { get; set; }

        [JsonIgnore]
        public string TempUnits { get; set; }
    }
}
