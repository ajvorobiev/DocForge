// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MainWindowViewModel.cs" company="Red Hammer Studios">
//   Copyright (c) 2015 Red Hammer Studios
// </copyright>
// <summary>
//   The main window view model.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace DocForge.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using ClassForge;
    using ClassForge.Model;
    using Helpers;
    using Microsoft.WindowsAPICodePack.Dialogs;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;
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

        /// <summary>
        /// The model name
        /// </summary>
        private string modelName;

        /// <summary>
        /// The model description.
        /// </summary>
        private string modelDescription;

        /// <summary>
        /// The model version.
        /// </summary>
        private string modelVersion;

        /// <summary>
        /// The output path.
        /// </summary>
        private string outputPath;

        /// <summary>
        /// The property filters.
        /// </summary>
        private string[] propertyFilters;

        /// <summary>
        /// The top level filters.
        /// </summary>
        private string[] topLevelFilters;

        /// <summary>
        /// The bottom level filters.
        /// </summary>
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
        public string PropertyIncludeFilterString
        {
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

        /// <summary>
        /// Gets or sets the model name.
        /// </summary>
        public string ModelName
        {
            get { return this.modelName; }
            set { this.RaiseAndSetIfChanged(ref this.modelName, value); }
        }

        /// <summary>
        /// Gets or sets the model description.
        /// </summary>
        public string ModelDescription
        {
            get { return this.modelDescription; }
            set { this.RaiseAndSetIfChanged(ref this.modelDescription, value); }
        }

        /// <summary>
        /// Gets or sets the model version.
        /// </summary>
        public string ModelVersion
        {
            get { return this.modelVersion; }
            set { this.RaiseAndSetIfChanged(ref this.modelVersion, value); }
        }

        /// <summary>
        /// Gets or sets the output path.
        /// </summary>
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

        /// <summary>
        /// Gets the browse output command.
        /// </summary>
        public ReactiveCommand<object> BrowseOutputCommand { get; private set; }

        /// <summary>
        /// Gets the save command.
        /// </summary>
        public ReactiveCommand<object> SaveCommand { get; private set; }

        /// <summary>
        /// Gets the load command.
        /// </summary>
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
        /// Gets the generate command.
        /// </summary>
        public ReactiveCommand<object> GenerateJsonCommand { get; private set; }

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

            this.GenerateJsonCommand = ReactiveCommand.Create();
            this.GenerateJsonCommand.Subscribe(_ => this.GenerateJsonCommandExecute());

            this.BrowseOutputCommand = ReactiveCommand.Create();
            this.BrowseOutputCommand.Subscribe(_ => this.BrowseOutputCommandExecute());

            // initialize commands
            this.SetProperties();
        }

        /// <summary>
        /// Execute the load command
        /// </summary>
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

        /// <summary>
        /// Execute the save command
        /// </summary>
        private void SaveCommandExecute()
        {
            var dlg = new CommonSaveFileDialog { DefaultFileName = "DocForgeSettings1.xml" };

            if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
            {
                var settings = new Settings();
                settings.Save(this, dlg.FileName);
            }
        }

        /// <summary>
        /// Execute the browse for output folder command
        /// </summary>
        private void BrowseOutputCommandExecute()
        {
            var dlg = new CommonOpenFileDialog { IsFolderPicker = true };

            if (dlg.ShowDialog() == CommonFileDialogResult.Ok && Directory.Exists(dlg.FileName))
            {
                this.OutputPath = dlg.FileName;
            }
        }

        /// <summary>
        /// Execute the generate command
        /// </summary>
        private void GenerateCommandExecute()
        {
            if (this.FilteredModel == null || this.FilteredModel.Count == 0 || string.IsNullOrWhiteSpace(this.OutputPath)) return;

            var docGen = new HtmlGenerator();

            docGen.GenerateDocumentation(this.FilteredModel[0], this.OutputPath, this.ModelName, this.ModelVersion, this.ModelDescription, this.topLevelFilters, this.bottomLevelFilters, this.propertyFilters);
        }

        /// <summary>
        /// Execute the generate command
        /// </summary>
        private void GenerateJsonCommandExecute()
        {
            if (this.FilteredModel == null || this.FilteredModel.Count == 0 || string.IsNullOrWhiteSpace(this.OutputPath)) return;
            var parser = new CfgSimpleParser();
            var dm = parser.GetDereferencedModel(this.FilteredModel[0]);

            var header = new DereferencedHeader
            {
                BottomLevelFilters = string.Join(", ", this.bottomLevelFilters),
                Description = this.ModelDescription,
                Name = this.ModelName,
                PropertyFilters = string.Join(", ", this.propertyFilters),
                TopLevelFilters = string.Join(", ", this.topLevelFilters),
                Version = this.ModelVersion
            };

            var exportStruct = new DereferencedStructure
            {
                Header = header,
                Model = dm
            };

            const string savepath = "output.json";

            if (!Directory.Exists(this.outputPath))
            {
                Directory.CreateDirectory(this.outputPath);
            }

            File.WriteAllText(this.outputPath + "\\" + savepath, JsonConvert.SerializeObject(exportStruct, Formatting.Indented, new JsonSerializerSettings() { ContractResolver = new CamelCasePropertyNamesContractResolver()}));
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

            for (int index = 0; index < this.topLevelFilters.Length; index++)
            {
                var topLevelFilter = this.topLevelFilters[index];
                this.topLevelFilters[index] = topLevelFilter.Trim();
            }

            if (this.FullModel.Count != 0)
            {
                var fModel = new Model { Name = this.FullModel[0].Name };
                foreach (var cl in this.FullModel[0].Classes)
                {
                    if (this.topLevelFilters.Contains(cl.Name))
                    {
                        fModel.Classes.Add(cl.Copy());
                    }
                }

                foreach (Class t in fModel.Classes)
                {
                    var nCl = t;
                    this.FilterClasses(ref nCl);
                    this.FilterProperties(ref nCl);
                }

                fModel.UpdateReferences();

                this.FilteredModel.Add(fModel);
            }
        }

        /// <summary>
        /// Filter the properties
        /// </summary>
        /// <param name="nCl">The class to filter properties in</param>
        private void FilterProperties(ref Class nCl)
        {
            var propertyClone = new List<Property>(nCl.Properties);

            foreach (var property in propertyClone)
            {
                if (!this.propertyFilters.Contains(property.Name))
                {
                    nCl.Properties.RemoveAll(c => c.Name == property.Name);
                }
            }

            foreach (var cl in nCl.Classes)
            {
                var c = cl;
                this.FilterProperties(ref c);
            }
        }

        /// <summary>
        /// Filters the classes
        /// </summary>
        /// <param name="classToFilter">The class to filter</param>
        private void FilterClasses(ref Class classToFilter)
        {
            var classesClone = new List<Class>(classToFilter.Classes);

            classToFilter.InheritanceChildren = classToFilter.InheritanceChildren.DistinctBy(c => c.Name).ToList();

            foreach (Class t in classesClone)
            {
                if (this.bottomLevelFilters.Contains(t.Name))
                {
                    classToFilter.Classes.Remove(classToFilter.Classes.First(c => c.Name == t.Name));
                }
                else
                {
                    if (t.Classes.Count > 0)
                    {
                        var foundClass = classToFilter.Classes.First(c => c.Name == t.Name);
                        this.FilterClasses(ref foundClass);
                    }
                }
            }
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