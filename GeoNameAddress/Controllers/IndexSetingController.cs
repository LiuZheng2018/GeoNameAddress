using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GeoNameAddress.Services;
using GeoNameAddress.Models;
using Nest;
using Newtonsoft.Json;
using System.IO;

namespace GeoNameAddress.Controllers
{
    public class IndexSetingController : Controller
    {
        static List<R_FieldInf> FieldInfs = new List<R_FieldInf>();
        static Models.FieldMapping VitalFields;
        static List<string> allFields = new List<string>();
        // GET: IndexSeting
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ReadDbf()
        {
            string DbfPath = "C:\\testdata\\Address_SpatialJoin2new.dbf";
            ShpOperations shpOperations = new ShpOperations();
            FieldInfs=shpOperations.GetFieldInfs(DbfPath);
            //将字段信息写进config
            JsonSerializer serializer3 = new JsonSerializer();
            StringWriter sw3 = new StringWriter();
            serializer3.Serialize(new JsonTextWriter(sw3), FieldInfs);
            string fieldinfs = sw3.GetStringBuilder().ToString();
            string path = System.AppDomain.CurrentDomain.BaseDirectory + "config\\fieldInfs.txt";
            FileStream fs3 = new FileStream(path, FileMode.Create, FileAccess.Write);//创建写入文件 
            StreamWriter Asss3 = new StreamWriter(fs3);
            Asss3.WriteLine(fieldinfs);//开始写入值
            Asss3.Close();
            fs3.Close();
            return RedirectToAction("CreateIndex");
        }

        public ActionResult CreateIndex()
        {
            for (int i = 0; i < FieldInfs.Count(); i++)
            {
                allFields.Add(FieldInfs[i].FieldName);
            }
            ViewBag.AllFields = new SelectList(allFields);
            return View(VitalFields);
        }
        [HttpPost]
        public ActionResult CreateIndex(Models.FieldMapping _vitalFields)
        {
            VitalFields = _vitalFields;

            //将映射转为json存入config
            JsonSerializer serializer = new JsonSerializer();
            StringWriter sw = new StringWriter();
            serializer.Serialize(new JsonTextWriter(sw), VitalFields);
            string mapping = sw.GetStringBuilder().ToString();
            string path = System.AppDomain.CurrentDomain.BaseDirectory + "config\\mapping.txt";
            FileStream fs1 = new FileStream(path, FileMode.Create, FileAccess.Write);//创建写入文件 
            StreamWriter Asss = new StreamWriter(fs1);
            Asss.WriteLine(mapping);//开始写入值
            Asss.Close();
            fs1.Close();
            //将OtherTemplate保存进配置
            CommonServices common = new CommonServices();
            string OtherTemplate = common.GetOtherTemplate(FieldInfs, VitalFields);
            string path2 = System.AppDomain.CurrentDomain.BaseDirectory + "config\\OtherTemplate.txt";
            FileStream fs2 = new FileStream(path2, FileMode.Create, FileAccess.Write);//创建写入文件 
            StreamWriter Asss2 = new StreamWriter(fs2);
            Asss2.WriteLine(OtherTemplate);//开始写入值
            Asss2.Close();
            fs2.Close();
            //
            ESServices eSServices = new ESServices();
            ElasticClient client= eSServices.Connect_ES();
            eSServices.CreateIndex(client);
            ShpOperations shpOperations = new ShpOperations();
            string DbfPath = "C:\\testdata\\Address_SpatialJoin2new.dbf";
            List<InputFields> inputFields = new List<InputFields>();
            inputFields=shpOperations.GetFieldValues(FieldInfs, VitalFields, DbfPath);
            eSServices.PutDoc(inputFields,client);
            return RedirectToAction("Index");
        }
    }
} 