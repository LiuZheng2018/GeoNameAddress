using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GeoNameAddress.Models
{
    public class PointInf
    {
        public PointInf(string _value, string _type)
        {
            Value = _value;
            Type = _type;
        }
        public string Value { get; set; }
        public string Type { get; set; }
    }
}