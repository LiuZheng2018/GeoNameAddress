using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GeoNameAddress.Models
{
    public class AnalyzeResult
    {
        public AnalyzeResult(string _token, string _start, string _end)
        {
            Token = _token;
            Start = _start;
            End = _end;
        }
        [Display(Name = "Token")]
        public string Token { get; set; }
        [Display(Name = "StartOffset")]
        public string Start { get; set; }
        [Display(Name = "EndOffset")]
        public string End { get; set; }
    }
}