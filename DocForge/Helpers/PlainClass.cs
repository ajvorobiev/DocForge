// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PlainClass.cs" company="Red Hammer Studios">
//   Copyright (c) 2015 Red Hammer Studios
// </copyright>
// <summary>
//   Defines the PlainClass type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace DocForge.Helpers
{
    using System.Collections.Generic;

    /// <summary>
    /// The plain class. Purely for serialization.
    /// </summary>
    public class PlainClass
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// Gets or sets the children.
        /// </summary>
        public List<PlainClass> children { get; set; }
    }
}
