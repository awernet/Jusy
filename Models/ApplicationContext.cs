using Microsoft.EntityFrameworkCore;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Jusy.Models
{
    public class ApplicationContext : DbContext
    {
        public DbSet<ItemModel> Items { get; set; } = null!;
        public DbSet<MessageModel> Messages { get; set; } = null!;
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            try
            {
                // Получаем путь к директории
                string dbDirectory = Settings.GetPach();

                // Проверяем, что путь получен
                if (string.IsNullOrWhiteSpace(dbDirectory))
                {
                    throw new InvalidOperationException($"Не указана директория для базы данных {dbDirectory}");
                }


                // Формируем полный путь к файлу БД
                string dbPath = Path.Combine(dbDirectory, "database.db");

                // Всегда используем SQLite, даже если файла нет (он создастся автоматически)
                optionsBuilder.UseSqlite($"Data Source={dbPath};");

                // Для отладки можно вывести путь
                Debug.WriteLine($"Используется база данных: {dbPath}");
            }
            catch (Exception ex)
            {
                // Показываем понятное сообщение об ошибке
                var errorWindow = new ErrorWindow(
                    "Ошибка подключения к базе данных",
                    $"Не удалось настроить подключение к базе данных:\n{ex.Message}");

                errorWindow.Show();

                // Прерываем выполнение, так как без БД приложение не может работать
                throw;
            }
        }
    }
}
