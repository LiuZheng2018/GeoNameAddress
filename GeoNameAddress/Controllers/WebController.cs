using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using GeoNameAddress.Models;
using GeoNameAddress.Services;
using Nest;
using Newtonsoft.Json;

namespace GeoNameAddress.Controllers
{
    public class WebController : ApiController
    {
        [HttpGet]
        [Route("Web/DownloadStart")]
        public HttpResponseMessage DownloadStart()
        {
            try
            {
                //读取配置
                string path = System.AppDomain.CurrentDomain.BaseDirectory + "config\\fieldInfs.txt";
                string c = File.ReadAllText(path);
                List<R_FieldInf> fieldInfs = JsonConvert.DeserializeObject<List<R_FieldInf>>(c);
                string path2 = System.AppDomain.CurrentDomain.BaseDirectory + "config\\mapping.txt";
                string d = File.ReadAllText(path2);
                Models.FieldMapping vitalFields = JsonConvert.DeserializeObject<Models.FieldMapping>(d);
                //
                CommonServices commonServices = new CommonServices();
                string otherTypes = commonServices.GetOtherType(fieldInfs, vitalFields);
                //
                ESServices eSServices = new ESServices();
                ElasticClient client = eSServices.Connect_ES();
                string[] pointsJson = eSServices.GetAllPoints(client, vitalFields, otherTypes);
                //
                string fguid = Guid.NewGuid().ToString();
                string path3 = System.AppDomain.CurrentDomain.BaseDirectory + "\\Download\\" + fguid;
                 MemoryStream stream = commonServices.CreateShpZipStream(path3, pointsJson);
                commonServices.DeleteZipFile(path3);
                //下载文件
                HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StreamContent(stream);
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = "result.zip"
                };
                return response;
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex.Message);
                //return new HttpResponseMessage(HttpStatusCode.NoContent);
            }
            finally
            {
                
            }
        }
    }
}
