using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Web;
using GeoNameAddress.Models;
using shapelib;
using SocialExplorer.IO.FastDBF;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GeoNameAddress.Services
{
    public class ShpOperations
    {
        //从源文件获取字段信息
        public List<R_FieldInf> GetFieldInfs(string path)
        {
            List<R_FieldInf> FieldInfs = new List<R_FieldInf>();
            string dbfpath = path;

            #region 该部分使用FastDBF获取字段名称  返回List<string> fieldNames
            DbfFile dbf = new DbfFile(Encoding.Default);
            dbf.Open(dbfpath, FileMode.Open);
            DbfHeader dh = dbf.Header;
            List<string> fieldNames = new List<string>();
            int fieldCount = dh.ColumnCount;
            for (int index = 0; index < fieldCount; index++)
            {
                fieldNames.Add(dh[index].Name);
            }
            dbf.Close();
            #endregion

            #region 该部分使用Shapelib获取字段类型 返回List<string> fieldTypes
            //获取字段类型
            IntPtr hDbf = ShapeLib.DBFOpen(dbfpath, "rb+");//"rb"(只读)"rb+"(读/写)  
            int pointCount = ShapeLib.DBFGetRecordCount(hDbf);
            List<string> fieldTypes = new List<string>();
            StringBuilder stringBuilder = new StringBuilder(20);
            int pnWidth = 10;
            int pnDecimals = 10;
            for (int index = 0; index < fieldCount; index++)
            {
                string type = TypeConvert(ShapeLib.DBFGetFieldInfo(hDbf, index, stringBuilder, ref pnWidth, ref pnDecimals).ToString());
                fieldTypes.Add(type);
            }
            ShapeLib.DBFClose(hDbf);
            #endregion

            //实例化类型            
            for (int index = 0; index < fieldCount; index++)
            {
                FieldInfs.Add(new R_FieldInf(fieldNames[index], fieldTypes[index]));
            }
            return FieldInfs;
        }

        //Shapelib提供的类型转换为ES支持的类型名称
        private string TypeConvert(string FTType)
        {
            string ConvertResult = "text";
            switch (FTType)
            {
                case "FTString":
                    break;
                case "FTInteger":
                    ConvertResult = "integer";
                    break;
                case "FTDouble":
                    ConvertResult = "double";
                    break;
                default:
                    ConvertResult = "text";
                    break;
            }
            return ConvertResult;
        }

        

        public List<InputFields> GetFieldValues(List<R_FieldInf> fieldInfs,FieldMapping vitalFields,string path)
        {
            List<InputFields> result = new List<InputFields>();
            string dbfpath = path;
            IntPtr hDbf = ShapeLib.DBFOpen(dbfpath, "rb+");
          
            int FieldCount = fieldInfs.Count();
            int PointCount = ShapeLib.DBFGetRecordCount(hDbf);
            for (int pi = 0; pi < PointCount; pi++)
            {
                InputFields point=new InputFields();
                string other="{";
                for (int i = 0; i < FieldCount; i++)
                {
                        
                    if (fieldInfs[i].FieldName == vitalFields.ID)
                    {
                         IntPtr FieldvaluePtr = ShapeLib.DBFReadStringAttribute(hDbf, pi, i);
                         string FieldValue = Marshal.PtrToStringAnsi(FieldvaluePtr, 255).Replace("\0", "");
                         point.ID = FieldValue;
                    }
                    else
                    {
                         if (fieldInfs[i].FieldName == vitalFields.Name)
                         {
                             IntPtr FieldvaluePtr = ShapeLib.DBFReadStringAttribute(hDbf, pi, i);
                             string FieldValue = Marshal.PtrToStringAnsi(FieldvaluePtr, 255).Replace("\0", "");
                             point.Name = FieldValue;
                         }
                         else
                         {
                             if (fieldInfs[i].FieldName == vitalFields.Address)
                             {
                                 IntPtr FieldvaluePtr = ShapeLib.DBFReadStringAttribute(hDbf, pi, i);
                                 string FieldValue = Marshal.PtrToStringAnsi(FieldvaluePtr, 255).Replace("\0", "");
                                 point.Addresss = FieldValue;
                             }
                             else
                             {
                                 if (fieldInfs[i].FieldName == vitalFields.Xcoordinate)
                                 {
                                      IntPtr FieldvaluePtr = ShapeLib.DBFReadStringAttribute(hDbf, pi, i);
                                      string FieldValue = Marshal.PtrToStringAnsi(FieldvaluePtr, 255).Replace("\0", "");
                                      point.X = FieldValue;
                                 }
                                 else
                                 {
                                      if (fieldInfs[i].FieldName == vitalFields.Ycoordinate)
                                      {
                                          IntPtr FieldvaluePtr = ShapeLib.DBFReadStringAttribute(hDbf, pi, i);
                                          string FieldValue = Marshal.PtrToStringAnsi(FieldvaluePtr, 255).Replace("\0", "");
                                          point.Y = FieldValue;
                                      }
                                      else
                                      {
                                          if (fieldInfs[i].FieldType == "text")
                                          {
                                              IntPtr FieldvaluePtr = ShapeLib.DBFReadStringAttribute(hDbf, pi, i);
                                              string FieldValue = Marshal.PtrToStringAnsi(FieldvaluePtr, 255).Replace("\0", "");
                                              other += "\"" + fieldInfs[i].FieldName + "\":\"" + FieldValue + "\",";
                                          }
                                          else
                                          {
                                              if (fieldInfs[i].FieldType == "integer")
                                              {
                                                  int FieldValue = ShapeLib.DBFReadIntegerAttribute(hDbf, pi, i);
                                                  other += "\"" + fieldInfs[i].FieldName + "\":" + FieldValue+",";
                                              }
                                              else
                                              {
                                                  double FieldValue = ShapeLib.DBFReadDoubleAttribute(hDbf, pi, i);
                                                  other += "\"" + fieldInfs[i].FieldName + "\":" + FieldValue+",";
                                              }
                                          }                                
                                      }
                                 }
                             }
                         }
                    }                        
                }
                other = other.Substring(0, other.Length - 1)+"}";
                point.Other = other;
                result.Add(point);
            }
            ShapeLib.DBFClose(hDbf);
            return result;
          
                
        
        }

        IntPtr hShpPoint;
        public bool CreatePointFile(string path, string[] pointJson)
        {
            if (pointJson == null || pointJson.Length == 0) return false;

            if (System.IO.Directory.Exists(path) == false) Directory.CreateDirectory(path);
            ShapeLib.ShapeType shpType = ShapeLib.ShapeType.Point;

            hShpPoint = ShapeLib.SHPCreate(path + "\\poi", shpType);
            DbfFile odbf = CreateAttr(path, pointJson);
            DbfRecord orec = new DbfRecord(odbf.Header) { AllowDecimalTruncate = true };


            //逐个点录入属性信息及提取坐标
            for (int i = 0; i < pointJson.Length; i++)
            {
                //取出JSON中的属性信息
                JObject obj = (JObject)JsonConvert.DeserializeObject(pointJson[i]);
                string attrs = obj["attr"].ToString();
                string attrtypes = obj["attrtype"].ToString();
                string geometry = obj["geometry"].ToString();

                //将该点属性信息记录为List<PointInf>
                List<PointInf> pointinf = new List<PointInf>();
                List<string> values = new List<string>();
                List<string> types = new List<string>();
                JToken attrname = (JToken)JsonConvert.DeserializeObject(attrs);
                int listLenhth = 0;
                foreach (JProperty jp in attrname)
                {
                    values.Add(jp.Value.ToString());
                    listLenhth++;
                }
                JToken attrtype = (JToken)JsonConvert.DeserializeObject(attrtypes);
                foreach (JProperty jp in attrtype)
                {
                    types.Add(jp.Value.ToString());
                }
                for (int a = 0; a < listLenhth - 1; a++)
                {
                    pointinf.Add(new PointInf(values[a], types[a]));
                }

                try
                {
                    //生成坐标点
                    List<Point> geo = CreatePointFilePointList(geometry);
                    double[] xCoord = new double[1];
                    double[] yCoord = new double[1];
                    xCoord[0] = geo[0].X;
                    yCoord[0] = geo[0].Y;
                    IntPtr pShp = ShapeLib.SHPCreateSimpleObject(shpType, 1, xCoord, yCoord, null);
                    ShapeLib.SHPWriteObject(hShpPoint, -1, pShp);
                    ShapeLib.SHPDestroyObject(pShp);

                    //录入属性信息
                    for (int a = 0; a < listLenhth - 1; a++)
                    {
                        if (pointinf[a].Type == "text")
                        {
                            orec[a] = pointinf[a].Value;
                        }
                        else
                        {
                            string c= pointinf[a].Value.ToString();
                            orec[a] = c;
                            
                        }
                    }
                    odbf.Write(orec, true);
                  
                }
                catch
                {
                    string c = pointinf[0].Value.ToString();
                }

            }
            //关闭
            odbf.Close();
            if (hShpPoint != IntPtr.Zero)
            {
                ShapeLib.SHPClose(hShpPoint);
            }

            return true;
        }

        private DbfFile CreateAttr(string path, string[] pointJson)
        {
            //这一块代码是为了将字段名和字段类型分别录入List<string>names和types中去
            JObject obj = (JObject)JsonConvert.DeserializeObject(pointJson[0]);
            string attrtypes = obj["attrtype"].ToString();
            string attrs = obj["attr"].ToString();
            List<string> names = new List<string>();
            List<string> types = new List<string>();
            JToken attrtype = (JToken)JsonConvert.DeserializeObject(attrtypes);
            int listLenhth = 0;
            foreach (JProperty jp in attrtype)
            {
                types.Add(jp.Value.ToString());
                listLenhth++;
            }
            JToken fieldname = (JToken)JsonConvert.DeserializeObject(attrtypes);
            foreach (JProperty jp in fieldname)
            {
                names.Add(jp.Name.ToString());

            }


            //创建DBF文件并根据字段名创建属性空表
            string testPath = path;
            var odbf = new DbfFile(Encoding.GetEncoding(65001));
            odbf.Open(Path.Combine(testPath, "poi.dbf"), FileMode.Create);

            for (int a = 0; a < listLenhth - 1; a++)
            {
                if (types[a] == "double")
                {
                    odbf.Header.AddColumn(new DbfColumn(names[a], DbfColumn.DbfColumnType.Number, 20, 10));
                }
                else
                { if (types[a] == "integer")
                    {
                        //odbf.Header.AddColumn(new DbfColumn(names[a], DbfColumn.DbfColumnType.Integer, 20, 10));
                        odbf.Header.AddColumn(new DbfColumn(names[a], DbfColumn.DbfColumnType.Number, 20, 10));
                    }
                    else {
                        odbf.Header.AddColumn(new DbfColumn(names[a], DbfColumn.DbfColumnType.Character, 100, 0));
                    }
                    
                }
            }

            return odbf;
        }
        private List<Point> CreatePointFilePointList(string wkt)
        {
            List<Point> geo = new List<Point>();
            string[] strs = wkt.Split("&".ToCharArray());
            Point pt = new Point(double.Parse(strs[0]), double.Parse(strs[1]));
            geo.Add(pt);
            return geo;
        }
    }
}