using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Live;
using System.Windows.Media.Imaging;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework.GamerServices;
using System.Diagnostics;
using Microsoft.Phone.Shell;

namespace SkyPhoto
{
    /// <summary>
    /// AlbumPage page shows all picture folder from sky drive
    /// </summary>
    public partial class AlbumPage : PhoneApplicationPage
    {
        /// <summary>
        /// collections of picture folder
        /// </summary>
        public ObservableCollection<SkydriveAlbum> Albums { get; private set; }
        
        /// <summary>
        /// number of images in each foder ImageCounte. ImageCounte hide progessbar 
        /// upon completeing all piture
        /// </summary>
        private int ImageCounter { get; set; }
        
        /// <summary>
        /// NewAlbumName holds user input for new album name
        /// </summary>
        private string NewAlbumName { get; set; }
        
        /// <summary>
        /// AlbumPage class contructor
        /// </summary>
        public AlbumPage()
        {

            InitializeComponent();
            ApplicationBarIconButton album = ApplicationBar.Buttons[0] as ApplicationBarIconButton;
            ApplicationBarIconButton refresh = ApplicationBar.Buttons[1] as ApplicationBarIconButton;
            NewAlbumName = null;
            album.Text = SkyPhoto.Resources.Resources.NewAlubum;
            refresh.Text = SkyPhoto.Resources.Resources.Refresh;
            Albums = new ObservableCollection<SkydriveAlbum>();
            AlbumListBox.ItemsSource = Albums;
            GetAlubmData();
        }

        /// <summary>
        /// GetAlubmData gets all the album
        /// </summary>
        private async void GetAlubmData()
        {
            ImageCounter = 0;
            // show progressbar.
            string progMsgDownAlbum = SkyPhoto.Resources.Resources.progMsgDownAlbum;
            ShowProgressBar(progMsgDownAlbum);
            LiveConnectClient clientFolder = new LiveConnectClient(App.Session);
            LiveOperationResult result = await clientFolder.GetAsync("/me/albums");
            GetAlubmData_Completed(result);
        }

        /// <summary>
        /// LiveConnectClient event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void GetAlubmData_Completed(LiveOperationResult e)
        {
            List<object> data = (List<object>)e.Result["data"];
            if (ImageCounter == 0)
            {
                ImageCounter = data.Count;
            }

            foreach (IDictionary<string, object> album in data)
            {
                SkydriveAlbum albumItem = new SkydriveAlbum();
                albumItem.Title = (string)album["name"];
                albumItem.Description = (string)album["description"];
                albumItem.ID = (string)album["id"];
                Albums.Add(albumItem);
                GetAlbumPicture(albumItem);
            }
        }

        /// <summary>
        /// GetAlbumPicture get all the picture from each album
        /// </summary>
        /// <param name="albumItem"></param>
        private async void GetAlbumPicture(SkydriveAlbum albumItem)
        {
            try
            {
                LiveConnectClient albumPictureClient = new LiveConnectClient(App.Session);

                LiveOperationResult result = await albumPictureClient.GetAsync(albumItem.ID + "/picture");
                albumPictureClient_GetCompleted(result, albumItem);
            }
            catch (LiveConnectException lce)
            {
                albumPictureClient_GetCompleted(null, albumItem);
            }
        }

        /// <summary>
        /// LiveConnectClient event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void albumPictureClient_GetCompleted(LiveOperationResult e, SkydriveAlbum album)
        {
            if (e != null)
            {
                album.AlbumPicture = (string)e.Result["location"];
            }
            else
            {
                album.AlbumPicture = "/icons/empty_album.png";
            }
            // hide progressbar.
            ImageCounter--;

            if (ImageCounter == 0)
            {
                HideProgressBar();
            }
        }

        /// <summary>
        /// AlbumListBox listbox tap event handler 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AlbumListBox_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            ListBox lb = sender as ListBox;
            App.CurrentAlbum = Albums[lb.SelectedIndex];
            NavigationService.Navigate(new Uri("/AlbumDetailPage.xaml", UriKind.Relative));
        }

        /// <summary>
        /// shows progessbar
        /// </summary>
        /// <param name="progressText"></param>
        private void ShowProgressBar(string progressText)
        {
            // Making progessbar visible
            Loading.Visibility = Visibility.Visible;
            ContentPanel.Opacity = 0.25;
            progressBar.IsIndeterminate = true;
            progressBarText.Text = progressText;
            ContentPanel.IsHitTestVisible = false;
            (ApplicationBar as ApplicationBar).IsVisible = false;
        }

        /// <summary>
        /// hide progressbar
        /// </summary>
        private void HideProgressBar()
        {
            // Making progress bar invisible.
            Loading.Visibility = Visibility.Collapsed;
            ContentPanel.Opacity = 1;
            ContentPanel.IsHitTestVisible = true;
            progressBar.IsIndeterminate = false;
            progressBarText.Text = string.Empty;
            (ApplicationBar as ApplicationBar).IsVisible = true;
        }

        /// <summary>
        /// ContextMenu delete button tergar MenuDelte_Click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void MenuDelte_Click(object sender, RoutedEventArgs e)
        {
            var Menu = sender as MenuItem;
            SkydriveAlbum selectedAlbum = Menu.DataContext as SkydriveAlbum;
            string msgDel = SkyPhoto.Resources.Resources.msgDelAlbum;
            string msgDelAlbumTitel = SkyPhoto.Resources.Resources.msgDelAlbumTitel;

            MessageBoxResult m = MessageBox.Show(msgDel + selectedAlbum.Title + "?", msgDelAlbumTitel, MessageBoxButton.OKCancel);
            if (m == MessageBoxResult.OK)
            {
                LiveConnectClient client = new LiveConnectClient(App.Session);
                LiveOperationResult result = await client.DeleteAsync(selectedAlbum.ID);
                DeleteFolder_Completed(selectedAlbum, result);

            }
            else
            {
                return;
            }
        }

        /// <summary>
        /// LiveConnectClient event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void DeleteFolder_Completed(SkydriveAlbum selectedAlbum, LiveOperationResult e)
        {
            if (e.Result != null)
            {
                Albums.Remove(selectedAlbum);
            }
            else
            {
                string msgDelFailed = SkyPhoto.Resources.Resources.msgDelFailed;
                MessageBox.Show(msgDelFailed);
            }
        }

        /// <summary>
        /// menubar button new album click tregar New_album_click  
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void New_album_click(object sender, EventArgs e)
        {
            string fileName = SkyPhoto.Resources.Resources.fileName;
            Guide.BeginShowKeyboardInput(Microsoft.Xna.Framework.PlayerIndex.One, fileName, " ", " ", new AsyncCallback(GetInputString), null);
        }

        /// <summary>
        /// get input string from album name input dialog
        /// </summary>
        /// <param name="res"></param>
        void GetInputString(IAsyncResult res)
        {
            NewAlbumName = Guide.EndShowKeyboardInput(res);
            // check null or empty string.
            if (string.IsNullOrEmpty(NewAlbumName) || 
                string.IsNullOrWhiteSpace(NewAlbumName))
            {
                return;
            }

            Create_new_album();
        }

        /// <summary>
        /// Create_new_album crates a new album
        /// </summary>
        private async void Create_new_album()
        {
            Dictionary<string, object> folderData = new Dictionary<string, object>();
            folderData.Add("name", NewAlbumName);
            folderData.Add("type", "album");
            LiveConnectClient client = new LiveConnectClient(App.Session);
            LiveOperationResult result = await client.PostAsync("me/skydrive", folderData);
            CreateFolder_Completed(result);
        }


        void CreateFolder_Completed(LiveOperationResult result)
        {
            if (result != null)
            {
                // succesful.
                Dispatcher.BeginInvoke(() =>
                {
                    Albums.Clear();
                    GetAlubmData();
                }
                );
            }
            else
            {
                string newAlbumFaild = SkyPhoto.Resources.Resources.newAlbumFaild;
                MessageBox.Show(newAlbumFaild);
            }
        }

        /// <summary>
        /// menubar button refresh click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Refresh_Click(object sender, EventArgs e)
        {
            Albums.Clear();
            GetAlubmData();
        }
    }
}