using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.RenderTree;
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

        [Inject] private IUriHelper UriHelper { get; set; }
        [Inject] private INavigationInterception NavigationInterception { get; set; }
        [Inject] private IComponentContext ComponentContext { get; set; }
        [Parameter] public RenderFragment ChildContent { get; set; }
        [Parameter] public EventHandler<RouteMatchedEventArgs> OnMatch { get; set; }

        private readonly RouteTable routes = new RouteTable();
        private bool navigationInterceptionEnabled;
        private string location = "";
        private string baseUri = "";
        private RenderFragment currentFragment;
        private IDictionary<string, object> parameters;

        static readonly char[] queryOrHashStartChar = new[] { '?', '#' };
        private async void LocationChanged(object sender, LocationChangedEventArgs e)
        {
            this.location = e.Location;

            await SwitchContent(e.IsNavigationIntercepted);
        }

        private string StringUntilAny(string str, char[] chars)
        {
            var firstIndex = str.IndexOfAny(chars);
            return firstIndex < 0 ? str : str.Substring(0, firstIndex);
        }

        private Task SwitchContent(bool isNavigationIntercepted)
        {
            var path = UriHelper.ToBaseRelativePath(this.baseUri, this.location);
            path = "/" + StringUntilAny(path, queryOrHashStartChar);

            var context = new RouteContext(path);
            routes.Route(context);

            if (context.Fragment != null)
            {
                currentFragment = context.Fragment;
                parameters = context.Parameters;
                OnMatch?.Invoke(this, new RouteMatchedEventArgs(this.location, context.TemplateText, context.Parameters, context.Fragment));

                this.StateHasChanged();
            }
            else
            {
                if (isNavigationIntercepted)
                {
                    UriHelper.NavigateTo(this.location, forceLoad: true);
                }
            }

            return Task.CompletedTask;
        }

        protected override Task OnInitializedAsync()
        {
            this.baseUri = UriHelper.GetBaseUri();
            this.location = UriHelper.GetAbsoluteUri();
            UriHelper.OnLocationChanged += LocationChanged;
            return Task.CompletedTask;
        }

        public Task RegisterRoute(RenderFragment fragment, string template)
        {
            routes.Add(template, fragment);
            return Task.CompletedTask;
        }

        protected override async Task OnAfterRenderAsync()
        {
            if (!this.navigationInterceptionEnabled && ComponentContext.IsConnected)
            {
                this.navigationInterceptionEnabled = true;
                await SwitchContent(false);
                await NavigationInterception.EnableNavigationInterceptionAsync();
            }
        }

        public void Dispose()
        {
            UriHelper.OnLocationChanged -= LocationChanged;
        }
    }
}