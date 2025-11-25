using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jusy.Models
{
    public static class Logger
    {
        public static void ShowWindows(string title, string errorText) {
            new ErrorWindow(title, errorText).Show();          
        }
    }
}
