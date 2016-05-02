namespace DocForge.Helpers
{
    using ClassForge.Model;

    /// <summary>
    /// Holds the full dereferenced model necessary for export.
    /// </summary>
    public class DereferencedStructure
    {
        /// <summary>
        /// Gets or sets the header.
        /// </summary>
        public DereferencedHeader Header { get; set; }

        /// <summary>
        /// Gets or sets the model.
        /// </summary>
        public DereferencedModel Model { get; set; }
    }
}
