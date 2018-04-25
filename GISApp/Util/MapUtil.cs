using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using GISApp.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GISApp
{
    class MapUtil
    {
        public static void ZoomToFullMap(MapView mapView)
        {
            Envelope envelope = getMapExtent(mapView);
            if (envelope != null)
            {
                mapView.SetViewpoint(new Viewpoint(envelope));
            }
        }

        public static Envelope getMapExtent(MapView mapView)
        {
            Envelope envelope = null;
            try
            {
                //从数据库中找配置信息
                SysConfig config = SQLiteDbHelper.FindByPrimaryKey<SysConfig>("MAP_EXTENT");
                string[] strs = config.Value.Split(new char[] { ',', ';' });
                if (strs.Length == 4)
                {
                    double xmin = Convert.ToDouble(strs[0]);
                    double ymin = Convert.ToDouble(strs[1]);
                    double xmax = Convert.ToDouble(strs[2]);
                    double ymax = Convert.ToDouble(strs[3]);
                    envelope = new Envelope(xmin, ymin, xmax, ymax, mapView.SpatialReference);
                    return envelope;
                }
            }
            catch (Exception) { }

            //从图层中找到一个全图范围定位
            LayerCollection layers = mapView.Map.Basemap.BaseLayers;
            foreach (Layer layer in layers)
            {
                if (layer.FullExtent != null)
                {
                    envelope = layer.FullExtent;
                    return envelope;
                }
            }
            return envelope;
        }

    }
}
