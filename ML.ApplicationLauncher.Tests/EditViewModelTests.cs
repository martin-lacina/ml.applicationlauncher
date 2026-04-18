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
    }
}
