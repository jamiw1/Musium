using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Musium.Services
{
    public class SettingsService : INotifyPropertyChanged
    {
        public AudioService Audio { get; } = AudioService.Instance;
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private static SettingsService _instance;
        public static SettingsService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new SettingsService();
                }
                return _instance;
            }
        }

        private SettingsService()
        {
            LoadSettings();
        }

        private void LoadSettings()
        {
            if (ApplicationData.Current.LocalSettings.Values.ContainsKey("LibraryPath"))
            {
                _libraryPath = (string)ApplicationData.Current.LocalSettings.Values["LibraryPath"];
            }
        }

        private string _libraryPath;
        public string LibraryPath
        {
            get => _libraryPath;
            set
            {
                if (_libraryPath != value)
                {
                    _libraryPath = value;
                    ApplicationData.Current.LocalSettings.Values["LibraryPath"] = value;
                    Audio.SetLibrary(value);
                    if (Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread() != null)
                    {
                        Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread().TryEnqueue(() =>
                        {
                            OnPropertyChanged();
                        });
                    }
                    else
                    {
                        OnPropertyChanged();
                    }
                }
            }
        }
    }
}
