using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace BlazorRouter
{
    public partial class Route : ComponentBase, IDisposable
    {
        [CascadingParameter(Name = "SwitchInstance")] protected Switch SwitchInstance { get; set; }
        [Parameter] public string Template { get; set; } = "";
        [Parameter] public RenderFragment ChildContent { get; set; }

        private readonly string id = Guid.NewGuid().ToString();

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            if (SwitchInstance == null)
            {
                throw new InvalidOperationException("A Route markup must be nested in a Switch markup.");
            }

            await SwitchInstance.RegisterRouteAsync(id, ChildContent, Template);
        }

        public void Dispose()
        {
            if (SwitchInstance != null)
            {
                SwitchInstance.UnregisterRoute(id);
            }
        }
    }
}
