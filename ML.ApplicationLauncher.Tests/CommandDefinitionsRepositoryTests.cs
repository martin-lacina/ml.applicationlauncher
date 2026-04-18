using NUnit.Framework;
using System;
using System.IO;
using System.Threading.Tasks;
using ML.ApplicationLauncher.Source;
using System.Linq;
using System.Collections.Generic;

namespace ML.ApplicationLauncher.Tests
{
    public class CommandDefinitionsRepositoryTests
    {
        private string _tempFile;
        private CommandDefinitionsRepository _repo;

        [SetUp]
        public void Setup()
        {
            _tempFile = Path.GetTempFileName();
            _repo = new CommandDefinitionsRepository(_tempFile);
        }

        [TearDown]
        public void TearDown()
        {
            if (File.Exists(_tempFile)) File.Delete(_tempFile);
        }

        [Test]
        public async Task LoadAsync_EmptyFile_ReturnsEmptyList()
        {
            var groups = await _repo.LoadAsync();
            Assert.IsNotNull(groups);
            Assert.IsEmpty(groups);
        }

        [Test]
        public async Task AddGroupAsync_SavesAndLoads()
        {
            var group = new CommandGroup { Name = "Test" };
            await _repo.AddGroupAsync(group);
            var groups = await _repo.LoadAsync();
            Assert.AreEqual(1, groups.Count);
            Assert.AreEqual("Test", groups[0].Name);
        }

        [Test]
        public async Task RemoveGroupAsync_RemovesGroup()
        {
            var group = new CommandGroup { Name = "ToRemove" };
            await _repo.AddGroupAsync(group);
            await _repo.RemoveGroupAsync(group.Id);
            var groups = await _repo.LoadAsync();
            Assert.IsEmpty(groups);
        }

        [Test]
        public async Task AddProcessAsync_AddsProcessToGroup()
        {
            var group = new CommandGroup { Name = "WithProc" };
            await _repo.AddGroupAsync(group);
            var loaded = await _repo.LoadAsync();
            var gId = loaded.First().Id;
            var proc = new CommandProcess { Name = "P1", Path = "C:\\Windows\\notepad.exe" };
            await _repo.AddProcessAsync(gId, proc);
            var reloaded = await _repo.LoadAsync();
            Assert.AreEqual(1, reloaded.First().Processes.Count);
            Assert.AreEqual("P1", reloaded.First().Processes[0].Name);
        }

        [Test]
        public async Task MoveProcessAsync_ReordersProcesses()
        {
            var group = new CommandGroup { Name = "MoveGroup" };
            await _repo.AddGroupAsync(group);
            var loaded = await _repo.LoadAsync();
            var gId = loaded.First().Id;
            var p1 = new CommandProcess { Name = "A", Path = "C:\\a.exe" };
            var p2 = new CommandProcess { Name = "B", Path = "C:\\b.exe" };
            await _repo.AddProcessAsync(gId, p1);
            await _repo.AddProcessAsync(gId, p2);
            var before = (await _repo.LoadAsync()).First().Processes.Select(p => p.Name).ToList();
            Assert.AreEqual(new List<string>{"A","B"}, before);
            await _repo.MoveProcessAsync(gId, p1.Id, 1);
            var after = (await _repo.LoadAsync()).First().Processes.Select(p => p.Name).ToList();
            Assert.AreEqual(new List<string>{"B","A"}, after);
        }

        [Test]
        public void SaveAsync_DuplicateIds_Throws()
        {
            var g1 = new CommandGroup { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), Name = "G1" };
            var g2 = new CommandGroup { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), Name = "G2" };
            var list = new List<CommandGroup> { g1, g2 };
            Assert.ThrowsAsync<InvalidOperationException>(async () => await _repo.SaveAsync(list));
        }

        [Test]
        public void SaveAsync_CircularReference_Throws()
        {
            var g = new CommandGroup { Name = "Self" };
            // create a direct circular reference
            g.Children.Add(g);
            var list = new List<CommandGroup> { g };
            Assert.ThrowsAsync<InvalidOperationException>(async () => await _repo.SaveAsync(list));
        }
    }
}
