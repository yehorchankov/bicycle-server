using System;

namespace BiisControllers
{
    public class Label : IBiisControl
    {
        public string Name { get; set; }
        public string Style { get; set; }
        public string Id { get; set; }
        public string RawHtmlView { get; set; }
        public string HandlerFunction { get; set; }
        public string Value { get; set; }

        public Label(string innerHtml)
        {
            Value = innerHtml;
        }

        public string ToHtmlString()
        {
            return string.Format(
                "<span name=\"{0}\" id=\"{1}\" style=\"{2}\">{3}</span>",
                Name ?? "label", Id ?? Guid.NewGuid().ToString(), Style, Value);
        }
    }
}
