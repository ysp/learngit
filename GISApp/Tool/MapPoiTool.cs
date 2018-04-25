using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using GISApp.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace GISApp
{
    class MapPoiTool : IUserMapTool
    {
        private GraphicsOverlay _graphicsOverlay;
        private Graphic _graphic;

        //可以点击兴趣点，显示兴趣点名称，并且是否收藏
        public MapPoiTool(MapPoi poi, MapView mapView)
        {
            MapView = mapView;
            MyPoi = poi;

            _graphicsOverlay = new GraphicsOverlay();
            MapView.GraphicsOverlays.Add(_graphicsOverlay);
        }

        public MapView MapView { get; set; }
        public MapPoi MyPoi { get; set; }

        public string GetToolName()
        {
            return "兴趣点书签";
        }
        public async void InitTool()
        {
            //添加点
            MapPoint mapLocation = new MapPoint(MyPoi.X, MyPoi.Y, MapView.SpatialReference);
            _graphic = GraphicForPoint(mapLocation);
            _graphicsOverlay.Graphics.Add(_graphic);

            //定位到点
            await MapView.SetViewpointAsync(new Viewpoint(mapLocation, 2000));

            //CalloutDefinition myCalloutDefinition = new CalloutDefinition("兴趣点：", MyPoi.ToString());
            //myCalloutDefinition.LeaderOffsetY = 16d;
            //MapView.ShowCalloutAt(mapLocation, myCalloutDefinition);      

            //添加事件
            MapView.GeoViewTapped += MyMapView_GeoViewTapped;
        }

        private async void MyMapView_GeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            MapView.DismissCallout();

            var tolerance = 10d;
            var maximumResults = 1;
            var onlyReturnPopups = false;

            IdentifyGraphicsOverlayResult identifyResults = await MapView.IdentifyGraphicsOverlayAsync(
                 _graphicsOverlay, e.Position, tolerance, onlyReturnPopups, maximumResults);

            if (identifyResults.Graphics.Count > 0)
            {
                Graphic graphic = identifyResults.Graphics.First();
                if(graphic == _graphic)
                {
                    StackPanel stackPanel = new StackPanel();
                    stackPanel.MinWidth = 120;
                    TextBlock textBlock = new TextBlock();
                    textBlock.Text = MyPoi.ToString();
                    HyperlinkButton hyperlink = new HyperlinkButton();
                    hyperlink.FontSize = 12;
                    if (MyPoi.Tag == "1")
                    {
                        //已经存在书签中
                        hyperlink.Content = "从书签删除";
                        hyperlink.Click += async (object sender2, RoutedEventArgs e2) =>
                        {
                            if (MyPoi.Tag == "0")
                                return;
                            MyPoi.Tag = "0";
                            int count = SQLiteDbHelper.Update(MyPoi);
                            if (count > 0)
                            {
                                MessageDialog dialog = new MessageDialog("书签删除成功");
                                await dialog.ShowAsync();
                                hyperlink.IsEnabled = false;
                            }
                        };
                    }
                    else
                    {
                        //还不在书签中
                        hyperlink.Content = "添加到书签";
                        hyperlink.Click += async (object sender2, RoutedEventArgs e2) =>
                        {
                            if (MyPoi.Tag == "1")
                                return;
                            MyPoi.Tag = "1";
                            int count = SQLiteDbHelper.Update(MyPoi);
                            if (count > 0)
                            {
                                MessageDialog dialog = new MessageDialog("书签添加成功");
                                await dialog.ShowAsync();
                                hyperlink.IsEnabled = false;
                            }
                        };
                    }

                    stackPanel.Children.Add(textBlock);
                    stackPanel.Children.Add(hyperlink);
                    MapPoint mapLocation = new MapPoint(MyPoi.X, MyPoi.Y, MapView.SpatialReference);
                    MapView.ShowCalloutAt(mapLocation, stackPanel, new Windows.Foundation.Point(0, 16));
                }
            }
        }

        private Graphic GraphicForPoint(MapPoint point)
        {
            var symbolUri = new Uri(StorageManager.GetDataPath("location.png"));
            PictureMarkerSymbol pinSymbol = new PictureMarkerSymbol(symbolUri);
            return new Graphic(point, pinSymbol);
        }

        public void DestroyTool()
        {
            MapView.GeoViewTapped -= MyMapView_GeoViewTapped;
            MapView.DismissCallout();
            _graphicsOverlay.Graphics.Clear();
        }
    }
}
