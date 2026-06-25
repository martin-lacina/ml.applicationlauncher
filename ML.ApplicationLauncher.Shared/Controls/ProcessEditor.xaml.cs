// Copyright © Martin Lacina

using System;
using System.Windows.Controls;
using ML.ApplicationLauncher.Source.Model;

namespace ML.ApplicationLauncher.Shared.Controls
{
    /// <summary>
    /// Interaction logic for ProcessEditor.xaml
    /// </summary>
    public partial class ProcessEditor : UserControl
    {
        public ProcessEditor()
        {
            InitializeComponent();

            // Populate execution mode values
            ExecutionModeCombo.ItemsSource = Enum.GetValues(typeof(ExecutionMode));
        }
    }
}
