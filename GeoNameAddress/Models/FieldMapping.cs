using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GeoNameAddress.Models
{
    //源文件与ElasticSearch中字段的映射
    public class FieldMapping
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string Address { get; set;}
        public string Xcoordinate { get; set; }
        public string Ycoordinate { get; set; }
         
    }
}