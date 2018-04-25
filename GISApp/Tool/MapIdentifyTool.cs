using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using GISApp.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Controls.Primitives;

namespace GISApp
{
    class MapIdentifyTool : IUserMapTool
    {
        private GraphicsOverlay _graphicsOverlay;

        public MapIdentifyTool(MainPage mainPage, MapView mapView, ToggleButton toggle)
        {
            MapView = mapView;
            MainPage = mainPage;
            Toggle = toggle;

            _graphicsOverlay = new GraphicsOverlay();
            MapView.GraphicsOverlays.Add(_graphicsOverlay);
        }
        public MapView MapView { get; set; }
        public MainPage MainPage { get; set; }
        public ToggleButton Toggle { get; set; }
        public string GetToolName()
        {
            return "点击查询";
        }
        public void InitTool()
        {
            MapView.GeoViewTapped += MyMapView_GeoViewTapped;
            Toggle.IsChecked = true;
        }

        private async Task<Geodatabase> GetFeatureTableToQuery()
        {
            LayerCollection baseLayers = MapView.Map.Basemap.BaseLayers;
            //逆向遍历图层
            int num = baseLayers.Count;
            for (int i = num - 1; i >= 0; --i)
            {
                Layer baseLayer = baseLayers.ElementAt(i);
                if (!baseLayer.IsVisible)
                    continue;
                string layerName = baseLayer.Name;

                string geodatabasePath = StorageManager.GetGeodatabasePath(layerName);
                if (File.Exists(geodatabasePath))
                {
                    Geodatabase localGeodatabase = await Geodatabase.OpenAsync(geodatabasePath);
                    return localGeodatabase;
                }
            }
            return null;
        }

        private Layer GetLayerToQuery()
        {
            LayerCollection operationalLayers = MapView.Map.OperationalLayers;
            //逆向遍历图层
            int num = operationalLayers.Count;
            for (int i = num - 1; i >= 0; --i)
            {
                Layer operationalLayer = operationalLayers.ElementAt(i);
                if (operationalLayer.IsVisible)
                    return operationalLayer;
            }
            return null;
        }

        private async void MyMapView_GeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            //先关闭查询结果面板
            MainPage.PanelInfoVisible = false;
            _graphicsOverlay.Graphics.Clear();

            MapPoint mapLocation = e.Location;
            Feature feature = null;

            Layer fLayer = GetLayerToQuery();
            if (fLayer != null)
            {
                IdentifyLayerResult results = await MapView.IdentifyLayerAsync(fLayer, e.Position, 5, false);
                if (results.GeoElements.Count > 0)
                {
                    feature = results.GeoElements.First() as Feature;
                }
            }

            if (feature == null)
            {
                Geodatabase localGeodatabase = await GetFeatureTableToQuery();
                if (localGeodatabase != null)
                {
                    feature = await QueryGeodatabase(localGeodatabase, mapLocation);
                }
            }

            if (feature == null)
                return;

            Graphic graphic = CreateGraphic(feature.Geometry);
            _graphicsOverlay.Graphics.Add(graphic);

            FeatureInfoPage.FeatureInfoList featureInfos = CreateFeatureInfos(feature);
            MainPage.FrameInfoRight.Navigate(typeof(FeatureInfoPage), featureInfos);
            MainPage.PanelInfoVisible = true;
        }

        private FeatureInfoPage.FeatureInfoList CreateFeatureInfos(Feature feature)
        {
            FeatureInfoPage.FeatureInfoList featureInfos = new FeatureInfoPage.FeatureInfoList();
            IReadOnlyList<Field> myFields = feature.FeatureTable.Fields;
            foreach (Field field in myFields)
            {
                string myKey = String.IsNullOrEmpty(field.Alias) ? field.Name : field.Alias;
                string myValue = feature.GetAttributeValue(field).ToString();
                featureInfos.Add(new FeatureInfoPage.FeatureInfoItem(myKey, myValue));
            }
            return featureInfos;
        }

        private async Task<Feature> QueryGeodatabase(Geodatabase localGeodatabase, MapPoint mapLocation)
        {
            List<Feature> features = new List<Feature>();
            foreach (GeodatabaseFeatureTable table in localGeodatabase.GeodatabaseFeatureTables)
            {
                QueryParameters queryParams = new QueryParameters();
                queryParams.Geometry = mapLocation;
                queryParams.MaxAllowableOffset = 0.1;
                queryParams.ReturnGeometry = true;
                FeatureQueryResult queryResult = await table.QueryFeaturesAsync(queryParams);

                features = queryResult.ToList();
                Geometry projLocation = GeometryEngine.Project(mapLocation, table.SpatialReference);
                foreach (Feature feature in features)
                {
                    //TODO: 空间查询精度不够，补充相交测试
                    if (GeometryEngine.Intersects(projLocation, feature.Geometry))
                    {
                        return feature;
                    }
                }
            }
            if (features.Any())
            {
                return features[0];
            }
            return null;
        }


        private Graphic CreateGraphic(Geometry geometry)
        {
            Symbol symbol = null;
            switch (geometry.GeometryType)
            {
                // Symbolize with a fill symbol
                case GeometryType.Envelope:
                case GeometryType.Polygon:
                    {
                        symbol = new SimpleFillSymbol()
                        {
                            Color = Color.FromArgb(128, 128, 0, 128),
                            Style = SimpleFillSymbolStyle.Solid,
                            Outline = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Colors.Red, 2.0)
                        };
                        break;
                    }
                // Symbolize with a line symbol
                case GeometryType.Polyline:
                    {
                        symbol = new SimpleLineSymbol()
                        {
                            Color = Color.FromArgb(200, 250, 0, 0),
                            Style = SimpleLineSymbolStyle.Solid,
                            Width = 2.0
                        };
                        break;
                    }
                // Symbolize with a marker symbol
                case GeometryType.Point:
                case GeometryType.Multipoint:
                    {
                        symbol = new SimpleMarkerSymbol()
                        {
                            Color = Colors.Red,
                            Style = SimpleMarkerSymbolStyle.Circle,
                            Size = 16.0
                        };
                        break;
                    }
            }

            return new Graphic(geometry, symbol);
        }

        public void DestroyTool()
        {
            Toggle.IsChecked = false;
            MapView.GeoViewTapped -= MyMapView_GeoViewTapped;
            MainPage.PanelInfoVisible = false;
            _graphicsOverlay.Graphics.Clear();
        }
    }
}
