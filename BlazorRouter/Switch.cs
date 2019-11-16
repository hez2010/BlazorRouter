using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Routing;

namespace BlazorRouter
{
    public class Switch : ComponentBase, IDisposable
    {
        private int CreateCascadingValue<T>(RenderTreeBuilder builder, int seq, T value, string name, RenderFragment child)
        {
            builder.OpenComponent<CascadingValue<T>>(seq++);
            builder.AddAttribute(seq++, "Value", value);
            builder.AddAttribute(seq++, "Name", name);
            builder.AddAttribute(seq++, "ChildContent", child);
            builder.CloseComponent();
            return seq;
        }
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            var seq = 0;

            seq = CreateCascadingValue(builder, seq, this, "SwitchInstance", ChildContent);
            CreateCascadingValue(builder, seq, parameters, "RouteParameters", currentFragment);
        }

        [Inject] private NavigationManager NaviManager { get; set; }
        [Inject] private INavigationInterception NavigationInterception { get; set; }
        [Parameter] public RenderFragment ChildContent { get; set; }
        [Parameter] public Assembly Assembly { get; set; }
        [Parameter] public EventHandler<RouteMatchedEventArgs> OnMatch { get; set; }

        private readonly RouteTable routes = new RouteTable();
        private string location = "";
        private RenderFragment currentFragment;
        private IDictionary<string, object> parameters;

        static readonly char[] queryOrHashStartChar = { '?', '#' };
        private async void LocationChanged(object sender, LocationChangedEventArgs e)
        {
            location = e.Location;
            await SwitchContent(e.IsNavigationIntercepted);
        }

        private string StringUntilAny(string str, char[] chars)
        {
            var firstIndex = str.IndexOfAny(chars);
            return firstIndex < 0 ? str : str.Substring(0, firstIndex);
        }

        private Task SwitchContent(bool isNavigationIntercepted)
        {
            var path = NaviManager.ToBaseRelativePath(NaviManager.Uri);
            path = "/" + StringUntilAny(path, queryOrHashStartChar);

            var context = new RouteContext(path);
            routes.Route(context);

            if (context.Fragment != null)
            {
                currentFragment = context.Fragment;
                parameters = context.Parameters;
                OnMatch?.Invoke(this, new RouteMatchedEventArgs(location, context.TemplateText, context.Parameters, context.Fragment));

                StateHasChanged();
            }
            else
            {
                if (isNavigationIntercepted)
                {
                    NaviManager.NavigateTo(location, forceLoad: true);
                }
            }

            return Task.CompletedTask;
        }

        protected override Task OnInitializedAsync()
        {
            location = NaviManager.Uri;
            NaviManager.LocationChanged += LocationChanged;

            if (Assembly != null)
            {
                // TODO
            }
            return Task.CompletedTask;
        }

        public Task RegisterRoute(RenderFragment fragment, string template)
        {
            routes.Add(template, fragment);
            return Task.CompletedTask;
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await SwitchContent(false);
                await NavigationInterception.EnableNavigationInterceptionAsync();
            }
        }

        public void Dispose()
        {
            NaviManager.LocationChanged -= LocationChanged;
        }
    }
}