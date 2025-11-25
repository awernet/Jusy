using Avalonia.Controls;
using Jusy.ViewModels;

namespace Jusy;

public partial class CreateWindow : Window
{
    public CreateWindow(MainWindowViewModel mainWindowViewModel)
    {
        InitializeComponent();
        DataContext = new CreateWindowViewModel(mainWindowViewModel, this);
    }
}