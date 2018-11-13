using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Nest;
using GeoNameAddress.Models;
namespace GeoNameAddress.Services
{
    public class ESServices
    {
        public ElasticClient Connect_ES()
        {
            var settings = new ConnectionSettings(new Uri("http://localhost:9200"))
               .DefaultMappingFor<InputFields>(i => i
               .IndexName("myindex")
               .TypeName("mytype")
                );           
            var client = new ElasticClient(settings);
            return client;
        }
        public void CreateIndex(ElasticClient client)
        {
            //先删除原来的
            try
            {
                DeleteIndexRequest deleteIndexRequest = new DeleteIndexRequest("myindex");
                client.DeleteIndex(deleteIndexRequest);
            }
            catch { }
            //新建
            var descriptor = new CreateIndexDescriptor("myindex")
                .Settings(s => s.NumberOfShards(5).NumberOfReplicas(1))
                .Mappings(ms => ms
                     .Map<InputFields>(m => m.AutoMap())
                );
            client.CreateIndex(descriptor);
        }

        public void PutDoc(List<InputFields> doc,ElasticClient client)
        {
            client.IndexMany<InputFields>(doc);
        }

        public void PutSingleDoc(InputFields doc, ElasticClient client)
        {
            client.IndexDocument(doc);
        }


        public List<SearchResult> GetAllResult(ElasticClient client, string searchStr)
        {
            var searchResponse = client.Search<InputFields>(s => s
                .Query(q => q
                    .MatchAll()
                )
                 .Size(10000)
            );
            int a = searchResponse.Documents.Count();
            List<SearchResult> SearchResults = new List<SearchResult>();
           
            foreach (var item in searchResponse.Documents)
            {
                SearchResults.Add(new SearchResult(item.ID, item.Name, item.Addresss, item.X, item.Y,"1"));                     
            }
            return SearchResults;
        }

        public List<SearchResult> GetSearchResult(ElasticClient client, string searchStr)
        {
            //var searchResponse = client.Search<Indexmodel>(s => s
            //     .Query(q => q
            //         .Match(m => m
            //               .Field(f => f.Addresss)
            //               .Query(searchStr)
            //                 )
            //             )
            //     .Size(10000)
            // );

            //SearchDescriptor<Indexmodel> searchDescriptor = new SearchDescriptor<Indexmodel>();
            //searchDescriptor.Index("poi")
            //    //.Source(x => x.Includes(t => t.Field("name")))
            //    .Query(x => x.Match(t => t.Field("address").Query(searchStr)))
            //    .Size(10000);
            //var searchResponse = client.Search<Indexmodel>(searchDescriptor);
            //.StoredFields(sf => sf
            //    .Fields(
            //        "address"
            //        )
            //   )
            var searchResponse = client.Search<InputFields>(s => s
                 .Query(q => q
                     .Match(m => m
                           .Field("address")
                           .Query(searchStr)
                             )
                         )
                 .Size(10000)
             );
            int a = searchResponse.Documents.Count();
            List<SearchResult> SearchResults = new List<SearchResult>();
            List<string> scores = new List<string>();
            foreach (var item in searchResponse.Hits)
            {

                scores.Add(item.Score.ToString());

            }
            int i = 0;
            foreach (var item in searchResponse.Documents)
            {
                SearchResults.Add(new SearchResult(item.ID,item.Name,item.Addresss,item.X,item.Y,scores[i]));
                //SearchResults.Add(new SearchResult(GetModelValue("Addresss", item.Source), "b", "c", scores[i]));
                i++;
            }
            return SearchResults;
        }
        public InputFields GetSpecificPoint(ElasticClient client, string id)
        {
            InputFields SpecificPoint=new InputFields();
            var searchResponse = client.Search<InputFields>(s => s
                 .Query(q => q
                     .Match(m => m
                           .Field("_id")
                           .Query(id)
                             )
                         )
             );
            SpecificPoint = searchResponse.Documents.FirstOrDefault();
            return SpecificPoint;
        }

        public void DeleteSpecificPoint(ElasticClient client, string id)
        {
            client.Delete<InputFields>(
                id,
                i => i.Index("myindex").Type("mytype")
                );
        }

        //
        public string[] GetAllPoints(ElasticClient client, Models.FieldMapping mapping,string otherTypes)
        {            
            var searchResponse = client.Search<InputFields>(s => s
                .Query(q => q
                    .MatchAll()
                )
                 .Size(10000)
            );
            int a = searchResponse.Documents.Count();
            string[] points = new string[a];         
            List<SearchResult> SearchResults = new List<SearchResult>();
            int i = 0;
            foreach (var item in searchResponse.Documents)
            {
                string point = "{\"geometry\":"+ "\"" + item.X + "&" + item.Y + "\"";                   
                point += ",\"attr\":{\""+mapping.ID+"\":\""+item.ID+"\",\""+mapping.Name+"\":\""+item.Name + "\",\"" +mapping.Address+ "\":\"" + item.Addresss + "\",\""+mapping.Xcoordinate+"\":\""+item.X+"\",\""+mapping.Ycoordinate+"\":\""+item.Y+"\",";
                string wkt = item.Other;
                int index1 = wkt.IndexOf("{");
                int index2 = wkt.IndexOf("}");
                point+=wkt.Substring(index1 + 1, index2 - index1 - 1).Trim() + "},\"attrtype\":{\"" + mapping.ID + "\":\"text\",\"" + mapping.Name + "\":\"text\",\"" + mapping.Address + "\":\"text\",\"" + mapping.Xcoordinate + "\":\"text\",\"" + mapping.Ycoordinate + "\":\"text\",";
                point += otherTypes+"}}";
                points[i] = point.Replace(" ",""); ;
                i++;
            }
            return points;
        }
    }
}