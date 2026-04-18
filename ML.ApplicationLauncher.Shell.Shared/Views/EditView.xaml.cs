using System.Windows;
using System.Windows.Controls;
using ML.ApplicationLauncher.Shell.Shared.ViewModels;

namespace ML.ApplicationLauncher.Shell.Shared.Views
{
    public partial class EditView : UserControl
    {
        public EditView()
        {
            InitializeComponent();
            // Assuming the config file path is known; replace with actual path
            DataContext = new EditViewModel("CommandDefinitions.json");
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (DataContext is EditViewModel vm)
            {
                vm.SelectedGroup = e.NewValue as CommandGroupViewModel;
            }
        }
    }
}
