using System;
using System.Windows;
using System.Collections.Generic;
using Microsoft.Phone.Controls;
using Microsoft.Live;
using Microsoft.Live.Controls;
using System.Windows.Media.Imaging;
using System.Threading;


namespace SkyPhoto
{
    public partial class MainPage : PhoneApplicationPage
    {
        private LiveConnectClient client;
        string FirstName = "";
        string LastName = "";
        /// <summary>
        /// MainPage MainPage constructor
        /// </summary>
        /// 
        public MainPage()
        {
            InitializeComponent();
        }
        /// <summary>
        /// btnSignin_SessionChanged tregar signin button clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnSignin_SessionChanged(object sender, LiveConnectSessionChangedEventArgs e)
        {
            if (e.Session != null && e.Status == LiveConnectSessionStatus.Connected)
            {
                App.Session = e.Session;
                client = new LiveConnectClient(e.Session);
                LiveOperationResult result =  await client.GetAsync("me");
                OnGetCompleted(result);
            }
            else
            {
                // remove all live profile informaion.
                client = null;
                App.Session = null;
                gotoAlbum.Visibility = Visibility.Collapsed;
                ProfileName.Text = "";
                ProfilePic.Source = null;
            }
        }

        void OnGetCompleted(LiveOperationResult e)
        {
            if (e.Result.ContainsKey("first_name") ||
                e.Result.ContainsKey("last_name"))
            {
                if (e.Result.ContainsKey("first_name"))
                {
                    if (e.Result["first_name"] != null)
                    {
                        FirstName = e.Result["first_name"].ToString();
                    }
                }
                if (e.Result.ContainsKey("last_name"))
                {
                    if (e.Result["last_name"] != null)
                    {
                        LastName = e.Result["last_name"].ToString();
                    }
                }
                String Welcome = SkyPhoto.Resources.Resources.Welcome;
                ProfileName.Text = Welcome + " " + FirstName + " " + LastName;
                gotoAlbum.Visibility = Visibility.Visible;
                GetProfilePicture();
                NavigationService.Navigate(new Uri("/AlbumPage.xaml", UriKind.Relative));
            }
        }
        /// <summary>
        /// gotoAlbum button click envet handler.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void gotoAlbum_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/AlbumPage.xaml", UriKind.Relative));
        }
        /// <summary>
        /// GetProfilePicture retrive live acount profile picture.
        /// </summary>
        private async void GetProfilePicture()
        {
           LiveConnectClient clientGetPicture = new LiveConnectClient(App.Session);
           LiveOperationResult result = await clientGetPicture.GetAsync("me/picture");
           GetPicture_GetCompleted(result);
        }
        /// <summary>
        /// LiveOperationCompletedEventArgs event handler GetPicture_GetCompleted
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void GetPicture_GetCompleted(LiveOperationResult e)
        {
            ProfilePic.Source = new BitmapImage(new Uri((string)e.Result["location"], UriKind.RelativeOrAbsolute));   
        }

    }
}