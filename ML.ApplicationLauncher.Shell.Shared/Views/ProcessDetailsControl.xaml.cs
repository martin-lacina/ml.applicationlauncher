using System;
using System.Windows.Controls;
using ML.ApplicationLauncher.Source.Model;

namespace ML.ApplicationLauncher.Shell.Shared.Views
{
    public partial class ProcessDetailsControl : UserControl
    {
        public ProcessDetailsControl()
        {
            InitializeComponent();

            // Populate execution mode values
            ExecutionModeCombo.ItemsSource = Enum.GetValues(typeof(ExecutionMode));
        }
    }
}
