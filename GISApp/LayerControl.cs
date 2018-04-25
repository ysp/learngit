using Esri.ArcGISRuntime.Mapping;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GISApp
{
    public class LayerControl : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string layerName;
        private double opacity;
        private bool isVisible;
        //可以同时控制多个图层
        private List<Layer> layer = new List<Layer>();

        public LayerControl(Layer ilayer)
        {
            layer.Add(ilayer);
            LayerName = ilayer.Name;
            Opacity = ilayer.Opacity;
            IsVisible = ilayer.IsVisible;
        }

        public LayerControl(List<Layer> layers, string layerName, double opacity, bool isVisible)
        {
            layer = layers;
            LayerName = layerName;
            Opacity = opacity;
            IsVisible = isVisible;
        }

        public string LayerName
        {
            get { return layerName; }
            set
            {
                layerName = value;
                OnPropertyChanged("LayerName");
            }
        }

        public double Opacity
        {
            get { return opacity; }
            set
            {
                opacity = value;
                foreach (Layer lyr in layer)
                {
                    lyr.Opacity = opacity;
                }
                if (opacity < 0.02)
                {
                    IsVisible = false;
                }
                else
                {
                    IsVisible = true;
                }
                OnPropertyChanged("Opacity");
            }
        }

        public bool IsVisible
        {
            get { return isVisible; }
            set
            {
                isVisible = value;
                foreach (Layer lyr in layer)
                {
                    lyr.IsVisible = isVisible;
                }
                OnPropertyChanged("IsVisible");
            }
        }

        protected void OnPropertyChanged(string info)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(info));
            }
        }
    }

    public class LayerControlCollection : ObservableCollection<LayerControl>
    {
        public LayerControlCollection() : base()
        {
            //Layer osmLayer = new OpenStreetMapLayer();
            //osmLayer.Name = "Open Street Map";
            //Add(new LayerControl(osmLayer));
            //Add(new LayerControl(osmLayer));
            //Add(new LayerControl(osmLayer));
            //Add(new LayerControl(osmLayer));
            //Add(new LayerControl(osmLayer));
            //Add(new LayerControl(osmLayer));
        }
    }
}
