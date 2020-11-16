using System.Collections.Generic;
using Microsoft.AspNetCore.Components;

namespace BlazorRouter
{
    internal class RouteTable
    {
        private readonly Dictionary<string, RouteEntry> routes = new ();

        public void Add(string id, string templateText, RenderFragment fragment)
        {
            if (routes.ContainsKey(id)) return;
            var template = TemplateParser.ParseTemplate(templateText);
            var entry = new RouteEntry(template, fragment);
            routes[id] = entry;
        }

        public void Remove(string id)
        {
            routes.Remove(id);
        }

        internal void Route(RouteContext routeContext)
        {
            foreach (var route in routes)
            {
                route.Value.Match(routeContext);
                if (routeContext.Fragment != null)
                {
                    routeContext.Id = route.Key;
                    return;
                }
            }
        }
    }
}
