using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jusy.ViewModels
{
    public class ErrorWindowViewModel : ViewModelBase
    {
        public ErrorWindowViewModel(string title, string errorText) {
            Title = title;
            ErrorText = errorText;
        }
        public string Title { get; set; } = "Error";
        public string ErrorText { get; set; } = "Error";
    }
}
