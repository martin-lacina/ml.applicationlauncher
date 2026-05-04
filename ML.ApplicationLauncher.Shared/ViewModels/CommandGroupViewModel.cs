using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ML.ApplicationLauncher.Source.Model;

namespace ML.ApplicationLauncher.Shared.ViewModels
{
    public class CommandGroupViewModel : ValidatableViewModelBase
    {
        private Guid _id = Guid.NewGuid();
        public Guid Id { get => _id; set => SetProperty(ref _id, value); }

        private string _name = string.Empty;
        public string Name { get => _name; set { SetProperty(ref _name, value); ValidateProperty(nameof(Name), value); } }

        public ObservableCollection<CommandGroupViewModel> Children { get; } = new();
        public ObservableCollection<CommandProcessViewModel> Processes { get; } = new();
    }
}

