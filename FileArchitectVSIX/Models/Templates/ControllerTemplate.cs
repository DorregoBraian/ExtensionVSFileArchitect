using FileArchitectLibrary.Models.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace FileArchitectLibrary.Models.Templates
{
    public class ControllerTemplate : TemplateFile
    {
        public string ControllerName { get; set; } = string.Empty;
        public string BaseRoute { get; set; } = "api/[controller]";
        public List<ControllerAction> Actions { get; set; } = new();
        public List<string> Attributes { get; set; } = new()
        {
            "[ApiController]",
            "[Route(\"api/[controller]\")]"
        };
    }

    public class ControllerAction
    {
        public string Name { get; set; } = string.Empty;
        public HttpMethod Method { get; set; }
        public string Route { get; set; } = string.Empty;
        public string ReturnType { get; set; } = "IActionResult";
        public List<string> Attributes { get; set; } = new();
    }

    public enum HttpMethod
    {
        GET,
        POST,
        PUT,
        DELETE,
        PATCH
    }
}
