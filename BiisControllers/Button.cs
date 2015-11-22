using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BiisControllers
{
    public class Button : IBiisControl
    {
        public string Name { get; set; }
        public string Style { get; set; }
        public string Id { get; set; }
        public string RawHtmlView { get; set; }
        public string HandlerFunction { get; set; }
        public string Value { get; set; }

        public Button(string value)
        {
            Value = value;
        }
        
        public string ToHtmlString()
        {
            return string.Format("<input type=\"submit\" name=\"{0}\" id=\"{1}\" style=\"{2}\" value=\"{3}\" />",
                Name ?? "button", Id ?? Guid.NewGuid().ToString(), Style, Value);
        }

    }
}
