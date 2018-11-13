using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GeoNameAddress.Models
{
    public class SearchResult
    {
        public SearchResult(string _id, string _name, string _address, string _x, string _y,string _score)
        {
            ID = _id;
            Name = _name;
            Addresss = _address;
            X = _x;
            Y = _y;
            Score = _score;
        }

        [Display(Name = "ID")]        
        public string ID { get; set; }

        [Display(Name = "地名")]       
        public string Name { get; set; }

        [Display(Name = "地址")]
        public string Addresss { get; set; }

        [Display(Name = "X坐标")]
        public string X { get; set; }

        [Display(Name = "Y坐标")]
        public string Y { get; set; }

        [Display(Name = "匹配度")]
        public string Score { get; set; }
    }
}
