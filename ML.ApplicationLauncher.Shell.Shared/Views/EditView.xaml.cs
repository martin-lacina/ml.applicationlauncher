using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ML.ApplicationLauncher.Shell.Shared.ViewModels;

namespace ML.ApplicationLauncher.Shell.Shared.Views
{
    public partial class EditView : UserControl
    {
        private System.Windows.Point _dragStartPoint;
        private CommandProcessViewModel? _draggedProcess;

        public EditView()
        {
            InitializeComponent();
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (DataContext is EditViewModel vm)
            {
                vm.SelectedGroup = e.NewValue as CommandGroupViewModel;
            }
        }

        private void ProcessesListView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _dragStartPoint = e.GetPosition(null);
            _draggedProcess = null;
            var dep = (DependencyObject)e.OriginalSource;
            var item = VisualUpwardSearch(dep) as ListViewItem;
            if (item != null)
            {
                _draggedProcess = item.DataContext as CommandProcessViewModel;
            }
        }

        private void ProcessesListView_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;
            if (_draggedProcess == null) return;
            var currentPos = e.GetPosition(null);
            if (Math.Abs(currentPos.X - _dragStartPoint.X) < SystemParameters.MinimumHorizontalDragDistance &&
                Math.Abs(currentPos.Y - _dragStartPoint.Y) < SystemParameters.MinimumVerticalDragDistance)
                return;

            var data = new DataObject("process", _draggedProcess);
            DragDrop.DoDragDrop(this, data, DragDropEffects.Move);
            _draggedProcess = null;
        }

        private void ProcessesListView_Drop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent("process")) return;
            var dragged = e.Data.GetData("process") as CommandProcessViewModel;
            if (dragged == null) return;
            if (!(DataContext is EditViewModel vm)) return;
            if (vm.SelectedGroup == null) return;

            var dep = (DependencyObject)e.OriginalSource;
            var item = VisualUpwardSearch(dep) as ListViewItem;
            CommandProcessViewModel? target = null;
            if (item != null) target = item.DataContext as CommandProcessViewModel;

            var list = vm.SelectedGroup.Processes;
            var oldIndex = list.IndexOf(dragged);
            if (oldIndex < 0) return;

            int newIndex = target == null ? list.Count - 1 : list.IndexOf(target);
            if (newIndex < 0) newIndex = list.Count - 1;
            if (oldIndex < newIndex) newIndex--; // adjust for removal shifting
            if (oldIndex == newIndex) return;

            vm.MoveProcess(dragged.Id, newIndex);
        }

        private static DependencyObject VisualUpwardSearch(DependencyObject source)
        {
            while (source != null && !(source is ListViewItem))
            {
                source = VisualTreeHelper.GetParent(source);
            }
            return source;
        }
    }
}
