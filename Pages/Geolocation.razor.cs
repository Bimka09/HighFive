using Darnton.Blazor.DeviceInterop.Geolocation;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Xml;
using HighFive.Data;
using Newtonsoft.Json;
using Fluxor;


namespace HighFive.Pages
{
    [FeatureState]
    public class GeolocationBase : ComponentBase
    {
        private readonly string _apiOrgSearch = "7b8614d5-54f5-44d6-ad37-032404b0c2d6";
        private readonly string _apiGeoCode = "ab4445ef-d950-4bfe-85a3-c44aa3c6ecdc";
        [Inject] public IGeolocationService GeolocationService { get; set; }

        protected GeolocationResult CurrentPositionResult { get; set; }
        protected string CurrentLatitude => CurrentPositionResult?.Position?.Coords?.Latitude.ToString().Replace(",","."); //ToString("F2")
        protected string CurrentLongitude => CurrentPositionResult?.Position?.Coords?.Longitude.ToString().Replace(",",".");
        protected bool ShowCurrentPositionError => CurrentPositionResult?.Error != null;
        public List<OrganizationInfo> foundOrganizations { get; set; }
        public string adressToSearch { get; set; }
        public string currentAdress { get; set; }


        public GeolocationBase() : base()
        {
            foundOrganizations = new List<OrganizationInfo>();
        }
        public async void GetOrganizations()
        {
            foundOrganizations.Clear();
            var url = @"https://search-maps.yandex.ru/v1/?text=" + adressToSearch + "&type=biz&lang=ru_RU&apikey=" + _apiOrgSearch;
            using var client = new HttpClient();
            var result = await client.GetStringAsync(url);

            JObject obj = JObject.Parse(result);
            var features = obj["features"].Children();
            foreach (var feature in features)
            {
                OrganizationInfo organization = JsonConvert.DeserializeObject<OrganizationInfo>(feature["properties"]["CompanyMetaData"].ToString());
                foundOrganizations.Add(new OrganizationInfo
                {
                    address = organization.address,
                    name = organization.name,
                    url = organization.url
                });
                Console.WriteLine(organization.address);
                StateHasChanged();
            }
        }
        private async void GetCurrentPlaceOrganizations()
        {
            foundOrganizations.Clear();
            var url = @"https://search-maps.yandex.ru/v1/?text="+ currentAdress +  "&type=biz&lang=ru_RU&apikey=" + _apiOrgSearch; //CurrentLongitude + "," + CurrentLatitude +
            using var client = new HttpClient();
            var result = await client.GetStringAsync(url);

            JObject obj = JObject.Parse(result);
            var features = obj["features"].Children();
            foreach (var feature in features)
            {
                OrganizationInfo organization = JsonConvert.DeserializeObject<OrganizationInfo>(feature["properties"]["CompanyMetaData"].ToString());
                foundOrganizations.Add(new OrganizationInfo
                {
                    address = organization.address,
                    name = organization.name,
                    url = organization.url
                });
                Console.WriteLine(organization.address);
                StateHasChanged();
            }
        }
        private async Task GetAdress()
        {
            var palceInfo = new PlaceInfo();
            string url = @"https://geocode-maps.yandex.ru/1.x/?apikey=" + _apiGeoCode + "&geocode=" + CurrentLongitude + "," + CurrentLatitude;
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load(url);
            XmlElement? xRoot = xDoc.DocumentElement;
            if (xRoot != null)
            {
                currentAdress = xRoot.ChildNodes[0].ChildNodes[1].ChildNodes[0].ChildNodes[0].ChildNodes[0].ChildNodes[1].InnerText;
                var adressInfo = xRoot.ChildNodes[0].ChildNodes[1].ChildNodes[0].ChildNodes[0].ChildNodes[0].ChildNodes[3];
                foreach(XmlNode childnode in adressInfo.ChildNodes)
                {
                    palceInfo = AnalyzeXMLPlaceInfo(childnode, palceInfo);
                }
            }
        }
        private PlaceInfo AnalyzeXMLPlaceInfo(XmlNode childnode, PlaceInfo placeInfo)
        {
            if (childnode.Name == "country_code")
            {
                placeInfo.country_code = childnode.InnerText;
            }
            else if (childnode.Name == "postal_code")
            {
                placeInfo.postal_code = childnode.InnerText;
            }
            else if (childnode.Name == "Component")
            {
                if (childnode.ChildNodes[0].InnerText == "country")
                {
                    placeInfo.country = childnode.ChildNodes[1].InnerText;
                }
                else if (childnode.ChildNodes[0].InnerText == "province")
                {
                    placeInfo.province = childnode.ChildNodes[1].InnerText;
                }
                else if (childnode.ChildNodes[0].InnerText == "locality")
                {
                    placeInfo.locality = childnode.ChildNodes[1].InnerText;
                }
                else if (childnode.ChildNodes[0].InnerText == "street")
                {
                    placeInfo.street = childnode.ChildNodes[1].InnerText;
                }
                else if (childnode.ChildNodes[0].InnerText == "house")
                {
                    placeInfo.house = childnode.ChildNodes[1].InnerText;
                }
            }
            return placeInfo;
        }
        public async void ShowCurrentPosition()
        {

            CurrentPositionResult = await GeolocationService.GetCurrentPosition();
            await GetAdress();
            GetCurrentPlaceOrganizations();
            
            StateHasChanged();
        }

    }
}
