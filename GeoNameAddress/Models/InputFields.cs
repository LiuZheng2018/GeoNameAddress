using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using Nest;
namespace GeoNameAddress.Models
{
    [ElasticsearchType(Name = "mytype")]
    public class InputFields
    {
        //public InputFields(string _id,string _name, string _address, string _x,string _y,string _other)
        //{
        //    ID = _id;
        //    Name = _name;
        //    Addresss = _address;
        //    X = _x;
        //    Y = _y;
        //    Other = _other;
        //}

        [Display(Name = "ID")]    
        [Text(Name = "id",Fielddata = true, Index = true)]
        public string ID { get; set; }

        [Display(Name = "地名")]
        [Text(Name = "name", Analyzer = "ik_max_word", Fielddata = true, Index = true)]
        public string Name { get; set; }

        [Display(Name = "地址")]
        [Text(Name = "address", Analyzer = "ik_max_word", Fielddata = true, Index = true)]
        public string Addresss { get; set; }
        
        [Display(Name = "X坐标")]
        [Text(Name = "x", Fielddata = true, Index = true)]
        public string X { get; set; }

        [Display(Name = "Y坐标")]
        [Text(Name = "y", Fielddata = true, Index = true)]
        public string Y { get; set; }

        [Text(Name = "other", Fielddata = true, Index = true)]
        public string Other { get; set; }
    }
}