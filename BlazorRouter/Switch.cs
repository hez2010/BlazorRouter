using System;
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
        [Parameter] public EventCallback<RouteMatchedEventArgs> OnMatch { get; set; }

        private readonly RouteTable routes = new RouteTable();
        private string location = "";
        private RenderFragment currentFragment;
        private IDictionary<string, object> parameters;
        private bool firstMatched;
        private bool disposed;
        private RouteContext context;

        static readonly char[] queryOrHashStartChar = { '?', '#' };
        private async void LocationChanged(object sender, LocationChangedEventArgs e)
        {
            try
            {
                await InvokeAsync(async () =>
                {
                    if (disposed) return;

                    location = e.Location;
                    UpdateRouteContext();
                    await SwitchContentAsync();
                });
            }
            catch (Exception exception)
            {
                await DispatchExceptionIfNotDisposedAsync(exception);
            }
        }

        private Task DispatchExceptionIfNotDisposedAsync(Exception exception)
        {
            // Check on the dispatcher so disposal cannot interleave with exception dispatch.
            return InvokeAsync(() => disposed ? Task.CompletedTask : DispatchExceptionAsync(exception));
        }

        private void UpdateRouteContext()
        {
            var path = NaviManager.ToBaseRelativePath(location);
            path = "/" + StringUntilAny(path, queryOrHashStartChar);
            context = new RouteContext(path);
        }

        private string StringUntilAny(string str, char[] chars)
        {
            var firstIndex = str.IndexOfAny(chars);
            return firstIndex < 0 ? str : str.Substring(0, firstIndex);
        }

        private async Task SwitchContentAsync(string id = null, RouteEntry entry = null)
        {
            if (disposed) return;

            if (id == null || entry == null)
            {
                routes.Route(context);
            }
            else
            {
                routes.Route(context, id, entry);
            }

            if (context.Fragment == null) return;

            // Reserve the first match before yielding to other route registrations.
            firstMatched = true;
            var matchedContext = context;
            var args = new RouteMatchedEventArgs(location, matchedContext.TemplateText, matchedContext.Parameters, matchedContext.Fragment);
            var onMatch = OnMatch;

            if (onMatch.HasDelegate)
            {
                // Hide routed content while prefetching, including during callback-triggered renders.
                currentFragment = null;
                parameters = null;

                var matchTask = onMatch.InvokeAsync(args);
                if (!matchTask.IsCompleted)
                {
                    StateHasChanged();
                }

                await matchTask;

                // Navigation, route removal, or disposal must win over an older callback.
                if (disposed || context != matchedContext) return;
            }

            currentFragment = matchedContext.Fragment;
            parameters = matchedContext.Parameters;
            StateHasChanged();
        }

        protected override void OnInitialized()
        {
            location = NaviManager.Uri;
            NaviManager.LocationChanged += LocationChanged;
            UpdateRouteContext();

            base.OnInitialized();
        }

        public Task RegisterRouteAsync(string id, RenderFragment fragment, string template)
        {
            if (disposed) return Task.CompletedTask;

            var entry = routes.Add(id, template, fragment);
            return firstMatched ? Task.CompletedTask : SwitchContentAsync(id, entry);
        }

        public async Task UnregisterRouteAsync(string id)
        {
            routes.Remove(id);
            if (disposed || context?.Id != id) return;

            // Replacing the context invalidates any callback awaiting this route's match.
            UpdateRouteContext();
            firstMatched = false;
            currentFragment = null;
            parameters = null;
            var unmatchedContext = context;

            try
            {
                await InvokeAsync(async () =>
                {
                    if (disposed || context != unmatchedContext) return;

                    await SwitchContentAsync();
                    if (!disposed && context == unmatchedContext && context.Fragment == null)
                    {
                        // Clear the removed route even when no remaining route matches.
                        StateHasChanged();
                    }
                });
            }
            catch (Exception exception)
            {
                await DispatchExceptionIfNotDisposedAsync(exception);
            }
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
            disposed = true;
            NaviManager.LocationChanged -= LocationChanged;
        }

        [Inject] private INavigationInterception NavigationInterception { get; set; }
        [Inject] private NavigationManager NaviManager { get; set; }
    }
}
