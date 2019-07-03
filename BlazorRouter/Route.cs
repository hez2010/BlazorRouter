using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace BlazorRouter
{
    public class Route : ComponentBase
    {
        [CascadingParameter(Name = "SwitchInstance")] protected Switch SwitchInstance { get; set; }
        [Parameter] public string Template { get; set; } = "";
        [Parameter] public RenderFragment ChildContent { get; set; }

        private bool hasRegisterd = false;

        protected override Task OnParametersSetAsync()
        {
            if (!hasRegisterd)
            {
                hasRegisterd = true;
                base.OnParametersSetAsync();
                if (SwitchInstance == null)
                {
                    throw new InvalidOperationException("A Route markup must be included in a Switch markup.");
                }
                return SwitchInstance.RegisterRoute(ChildContent, Template);
            }
            return Task.CompletedTask;
        }
    }
}
