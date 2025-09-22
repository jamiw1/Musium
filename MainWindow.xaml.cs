using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace localMusicPlayerTest
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            ExtendsContentIntoTitleBar = true;
            SetTitleBar(Titlebar);

            RootFrame.Navigate(typeof(localMusicPlayerTest.Pages.Albums));
        }

        private void rootNav_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (args.InvokedItemContainer != null)
            {
                var navItemTag = args.InvokedItemContainer.Tag.ToString();

                Type pageType = Type.GetType(navItemTag);

                var currentPageType = RootFrame.CurrentSourcePageType;

                if (pageType != null && !Type.Equals(pageType, currentPageType))
                {
                    RootFrame.Navigate(pageType);
                }
            }
        }
        private void TitleBar_BackRequested(TitleBar sender, object args)
        {
            if (RootFrame.CanGoBack)
            {
                RootFrame.GoBack();
            }
        }

        private void TitleBar_PaneToggleRequested(TitleBar sender, object args)
        {
            RootNavigationView.IsPaneOpen = !RootNavigationView.IsPaneOpen;
        }
    }
}
