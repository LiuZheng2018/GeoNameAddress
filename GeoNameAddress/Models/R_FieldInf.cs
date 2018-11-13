using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GeoNameAddress.Models
{
    //从DBF文件读取的字段名和字段类型
    public class R_FieldInf
    {
        public R_FieldInf(string _fieldNmae,string _fieldType)
        {
            FieldName = _fieldNmae;
            FieldType = _fieldType;
        }
        public string FieldName { get; set; }
        public string FieldType { get; set; }
    }
}