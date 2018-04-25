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
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using Windows.UI.Popups;
using Windows.Storage;
using System.Collections.ObjectModel;
using Esri.ArcGISRuntime.Geometry;
using Windows.UI.Core;
using GISApp.Tables;
using System.Text;
using Windows.Devices.Geolocation;
using Windows.UI;

namespace GISApp
{
    /// <summary>
    /// A map page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        List<string> tpks = new List<string> { "02电子地图.tpk", "10土地利用现状图.tpk" };
        List<string> geodatabases = new List<string> { "土地利用现状图.geodatabase" };
        List<string> extraGeodatabases = new List<string>();
        private LayerControlCollection myLayerControls = new LayerControlCollection();
        private bool layerTreeVisible = false;
        private bool panelInfoVisible = false;
        private String currentMapToolName = "";
        private IUserMapTool currentMapTool = null;

        public MainPage()
        {
            string licenseKey = "runtimelite,1000,rud5298450235,none,NKMFA0PL40LKC2EN0040";
            Esri.ArcGISRuntime.ArcGISRuntimeEnvironment.SetLicense(licenseKey);

            this.InitializeComponent();
            //SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;

            this.DataContext = this;
            LayerTreeVisible = layerTreeVisible;

        }

        //绑定，控制每个图层的透明度、可见性
        public LayerControlCollection MyLayerControls
        {
            get { return myLayerControls; }
        }

        //绑定，控制图层树是否可见
        public bool LayerTreeVisible
        {
            get
            {
                return layerTreeVisible;
            }
            set
            {
                layerTreeVisible = value;
                PanelLayertree.Visibility = layerTreeVisible ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        //绑定，控制右侧信息面板是否可见
        public bool PanelInfoVisible
        {
            get
            {
                return panelInfoVisible;
            }
            set
            {
                panelInfoVisible = value;
                PanelInfoRight.Visibility = panelInfoVisible ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        //可绑定，显示当前用户操作的名称
        public String CurrentMapToolName
        {
            get { return currentMapToolName; }
            set { currentMapToolName = value; }
        }

        //返回内部的Frame，用于内嵌其他用户界面
        public Frame FrameInfoRight
        {
            get { return frameInfoRight; }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            InitMapLayers();
        }

        public async void InitMapLayers()
        {
            tpks = StorageManager.GetAllTPKNames();
            geodatabases = StorageManager.GetAllGeodatabaseNames();
            Map myMap = MapControl.Map;
            Basemap myBasemap = myMap.Basemap;
            LayerCollection myLayerCollection = myBasemap.BaseLayers;

            foreach (String tpk in tpks)
            {
                String tpkPath = StorageManager.GetDataPath(tpk);
                TileCache tileCache = new TileCache(tpkPath);
                ArcGISTiledLayer tileLayer = new ArcGISTiledLayer(tileCache);
                await tileLayer.LoadAsync();
                tileLayer.Name = Path.GetFileNameWithoutExtension(tpk);
                myLayerCollection.Add(tileLayer);

                LayerControl layerControl = new LayerControl(tileLayer);
                myLayerControls.Add(layerControl);

                //显示最先加载的两个图层
                layerControl.IsVisible = (myLayerCollection.Count < 3 ? true : false);
            }

            extraGeodatabases = StorageManager.GetExtraGeodatabaseNames();
            LayerCollection operationalLayers = myMap.OperationalLayers;
            foreach (String gdbFileName in extraGeodatabases)
            {
                String gdbPath = StorageManager.GetExtraDataPath(gdbFileName);
                string gdbName = Path.GetFileNameWithoutExtension(gdbFileName);
                Geodatabase gdb = await Geodatabase.OpenAsync(gdbPath);
                List<Layer> layers = new List<Layer>();
                foreach (GeodatabaseFeatureTable table in gdb.GeodatabaseFeatureTables)
                {
                    await table.LoadAsync();
                    //table.TableName;
                    var featureLayer = new FeatureLayer(table);
                    operationalLayers.Add(featureLayer);
                    layers.Add(featureLayer);
                }
                LayerControl layerControl = new LayerControl(layers, gdbName, 1.0, false);
                myLayerControls.Add(layerControl);
            }

            //定位到全图
            MapUtil.ZoomToFullMap(MapControl);
        }

        public void SetMapTool(IUserMapTool mapTool)
        {
            if (currentMapTool != null)
            {
                //销毁之前的地图工具
                currentMapTool.DestroyTool();
                CurrentMapToolName = "";
                currentMapTool = null;
            }
            if (mapTool != null)
            {
                currentMapTool = mapTool;
                //获取当前地图工具的名称
                CurrentMapToolName = mapTool.GetToolName();
                //初始化地图工具
                mapTool.InitTool();
            }
        }

        /// <summary>
        /// Gets the view-model that provides mapping capabilities to the view
        /// </summary>
        public MapViewModel ViewModel { get; } = new MapViewModel();

        private void BtnLayertree_Click(object sender, RoutedEventArgs e)
        {
            LayerTreeVisible = !LayerTreeVisible;
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            //清除当前地图工具
            SetMapTool(null);

        }

        private async void BtnLocate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Geopoint position = (await new Geolocator().GetGeopositionAsync()).Coordinate.Point; //获得设备位置   
                LocatorUtil locatorUtil = new LocatorUtil(MapControl);
                locatorUtil.locate(position);
            }
            catch (Exception)
            {
                MessageDialog dialog = new MessageDialog("定位出错，请打开位置服务");
                await dialog.ShowAsync();
            }
        }

        private void BtnFullmap_Click(object sender, RoutedEventArgs e)
        {
            //定位到全图
            MapUtil.ZoomToFullMap(MapControl);
        }

        private void BtnQuery_Click(object sender, RoutedEventArgs e)
        {
            MapIdentifyTool mapIdentifyTool = new MapIdentifyTool(this, MapControl, BtnQuery);
            SetMapTool(mapIdentifyTool);
        }

        private void BtnBookmark_Click(object sender, RoutedEventArgs e)
        {
            var bookmarks = new List<MapPoi>();
            string sql = "SELECT * FROM MAPPOI t WHERE t.TAG = 1 ORDER BY t.ID LIMIT 50";
            bookmarks = SQLiteDbHelper.Query<MapPoi>(sql);

            if (bookmarks.Count > 0)
            {
                BookmarkList.ItemsSource = bookmarks;
            }
            else
            {
                BookmarkList.ItemsSource = new string[] { "没有收藏书签" };
            }
        }

        private void BtnAnalyse_Click(object sender, RoutedEventArgs e)
        {

        }


        private async void ShowMessage(string strMessage)
        {
            var dialog = new MessageDialog(strMessage);

            dialog.Commands.Add(new UICommand("确定", cmd => { }, commandId: 0));
            dialog.Commands.Add(new UICommand("取消", cmd => { }, commandId: 1));

            //设置默认按钮，不设置的话默认的确认按钮是第一个按钮
            dialog.DefaultCommandIndex = 0;
            dialog.CancelCommandIndex = 1;

            //获取返回值
            var result = await dialog.ShowAsync();
        }

        private void MItemMesureLength_Click(object sender, RoutedEventArgs e)
        {
            MapMesureTool mesureLengthTool = new MapMesureTool(MesureType.MesureLength, MapControl);
            SetMapTool(mesureLengthTool);
        }

        private void MItemMesureArea_Click(object sender, RoutedEventArgs e)
        {
            MapMesureTool mesureAreaTool = new MapMesureTool(MesureType.MesureArea, MapControl);
            SetMapTool(mesureAreaTool);
        }

        private void BoxSearch_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                if (sender.Text.Trim().Length < 1)
                {
                    BoxSearch.ItemsSource = new string[] { };
                    return;
                }

                var suggestions = new List<MapPoi>();
                string sql = "SELECT * FROM MAPPOI t WHERE t.POINAME LIKE ?  LIMIT 100";
                suggestions = SQLiteDbHelper.Query<MapPoi>(sql, new object[] { $"%{sender.Text.Trim()}%" });

                if (suggestions.Count > 0)
                {
                    BoxSearch.ItemsSource = suggestions;
                }
                else
                {
                    BoxSearch.ItemsSource = new string[] { "没有查询到兴趣点" };
                }
            }
        }

        private void BoxSearch_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.ChosenSuggestion != null && args.ChosenSuggestion is MapPoi)
            {
                MapPoi poi = args.ChosenSuggestion as MapPoi;
                MapPoiTool poiTool = new MapPoiTool(poi, MapControl);
                SetMapTool(poiTool);
            }
            else if (!string.IsNullOrEmpty(args.QueryText))
            {
                if (args.QueryText.Trim().Length < 1)
                {
                    BoxSearch.ItemsSource = new string[] { };
                    return;
                }

                var suggestions = new List<MapPoi>();
                string sql = "SELECT * FROM MAPPOI t WHERE t.POINAME LIKE ?  LIMIT 100";
                suggestions = SQLiteDbHelper.Query<MapPoi>(sql, new object[] { $"%{args.QueryText.Trim()}%" });

                if (suggestions.Count == 0)
                {
                    BoxSearch.ItemsSource = new string[] { "没有查询到兴趣点" };
                    BoxSearch.IsSuggestionListOpen = true;
                }
                else if (suggestions.Count == 1)
                {
                    MapPoi poi = suggestions.First();
                    MapPoiTool poiTool = new MapPoiTool(poi, MapControl);
                    SetMapTool(poiTool);
                }
                else
                {
                    BoxSearch.ItemsSource = suggestions;
                    BoxSearch.IsSuggestionListOpen = true;
                }
            }

        }

        private void BookmarkList_ItemClick(object sender, ItemClickEventArgs e)
        {
            BtnBookmark.Flyout.Hide();
            MapPoi poi = e.ClickedItem as MapPoi;
            if (poi != null)
            {
                MapPoiTool poiTool = new MapPoiTool(poi, MapControl);
                SetMapTool(poiTool);
            }
        }

        private void BtnQuit_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            App.Current.Exit();
        }

        private void BtnConfig_Click(object sender, RoutedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;
            if (rootFrame != null)
            {
                rootFrame.Navigate(typeof(ConfigPage));
            }
        }


        // Map initialization logic is contained in MapViewModel.cs
    }
}
