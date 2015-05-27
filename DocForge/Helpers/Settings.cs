// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Settings.cs" company="Red Hammer Studios">
//   Copyright (c) 2015 Red Hammer Studios
// </copyright>
// <summary>
//   The settings file
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace DocForge.Helpers
{
    using System.IO;
    using System.Xml.Serialization;
    using ViewModel;

    /// <summary>
    /// The settings file
    /// </summary>
    public class Settings
    {
        /// <summary>
        /// Get or sets top class filter string
        /// </summary>
        public string TopClassFilterString { get; set; }

        /// <summary>
        /// Gets or sets the bottom class filter string.
        /// </summary>
        public string BottomClassFilterString { get; set; }

        /// <summary>
        /// Gets or sets the property include string
        /// </summary>
        public string PropertyIncludeFilterString { get; set; }

        /// <summary>
        /// Gets or sets the folder path.
        /// </summary>
        public string FolderPath { get; set; }

        /// <summary>
        /// Gets or sets the model name
        /// </summary>
        public string ModelName { get; set; }

        /// <summary>
        /// Gets or sets the model description
        /// </summary>
        public string ModelDescription { get; set; }

        /// <summary>
        /// Gets or sets the version
        /// </summary>
        public string ModelVersion { get; set; }

        /// <summary>
        /// Gets or sets the output path
        /// </summary>
        public string OutputPath { get; set; }

        /// <summary>
        /// Saves the settings of the viewmodel into a specified file
        /// </summary>
        /// <param name="vm">The <see cref="MainWindowViewModel"/> that needs properties saved.</param>
        /// <param name="path">The path to save to.</param>
        public void Save(MainWindowViewModel vm, string path)
        {
            this.BottomClassFilterString = vm.BottomClassFilterString;
            this.FolderPath = vm.FolderPath;
            this.ModelDescription = vm.ModelDescription;
            this.ModelName = vm.ModelName;
            this.ModelVersion = vm.ModelVersion;
            this.OutputPath = vm.OutputPath;
            this.PropertyIncludeFilterString = vm.PropertyIncludeFilterString;
            this.TopClassFilterString = vm.TopClassFilterString;

            var serializer = new XmlSerializer(typeof(Settings));
            using (TextWriter writer = new StreamWriter(path))
            {
                serializer.Serialize(writer, this);
            }
        }

        /// <summary>
        /// Loads properties from a given path to a specified <see cref="MainWindowViewModel"/>
        /// </summary>
        /// <param name="vm">The <see cref="MainWindowViewModel"/></param>
        /// <param name="path">The path from which to load properties</param>
        public static void Load(MainWindowViewModel vm, string path)
        {
            var deserializer = new XmlSerializer(typeof(Settings));
            TextReader reader = new StreamReader(path);
            object obj = deserializer.Deserialize(reader);
            var settings = (Settings)obj;
            reader.Close();

            vm.BottomClassFilterString = settings.BottomClassFilterString;
            vm.FolderPath = settings.FolderPath;
            vm.ModelDescription = settings.ModelDescription;
            vm.ModelName = settings.ModelName;
            vm.ModelVersion = settings.ModelVersion;
            vm.OutputPath = settings.OutputPath;
            vm.PropertyIncludeFilterString = settings.PropertyIncludeFilterString;
            vm.TopClassFilterString = settings.TopClassFilterString;
        }
    }
}