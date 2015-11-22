using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace BiisControllers
{
    /// <summary>
    /// Basis of typical BIIS Controls
    /// </summary>
    public interface IBiisControl
    {
        /// <summary>
        /// Forms html representation of control
        /// </summary>
        /// <returns>Html view</returns>
        string ToHtmlString();
        string Name { get; set; }
        string Style { get; set; }
        string Id { get; set; }
        string RawHtmlView { get; set; }
        string HandlerFunction { get; set; }
        string Value { get; set; }
    }
}
