using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace GISApp
{
    public class MapMesureTool : IUserMapTool
    {
        private GraphicsOverlay _sketchOverlay;

        public MapMesureTool(MesureType mesureType, MapView mapView)
        {
            MapView = mapView;
            MesureType = mesureType;

            _sketchOverlay = new GraphicsOverlay();
            MapView.GraphicsOverlays.Add(_sketchOverlay);
        }

        public MapView MapView { get; set; }

        public MesureType MesureType { get; set; }

        public string GetToolName()
        {
            if (MesureType == MesureType.MesureLength)
                return "长度测量";
            else
                return "面积测量";
        }

        public void InitTool()
        {
            InitMesureTool();
        }

        public async void InitMesureTool()
        {
            SketchCreationMode creationMode = SketchCreationMode.Polyline;
            if (MesureType == MesureType.MesureLength)
                creationMode = SketchCreationMode.Polyline;
            else
                creationMode = SketchCreationMode.Polygon;

            try
            {
                MapView.SketchEditor.GeometryChanged += OnGeometryChanged;
                Geometry geometry = await MapView.SketchEditor.StartAsync(creationMode, false);
                Graphic graphic = CreateGraphic(creationMode, geometry);
                _sketchOverlay.Graphics.Add(graphic);
            }
            catch (TaskCanceledException)
            {
                //绘制操作被中断，会捕获异常
            }
        }

        public Graphic CreateGraphic(SketchCreationMode creationMode, Geometry geometry)
        {
            Symbol symbol = null;
            switch (creationMode)
            {
                case SketchCreationMode.Polyline:
                    {
                        symbol = new SimpleLineSymbol()
                        {
                            Color = Color.FromArgb(250, 255, 0, 0),
                            Style = SimpleLineSymbolStyle.Solid,
                            Width = 2.0
                        };
                        break;
                    }
                case SketchCreationMode.Polygon:
                    {
                        symbol = new SimpleFillSymbol()
                        {
                            Color = Color.FromArgb(100, 255, 0, 0),
                            Style = SimpleFillSymbolStyle.Solid,
                            Outline = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Colors.Red, 1.0)
                        };
                        break;
                    }
            }
            return new Graphic(geometry, symbol);
        }

        public void OnGeometryChanged(object sender, GeometryChangedEventArgs e)
        {
            Geometry geometry = e.NewGeometry;
            MapPoint lastMapPoint = null;
            CalloutDefinition myCalloutDefinition = null;
            MapView.DismissCallout();

            if (geometry.GeometryType == GeometryType.Polyline)
            {
                Polyline polyline = geometry as Polyline;
                ReadOnlyPart part = polyline.Parts.Last();
                if (part.Points.Count < 2)
                {
                    return;
                }
                lastMapPoint = part.Points.Last();

                double geolen = GeometryEngine.Length(polyline);
                geolen = Math.Abs(geolen);
                string mapDescription = GetLengthDescription(geolen);
                myCalloutDefinition = new CalloutDefinition("测量长度：", mapDescription);
            }
            else if (geometry.GeometryType == GeometryType.Polygon)
            {
                Polygon polygon = geometry as Polygon;
                ReadOnlyPart part = polygon.Parts.Last();
                if (part.Points.Count < 3)
                {
                    return;
                }
                lastMapPoint = part.Points.Last();

                double geoarea = GeometryEngine.Area(polygon);
                geoarea = Math.Abs(geoarea);
                string mapDescription = GetAreaDescription(geoarea);
                myCalloutDefinition = new CalloutDefinition("测量面积：", mapDescription);
            }

            MapView.ShowCalloutAt(lastMapPoint, myCalloutDefinition);
        }

        private string GetLengthDescription(double length)
        {
            double km = length / 1000.0; //公里
            string mapDescription = string.Format("约 {0:F3} 公里", km);
            if (length < 100)
            {
                mapDescription = string.Format("约 {0:F3} 米", length);
            }
            return mapDescription;
        }

        private string GetAreaDescription(double area)
        {
            double hectare = area / 10000.0; //公顷
            double mu = hectare * 15.0; //亩
            string mapDescription = string.Format("约{0:F3}公顷 / {1:F3}亩", hectare, mu);
            return mapDescription;
        }


        public void DestroyTool()
        {
            MapView.SketchEditor.GeometryChanged -= OnGeometryChanged;
            _sketchOverlay.Graphics.Clear();
            MapView.DismissCallout();

            // 取消任何没有完成的操作
            if (MapView.SketchEditor.CancelCommand.CanExecute(null))
            {
                MapView.SketchEditor.CancelCommand.Execute(null);
            }
        }
    }

    public enum MesureType
    {
        MesureLength = 0,
        MesureArea = 1
    }
}
