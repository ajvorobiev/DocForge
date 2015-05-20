// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MainWindowViewModel.cs" company="Red Hammer Studios">
//   Copyright (c) 2015 Red Hammer Studios
// </copyright>
// <summary>
//   The main window view model.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace DocForge.ViewModel
{
    using System;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using ClassForge;
    using ClassForge.Model;
    using ReactiveUI;

    /// <summary>
    /// The main window view model.
    /// </summary>
    public class MainWindowViewModel : ReactiveObject
    {
        /// <summary>
        /// The folder path.
        /// </summary>
        private string folderPath;

        /// <summary>
        /// The full model.
        /// </summary>
        private ObservableCollection<Model> fullModel;

        /// <summary>
        /// The filtered model.
        /// </summary>
        private ObservableCollection<Model> filteredModel;

        /// <summary>
        /// The top class level inclusion string
        /// </summary>
        private string topClassFilterString;

        /// <summary>
        /// The bottom class level inclusion string
        /// </summary>
        private string bottomClassFilterString;

        /// <summary>
        /// Gets or sets the top class filter string.
        /// </summary>
        public string TopClassFilterString
        {
            get { return this.topClassFilterString; }
            set { this.RaiseAndSetIfChanged(ref this.topClassFilterString, value); }
        }

        /// <summary>
        /// Gets or sets the bottom class filter string.
        /// </summary>
        public string BottomClassFilterString
        {
            get { return this.bottomClassFilterString; }
            set { this.RaiseAndSetIfChanged(ref this.bottomClassFilterString, value); }
        }

        /// <summary>
        /// Gets or sets the folder path.
        /// </summary>
        public string FolderPath
        {
            get { return this.folderPath; }
            set { this.RaiseAndSetIfChanged(ref this.folderPath, value); }
        }

        /// <summary>
        /// Gets or sets the full model.
        /// </summary>
        public ObservableCollection<Model> FullModel
        {
            get { return this.fullModel; }
            set { this.RaiseAndSetIfChanged(ref this.fullModel, value); }
        }

        /// <summary>
        /// Gets or sets the filtered model.
        /// </summary>
        public ObservableCollection<Model> FilteredModel
        {
            get { return this.filteredModel; }
            set { this.RaiseAndSetIfChanged(ref this.filteredModel, value); }
        }

        /// <summary>
        /// Gets the browse folder command.
        /// </summary>
        public ReactiveCommand<object> BrowseFolderCommand { get; private set; }

        /// <summary>
        /// Gets the parse command.
        /// </summary>
        public ReactiveCommand<object> ParseCommand { get; private set; }

        /// <summary>
        /// Gets the filter command.
        /// </summary>
        public ReactiveCommand<object> FilterCommand { get; private set; } 

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindowViewModel"/> class.
        /// </summary>
        public MainWindowViewModel()
        {
            this.FullModel = new ObservableCollection<Model>();
            this.FilteredModel = new ObservableCollection<Model>();

            // intialize commands
            var canParse = this.WhenAny(vm => vm.FolderPath, fp => !string.IsNullOrWhiteSpace(fp.Value));
            this.ParseCommand = ReactiveCommand.Create(canParse);
            this.ParseCommand.Subscribe(_ => this.ParseCommandExecute());

            this.BrowseFolderCommand = ReactiveCommand.Create();
            this.BrowseFolderCommand.Subscribe(_ => this.BrowseFolderCommandExecute());

            this.FilterCommand = ReactiveCommand.Create();
            this.FilterCommand.Subscribe(_ => this.FilterCommandExecute());

            // initialize commands
            this.SetProperties();
        }

        /// <summary>
        /// Execute the filter command.
        /// </summary>
        private void FilterCommandExecute()
        {
            this.FilteredModel = new ObservableCollection<Model>();

            var topLevelFilters = this.TopClassFilterString.Split(new[] { ',' });
            var bottomLevelFilters = this.BottomClassFilterString.Split(new[] { ',' });

            for (int index = 0; index < topLevelFilters.Length; index++)
            {
                var topLevelFilter = topLevelFilters[index];
                topLevelFilters[index] = topLevelFilter.Trim();
            }

            if (this.FullModel.Count != 0)
            {
                var fModel = new Model();

                foreach (var cl in this.FullModel[0].Classes)
                {
                    if (topLevelFilters.Contains(cl.Name))
                    {
                        fModel.Classes.Add(cl.Copy());
                    }
                }

                foreach (Class t in fModel.Classes)
                {
                    var nCl = t;
                    this.FilterClasses(ref nCl, bottomLevelFilters);
                }

                this.FilteredModel.Add(fModel);
            }
        }

        private void FilterClasses(ref Class classToFilter, string[] bottomLevelFilters)
        {
            var classesClone = new List<Class>(classToFilter.Classes);

            foreach (Class t in classesClone)
            {
                if (bottomLevelFilters.Contains(t.Name))
                {
                    classToFilter.Classes.Remove(classToFilter.Classes.First(c => c.Name == t.Name));
                }
                else
                {
                    if (t.Classes.Count > 0)
                    {
                        var foundClass = classToFilter.Classes.First(c => c.Name == t.Name);
                        this.FilterClasses(ref foundClass, bottomLevelFilters); 
                    }
                }
            }

            //classToFilter.Classes = classesClone;
        }

        /// <summary>
        /// Set the public properties.
        /// </summary>
        private void SetProperties()
        {
#if DEBUG
            this.FolderPath = Directory.GetCurrentDirectory() + "\\mergetest";
            this.TopClassFilterString = "CfgPatches, CfgVehicles, CfgAmmo, CfgMagazines, CfgWeapons, CfgGroups, CfgVehicleClasses, CfgFactions";
            this.BottomClassFilterString = "ViewPilot,OpticsIn,CargoLight,HitPoints,Sounds,SpeechVariants,textureSources,AnimationSources,UserActions,Damage,Exhausts,Reflectors,ViewOptics,Library,EventHandlers";
#endif
        }

        /// <summary>
        /// Executes the browse folder command
        /// </summary>
        public void BrowseFolderCommandExecute()
        {
            // Do a folder picker and assign to the textbox
        }

        /// <summary>
        /// Executes the parse command
        /// </summary>
        public void ParseCommandExecute()
        {
            this.FullModel = new ObservableCollection<Model>();

            // Call on ClassForge
            var parser = new CfgSimpleParser();
            this.FullModel.Add(parser.ParseDirectoryMerged(this.FolderPath));
        }
    }
}
