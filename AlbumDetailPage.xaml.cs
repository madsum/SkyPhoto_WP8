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
using System.Diagnostics;
using Microsoft.Phone.Tasks;
using Microsoft.Phone.Shell;

namespace SkyPhoto
{
    /// <summary>
    /// AlbumDetailPage shows all the piteur from pictuer foler
    /// </summary>
    public partial class AlbumDetailPage : PhoneApplicationPage
    {
        /// <summary>
        /// cameraCaptureTask to use camera
        /// </summary>
        private CameraCaptureTask cameraCaptureTask;

        /// <summary>
        /// PhotoChooserTask to seletc a picture from gallery
        /// </summary>
        private PhotoChooserTask photoChooserTask;
        
        /// <summary>
        /// told number of picture in in current album.
        /// </summary>
        private int NumberOfImages { get; set; }
        
        /// <summary>
        /// ImageCounter show and hide progessbar
        /// </summary>
        private int ImageCounter { get; set; }

        private SkydrivePhoto CurrentPhoto { get; set; }

        /// <summary>
        /// AlbumDetailPage class constructor
        /// </summary>
        public AlbumDetailPage()
        {
            InitializeComponent();
            ApplicationBarIconButton camera = ApplicationBar.Buttons[0] as ApplicationBarIconButton;
            ApplicationBarIconButton upload = ApplicationBar.Buttons[1] as ApplicationBarIconButton;
            ApplicationBarIconButton refresh = ApplicationBar.Buttons[2] as ApplicationBarIconButton;
            camera.Text = SkyPhoto.Resources.Resources.Camera;
            upload.Text = SkyPhoto.Resources.Resources.Upload;
            refresh.Text = SkyPhoto.Resources.Resources.Refresh;
            cameraCaptureTask = new CameraCaptureTask();
            cameraCaptureTask.Completed += new EventHandler<PhotoResult>(cameraCapture_Completed);
            photoChooserTask = new PhotoChooserTask();
            photoChooserTask.Completed += new EventHandler<PhotoResult>(photoChooserTask_Completed); 
            photoListBox.ItemsSource = App.CurrentAlbum.Photos;
            albumTitle.Text = App.CurrentAlbum.Title;
            string progMsgDownAlumPic = SkyPhoto.Resources.Resources.progMsgDownAlumPic;
            ShowProgressBar(progMsgDownAlumPic);
            ImageCounter = 0;
            DownloadPictures(App.CurrentAlbum);
        }

        /// <summary>
        /// DownloadPictures downs picutre link from sky dirve
        /// </summary>
        /// <param name="albumItem"></param>
        private async void DownloadPictures(SkydriveAlbum albumItem)
        {
            LiveConnectClient folderListClient = new LiveConnectClient(App.Session);
            LiveOperationResult result = await folderListClient.GetAsync(albumItem.ID + "/files");
            FolderListClient_GetCompleted(result, albumItem);

        }

        /// <summary>
        /// folderListClient event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void FolderListClient_GetCompleted(LiveOperationResult e, SkydriveAlbum album)
        {
            album.Photos.Clear();
            List<object> data = (List<object>)e.Result["data"];
            NumberOfImages = data.Count;

            if (NumberOfImages == 0)
            {
                HideProgressBar();
                return;
            }

            foreach (IDictionary<string, object> photo in data)
            {
                var item = new SkydrivePhoto();
                item.Title = (string)photo["name"];
                item.Subtitle = (string)photo["name"];
                item.PhotoUrl = (string)photo["source"];
                item.Description = (string)photo["description"];
                item.ID = (string)photo["id"];

                if (album != null)
                {
                    album.Photos.Add(item);
                }
            }
        }

        /// <summary>
        /// double tab event downlaod the picture from skydirve and save it to gallery
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DoubleTapped(object sender, Microsoft.Phone.Controls.GestureEventArgs e)
        {
            SkydrivePhoto selectedPhoto = App.CurrentAlbum.Photos[photoListBox.SelectedIndex];
            NavigationService.Navigate(new Uri("/FullPhotoPage.xaml?phtoUrl="+selectedPhoto.PhotoUrl, UriKind.Relative));
        }

        /// <summary>
        /// WebClient event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_OpenReadCompleted(object sender, OpenReadCompletedEventArgs e)
        {
            if (CurrentPhoto != null)
            {
                try
                {
                    Microsoft.Xna.Framework.Media.MediaLibrary mediaLibrary = new Microsoft.Xna.Framework.Media.MediaLibrary();
                    // Save the image to the saved pictures album.
                    mediaLibrary.SavePicture(CurrentPhoto.Title, e.Result);

                }
                catch (WebException we)
                {
                    string downFaild = SkyPhoto.Resources.Resources.downFaild;
                    MessageBox.Show(downFaild);
                }
            }
            HideProgressBar();
        }

        /// <summary>
        /// shows progressbar
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
        /// hinds progressbar
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
        /// Image_ImageOpened called from Image source of photoListBox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Image_ImageOpened(object sender, RoutedEventArgs e)
        {
            if (NumberOfImages == 0)
            {
                return;
            }

            if (ImageCounter == 0)
            {
                ImageCounter = NumberOfImages;
            }

            ImageCounter--;

            if (ImageCounter == 0)
            {
                HideProgressBar();
            }
        }

        /// <summary>
        /// to select a picture from gallery
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void photoChooserTask_Completed(object sender, PhotoResult e)
        {
            UploadPicture(e);
        }

        /// <summary>
        /// uplaods a picture to sky drive.
        /// </summary>
        /// <param name="e"></param>
        private async void UploadPicture(PhotoResult e)
        {
            LiveConnectClient uploadClient = new LiveConnectClient(App.Session);
            if (e.OriginalFileName == null)
            {
                return;
            }

            // file name is the current datetime stapm
            string ext = e.OriginalFileName.Substring(e.OriginalFileName.LastIndexOf('.'));
            DateTime dt = DateTime.Now;
            string fileName = String.Format("{0:d_MM_yyy_HH_mm_ss}", dt);
            fileName = fileName + ext;
            
            string progMsgUpPic = SkyPhoto.Resources.Resources.progMsgUpPic;
            ShowProgressBar(progMsgUpPic);

            try
            {
               LiveOperationResult result = await uploadClient.UploadAsync(App.CurrentAlbum.ID, fileName, e.ChosenPhoto, OverwriteOption.Overwrite);
               ISFile_UploadCompleted(result);
            }
            catch (Exception)
            {
                string upFaild = SkyPhoto.Resources.Resources.upFaild;
                MessageBox.Show(upFaild);
                HideProgressBar();
            }
        }

        /// <summary>
        /// uploadClient event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void ISFile_UploadCompleted(LiveOperationResult args)
        {
            HideProgressBar();
            string progMsgDownAlumPic = SkyPhoto.Resources.Resources.progMsgDownAlumPic;
            ShowProgressBar(progMsgDownAlumPic);
            ImageCounter = 0;
            DownloadPictures(App.CurrentAlbum);
        }


        /// <summary>
        /// take picture by camera and upload to sky dirve
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void cameraCapture_Completed(object sender, PhotoResult e)
        {
            if (e.TaskResult == TaskResult.OK)
            {
                string msgUp = SkyPhoto.Resources.Resources.msgUp;
                string msgTitle = SkyPhoto.Resources.Resources.msgUpTitle;
                MessageBoxResult m = MessageBox.Show(msgUp, msgTitle, MessageBoxButton.OKCancel);
                if (m == MessageBoxResult.OK)
                {
                    // if ok uplaod to sky drive
                    UploadPicture(e);
                }
                else
                {
                    // if cancle save picture to gallery
                    string ext = e.OriginalFileName.Substring(e.OriginalFileName.LastIndexOf('.'));
                    DateTime dt = DateTime.Now;
                    string fileName = String.Format("{0:d_M_yyyy_hh_mm_ss}", dt);
                    fileName += ext;
                    Microsoft.Xna.Framework.Media.MediaLibrary mediaLibrary = new Microsoft.Xna.Framework.Media.MediaLibrary();
                    // Save the image to the saved pictures album.
                    mediaLibrary.SavePicture(fileName, e.ChosenPhoto);
                }
            }
        }

        /// <summary>
        /// menubar button uplaod click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Upload_Click(object sender, EventArgs e)
        {
            photoChooserTask.Show();
        }

        /// <summary>
        /// menubar button camera click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Camera_Click(object sender, EventArgs e)
        {
            cameraCaptureTask.Show();
        }

        /// <summary>
        /// ContextMenu delete button tergar MenuDelte_Click and delete the picture from sky drive
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void MenuDelte_Click(object sender, RoutedEventArgs e)
        {
            var Menu = sender as MenuItem;
            SkydrivePhoto selectedFile = Menu.DataContext as SkydrivePhoto;

            string msgDelPic = SkyPhoto.Resources.Resources.msgDelPic;
            string msgDelPicTitel = SkyPhoto.Resources.Resources.msgDelPicTitle;
            MessageBoxResult m = MessageBox.Show(msgDelPic + selectedFile.Title + "?", msgDelPicTitel, MessageBoxButton.OKCancel);
            if (m == MessageBoxResult.OK)
            {
                LiveConnectClient client = new LiveConnectClient(App.Session);
                LiveOperationResult result = await client.DeleteAsync(selectedFile.ID);
                DeleteFile_Completed(selectedFile, result);

            }
            else
            {
                return;
            }
        }

        /// <summary>
        /// ContextMenu download button tergar MenuDownload_Click and download the picture from sky drive to gallery
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void MenuDownload_Click(object sender, RoutedEventArgs e)
        {
            var Menu = sender as MenuItem;
            SkydrivePhoto selectedPhoto = Menu.DataContext as SkydrivePhoto;
            WebClient client = new WebClient();
            CurrentPhoto = selectedPhoto;
            client.OpenReadCompleted += new OpenReadCompletedEventHandler(client_OpenReadCompleted);
            string progMsgDownPic = SkyPhoto.Resources.Resources.progMsgDownPic;
            ShowProgressBar(progMsgDownPic);
            try
            {
                client.OpenReadAsync(new Uri(selectedPhoto.PhotoUrl), client);
            }
            catch (Exception)
            {
                string downFaild = SkyPhoto.Resources.Resources.downFaild;
                MessageBox.Show(downFaild);
                HideProgressBar();
            }
        }


        /// <summary>
        /// LiveConnectClient event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void DeleteFile_Completed(SkydrivePhoto selectedFile, LiveOperationResult e)
        {
            if (e.Result != null)
            {
                App.CurrentAlbum.Photos.Remove(selectedFile);
            }
            else
            {
                string downFaild = SkyPhoto.Resources.Resources.downFaild;
                MessageBox.Show(downFaild);
            }
        }

        /// <summary>
        /// menubar button refresh click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Refresh_Click(object sender, EventArgs e)
        {
            string progMsgDownAlumPic = SkyPhoto.Resources.Resources.progMsgDownAlumPic;
            ShowProgressBar(progMsgDownAlumPic);
            ImageCounter = 0;
            DownloadPictures(App.CurrentAlbum);
        }
    }
}