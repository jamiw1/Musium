using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Musium.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using TagLib;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using WinRT.Interop;

namespace Musium.Pages;

public sealed partial class SettingsPage : Page
{
    public SettingsPage()
    {
        InitializeComponent();
    }

    private async void LibraryPathSetting_Click(object sender, RoutedEventArgs e)
    {
        var folderPicker = new Windows.Storage.Pickers.FolderPicker();
        var hwnd = WindowNative.GetWindowHandle(App.MainWindow);
        InitializeWithWindow.Initialize(folderPicker, hwnd);
        folderPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.MusicLibrary;
        folderPicker.FileTypeFilter.Add("*");

        Windows.Storage.StorageFolder folder = await folderPicker.PickSingleFolderAsync();
        if (folder != null)
        {
            Windows.Storage.AccessCache.StorageApplicationPermissions.
            FutureAccessList.AddOrReplace("PickedFolderToken", folder);
            SettingsService.Instance.LibraryPath = folder.Path;
        }

    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {

    }
}
