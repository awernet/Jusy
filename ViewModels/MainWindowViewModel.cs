using Avalonia.Controls;
using Avalonia.Threading;
using Jusy.Models;
using Jusy.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Tmds.DBus.Protocol;

namespace Jusy.ViewModels
{
    public class MainWindowViewModel : ViewModelBase, IDisposable
    {
        public ApplicationContext _db;
        private readonly ObservableCollection<ItemModel> _items = new();
        private readonly IDisposable _refreshSubscription;
        public UserModel _user;
        private ItemModel _selectedItem;
        private string _searchProduct;
        private string _searchNumber;
        private string _searchSpecialist;
        private string _searchConnected;
        private bool _isRefreshing;
        private bool _isEnableRegisterNewButton;
        private bool _isEnableChatButton;
        private bool _isRealization;
        private bool _isEnableEditButton;
        private int? _lastSelectedId;
        private string _message;
        private int _inProgressCount;
        public int InProgressCount
        {
            get => _inProgressCount;
            set => this.RaiseAndSetIfChanged(ref _inProgressCount, value);
        }

        private int _completedCount;
        public int CompletedCount
        {
            get => _completedCount;
            set => this.RaiseAndSetIfChanged(ref _completedCount, value);
        }

        private int _totalCount;
        public int TotalCount
        {
            get => _totalCount;
            set => this.RaiseAndSetIfChanged(ref _totalCount, value);
        }

        // Метод для обновления статистики на основе отображаемых Items
        public void UpdateStatistics()
        {
            try
            {
                TotalCount = _items.Count;
                CompletedCount = _items.Count(item => item.Implementation == "Реализовано");
                InProgressCount = _items.Count(item => item.Implementation != "Реализовано");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка обновления статистики: {ex.Message}");
            }
        }
        public void UpdateInfoMessages()
        {
            string dirPath = Settings.GetPach();
            string pathToInfoMessageFile = Path.Combine(dirPath, "info.io");
            if (File.Exists(pathToInfoMessageFile))
            {
                InfoMessages.Clear();
                string[] lines = File.ReadAllLines(pathToInfoMessageFile);
                foreach (string line in lines)
                {
                    InfoMessages.Add(new InfoMessage()
                    {
                        Message = line.Replace("\\r\\n", "\r\n")
                                     .Replace("\\n", "\n")
                    });
                }
                InfoMessages.Reverse();
            }
        }
        private MainWindow _mainWindow;

        public MainWindowViewModel(MainWindow mainWindow)
        {
            try
            {
                _mainWindow = mainWindow;
                _db = new ApplicationContext();
                LoadInitialData();
                LoadUserData();
                UpdateStatistics(); // Обновляем статистику при загрузке
                UpdateInfoMessages();
                EditCommand = ReactiveCommand.CreateFromTask(EditDocument);
                CreateCommand = ReactiveCommand.Create(CreateDocument);
                SendMessageCommand = ReactiveCommand.Create(SendMessage);
                SaveMessageCommand = ReactiveCommand.Create(SaveMessage);

                // Настройка фильтрации
                this.WhenAnyValue(
                    x => x.SearchProduct,
                    x => x.SearchNumber,
                    x => x.SearchSpecialist,
                    x => x.SearchConnected)
                    .Throttle(TimeSpan.FromMilliseconds(400))
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(_ => ApplyFilters());

                // Настройка периодического обновления (каждые 5 секунд)
                _refreshSubscription = Observable.Interval(TimeSpan.FromSeconds(5))
                    .Where(_ => !_isRefreshing)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(_ => RefreshData());
            }
            catch (Exception ex)
            {
            }

        }

        public void SendMessage()
        {
            if (_message != null)
            {
                var message = new MessageModel
                {
                    ItemId = _selectedItem.Id,
                    UserName = _user.user_name,
                    Message = _message,
                    Timestamp = DateTime.Now
                };
                _db.Messages.Add(message);
                _db.SaveChanges();
                Message = string.Empty;
            }
        }
        public void SaveMessage(){
            var ActiveDirectoryPath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "History");
            if(!Directory.Exists(ActiveDirectoryPath))
                Directory.CreateDirectory(ActiveDirectoryPath);
            string filePath = Path.Combine(ActiveDirectoryPath, $"{SelectedItem.InputDocument} {SelectedItem.LastName}.txt");

            string content = $"{SelectedItem.Date}|{SelectedItem.PartNumber}|{SelectedItem.ListCount}|{SelectedItem.LastName}\r\nИстория согласования:\r\n";
            foreach (var message in Messages) {
                content += $"[{message.Timestamp}] {message.UserName} : {message.Message}\r\n";
            }

            File.WriteAllText(filePath, content, Encoding.UTF8);
            ErrorWindow errorWindow = new ErrorWindow("Сохранение истории согласования", $"История согласования сохранена в файл:\r\n{filePath}");
            errorWindow.Show();
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "explorer",
                    Arguments = $"\"{ActiveDirectoryPath}\"",
                    UseShellExecute = true
                });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", ActiveDirectoryPath);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start("xdg-open", ActiveDirectoryPath);
            }


        }

        public void CreateDocument()
        {
            CreateWindow createWindow = new CreateWindow(this);
            createWindow.ShowDialog(_mainWindow);
        }

        private void LoadInitialData()
        {
            try
            {
                var items = _db.Items.AsNoTracking().ToList();
                _items.Clear();
                foreach (var item in items)
                {
                    _items.Add(item);
                }
            }
            catch (Exception ex)
            {
            }
        }

        private void LoadUserData()
        {
            string appDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string filePath = Path.Combine(appDirectory, "bin", "users.json");

            if (File.Exists($"{Settings.GetPach()}\\users.json"))
            {
                try
                {
                    var json = File.ReadAllText($"{Settings.GetPach()}\\users.json");
                    var users = JsonConvert.DeserializeObject<List<UserModel>>(json);
                    _user = users?.FirstOrDefault(x => x.enviropment.Equals(Environment.UserName)) ?? new();
                    IsRealization = _user.edit_realization;
                    IsEnableRegisterNewButton = _user.register;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка загрузки пользователя: {ex.Message}");
                }
            }
        }

        public void RefreshData()
        {
            if (_isRefreshing) return;
            _isRefreshing = true;

            try
            {
                _lastSelectedId = SelectedItem?.Id;

                using (var freshContext = new ApplicationContext())
                {
                    // Применяем те же фильтры, что и в ApplyFilters
                    var filtered = freshContext.Items.AsNoTracking().AsEnumerable();

                    if (!string.IsNullOrWhiteSpace(SearchProduct))
                        filtered = filtered.Where(item =>
                            item.InputDocument?.Contains(SearchProduct, StringComparison.OrdinalIgnoreCase) == true);

                    if (!string.IsNullOrWhiteSpace(SearchNumber))
                        filtered = filtered.Where(item =>
                            item.PartNumber?.Contains(SearchNumber, StringComparison.OrdinalIgnoreCase) == true);

                    if (!string.IsNullOrWhiteSpace(SearchSpecialist))
                    {
                        filtered = filtered.Where(item =>
                            item.LastName != null &&
                            item.LastName.Contains(SearchSpecialist, StringComparison.OrdinalIgnoreCase));
                    }
                    if (!string.IsNullOrWhiteSpace(SearchConnected))
                        filtered = filtered.Where(item =>
                            item.Connected?.Contains(SearchConnected, StringComparison.OrdinalIgnoreCase) == true);

                    var freshItems = filtered.ToList();

                    Dispatcher.UIThread.Post(() =>
                    {
                        UpdateItemsCollection(freshItems);
                        UpdateStatistics(); // Обновляем статистику после обновления данных
                        UpdateInfoMessages();

                        if (_lastSelectedId.HasValue)
                        {
                            SelectedItem = _items.FirstOrDefault(i => i.Id == _lastSelectedId);
                        }
                    });
                    
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка обновления: {ex.Message}");
            }
            finally
            {
                _isRefreshing = false;
            }
        }
        public void UpdateRefreshing() { 
            if (_isRefreshing)
                _isRefreshing = false;
            else _isRefreshing = true;
        }

        private void ApplyFilters()
        {
            try
            {
                var filtered = _db.Items.AsNoTracking().AsEnumerable();

                if (!string.IsNullOrWhiteSpace(SearchProduct))
                    filtered = filtered.Where(item =>
                        item.InputDocument?.Contains(SearchProduct, StringComparison.OrdinalIgnoreCase) == true);

                if (!string.IsNullOrWhiteSpace(SearchNumber))
                    filtered = filtered.Where(item =>
                        item.PartNumber?.Contains(SearchNumber, StringComparison.OrdinalIgnoreCase) == true);

                if (!string.IsNullOrWhiteSpace(SearchSpecialist))
                {
                    filtered = filtered.Where(item =>
                        item.LastName != null &&
                        item.LastName.Contains(SearchSpecialist, StringComparison.OrdinalIgnoreCase));
                }
                if (!string.IsNullOrWhiteSpace(SearchConnected))
                    filtered = filtered.Where(item =>
                        item.Connected?.Contains(SearchConnected, StringComparison.OrdinalIgnoreCase) == true);


                var filteredList = filtered.ToList();

                Dispatcher.UIThread.Post(() =>
                {
                    var selectedId = SelectedItem?.Id;
                    UpdateItemsCollection(filteredList);
                    UpdateStatistics(); // Обновляем статистику после фильтрации
                    if (selectedId != null) SelectedItem = _items.FirstOrDefault(i => i.Id == selectedId);
                });
            }
            catch (Exception ex)
            {

            }
        }

        /// <summary>
        /// Умное обновление коллекции без потери позиции скролла
        /// Новые элементы добавляются в начало списка
        /// </summary>
        private void UpdateItemsCollection(List<ItemModel> newItems)
        {
            // Реверсируем список, чтобы новые элементы были сверху
            newItems.Reverse();

            // Создаем словари для быстрого поиска по Id
            var existingDict = _items.ToDictionary(i => i.Id);
            var newDict = newItems.ToDictionary(i => i.Id);

            // Удаляем элементы, которых нет в новом списке
            for (int i = _items.Count - 1; i >= 0; i--)
            {
                if (!newDict.ContainsKey(_items[i].Id))
                {
                    _items.RemoveAt(i);
                }
            }

            // Обновляем существующие и добавляем новые элементы
            for (int i = 0; i < newItems.Count; i++)
            {
                var newItem = newItems[i];

                if (existingDict.TryGetValue(newItem.Id, out var existingItem))
                {
                    // Элемент существует - обновляем его свойства
                    int currentIndex = _items.IndexOf(existingItem);

                    if (currentIndex != i)
                    {
                        // Если позиция изменилась - перемещаем элемент
                        _items.Move(currentIndex, i);
                    }

                    // Обновляем свойства существующего элемента
                    UpdateItemProperties(existingItem, newItem);
                }
                else
                {
                    // Новый элемент - вставляем на нужную позицию
                    if (i < _items.Count)
                    {
                        _items.Insert(i, newItem);
                    }
                    else
                    {
                        _items.Add(newItem);
                    }
                }
            }
        }

        /// <summary>
        /// Обновление свойств элемента без замены объекта
        /// </summary>
        private void UpdateItemProperties(ItemModel existing, ItemModel updated)
        {
            // Обновляем только измененные свойства
            if (existing.InputDocument != updated.InputDocument)
                existing.InputDocument = updated.InputDocument;

            if (existing.ListCount != updated.ListCount)
                existing.ListCount = updated.ListCount;

            if (existing.PartNumber != updated.PartNumber)
                existing.PartNumber = updated.PartNumber;

            if (existing.LastName != updated.LastName)
                existing.LastName = updated.LastName;

            if (existing.Implementation != updated.Implementation)
                existing.Implementation = updated.Implementation;

            if (existing.Connected != updated.Connected)
                existing.Connected = updated.Connected;
        }

        public async Task EditDocument()
        {
            if (SelectedItem != null)
            {
                var editWindow = new EditWindow(this, SelectedItem);
                await editWindow.ShowDialog(_mainWindow);
            }
        }

        public void Dispose()
        {
            _refreshSubscription?.Dispose();
            _db?.Dispose();
            GC.SuppressFinalize(this);
        }

        // Свойства
        public IEnumerable<ItemModel> Items => _items;
        public ReactiveCommand<Unit, Unit> EditCommand { get; }
        public ReactiveCommand<Unit, Unit> CreateCommand { get; }
        public ReactiveCommand<Unit, Unit> SendMessageCommand { get; }
        public ReactiveCommand<Unit, Unit> SaveMessageCommand { get; }
        public ObservableCollection<MessageModel> Messages { get; set; } = new();
        public ObservableCollection<InfoMessage> InfoMessages { get; set; } = new();

        public ItemModel SelectedItem
        {
            get => _selectedItem;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedItem, value);

                if (value != null)
                {
                    IsEnableEditButton = _user.edit_document || _user.edit_realization;
                    IsEnableChatButton = value.LastName.Equals(_user.user_name) || _user.edit_realization;
                    _lastSelectedId = value.Id;

                    // Загружаем сообщения для выбранного элемента в обратном порядке
                    var messagesForItem = _db?.Messages?
                        .Where(x => x.ItemId == value.Id)
                        .OrderByDescending(x => x.Timestamp)
                        .ToList();

                    // Обновляем коллекцию без пересоздания (лучше для UI)
                    Messages.Clear();
                    if (messagesForItem != null)
                    {
                        foreach (var msg in messagesForItem)
                        {
                            Messages.Add(msg);
                        }
                    }
                }
                else
                {
                    IsEnableEditButton = false;
                    IsEnableChatButton = false;
                    Messages.Clear();
                }
            }
        }

        public string Message
        {
            get => _message;
            set => this.RaiseAndSetIfChanged(ref _message, value);
        }

        public string SearchProduct
        {
            get => _searchProduct;
            set => this.RaiseAndSetIfChanged(ref _searchProduct, value);
        }

        public string SearchNumber
        {
            get => _searchNumber;
            set => this.RaiseAndSetIfChanged(ref _searchNumber, value);
        }

        public string SearchSpecialist
        {
            get => _searchSpecialist;
            set => this.RaiseAndSetIfChanged(ref _searchSpecialist, value);
        }
        public string SearchConnected
        {
            get => _searchConnected;
            set => this.RaiseAndSetIfChanged(ref _searchConnected, value);
        }

        public bool IsEnableRegisterNewButton
        {
            get => _isEnableRegisterNewButton;
            private set => this.RaiseAndSetIfChanged(ref _isEnableRegisterNewButton, value);
        }

        public bool IsEnableChatButton
        {
            get => _isEnableChatButton;
            private set => this.RaiseAndSetIfChanged(ref _isEnableChatButton, value);
        }

        public bool IsRealization
        {
            get => _isRealization;
            set => this.RaiseAndSetIfChanged(ref _isRealization, value);
        }

        public bool IsEnableEditButton
        {
            get => _isEnableEditButton;
            set => this.RaiseAndSetIfChanged(ref _isEnableEditButton, value);
        }
    }
}