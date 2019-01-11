using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoEntities
{
    /// <summary>
    /// Attribute for every gameObject template. Defines the name of this template
    /// </summary>
    public sealed class EntityTemplateAttribute : Attribute
    {
        /// <summary>
        /// Gets the name of template
        /// </summary>
        public string Name { get; set; }
    }
}
