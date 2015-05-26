// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MainWindowViewModel.cs" company="Red Hammer Studios">
//   Copyright (c) 2015 Red Hammer Studios
// </copyright>
// <summary>
//   The main window view model.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Windows.Forms;
using DocForge.Helpers;
using Microsoft.WindowsAPICodePack.Dialogs;

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
        /// The property include string
        /// </summary>
        private string propertyIncludeFilterString;

        private string modelName;
        private string modelDescription;
        private string modelVersion;
        private string outputPath;
        private string[] propertyFilters;
        private string[] topLevelFilters;
        private string[] bottomLevelFilters;

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
        /// Gets or sets the property include string
        /// </summary>
        public string PropertyIncludeFilterString {
            get { return this.propertyIncludeFilterString; }
            set { this.RaiseAndSetIfChanged(ref this.propertyIncludeFilterString, value); }
        }

        /// <summary>
        /// Gets or sets the folder path.
        /// </summary>
        public string FolderPath
        {
            get { return this.folderPath; }
            set { this.RaiseAndSetIfChanged(ref this.folderPath, value); }
        }

        public string ModelName
        {
            get { return this.modelName; }
            set { this.RaiseAndSetIfChanged(ref this.modelName, value); }
        }

        public string ModelDescription
        {
            get { return this.modelDescription; }
            set { this.RaiseAndSetIfChanged(ref this.modelDescription, value); }
        }

        public string ModelVersion
        {
            get { return this.modelVersion; }
            set { this.RaiseAndSetIfChanged(ref this.modelVersion, value); }
        }

        public string OutputPath
        {
            get { return this.outputPath; }
            set { this.RaiseAndSetIfChanged(ref this.outputPath, value); }
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

        public ReactiveCommand<object> BrowseOutputCommand { get; private set; }

        public ReactiveCommand<object> SaveCommand { get; private set; }

        public ReactiveCommand<object> LoadCommand { get; private set; }

        /// <summary>
        /// Gets the parse command.
        /// </summary>
        public ReactiveCommand<object> ParseCommand { get; private set; }

        /// <summary>
        /// Gets the filter command.
        /// </summary>
        public ReactiveCommand<object> FilterCommand { get; private set; }

        /// <summary>
        /// Gets the generate command.
        /// </summary>
        public ReactiveCommand<object> GenerateCommand { get; private set; } 

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

            this.SaveCommand = ReactiveCommand.Create();
            this.SaveCommand.Subscribe(_ => this.SaveCommandExecute());

            this.LoadCommand = ReactiveCommand.Create();
            this.LoadCommand.Subscribe(_ => this.LoadCommandExecute());

            this.GenerateCommand = ReactiveCommand.Create();
            this.GenerateCommand.Subscribe(_ => this.GenerateCommandExecute());

            this.BrowseOutputCommand = ReactiveCommand.Create();
            this.BrowseOutputCommand.Subscribe(_ => this.BrowseOutputCommandExecute());

            // initialize commands
            this.SetProperties();
        }

        private void LoadCommandExecute()
        {
            var dlg = new CommonOpenFileDialog();
            dlg.Filters.Add(new CommonFileDialogFilter("XML file", "*.xml"));
            dlg.DefaultFileName = "DocForgeSettings1.xml";

            if (dlg.ShowDialog() == CommonFileDialogResult.Ok && File.Exists(dlg.FileName))
            {
                Settings.Load(this, dlg.FileName);
            }
        }

        private void SaveCommandExecute()
        {
            var dlg = new CommonSaveFileDialog { DefaultFileName = "DocForgeSettings1.xml" };

            if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
            {
                var settings = new Settings();
                settings.Save(this, dlg.FileName);
            }
        }

        private void BrowseOutputCommandExecute()
        {
            var dlg = new CommonOpenFileDialog { IsFolderPicker = true };

            if (dlg.ShowDialog() == CommonFileDialogResult.Ok && Directory.Exists(dlg.FileName))
            {
                this.OutputPath = dlg.FileName;
            }
        }

        private void GenerateCommandExecute()
        {
            if(this.FilteredModel == null || this.FilteredModel.Count == 0 || string.IsNullOrWhiteSpace(this.OutputPath)) return;

            var docGen = new HtmlGenerator();

            docGen.GenerateDocumentation(this.FilteredModel[0], this.OutputPath, this.ModelName, this.ModelVersion, this.ModelDescription, this.topLevelFilters, this.bottomLevelFilters, this.propertyFilters);
        }

        /// <summary>
        /// Execute the filter command.
        /// </summary>
        private void FilterCommandExecute()
        {
            if (this.FullModel == null || this.FullModel.Count == 0) return;

            this.FilteredModel = new ObservableCollection<Model>();

            this.topLevelFilters = this.TopClassFilterString.Split(new[] { ',' });
            this.bottomLevelFilters = this.BottomClassFilterString.Split(new[] { ',' });
            this.propertyFilters = this.PropertyIncludeFilterString.Split(new[] { ',' });

            for (int index = 0; index < topLevelFilters.Length; index++)
            {
                var topLevelFilter = topLevelFilters[index];
                topLevelFilters[index] = topLevelFilter.Trim();
            }

            if (this.FullModel.Count != 0)
            {
                var fModel = new Model() { Name = this.FullModel[0].Name };

                foreach (var cl in this.FullModel[0].Classes)
                {
                    if (topLevelFilters.Contains(cl.Name))
                    {
                        //cl.InheritanceChildren = cl.Classes.DistinctBy(c => c.Name).ToList();

                        fModel.Classes.Add(cl.Copy());
                    }
                }

                foreach (Class t in fModel.Classes)
                {
                    var nCl = t;
                    this.FilterClasses(ref nCl, bottomLevelFilters, propertyFilters);
                    this.FilterProperties(ref nCl, propertyFilters);
                }

                fModel.UpdateReferences();

                this.FilteredModel.Add(fModel);
            }
        }

        private void FilterProperties(ref Class nCl, string[] propertyFilters)
        {
            var propertyClone = new List<Property>(nCl.Properties);
            
            foreach (var property in propertyClone)
            {
                if (!propertyFilters.Contains(property.Name))
                {
                    nCl.Properties.RemoveAll(c => c.Name == property.Name);
                }
            }

            foreach (var cl in nCl.Classes)
            {
                var c = cl;
                this.FilterProperties(ref c, propertyFilters);
            }
        }

        /// <summary>
        /// Filters the classes
        /// </summary>
        /// <param name="classToFilter">The class to filter</param>
        /// <param name="bottomLevelFilters"></param>
        /// <param name="propertyFilters"></param>
        private void FilterClasses(ref Class classToFilter, string[] bottomLevelFilters, string[] propertyFilters)
        {
            var classesClone = new List<Class>(classToFilter.Classes);

            classToFilter.InheritanceChildren = classToFilter.InheritanceChildren.DistinctBy(c => c.Name).ToList();

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
                        this.FilterClasses(ref foundClass, bottomLevelFilters, propertyFilters); 
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
            this.TopClassFilterString = "CfgVehicles, CfgAmmo, CfgMagazines, CfgWeapons, CfgGroups, CfgVehicleClasses, CfgFactionClasses";
            this.PropertyIncludeFilterString = "scope,magazines[],weapons[],enginePower,maxOmega,peakTorque,fuelCapacity,canFloat,maxFordingDepth,idleRPM,redRPM,maxSpeed,GearboxRatios,differentialType,maxBrakeTorque,maxHandBrakeTorque,compatibleItems[],hit,indirectHit,indirectHitRange,defaultMagazine,cost,ais_ce_penetrators[],ammo,muzzles[],displayName,inertia,vehicleClass,crew,faction,hiddenSelections[],hiddenSelectionsTextures[],torqueCurve[]";
            this.ModelName = "SuperAwesomeStudio";
            this.ModelDescription = "MyProject";
            this.ModelVersion = "0.3.8";
            this.OutputPath = "";
            this.BottomClassFilterString = "Wheels,complexGearbox,ViewPilot,OpticsIn,CargoLight,HitPoints,Sounds,SpeechVariants,textureSources,AnimationSources,UserActions,Damage,Exhausts,Reflectors,ViewOptics,Library,EventHandlers,gunParticles,manual,close,short,medium,far,CamShakeExplode,CamShakeHit,CamShakeFire,CamShakePlayerFire,GunParticles,Single,FullAuto,single_medium_optics1,single_far_optics2,fullauto_medium,GP25Muzzle,Wounds,UniformInfo,RenderTargets,DestructionEffects,MFD,markerlights,MarkerLights,WingVortices,RotorLibHelicopterProperties,Viewoptics,Arguments,muzzle_rot1,HitEffect,Double,Volley,AIDouble,AIVolley,StandardSound,player,HE,AP,LowROFBMD2,HighROFBMD2,closeBMD2,shortBMD2,mediumBMD2,farBMD2,Single1,Single2,Single3,Burst1,Burst2,Burst3,gunClouds,Far_AI,Medium_AI,Close_AI,Burst,ItemInfo,Close,M1,M1a,M2,M3,M4,M5,M6,M7,M8,M9,M10,M11,BaseSoundModeType,OpticsModes,PutMuzzle,Rhs_Mine_Muzzle,ThrowMuzzle,Rhs_Throw_Grenade,Rhs_Throw_Smoke,Rhs_Throw_Flare,Rhs_Throw_Flash";
#endif
        }

        /// <summary>
        /// Executes the browse folder command
        /// </summary>
        public void BrowseFolderCommandExecute()
        {
            var dlg = new CommonOpenFileDialog { IsFolderPicker = true };

            if (dlg.ShowDialog() == CommonFileDialogResult.Ok && Directory.Exists(dlg.FileName))
            {
                this.FolderPath = dlg.FileName;
            }
        }

        /// <summary>
        /// Executes the parse command
        /// </summary>
        public void ParseCommandExecute()
        {
            this.FullModel = new ObservableCollection<Model>();

            // Call on ClassForge
            var parser = new CfgSimpleParser();
            var model = parser.ParseDirectoryMerged(this.FolderPath);
            model.Name = this.FolderPath;
            this.FullModel.Add(model);
        }
    }
}
