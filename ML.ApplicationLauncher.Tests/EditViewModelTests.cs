using NUnit.Framework;
using System;
using System.Threading.Tasks;
using ML.ApplicationLauncher.Source;
using ML.ApplicationLauncher.Shell.Shared.ViewModels;

namespace ML.ApplicationLauncher.Tests
{
    public class EditViewModelTests
    {
        [Test]
        public void AddGroup_ShouldAddGroupToCollection()
        {
            var vm = new EditViewModel("test.json");
            var initialCount = vm.Groups.Count;
            vm.AddGroupCommand.Execute(null);
            Assert.AreEqual(initialCount + 1, vm.Groups.Count);
        }

        [Test]
        public void AddProcess_ShouldAddProcessToSelectedGroup()
        {
            var vm = new EditViewModel("test.json");
            vm.AddGroupCommand.Execute(null);
            Assert.IsNotEmpty(vm.Groups);
            vm.SelectedGroup = vm.Groups[0];
            vm.AddProcessCommand.Execute(null);
            Assert.AreEqual(1, vm.SelectedGroup.Processes.Count);
        }

        [Test]
        public void RemoveProcess_ShouldRemoveProcessFromGroup()
        {
            var vm = new EditViewModel("test.json");
            vm.AddGroupCommand.Execute(null);
            vm.SelectedGroup = vm.Groups[0];
            vm.AddProcessCommand.Execute(null);
            var proc = vm.SelectedGroup.Processes[0];
            vm.SelectedProcess = proc;
            vm.RemoveCommand.Execute(null);
            Assert.AreEqual(0, vm.SelectedGroup.Processes.Count);
        }

        [Test]
        public void MoveProcess_ShouldReorderProcesses()
        {
            var vm = new EditViewModel("test.json");
            vm.AddGroupCommand.Execute(null);
            vm.SelectedGroup = vm.Groups[0];
            vm.AddProcessCommand.Execute(null);
            vm.AddProcessCommand.Execute(null);
            var first = vm.SelectedGroup.Processes[0];
            vm.SelectedProcess = first;
            vm.MoveDownCommand.Execute(null);
            Assert.AreNotEqual(first.Id, vm.SelectedGroup.Processes[0].Id);
        }

        [Test]
        public void UndoRedo_ShouldRevertAndRestoreChanges()
        {
            var vm = new EditViewModel("test.json");
            vm.AddGroupCommand.Execute(null);
            vm.SelectedGroup = vm.Groups[0];
            vm.AddProcessCommand.Execute(null);
            Assert.AreEqual(1, vm.SelectedGroup.Processes.Count);
            vm.UndoCommand.Execute(null);
            Assert.AreEqual(0, vm.SelectedGroup.Processes.Count);
            vm.RedoCommand.Execute(null);
            Assert.AreEqual(1, vm.SelectedGroup.Processes.Count);
        }
    }
}
