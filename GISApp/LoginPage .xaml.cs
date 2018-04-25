using GISApp.Tables;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace GISApp
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class LoginPage : Page
    {
        public LoginPage()
        {
            this.InitializeComponent();
        }

        private async void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string strName = TxbName.Text;
            string strPass = TxbPassword.Password;

            string sql = "SELECT * FROM User t WHERE t.UserName = ? AND t.PassWord = ?";
            List<User> users = SQLiteDbHelper.Query<User>(sql, new object[] { strName, strPass });
            if (users.Count < 1)
            {
                MessageDialog dialog = new MessageDialog("用户名密码错误，请重新输入");
                await dialog.ShowAsync();
                return;
            }

            Frame rootFrame = Window.Current.Content as Frame;
            if (rootFrame != null)
            {
                rootFrame.Navigate(typeof(MainPage));
            }
        }

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            App.Current.Exit();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            string userName = e.Parameter as String;
            if (!String.IsNullOrEmpty(userName))
            {
                TxbName.Text = userName;
            }
            base.OnNavigatedTo(e);
        }
    }
}
