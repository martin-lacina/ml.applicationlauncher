using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;
using ML.ApplicationLauncher.Source;
using ML.ApplicationLauncher.Source.Model;
using ML.ApplicationLauncher.Source.Services;
using Moq;
using System.Linq;
using System.Collections.Generic;

namespace ML.ApplicationLauncher.Tests
{
    public class CommandDefinitionsRepositoryTests
    {
        private Mock<IConfigurationManager<ProcessGroup[]>> _mockConfigManager;
        private Mock<IProcessModelMapper> _mockMapper;
        private CommandDefinitionsRepository _repo;

        [SetUp]
        public void Setup()
        {
            _mockConfigManager = new Mock<IConfigurationManager<ProcessGroup[]>>();
            _mockMapper = new Mock<IProcessModelMapper>();
            _repo = new CommandDefinitionsRepository(_mockConfigManager.Object, _mockMapper.Object);
        }

        #region LoadAsync Tests

        [Test]
        public async Task LoadAsync_CallsConfigManager_ReturnsEmptyList()
        {
            // Arrange
            _mockConfigManager
                .Setup(m => m.LoadConfigurationAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Array.Empty<ProcessGroup>());

            // Act
            var groups = await _repo.LoadAsync();

            // Assert
            Assert.IsNotNull(groups);
            Assert.IsEmpty(groups);
            _mockConfigManager.Verify(m => m.LoadConfigurationAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task LoadAsync_WithProcessGroups_MapsToCommandGroups()
        {
            // Arrange
            var processGroups = new[]
            {
                new ProcessGroup(
                    DisplayName: "Group1",
                    Comment: "Test",
                    CanLaunch: true,
                    Groups: Array.Empty<ProcessGroup>(),
                    Processes: new[]
                    {
                        new ProcessLaunchInformation(
                            DisplayName: "Process1",
                            Comment: "",
                            Executable: "C:\\notepad.exe",
                            Arguments: Array.Empty<string>(),
                            ExecutionMode: ExecutionMode.Default,
                            Disabled: false,
                            Hidden: false,
                            WorkingDirectory: null
                        )
                    }
                )
            };

            _mockConfigManager
                .Setup(m => m.LoadConfigurationAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(processGroups);

            // Act
            var groups = await _repo.LoadAsync();

            // Assert
            Assert.AreEqual(1, groups.Count);
            Assert.AreEqual("Group1", groups[0].Name);
            Assert.AreEqual(1, groups[0].Processes.Count);
            Assert.AreEqual("Process1", groups[0].Processes[0].Name);
        }

        #endregion

        #region SaveAsync Tests

        [Test]
        public async Task SaveAsync_CallsConfigManagerWithConvertedData()
        {
            // Arrange
            var group = new CommandGroup { Name = "TestGroup" };
            var process = new CommandProcess { Name = "P1", Path = "C:\\test.exe" };
            group.Processes.Add(process);

            _mockConfigManager
                .Setup(m => m.SaveConfigurationAsync(It.IsAny<ProcessGroup[]>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            await _repo.SaveAsync(new List<CommandGroup> { group });

            // Assert
            _mockConfigManager.Verify(
                m => m.SaveConfigurationAsync(It.IsAny<ProcessGroup[]>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task SaveAsync_ValidatesBeforeSaving()
        {
            // Arrange
            var g1 = new CommandGroup { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), Name = "G1" };
            var g2 = new CommandGroup { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), Name = "G2" };
            var list = new List<CommandGroup> { g1, g2 };

            _mockConfigManager
                .Setup(m => m.SaveConfigurationAsync(It.IsAny<ProcessGroup[]>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(async () => await _repo.SaveAsync(list));
            _mockConfigManager.Verify(
                m => m.SaveConfigurationAsync(It.IsAny<ProcessGroup[]>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task SaveAsync_EmptyPath_Throws()
        {
            // Arrange
            var group = new CommandGroup { Name = "Test" };
            var process = new CommandProcess { Name = "P1", Path = "" }; // Empty path
            group.Processes.Add(process);

            _mockConfigManager
                .Setup(m => m.SaveConfigurationAsync(It.IsAny<ProcessGroup[]>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(async () => await _repo.SaveAsync(new List<CommandGroup> { group }));
        }

        #endregion

        #region AddGroupAsync Tests

        [Test]
        public async Task AddGroupAsync_SavesAndLoads()
        {
            // Arrange
            var group = new CommandGroup { Name = "Test" };
            var loadedGroup = new CommandGroup { Id = group.Id, Name = "Test" };

            _mockConfigManager
                .Setup(m => m.LoadConfigurationAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Array.Empty<ProcessGroup>());

            _mockConfigManager
                .Setup(m => m.SaveConfigurationAsync(It.IsAny<ProcessGroup[]>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            await _repo.AddGroupAsync(group);

            // Assert
            _mockConfigManager.Verify(
                m => m.LoadConfigurationAsync(It.IsAny<CancellationToken>()),
                Times.Once);
            _mockConfigManager.Verify(
                m => m.SaveConfigurationAsync(It.IsAny<ProcessGroup[]>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        #endregion

        #region RemoveGroupAsync Tests

        [Test]
        public async Task RemoveGroupAsync_RemovesGroup()
        {
            // Arrange
            var groupId = Guid.NewGuid();
            var group = new CommandGroup { Id = groupId, Name = "ToRemove" };

            _mockConfigManager
                .Setup(m => m.LoadConfigurationAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[] {
                    new ProcessGroup(group.Name, "", true, Array.Empty<ProcessGroup>(),
                        new ProcessLaunchInformation[0], false, false)
                });

            _mockConfigManager
                .Setup(m => m.SaveConfigurationAsync(It.IsAny<ProcessGroup[]>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            await _repo.RemoveGroupAsync(groupId);

            // Assert
            _mockConfigManager.Verify(
                m => m.SaveConfigurationAsync(It.IsAny<ProcessGroup[]>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        #endregion

        #region AddProcessAsync Tests

        [Test]
        public async Task AddProcessAsync_GroupNotFound_Throws()
        {
            // Arrange
            var nonExistentGroupId = Guid.NewGuid();
            var proc = new CommandProcess { Name = "P1", Path = "C:\\Windows\\notepad.exe" };

            _mockConfigManager
                .Setup(m => m.LoadConfigurationAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Array.Empty<ProcessGroup>());

            // Act & Assert
            Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await _repo.AddProcessAsync(nonExistentGroupId, proc));
        }

        #endregion

        #region RemoveProcessAsync Tests

        [Test]
        public async Task RemoveProcessAsync_GroupNotFound_Throws()
        {
            // Arrange
            var nonExistentGroupId = Guid.NewGuid();
            var processId = Guid.NewGuid();

            _mockConfigManager
                .Setup(m => m.LoadConfigurationAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Array.Empty<ProcessGroup>());

            // Act & Assert
            Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await _repo.RemoveProcessAsync(nonExistentGroupId, processId));
        }
        #endregion

        #region MoveGroupAsync Tests

        [Test]
        public async Task MoveGroupAsync_GroupNotFound_Throws()
        {
            // Arrange
            var nonExistentGroupId = Guid.NewGuid();

            _mockConfigManager
                .Setup(m => m.LoadConfigurationAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Array.Empty<ProcessGroup>());

            // Act & Assert
            Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await _repo.MoveGroupAsync(nonExistentGroupId, 0));
        }

        #endregion

        #region MoveProcessAsync Tests

        [Test]
        public async Task MoveProcessAsync_GroupNotFound_Throws()
        {
            // Arrange
            var nonExistentGroupId = Guid.NewGuid();
            var processId = Guid.NewGuid();

            _mockConfigManager
                .Setup(m => m.LoadConfigurationAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Array.Empty<ProcessGroup>());

            // Act & Assert
            Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await _repo.MoveProcessAsync(nonExistentGroupId, processId, 0));
        }

        #endregion

        #region Validation Tests

        [Test]
        public void SaveAsync_DuplicateProcessIds_Throws()
        {
            // Arrange
            var duplicateId = Guid.NewGuid();
            var g1 = new CommandGroup { Name = "G1" };
            var p1 = new CommandProcess { Id = duplicateId, Name = "P1", Path = "C:\\p1.exe" };
            var p2 = new CommandProcess { Id = duplicateId, Name = "P2", Path = "C:\\p2.exe" };
            g1.Processes.Add(p1);
            g1.Processes.Add(p2);

            _mockConfigManager
                .Setup(m => m.SaveConfigurationAsync(It.IsAny<ProcessGroup[]>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(
                async () => await _repo.SaveAsync(new List<CommandGroup> { g1 }));
        }

        [Test]
        public void SaveAsync_CircularReference_Throws()
        {
            // Arrange
            var g = new CommandGroup { Name = "Self" };
            // create a direct circular reference
            g.Children.Add(g);
            var list = new List<CommandGroup> { g };

            _mockConfigManager
                .Setup(m => m.SaveConfigurationAsync(It.IsAny<ProcessGroup[]>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(async () => await _repo.SaveAsync(list));
        }

        [Test]
        public void SaveAsync_AllGroupsDuplicateIds_Throws()
        {
            // Arrange
            var duplicateId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var g1 = new CommandGroup { Id = duplicateId, Name = "G1" };
            var g2 = new CommandGroup { Id = duplicateId, Name = "G2" };
            var list = new List<CommandGroup> { g1, g2 };

            _mockConfigManager
                .Setup(m => m.SaveConfigurationAsync(It.IsAny<ProcessGroup[]>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(async () => await _repo.SaveAsync(list));
        }

        #endregion

        #region Mapping Tests

        [Test]
        public async Task LoadAsync_NestedGroups_MapsCorrectly()
        {
            // Arrange
            var childGroup = new ProcessGroup(
                "ChildGroup", "", true, Array.Empty<ProcessGroup>(),
                new ProcessLaunchInformation[0], false, false);

            var parentGroup = new ProcessGroup(
                "ParentGroup", "", true, new[] { childGroup },
                new ProcessLaunchInformation[0], false, false);

            _mockConfigManager
                .Setup(m => m.LoadConfigurationAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[] { parentGroup });

            // Act
            var groups = await _repo.LoadAsync();

            // Assert
            Assert.AreEqual(1, groups.Count);
            Assert.AreEqual("ParentGroup", groups[0].Name);
            Assert.AreEqual(1, groups[0].Children.Count);
            Assert.AreEqual("ChildGroup", groups[0].Children[0].Name);
        }

        [Test]
        public async Task SaveAsync_ConvertsBackToProcessGroups()
        {
            // Arrange
            var group = new CommandGroup { Name = "Group1" };
            var process = new CommandProcess
            {
                Name = "Process1",
                Path = "C:\\test.exe",
                Arguments = "arg1 arg2",
                Comment = "Test process",
                ExecutionMode = ExecutionMode.Default,
                Disabled = false,
                Hidden = false,
                WorkingDirectory = "C:\\"
            };
            group.Processes.Add(process);

            ProcessGroup[] savedProcessGroups = null;
            _mockConfigManager
                .Setup(m => m.SaveConfigurationAsync(It.IsAny<ProcessGroup[]>(), It.IsAny<CancellationToken>()))
                .Callback<ProcessGroup[], CancellationToken>((pg, ct) => savedProcessGroups = pg)
                .Returns(Task.CompletedTask);

            // Act
            await _repo.SaveAsync(new List<CommandGroup> { group });

            // Assert
            Assert.IsNotNull(savedProcessGroups);
            Assert.AreEqual(1, savedProcessGroups.Length);
            Assert.AreEqual("Group1", savedProcessGroups[0].DisplayName);
            var procArray = savedProcessGroups[0].Processes.ToArray();
            Assert.AreEqual(1, procArray.Length);
            Assert.AreEqual("Process1", procArray[0].DisplayName);
        }

        #endregion
    }
}
