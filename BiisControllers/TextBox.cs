using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BiisControllers
{
    public class TextBox : IBiisControl
    {
        public string Name { get; set; }
        public string Style { get; set; }
        public string Id { get; set; }
        public string RawHtmlView { get; set; }
        public string HandlerFunction { get; set; }
        public string Value { get; set; }

        public TextBox(string value)
        {
            Value = value;
        }

        public string ToHtmlString()
        {
            return string.Format("<input type=\"text\" name=\"{0}\" id=\"{1}\" style=\"{2}\"/>",
                Name ?? "text", Id ?? Guid.NewGuid().ToString(), Style);
        }
    }
}
