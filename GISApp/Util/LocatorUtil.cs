using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.UI.Xaml;

namespace GISApp
{
    class LocatorUtil
    {
        private GraphicsOverlay _graphicsOverlay;
        private Graphic _graphic;

        public LocatorUtil(MapView mapView)
        {
            MapView = mapView;
            _graphicsOverlay = new GraphicsOverlay();
            MapView.GraphicsOverlays.Add(_graphicsOverlay);
        }

        public MapView MapView { get; set; }

        public async void locate(Geopoint position)
        {
            MapPoint mapLocation = new MapPoint(position.Position.Longitude, position.Position.Latitude, SpatialReferences.Wgs84);
            //测试缙云定位功能
            //MapPoint mapLocation = new MapPoint(120.17, 28.66, SpatialReferences.Wgs84);
            Geometry projLocation = GeometryEngine.Project(mapLocation, MapView.SpatialReference);

            _graphic = GraphicForPoint(projLocation as MapPoint);
            _graphicsOverlay.Graphics.Add(_graphic);

            //定位到点
            double scale = MapView.GetCurrentViewpoint(ViewpointType.CenterAndScale).TargetScale;
            await MapView.SetViewpointAsync(new Viewpoint(projLocation as MapPoint, scale));

            //3s后删除定位点
            DispatcherTimer time = new DispatcherTimer();
            time.Tick += (sender, e) =>
            {
                _graphicsOverlay.Graphics.Clear();
                time.Stop();
            };
            time.Interval = TimeSpan.FromMilliseconds(3000);
            time.Start();
        }

        private Graphic GraphicForPoint(MapPoint point)
        {
            var symbolUri = new Uri(StorageManager.GetDataPath("gps.png"));
            PictureMarkerSymbol pinSymbol = new PictureMarkerSymbol(symbolUri);
            return new Graphic(point, pinSymbol);
        }
    }
}
