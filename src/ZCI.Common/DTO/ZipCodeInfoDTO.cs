using System;
using System.Collections.Generic;
using System.Text;

namespace ZCI.Common.DTO
{
    public class ZipCodeInfoDTO
    {
        public string CityName { get; set; }
        public double Temperature { get; set; }
        public string TemperatureUnits { get; set; }
        public string TimeZone { get; set; }
    }
}
