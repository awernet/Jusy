using Avalonia.Controls;
using Jusy.ViewModels;

namespace Jusy;

public partial class ErrorWindow : Window
{
    public ErrorWindow(string title, string errorText)
    {
        InitializeComponent();
        DataContext = new ErrorWindowViewModel(title, errorText);
    }

}