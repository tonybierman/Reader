using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Fast.Components.FluentUI;
using Microsoft.Fast.Components.FluentUI.DesignTokens;
using Microsoft.JSInterop;

namespace Reader.Client.Shared
{
    public partial class MainLayout
    {
        [Inject]
        private IJSRuntime JSRuntime { get; set; } = default!;

        [Inject]
        private BaseLayerLuminance BaseLayerLuminance { get; set; } = default!;

        ElementReference? container;

        ErrorBoundary? errorBoundary;

        LocalizationDirection dir;
        float baseLayerLuminance = 1;

        protected override Task OnInitializedAsync()
        {
            return base.OnInitializedAsync();

        }

        public async Task SwitchDirection()
        {
            dir = dir == LocalizationDirection.rtl ? LocalizationDirection.ltr : LocalizationDirection.rtl;
            await JSRuntime.InvokeVoidAsync("switchDirection", dir.ToString());
        }

        public void SwitchTheme()
        {
            baseLayerLuminance = baseLayerLuminance == 0.2f ? 1 : 0.2f;
        }

        protected override void OnParametersSet()
        {
            errorBoundary?.Recover();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            baseLayerLuminance = 0.15f;
            await BaseLayerLuminance.SetValueFor(container, baseLayerLuminance);
        }
    }
}