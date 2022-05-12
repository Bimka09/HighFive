using Darnton.Blazor.Leaflet.LeafletMap;
using Darnton.Blazor.DeviceInterop.Geolocation;
using BlazorDeviceTestRig.Geolocation;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Xml;
using HighFive.Data;
using Newtonsoft.Json;
using Fluxor;

//https://api.2gis.ru/doc/maps/ru/quickstart/
//https://geocode-maps.yandex.ru/1.x/?apikey=ab4445ef-d950-4bfe-85a3-c44aa3c6ecdc&geocode=37.597576,55.771899
//https://yandex.ru/dev/maps/geosearch/doc/concepts/request.html
//https://search-maps.yandex.ru/v1/?text=%D0%A0%D0%B0%D0%B9,%D0%A0%D0%BE%D1%81%D1%81%D0%B8%D1%8F&type=geo&lang=ru_RU&apikey=7b8614d5-54f5-44d6-ad37-032404b0c2d6
//https://search-maps.yandex.ru/v1/?text=%D0%9F%D1%80%D0%B0%D0%B2%D0%B8%D1%82%D0%B5%D0%BB%D1%8C%D1%81%D1%82%D0%B2%D0%BE,%D0%9A%D1%80%D0%B0%D1%81%D0%BD%D0%BE%D0%BF%D1%80%D0%B5%D1%81%D0%BD%D0%B5%D0%BD%D1%81%D0%BA%D0%B0%D1%8F%20%D0%BD%D0%B0%D0%B1%D0%B5%D1%80%D0%B5%D0%B6%D0%BD%D0%B0%D1%8F,%202&type=biz&lang=ru_RU&apikey=7b8614d5-54f5-44d6-ad37-032404b0c2d6
//  biz!!

namespace HighFive.Pages
{
    [FeatureState]
    public class GeolocationBase : ComponentBase, IDisposable
    {
        private readonly string _apiOrgSearch = "7b8614d5-54f5-44d6-ad37-032404b0c2d6";
        private readonly string _apiGeoCode = "ab4445ef-d950-4bfe-85a3-c44aa3c6ecdc";
        [Inject] public IGeolocationService GeolocationService { get; set; }

        protected Map PositionMap;
        protected TileLayer PositionTileLayer;
        //Marker CurrentPositionMarker;

        protected Map WatchMap;
        protected TileLayer WatchTileLayer;
        protected Polyline WatchPath;
        protected List<Marker> WatchMarkers;

        protected GeolocationResult CurrentPositionResult { get; set; }
        protected string CurrentLatitude => CurrentPositionResult?.Position?.Coords?.Latitude.ToString().Replace(",","."); //ToString("F2")
        protected string CurrentLongitude => CurrentPositionResult?.Position?.Coords?.Longitude.ToString().Replace(",",".");
        protected bool ShowCurrentPositionError => CurrentPositionResult?.Error != null;
        private bool isWatching => WatchHandlerId.HasValue;
        protected long? WatchHandlerId { get; set; }
        protected GeolocationResult LastWatchPositionResult { get; set; }
        protected string LastWatchLatitude => LastWatchPositionResult?.Position?.Coords?.Latitude.ToString("F2");
        protected string LastWatchLongitude => LastWatchPositionResult?.Position?.Coords?.Longitude.ToString("F2");
        protected string LastWatchTimestamp => LastWatchPositionResult?.Position?.DateTimeOffset.ToString();
        protected string ToggleWatchCommand => isWatching ? "Stop watching" : "Start watching";
        public List<OrganizationInfo> foundOrganizations { get; set; }
        public string adressToSearch { get; set; }
        public string currentAdress { get; set; }


        public GeolocationBase() : base()
        {
            foundOrganizations = new List<OrganizationInfo>();
            PositionMap = new Map("geolocationPointMap", new MapOptions //Centred on New Zealand
            {
                Center = new LatLng(-42, 175),
                Zoom = 4
            });
            PositionTileLayer = new TileLayer(
                "https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png",
                new TileLayerOptions
                {
                    Attribution = @"Map data &copy; <a href=""https://www.openstreetmap.org/"">OpenStreetMap</a> contributors, " +
                        @"<a href=""https://creativecommons.org/licenses/by-sa/2.0/"">CC-BY-SA</a>"
                }
            );
            WatchMap = new Map("geolocationWatchMap", new MapOptions //Centred on New Zealand
            {
                Center = new LatLng(-42, 175),
                Zoom = 4
            });
            WatchTileLayer = new TileLayer(
                "https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png",
                new TileLayerOptions
                {
                    Attribution = @"Map data &copy; <a href=""https://www.openstreetmap.org/"">OpenStreetMap</a> contributors, " +
                        @"<a href=""https://creativecommons.org/licenses/by-sa/2.0/"">CC-BY-SA</a>"
                }
            );
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
            var url = @"https://search-maps.yandex.ru/v1/?text=" + CurrentLongitude + "," + CurrentLatitude + "&type=biz&lang=ru_RU&apikey=" + _apiOrgSearch;
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
        private void GetAdress()
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
            /*if (CurrentPositionMarker != null)
            {
                await CurrentPositionMarker.Remove();
            }*/
            CurrentPositionResult = await GeolocationService.GetCurrentPosition();
            GetCurrentPlaceOrganizations();
            GetAdress();
            /*if (CurrentPositionResult.IsSuccess)
            {
                CurrentPositionMarker = new Marker(
                        CurrentPositionResult.Position.ToLeafletLatLng(), null
                    );
                await CurrentPositionMarker.AddTo(PositionMap);
            }*/
            StateHasChanged();
        }
        public async void TogglePositionWatch()
        {
            if (isWatching)
            {
                await StopWatching();
                WatchHandlerId = null;
                foreach (var marker in WatchMarkers)
                {
                    await marker.Remove();
                }
                WatchMarkers.Clear();
                await WatchPath.Remove();
                WatchPath = null;
            }
            else
            {
                GeolocationService.WatchPositionReceived += HandleWatchPositionReceived;
                WatchHandlerId = await GeolocationService.WatchPosition();
            }
            StateHasChanged();
        }
        private async Task StopWatching()
        {
            GeolocationService.WatchPositionReceived -= HandleWatchPositionReceived;
            await GeolocationService.ClearWatch(WatchHandlerId.Value);
        }
        private async void HandleWatchPositionReceived(object sender, GeolocationEventArgs e)
        {
            LastWatchPositionResult = e.GeolocationResult;
            if (LastWatchPositionResult.IsSuccess)
            {
                var latlng = LastWatchPositionResult.Position.ToLeafletLatLng();
                var marker = new Darnton.Blazor.Leaflet.LeafletMap.Marker(latlng, null);
                if (WatchPath is null)
                {
                    WatchMarkers = new List<Darnton.Blazor.Leaflet.LeafletMap.Marker> { marker };
                    WatchPath = new Polyline(WatchMarkers.Select(m => m.LatLng), new PolylineOptions());
                    await WatchPath.AddTo(WatchMap);
                }
                else
                {
                    WatchMarkers.Add(marker);
                    await WatchPath.AddLatLng(latlng);
                }
                await marker.AddTo(WatchMap);
            }
            StateHasChanged();
        }

        public async void Dispose()
        {
            if (isWatching)
            {
                await StopWatching();
            }
        }
    }
}
