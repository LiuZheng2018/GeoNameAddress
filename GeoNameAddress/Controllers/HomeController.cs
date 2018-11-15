using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GeoNameAddress.Models;
using GeoNameAddress.Services;
using Nest;
using Newtonsoft.Json;

namespace GeoNameAddress.Controllers
{
    public class HomeController : Controller
    {
        ESServices eSServices = new ESServices();
        // GET: Home
        public ActionResult Index(string searchString)
        {
            ViewBag.SearchContent = searchString;

            if (searchString == null)
            {
               
                ElasticClient client = eSServices.Connect_ES();
                List<SearchResult> searchReault = eSServices.GetAllResult(client, searchString);
                return View(searchReault);
            }
            else
            {

                ElasticClient client = eSServices.Connect_ES();
                List<SearchResult> searchReault = eSServices.GetSearchResult(client, searchString);
                //oracleService.CloseConn(conn);
                return View(searchReault);
            }
           
        }

        public ActionResult Create()
        {
            string otherTemplate = System.IO.File.ReadAllText(System.AppDomain.CurrentDomain.BaseDirectory + "config\\OtherTemplate.txt");           
            ViewBag.OtherTemplate = otherTemplate;
            return View();
        }
        [HttpPost]
        public ActionResult Create(InputFields inputFields)
        {
            
            ElasticClient client = eSServices.Connect_ES();
            eSServices.PutSingleDoc(inputFields,client);
            return RedirectToAction("Index");
        }

        public ActionResult Edit(string id)
        {
            InputFields ThisPoint = new InputFields();
            
            ElasticClient client = eSServices.Connect_ES();
            ThisPoint= eSServices.GetSpecificPoint(client,id);
            return View(ThisPoint);
        }
        [HttpPost]
        public ActionResult Edit(InputFields thisPoint)
        {
            InputFields ThisPoint = thisPoint;
            
            ElasticClient client = eSServices.Connect_ES();
            eSServices.PutSingleDoc(thisPoint, client);
            return RedirectToAction("Index");
        }

        public ActionResult Details(string id)
        {
            InputFields ThisPoint = new InputFields();
           
            ElasticClient client = eSServices.Connect_ES();
            ThisPoint = eSServices.GetSpecificPoint(client, id);
            return View(ThisPoint);
        }

        public ActionResult Delete(string id)
        {
            InputFields ThisPoint = new InputFields();
            
            ElasticClient client = eSServices.Connect_ES();
            ThisPoint = eSServices.GetSpecificPoint(client, id);
            return View(ThisPoint);
        }
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(string id)
        {
            
            ElasticClient client = eSServices.Connect_ES();
            eSServices.DeleteSpecificPoint(client,id);
            return RedirectToAction("Index");
        }

        public ActionResult Analyze(string analyzeString)
        {
            ViewBag.AnalyzeContent = analyzeString;
            if (analyzeString == null)
            {
                return View();
            }
            else
            {
                
                ElasticClient client = eSServices.Connect_ES();
                List<AnalyzeResult> analyzeReault =eSServices.GetAnalyzeResults(client, analyzeString);
                //oracleService.CloseConn(conn);
                return View(analyzeReault);
            }

        }
    }
}