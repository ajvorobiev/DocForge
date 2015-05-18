// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MainWindowViewModel.cs" company="Red Hammer Studios">
//   Copyright (c) 2015 Red Hammer Studios
// </copyright>
// <summary>
//   The main window view model.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.IO;
using System.Reflection;
using ClassForge.Model;

namespace DocForge.ViewModel
{
    using ReactiveUI;

    /// <summary>
    /// The main window view model.
    /// </summary>
    public class MainWindowViewModel : ReactiveObject
    {
        private string folderPath;

        private Model fullModel;

        private Model filteredModel;

        public string FolderPath
        {
            get { return this.folderPath; }
            set { this.RaiseAndSetIfChanged(ref this.folderPath, value); }
        }

        public Model FullModel
        {
            get { return this.fullModel; }
            set { this.RaiseAndSetIfChanged(ref this.fullModel, value); }
        }

        public Model FilteredModel
        {
            get { return this.filteredModel; }
            set { this.RaiseAndSetIfChanged(ref this.filteredModel, value); }
        }

        public ReactiveCommand<object> BrowseFolderCommand { get; private set; }

        public ReactiveCommand<object> ParseCommand { get; private set; }

        public MainWindowViewModel()
        {
            // initialize commands
            this.SetProperties();
        }

        private void SetProperties()
        {
#if DEBUG
            this.folderPath = Directory.GetCurrentDirectory() + "\\mergetest";
#endif
        }

        public void BrowseFolderCommandExecute()
        {
            // Do a folder picker and assign to the textbox
        }

        public void ParseCommandExecute()
        {
            // Call on ClassForge
        }
    }
}
