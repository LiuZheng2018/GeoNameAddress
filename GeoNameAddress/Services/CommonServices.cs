using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using GeoNameAddress.Models;
using ICSharpCode.SharpZipLib.Zip;
using Zjgis.ZipHelper;

namespace GeoNameAddress.Services
{
    public class CommonServices
    {
        public string GetOtherTemplate(List<R_FieldInf> fieldInfs,FieldMapping vitalFields)
        {
            string OtherTemplate= "{";
            int FieldCount = fieldInfs.Count();
            for (int i = 0; i < FieldCount; i++)
            {
                if (fieldInfs[i].FieldName != vitalFields.ID && fieldInfs[i].FieldName != vitalFields.Name && fieldInfs[i].FieldName != vitalFields.Address && fieldInfs[i].FieldName != vitalFields.Xcoordinate && fieldInfs[i].FieldName != vitalFields.Ycoordinate)
                {
                    if (fieldInfs[i].FieldType == "text")
                    {
                        OtherTemplate += "\"" + fieldInfs[i].FieldName + "\":\"\",";
                    }
                    else
                    {                                           
                            OtherTemplate += "\"" + fieldInfs[i].FieldName + "\":0,";                       
                    }
                }
            }
            OtherTemplate = OtherTemplate.Substring(0, OtherTemplate.Length - 1) + "}";
            return OtherTemplate;
        }

        public string GetOtherType(List<R_FieldInf> fieldInfs, FieldMapping vitalFields)
        {
            string othertype="";
            int FieldCount = fieldInfs.Count();
            for (int i = 0; i < FieldCount; i++)
            {
                if (fieldInfs[i].FieldName != vitalFields.ID && fieldInfs[i].FieldName != vitalFields.Name && fieldInfs[i].FieldName != vitalFields.Address && fieldInfs[i].FieldName != vitalFields.Xcoordinate && fieldInfs[i].FieldName != vitalFields.Ycoordinate)
                {
                    othertype += "\""+ fieldInfs[i].FieldName+"\":\""+ fieldInfs[i].FieldType+"\",";
                }
            }
            othertype = othertype.Substring(0, othertype.Length - 1);
            return othertype;
        }
         
        public MemoryStream CreateShpZipStream(string path, string[] pointsJson)
        {
            ShpOperations shpOperations = new ShpOperations();
            shpOperations.CreatePointFile(path, pointsJson);
            
            string file = path + ".zip";
            IZipHelper iZipHelper = new ZipHelperClass();
            iZipHelper.ZipFileDirectory(path, path + ".zip");
            //ZipFile.Create(path);
            FileStream stream = null;
            byte[] pReadByte = new byte[0];
            //文件流转为内存流
            try
            {
                stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                BinaryReader r = new BinaryReader(stream);
                r.BaseStream.Seek(0, SeekOrigin.Begin);    //将文件指针设置到文件开  
                pReadByte = r.ReadBytes((int)r.BaseStream.Length);
            }
            catch (Exception e)
            {
                throw new Exception("读取文件流失败！");
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                    stream.Dispose();
                }
            }

            MemoryStream memStream = new MemoryStream(pReadByte);
            return memStream;
        }

        //删除文件
        public void DeleteZipFile(string path)
        {
            foreach (string d in Directory.GetFileSystemEntries(Path.GetDirectoryName(path) + "\\"))
            {
                if (File.Exists(d))
                {
                    FileInfo fi = new FileInfo(d);
                    if (fi.Attributes.ToString().IndexOf("ReadOnly") != -1)
                        fi.Attributes = FileAttributes.Normal;

                    File.Delete(d);
                }
                else
                {
                    Directory.Delete(d, true);
                }
            }
        }
    }
}