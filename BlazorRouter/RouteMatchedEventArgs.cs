using Microsoft.AspNetCore.Components;
using System.Collections.Generic;

namespace BlazorRouter
{
    public readonly struct RouteMatchedEventArgs
    {
        public string Location { get; }

        public string Template { get; }

        public IDictionary<string, object> Parameters { get; }

        public RenderFragment Fragment { get; }

        public RouteMatchedEventArgs(string location, string template, IDictionary<string, object> parameters, RenderFragment fragment)
        {
            Location = location;
            Template = template;
            Parameters = parameters;
            Fragment = fragment;
        }
    }
}
