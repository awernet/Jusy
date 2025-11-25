using Avalonia.Controls;
using Jusy;
using Jusy.Models;
using Jusy.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace Jusy.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            try
            {
                InitializeComponent();
                InitializeSettings();
            }
            catch (Exception ex)
            {
                var er = new ErrorWindow("Ошибка инициализации", ex.Message);
                er.Show();
            }
        }

        private async void InitializeSettings()
        {
            try
            {
                string appDirectory = Directory.GetCurrentDirectory();
                string binDirectory = Path.Combine(appDirectory, "bin");
                string filePath = Path.Combine(binDirectory, "settings.json");

                // Создаем папку bin, если не существует
                if (!Directory.Exists(binDirectory))
                {
                    Directory.CreateDirectory(binDirectory);
                }

                // Если файл настроек не существует - запрашиваем путь к серверу
                if (!File.Exists(filePath))
                {
                    string serverPath = await SelectServerFolder();

                    if (string.IsNullOrEmpty(serverPath))
                    {
                        throw new InvalidOperationException("Не выбрана папка серверных ресурсов");
                    }

                    // Создаем файл настроек
                    var settings = new SettingsModel { server_patch = serverPath };
                    var json = JsonConvert.SerializeObject(settings, Formatting.Indented);
                    File.WriteAllText(filePath, json);
                }

                // Загружаем настройки
                var settingsJson = File.ReadAllText(filePath);
                var loadedSettings = JsonConvert.DeserializeObject<SettingsModel>(settingsJson);
                Settings.Init(loadedSettings.server_patch);
                Settings.InitUser();
                DataContext = new MainWindowViewModel(this);
            }
            catch (Exception ex)
            {
                var er = new ErrorWindow("Ошибка загрузки настроек", ex.Message);
                er.Show();
            }
        }

        private async Task<string> SelectServerFolder()
        {
            var dialog = new OpenFolderDialog
            {
                Title = "Выберите папку серверных ресурсов"
            };

            var result = await dialog.ShowAsync(this);
            return result;
        }
    }
}