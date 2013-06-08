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
using System.Windows.Media.Imaging;

namespace SkyPhoto
{
    public partial class FullPhotoPage : PhoneApplicationPage
    {
        /// <summary>
        /// FullPhotoPage Construector
        /// </summary>
        public FullPhotoPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            BitmapImage imgSource = new BitmapImage();
            imgSource.UriSource = new Uri(NavigationContext.QueryString["phtoUrl"], UriKind.Absolute);
            FullImage.Source = imgSource;
            FullImage.Height = Application.Current.RootVisual.RenderSize.Height;
            FullImage.Width = Application.Current.RootVisual.RenderSize.Width;
        }

        protected override void OnOrientationChanged(OrientationChangedEventArgs e)
        {
            base.OnOrientationChanged(e);

            if (e.Orientation == PageOrientation.Landscape ||
                e.Orientation == PageOrientation.LandscapeLeft ||
                e.Orientation == PageOrientation.LandscapeRight)
            {
                FullImage.Height = Application.Current.RootVisual.RenderSize.Width;
                FullImage.Width = Application.Current.RootVisual.RenderSize.Height;
            }
            else
            {
                FullImage.Height = Application.Current.RootVisual.RenderSize.Height;
                FullImage.Width = Application.Current.RootVisual.RenderSize.Width;
            }
        }
    }
}