using Avalonia.Controls;
using Jusy.Models;
using Jusy.ViewModels;

namespace Jusy.Views
{
    public partial class EditWindow : Window
    {
        public EditWindow()
        {
            InitializeComponent();
        }

        public EditWindow(MainWindowViewModel mainWindowViewModel, ItemModel itemModel)
        {
            InitializeComponent();
            DataContext = new EditWindowViewModel(mainWindowViewModel, itemModel, this);
        }
    }
}