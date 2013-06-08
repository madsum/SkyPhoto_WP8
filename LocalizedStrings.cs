using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace SkyPhoto
{ 
    /// <summary>
    /// Wrapper class for using localized resources
    /// </summary>
    public class LocalizedStrings
    {
        /// <summary>
        /// LocalizedStrings class constructor.
        /// </summary>
        public LocalizedStrings()
        {
        }
        /// <summary>
        /// static localizedResources object.
        /// </summary>
        private static SkyPhoto.Resources.Resources localizedResources = new SkyPhoto.Resources.Resources();
        /// <summary>
        /// /// LocalizedStrings property.
        /// </summary>
        public SkyPhoto.Resources.Resources LocalizedResources { get { return localizedResources; } }

    }
}
