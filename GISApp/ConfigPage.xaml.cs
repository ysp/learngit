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
    public sealed partial class ConfigPage : Page
    {
        public ConfigPage()
        {
            this.InitializeComponent();
        }

        private async void BtnChangePass_Click(object sender, RoutedEventArgs e)
        {
            string strName = TxbUser.Text;
            string strNewPass = TxbNewPassword.Password;
            string strConfirmPass = TxbConfirmPassword.Password;

            if (String.IsNullOrEmpty(strName))
            {
                MessageDialog dialog = new MessageDialog("用户名不能为空，请重新输入");
                await dialog.ShowAsync();
                TxbUser.Focus(FocusState.Pointer);
                return;
            }

            if (!strNewPass.Equals(strConfirmPass))
            {
                MessageDialog dialog = new MessageDialog("两次输入的密码不相同,请重新输入");
                await dialog.ShowAsync();
                TxbNewPassword.Focus(FocusState.Pointer);
                return;
            }

            if(String.IsNullOrEmpty(strNewPass))
            {
                MessageDialog dialog = new MessageDialog("密码不能为空，请重新输入");
                await dialog.ShowAsync();
                TxbNewPassword.Focus(FocusState.Pointer);
                return;
            }

            string sql = "SELECT * FROM User t WHERE t.UserName = ?";
            List<User> users = SQLiteDbHelper.Query<User>(sql, new object[] { strName });
            //修改已存在的用户名密码
            if (users.Any())
            {
                User user = users.First();
                user.PassWord = strNewPass;
                string updateSQL = "UPDATE User SET PassWord = ? WHERE UserName = ?";
                int count = SQLiteDbHelper.Execute(updateSQL, new object[] { strNewPass, strName });
            }
            //修改不存在的用户名密码
            else
            {
                SQLiteDbHelper.Insert(new User() { UserName = strName, PassWord = strNewPass });
            }

            MessageDialog dlgSuccess = new MessageDialog("用户名密码设置成功");
            await dlgSuccess.ShowAsync();

            Frame rootFrame = Window.Current.Content as Frame;
            if (rootFrame != null)
            {
                rootFrame.Navigate(typeof(LoginPage), TxbUser.Text);
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;
            if (rootFrame.CanGoBack)
            {
                rootFrame.GoBack();
            }
            else
            {
                rootFrame.Navigate(typeof(MainPage));
            }
        }
    }
}
