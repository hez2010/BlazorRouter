﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Routing;

namespace BlazorRouter
{
    public partial class Switch : ComponentBase, IDisposable
    {
        private static int CreateCascadingValue<TValue>(RenderTreeBuilder builder, int seq, string name, TValue value, RenderFragment child)
        {
            builder.OpenComponent<CascadingValue<TValue>>(seq++);
            builder.AddAttribute(seq++, "Name", name);
            builder.AddAttribute(seq++, "Value", value);
            builder.AddAttribute(seq++, "ChildContent", (RenderFragment)(builder2 => builder2.AddContent(seq++, child)));
            builder.CloseComponent();
            return seq;
        }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            base.BuildRenderTree(builder);
            var seq = 0;
            seq = CreateCascadingValue(builder, seq, "SwitchInstance", this, ChildContent);
            _ = CreateCascadingValue(builder, seq, "RouteParameters", parameters, currentFragment);
        }

        [Parameter] public RenderFragment ChildContent { get; set; }
        [Parameter] public EventHandler<RouteMatchedEventArgs> OnMatch { get; set; }

        private readonly RouteTable routes = new RouteTable();
        private string location = "";
        private RenderFragment currentFragment;
        private IDictionary<string, object> parameters;
        private bool firstMatched;
        private RouteContext context;

        static readonly char[] queryOrHashStartChar = { '?', '#' };
        private void LocationChanged(object sender, LocationChangedEventArgs e)
        {
            location = e.Location;
            UpdateRouteContext();
            SwitchContent();
        }

        private void UpdateRouteContext()
        {
            var path = NaviManager.ToBaseRelativePath(NaviManager.Uri);
            path = "/" + StringUntilAny(path, queryOrHashStartChar);
            context = new RouteContext(path);
        }

        private string StringUntilAny(string str, char[] chars)
        {
            var firstIndex = str.IndexOfAny(chars);
            return firstIndex < 0 ? str : str.Substring(0, firstIndex);
        }

        private bool SwitchContent(string id = null, RouteEntry entry = null)
        {
            if (id == null || entry == null)
            {
                routes.Route(context);
            }
            else
            {
                routes.Route(context, id, entry);
            }

            if (context.Fragment != null)
            {
                currentFragment = context.Fragment;
                parameters = context.Parameters;
                OnMatch?.Invoke(this, new RouteMatchedEventArgs(location, context.TemplateText, parameters, context.Fragment));
                StateHasChanged();

                return true;
            }

            return false;
        }

        protected override void OnInitialized()
        {
            location = NaviManager.Uri;
            NaviManager.LocationChanged += LocationChanged;
            UpdateRouteContext();

            base.OnInitialized();
        }

        public void RegisterRoute(string id, RenderFragment fragment, string template)
        {
            var entry = routes.Add(id, template, fragment);
            if (!firstMatched)
            {
                firstMatched = SwitchContent(id, entry);
            }
        }

        public void UnregisterRoute(string id)
        {
            routes.Remove(id);
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await NavigationInterception.EnableNavigationInterceptionAsync();
            }
        }

        public void Dispose()
        {
            NaviManager.LocationChanged -= LocationChanged;
        }

        [Inject] private INavigationInterception NavigationInterception { get; set; }
        [Inject] private NavigationManager NaviManager { get; set; }
    }
}
