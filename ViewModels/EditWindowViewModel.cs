using Jusy.Models;
using Jusy.Views;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
namespace Jusy.ViewModels
{
    public class EditWindowViewModel : ViewModelBase
    {
        private MainWindowViewModel _mainWindowViewModel;
        private ItemModel _item;
        private EditWindow _editWindow;
        public EditWindowViewModel(MainWindowViewModel mainWindowViewModel, ItemModel itemModel, EditWindow editWindow)
        {

            _mainWindowViewModel = mainWindowViewModel;
            _mainWindowViewModel.UpdateRefreshing();
            _item = itemModel;
            IsRealization = mainWindowViewModel.IsRealization;
            SelectedImplementation = itemModel.Implementation;
            RegisterNewCommand = ReactiveCommand.Create(SaveDocument);
            _editWindow = editWindow;
            _editWindow.Closed += OnWindowClosed;
        }

        private void OnWindowClosed(object? sender, EventArgs e)
        {
            _mainWindowViewModel.UpdateRefreshing();
        }


        private string _selectedImplementation;

        // Список статусов реализации
        public List<string> ImplementationStatuses { get; } = new List<string>
        {
            "В работе",
            "Реализовано"
        };
        public string SelectedImplementation
        {
            get => _selectedImplementation;
            set => this.RaiseAndSetIfChanged(ref _selectedImplementation, value);
        }
        public void SaveDocument()
        {
            // Проверяем все обязательные поля
            if (string.IsNullOrWhiteSpace(_editWindow.PartNumberTextBox.Text))
            {
                //_createWindow.Close();
                ErrorMessage = "Не все поля заполнены!";
                return;
            }
            if (string.IsNullOrWhiteSpace(_editWindow.SheetCountTextBox.Text))
            {
                ErrorMessage = "Не все поля заполнены!";
                //_createWindow.Close();
                return;
            }
            if (string.IsNullOrWhiteSpace(_editWindow.ProductTextBox.Text))
            {
                ErrorMessage = "Не все поля заполнены!";
                //_createWindow.Close();
                return;
            }
            if (string.IsNullOrWhiteSpace(_editWindow.ConnectedTextBox.Text))
            {
                ErrorMessage = "Не все поля заполнены!";
                //_createWindow.Close();
                return;
            }
            // Проверяем, не существует ли уже запись с таким PartNumber у другой записи
            string partNumber = _editWindow.PartNumberTextBox.Text.Trim();
            bool partNumberExistsInOtherRecord = _mainWindowViewModel._db.Items
                .Any(item => item.PartNumber == partNumber && item.Id != _item.Id);

            if (partNumberExistsInOtherRecord)
            {
                ErrorMessage = "Документ с таким Номером ИИ уже существует!";
                return;
            }

            var chahcedItem = _mainWindowViewModel._db.Items.FirstOrDefault(x => x.Id == _item.Id);
            chahcedItem.PartNumber = _editWindow.PartNumberTextBox.Text;
            chahcedItem.ListCount = _editWindow.SheetCountTextBox.Text;
            chahcedItem.InputDocument = _editWindow.ProductTextBox.Text;
            chahcedItem.Connected = _editWindow.ConnectedTextBox.Text;
            chahcedItem.Implementation = SelectedImplementation;
            _mainWindowViewModel._db.SaveChanges();
            _editWindow.Close();
           
        }
        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            set => this.RaiseAndSetIfChanged(ref _errorMessage, value);
        }
        public ItemModel Item
        {
            get => _item;
            set => this.RaiseAndSetIfChanged(ref _item, value);
        }
        private bool _isRealization;
        public bool IsRealization
        {
            get => _isRealization;
            set => this.RaiseAndSetIfChanged(ref _isRealization, value);
        }
        public ReactiveCommand<Unit, Unit> RegisterNewCommand { get; }
    }
}