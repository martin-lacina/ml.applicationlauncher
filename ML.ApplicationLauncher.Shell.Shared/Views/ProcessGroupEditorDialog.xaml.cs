using System.Windows;
using System.Windows.Input;
using System.ComponentModel;
using ML.ApplicationLauncher.Shell.ViewModels;

namespace ML.ApplicationLauncher.Shell.Views
{
    public partial class ProcessGroupEditorDialog : Window
    {
        private bool _isDirty = false;

        public ProcessGroupEditorDialog()
        {
            InitializeComponent();
            if (DataContext is ProcessGroupEditorDialogViewModel vm)
            {
                vm.PropertyChanged += OnViewModelPropertyChanged;
            }
        }

        private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            _isDirty = true;
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);
            if (e.Key == Key.Escape)
            {
                if (AttemptClose())
                {
                    Close();
                }
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (!AttemptClose())
            {
                e.Cancel = true;
            }
            base.OnClosing(e);
        }

        private bool AttemptClose()
        {
            if (_isDirty)
            {
                var result = MessageBox.Show(this, "You have unsaved changes. Do you want to close without saving?", "Unsaved Changes", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result != MessageBoxResult.Yes)
                {
                    return false;
                }
            }
            return true;
        }

        void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            // This method is intentionally left empty. It can be used for additional logic if needed.
        }
    }
}
