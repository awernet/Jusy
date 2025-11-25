using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Jusy.Models
{
    public class ItemModel : INotifyPropertyChanged
    {
        private int _id;
        private DateTime _date;
        private string _partNumber;
        private string _listCount;
        private string _inputDocument;
        private string _lastName;
        private string _implementation;
        private string _connected;
        public ItemModel()
        {
            Date = DateTime.Now;
            PartNumber = string.Empty;
            ListCount = string.Empty;
            InputDocument = string.Empty;
            LastName = string.Empty;
            Implementation = "В работе";
        }

        public int Id
        {
            get => _id;
            set => SetField(ref _id, value);
        }

        public DateTime Date
        {
            get => _date;
            set => SetField(ref _date, value);
        }

        public string PartNumber
        {
            get => _partNumber;
            set => SetField(ref _partNumber, value);
        }

        public string ListCount
        {
            get => _listCount;
            set => SetField(ref _listCount, value);
        }

        public string InputDocument
        {
            get => _inputDocument;
            set => SetField(ref _inputDocument, value);
        }

        public string LastName
        {
            get => _lastName;
            set => SetField(ref _lastName, value);
        }

        public string Implementation
        {
            get => _implementation;
            set => SetField(ref _implementation, value);
        }

        public string Connected
        {
            get => _connected;
            set => SetField(ref _connected, value);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}