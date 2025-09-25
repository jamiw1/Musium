using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.AccessControl;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace localMusicPlayerTest
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            

            ExtendsContentIntoTitleBar = true;
            SetTitleBar(Titlebar);

            UpdateNavigationViewSelection(typeof(localMusicPlayerTest.Pages.Albums));

            AudioPlayerElement.SetMediaPlayer(App.Player);
        }
        private void UpdateNavigationViewSelection(Type pageType)
        {
            RootFrame.Navigate(pageType);
            var itemToSelect = RootNavigationView.MenuItems
                                      .OfType<NavigationViewItem>()
                                      .FirstOrDefault(item => item.Tag?.ToString() == pageType.FullName);
            RootNavigationView.SelectedItem = itemToSelect;
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
                    UpdateNavigationViewSelection(pageType);
                }
            }
        }
        private void rootNav_TitleNavClicked(object sender, RoutedEventArgs args)
        {
            var clickedButton = sender as Button;

            if (clickedButton?.Tag is string pageName)
            {
                Type pageType = Type.GetType(pageName);

                var currentPageType = RootFrame.CurrentSourcePageType;

                if (pageType != null && !Type.Equals(pageType, currentPageType))
                {
                    UpdateNavigationViewSelection(pageType);
                }
            }
        }
        private void TitleBar_BackRequested(TitleBar sender, object args)
        {
            if (RootFrame.CanGoBack)
            {
                RootFrame.GoBack();
                UpdateNavigationViewSelection(RootFrame.CurrentSourcePageType);
            }
        }

        private void TitleBar_PaneToggleRequested(TitleBar sender, object args)
        {
            RootNavigationView.IsPaneOpen = !RootNavigationView.IsPaneOpen;
        }
    }
}
