// Copyright © Martin Lacina

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Collections.ObjectModel;
using System.Linq;
using ML.ApplicationLauncher.Shared.ViewModels;
using ML.ApplicationLauncher.Source.Model;

namespace ML.ApplicationLauncher.Shared.Views
{
    public partial class EditView : UserControl
    {
        private System.Windows.Point _dragStartPoint;
        private CommandProcessViewModel? _draggedProcess;
        private System.Windows.Point _groupDragStartPoint;
        private CommandGroupViewModel? _draggedGroup;

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

        private static DependencyObject? VisualUpwardSearch(DependencyObject source)
        {
            while (source != null && !(source is ListViewItem))
            {
                source = VisualTreeHelper.GetParent(source);
            }
            return source;
        }

        private static DependencyObject? VisualUpwardSearchTreeItem(DependencyObject source)
        {
            while (source != null && !(source is TreeViewItem))
            {
                source = VisualTreeHelper.GetParent(source);
            }
            return source;
        }

        private void GroupsTreeView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _groupDragStartPoint = e.GetPosition(null);
            _draggedGroup = null;
            var dep = (DependencyObject)e.OriginalSource;
            var item = VisualUpwardSearchTreeItem(dep) as TreeViewItem;
            if (item != null)
            {
                _draggedGroup = item.DataContext as CommandGroupViewModel;
            }
        }

        private void GroupsTreeView_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;
            if (_draggedGroup == null) return;
            var currentPos = e.GetPosition(null);
            if (Math.Abs(currentPos.X - _groupDragStartPoint.X) < SystemParameters.MinimumHorizontalDragDistance &&
                Math.Abs(currentPos.Y - _groupDragStartPoint.Y) < SystemParameters.MinimumVerticalDragDistance)
                return;

            var data = new DataObject("group", _draggedGroup);
            DragDrop.DoDragDrop(this, data, DragDropEffects.Move);
            _draggedGroup = null;
        }

        private void GroupsTreeView_Drop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent("group")) return;
            var dragged = e.Data.GetData("group") as CommandGroupViewModel;
            if (dragged == null) return;
            if (!(DataContext is EditViewModel vm)) return;

            var dep = (DependencyObject)e.OriginalSource;
            var item = VisualUpwardSearchTreeItem(dep) as TreeViewItem;
            CommandGroupViewModel? target = null;
            if (item != null) target = item.DataContext as CommandGroupViewModel;

            // find parent collections for dragged and target
            var draggedParent = FindParentCollectionInTree(dragged.Id, vm.Groups) ?? vm.Groups;
            var targetParent = target == null ? vm.Groups : (FindParentCollectionInTree(target.Id, vm.Groups) ?? vm.Groups);

            // only support reordering within the same parent for now
            if (!ReferenceEquals(draggedParent, targetParent)) return;

            var list = draggedParent;
            var oldIndex = list.IndexOf(dragged);
            if (oldIndex < 0) return;

            int newIndex = target == null ? list.Count - 1 : list.IndexOf(target);
            if (newIndex < 0) newIndex = list.Count - 1;
            if (oldIndex < newIndex) newIndex--;
            if (oldIndex == newIndex) return;

            vm.MoveGroup(dragged.Id, newIndex);
        }

        private ObservableCollection<CommandGroupViewModel>? FindParentCollectionInTree(Guid id, ObservableCollection<CommandGroupViewModel> current)
        {
            foreach (var g in current)
            {
                if (g.Children.Any(c => c.Id == id)) return g.Children;
                var nested = FindParentCollectionInTree(id, g.Children);
                if (nested != null) return nested;
            }
            return null;
        }

        private void ExecutionModeCombo_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is ComboBox cb)
            {
                cb.ItemsSource = Enum.GetValues(typeof(ExecutionMode));
            }
        }
    }
}
