using Jusy.Models;
using ReactiveUI;
using System;
using System.Linq;
using System.Reactive;
using System.Runtime.CompilerServices;
namespace Jusy.ViewModels
{
    public class CreateWindowViewModel : ViewModelBase
    {
        public CreateWindowViewModel(MainWindowViewModel mainWindowViewModel, CreateWindow createWindow)
        {
            _mainWindowViewModel = mainWindowViewModel;
            CreateCommand = ReactiveCommand.Create(CreateDocument);
            _createWindow = createWindow;
        }
        public void CreateDocument()
        {
            // Проверяем все обязательные поля
            if (string.IsNullOrWhiteSpace(_createWindow.PartNumberTextBox.Text))
            {
                //_createWindow.Close();
                ErrorMessage = "Не все поля заполнены!";
                return;
            }
            if (string.IsNullOrWhiteSpace(_createWindow.SheetCountTextBox.Text))
            {
                ErrorMessage = "Не все поля заполнены!";
                //_createWindow.Close();
                return;
            }
            if (string.IsNullOrWhiteSpace(_createWindow.ProductTextBox.Text))
            {
                ErrorMessage = "Не все поля заполнены!";
                //_createWindow.Close();
                return;
            }
            if (string.IsNullOrWhiteSpace(_createWindow.ConnectedTextBox.Text))
            {
                ErrorMessage = "Не все поля заполнены!";
                //_createWindow.Close();
                return;
            }
            // Проверяем существование PartNumber в базе данных
            string partNumber = _createWindow.PartNumberTextBox.Text.Trim();
            bool partNumberExists = _mainWindowViewModel._db.Items
                .Any(item => item.PartNumber == partNumber);

            if (partNumberExists)
            {
                ErrorMessage = "Документ с таким Номером ИИ уже существует!";
                return;
            }

            // Если все проверки пройдены, создаем документ
            var item = new ItemModel
            {
                Date = DateTime.Now,
                LastName = _mainWindowViewModel._user.user_name,
                PartNumber = _createWindow.PartNumberTextBox.Text,
                ListCount = _createWindow.SheetCountTextBox.Text,
                InputDocument = _createWindow.ProductTextBox.Text,
                Connected = _createWindow.ConnectedTextBox.Text,
                Implementation = "В работе"
            };
            try
            {
                _mainWindowViewModel._db.Items.Add(item);
                _mainWindowViewModel._db.SaveChanges();
                _createWindow.Close();
            }
            catch (Exception ex)
            {
            }
        }
        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            set => this.RaiseAndSetIfChanged(ref _errorMessage, value);
        }
        private MainWindowViewModel _mainWindowViewModel;
        private ItemModel _item;
        private CreateWindow _createWindow;
        public ReactiveCommand<Unit, Unit> CreateCommand { get; }

    }
}