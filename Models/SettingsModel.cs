using Avalonia.Controls;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;

namespace Jusy.Models
{
    public class SettingsModel{
        public string server_patch { get; set; }
    }

    public class Settings { 
        public static string server_patch { get; set; }

        public static void Init(string patch) {
            server_patch = patch;
        }
        public static string GetPach() { 
            return server_patch;
        }
        public static void InitUser() {
            string[] files = Directory.GetFiles(server_patch);
            string filePath = $"{server_patch}/users.json";
            UserModel _user = new UserModel();

            try {
                if (File.Exists(filePath))
                {
                    try
                    {
                        var json = File.ReadAllText(filePath);
                        var users = JsonConvert.DeserializeObject<List<UserModel>>(json);
                        _user = users?.FirstOrDefault(x => x.enviropment.Equals(Environment.UserName)); //?? new();
                        if(_user == null)
                            throw new InvalidOperationException($"Пользователь не найден.");
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException($"Ошибка чтения файла пользователей.{ex}");
                    }
                }
                else
                {
                    var er = new ErrorWindow("Error", "dsad");
                    er.Show();
                    //Logger.ShowWindows("Ошибка чтения настроек", $"Файл {filePath} не существует\nили неверно указань путь.");
                }
            }
            catch (Exception ex)
            {
                var er = new ErrorWindow("Не удалось найти файл настроек", ex.Message);
                er.Show();  
            }
            


        }
    }
}
