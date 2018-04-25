using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace GISApp.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class FeatureInfoPage : Page
    {
        private FeatureInfoList featureInfos = new FeatureInfoList();

        public FeatureInfoPage()
        {
            this.InitializeComponent();
            this.DataContext = this;
        }

        public FeatureInfoList FeatureInfos { get { return featureInfos; } set { featureInfos = value; } }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.FeatureInfos.Clear();
            if (e.Parameter is FeatureInfoList)
            {
                this.FeatureInfos = e.Parameter as FeatureInfoList;
                ListFeatureInfos.ItemsSource = this.FeatureInfos;
            }
            base.OnNavigatedTo(e);
        }

        public class FeatureInfoItem
        {
            private string field;
            private string value;

            public FeatureInfoItem(string field, string value)
            {
                this.field = field;
                this.value = value;
            }

            public string Field { get { return field; } set { field = value; } }
            public string Value { get { return value; } set { this.value = value; } }
        }

        public class FeatureInfoList : List<FeatureInfoItem> { }
    }
}
