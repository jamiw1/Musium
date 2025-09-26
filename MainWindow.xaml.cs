using Musium.Pages;
using Musium.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.AccessControl;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.ApplicationSettings;

namespace Musium
{
    public sealed partial class MainWindow : Window
    {
        private readonly AudioService _audioService = AudioService.Instance;
        public static NavigationView MainNavView;
        public static Frame RootNavFrame;
        public MainWindow()
        {
            InitializeComponent();

            MainNavView = RootNavigationView;
            RootNavFrame = RootFrame;

            ExtendsContentIntoTitleBar = true;
            SetTitleBar(Titlebar);

            UpdateNavigationViewSelection(typeof(Musium.Pages.Albums));

            _audioService.SetDispatcherQueue(DispatcherQueue);
            _audioService.SetMediaPlayer(AudioPlayerElement);
        }
        public static void UpdateNavigationViewSelection(Type pageType)
        {
            RootNavFrame.Navigate(pageType);
            var itemToSelect = MainNavView.MenuItems
                                      .OfType<NavigationViewItem>()
                                      .FirstOrDefault(item => item.Tag?.ToString() == pageType.FullName);
            MainNavView.SelectedItem = itemToSelect;
        }

        private void rootNav_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked == true)
            {
                rootNav_Navigate(typeof(SettingsPage), args.RecommendedNavigationTransitionInfo);
            }
            else if (args.InvokedItemContainer != null)
            {
                Type navPageType = Type.GetType(args.InvokedItemContainer.Tag.ToString());
                rootNav_Navigate(navPageType, args.RecommendedNavigationTransitionInfo);
            }
        }
        private void rootNav_Navigate(Type navPageType, NavigationTransitionInfo transitionInfo)
        {
            Type preNavPageType = RootFrame.CurrentSourcePageType;

            if (navPageType is not null && !Type.Equals(preNavPageType, navPageType))
            {
                RootFrame.Navigate(navPageType, null, transitionInfo);
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
