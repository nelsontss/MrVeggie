﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using BingMapsRESTToolkit;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;


namespace MrVeggie.Controllers {

    [Route("[controller]/[action]")]
    public class SupermercadosViewController : Controller {

        [HttpPost]
        public IActionResult ShowLocalizacao([FromBody] string[] user_loc) {
            TempData["Loc"] = JsonConvert.SerializeObject(user_loc);
            return Content(Url.Action("ShowMapa", "SupermercadosView"));
        }

        [HttpGet]
        public async Task<ActionResult> ShowMapa() {
            string[] user_loc = null;
            var value = TempData["Loc"];
            if (value is string json) {
                user_loc = JsonConvert.DeserializeObject<string[]>(json);
            }
            Console.WriteLine("****User Loc: " + user_loc[0] + " , " + user_loc[1]);

            return View(await GetMarkets(user_loc[0], user_loc[1]));
        }


        public async Task<List<MarketLocation>> GetMarkets(string user_lat, string user_long) {
            double Radius = 10; // km  
            int maxResults = 5;
            string bingMapsKey = "Alvpuc-Z8ROrtuOcQZdVD1iaINzybihaHRnSHYWL8jwdEjVrXRj843L8ayxchoj7";
            string requestUrl = string.Format(CultureInfo.InvariantCulture, "http://spatial.virtualearth.net/REST/v1/data/c2ae584bbccc4916a0acf75d1e6947b4/NavteqEU/NavteqPOIs" +
                "?spatialFilter=nearby({0},{1},{2})&$filter=EntityTypeID Eq 5400&$top={3}&key={4}", user_lat, user_long, Radius, maxResults, bingMapsKey);

            HttpWebRequest request = WebRequest.Create(requestUrl) as HttpWebRequest;
            HttpWebResponse response = request.GetResponse() as HttpWebResponse;

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(response.GetResponseStream());

            return await ExtractInfoFromXML(xmlDoc);
        }

        private async Task<List<MarketLocation>> ExtractInfoFromXML(XmlDocument response) {
            List<MarketLocation> results = new List<MarketLocation>();
            XmlNodeList entryElements = response.GetElementsByTagName("entry");

            for (int i = 0; i <= entryElements.Count - 1; i++) {
                XmlElement element = (XmlElement)entryElements[i];
                XmlElement contentElement = (XmlElement)element.GetElementsByTagName("content")[0];
                XmlElement propElement = (XmlElement)contentElement.GetElementsByTagName("m:properties")[0];
                XmlNode nameElement = propElement.GetElementsByTagName("d:Name")[0];
                if (nameElement == null) throw new Exception("Name not found");

                XmlNode latElement = propElement.GetElementsByTagName("d:Latitude")[0];
                if (latElement == null) throw new Exception("Latitude not found");

                XmlNode longElement = propElement.GetElementsByTagName("d:Longitude")[0];
                if (longElement == null) throw new Exception("Longitude not found");

                string name = JsonConvert.SerializeObject(nameElement.InnerText);
                string latitude = latElement.InnerText;
                string longitude = longElement.InnerText;

                double lat; Double.TryParse(latitude, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out lat);
                double longi; Double.TryParse(longitude, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out longi);
                string address = await getAddress(new Coordinate(lat, longi));

                results.Add(new MarketLocation(name, latitude, longitude, address));
            }

            return results;
        }

        public async Task<String> getAddress(Coordinate point) {
            //Create a request.
            var request = new ReverseGeocodeRequest() {
                Point = point,
                IncludeIso2 = true,
                IncludeNeighborhood = true,
                BingMapsKey = "Alvpuc-Z8ROrtuOcQZdVD1iaINzybihaHRnSHYWL8jwdEjVrXRj843L8ayxchoj7"
            };

            //Process the request by using the ServiceManager.
            var response = await ServiceManager.GetResponseAsync(request);

            if (response != null &&
                response.ResourceSets != null &&
                response.ResourceSets.Length > 0 &&
                response.ResourceSets[0].Resources != null &&
                response.ResourceSets[0].Resources.Length > 0) {

                var result = response.ResourceSets[0].Resources[0] as BingMapsRESTToolkit.Location;
                return result.Address.FormattedAddress;
            }

            return null;
        }

        public class MarketLocation {
            public string latitude;
            public string longitude;
            public string nome;
            public string address;

            public MarketLocation(string nome, string latitude, string longitude, string address) {
                this.nome = nome;
                this.latitude = latitude;
                this.longitude = longitude;
                this.address = address;
            }
        }
    }

}
