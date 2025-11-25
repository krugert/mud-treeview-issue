using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Intent.RoslynWeaver.Attributes;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.WebUtilities;
using MudBlazor;
using MXAdmin.Frontend.Client.Components.Pages.Clients;
using MXAdmin.Frontend.Client.Contracts.MXAdminApp.Services.Clients;
using static MudBlazor.Colors;

[assembly: DefaultIntentManaged(Mode.Merge)]
[assembly: IntentTemplate("Intent.Blazor.Templates.Client.RazorComponentCodeBehindTemplate", Version = "1.0")]

namespace MXAdmin.Frontend.Client.Components
{
    public partial class ClientTreeView : ComponentBase
    {
        [Parameter]
        public int ClientIdParameter { get; set; }
        private int _previousClientIdParameter;
        [Inject]
        public AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;
        [Inject] NavigationManager? NavManager { get; set; }
        public List<ClientDto>? ClientModel { get; set; }
        public List<ClientNavigationContextDto>? Model { get; set; }
        public EventCallback<ClientDto> OnClick { get; set; }
        [Inject]
        public IClientsService ClientService { get; set; } = default!;
        [Inject]
        public IClientTreeService ClientTreeService { get; set; } = default!;

        [Inject]
        public NavigationManager NavigationManager { get; set; } = default!;
        [Inject]
        public ISnackbar Snackbar { get; set; } = default!;

        private MudMenu _contextMenu = null!;
        private MudMenu _siteContextMenu = null!;

        private async Task OpenContextMenu(MouseEventArgs args)
        {
            await _contextMenu.OpenMenuAsync(args);
        }

        private async Task OpenSiteContextMenu(MouseEventArgs args)
        {
            await _siteContextMenu.OpenMenuAsync(args);
        }

        /// <summary>
        /// To prevent the first error from showing (even when the user is authenticated) we wait when the component is rendered the first time.
        /// This is because the AuthenticationStateProvider might not be ready when the component is initialized.
        /// </summary>
        [IntentManaged(Mode.Merge, Signature = Mode.Ignore)]
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender)
            {
                return;
            }

            var state = await AuthenticationStateProvider.GetAuthenticationStateAsync();

            if (state.User.Identity?.IsAuthenticated != true)
            {
                return;
            }
            await LoadUserNavigationContext();
        }


        private async Task LoadUserNavigationContext()
        {
            try
            {
                Model = await ClientTreeService.APIFetchClientTreeAsync();
                StateHasChanged();
            }
            catch (Exception e)
            {
                Snackbar.Add(e.Message, Severity.Error);
            }
        }

        private void OnClientClick(ClientNavigationContextDto client)
        {
            // Check if the client exists
            if (client == null)
                return;

            EditClient(client.ClientId);
        }

        private void EditClient(int clientId)
        {
            // Navigate to edit page by ID
            NavigationManager.NavigateTo($"/clients/{clientId}", forceLoad: true);
        }

        /*
        public override async Task SetParametersAsync(ParameterView parameters)
        {
            // Always call the base implementation first to set the properties
            // decorated with [Parameter] or [CascadingParameter].
            await base.SetParametersAsync(parameters);

            // Check if the specific parameter has changed
            if (ClientIdParameter != _previousClientIdParameter)
            {
                // Perform actions when MyParameter changes
                System.Console.WriteLine($"MyParameter changed from '{_previousClientIdParameter}' to '{ClientIdParameter}'");

                // Update the previous value for future comparisons
                _previousClientIdParameter = ClientIdParameter;

                // If you need to trigger a re-render or other actions based on the change,
                // you might call StateHasChanged() or other methods here.
                // For example, if you have other internal state that needs updating based on the new parameter.
                // StateHasChanged(); 
            }
        }
        */

        private void OnSiteClick(ClientNavigationContextDto client, int siteId)
        {
            // Optional: check if the site exists
            if (client == null)
                return;

            EditSite(client.ClientId, siteId);
        }

        private void EditSite(int clientId, int siteId)
        {
            // Example: navigate to edit page by Id
            NavigationManager.NavigateTo($"/clients/{clientId}/sites/{siteId}", forceLoad: true);
        }

        private void OnDivisionClick(ClientNavigationContextDto client, int siteId, int divisionId)
        {
            // Optional: check if the site exists
            if (client == null)
                return;

            EditDivision(client.ClientId, siteId, divisionId);
        }

        private void EditDivision(int clientId, int siteId, int divisionId)
        {
            // Example: navigate to edit page by Id
            NavigationManager.NavigateTo($"/clients/{clientId}/sites/{siteId}/divisions/{divisionId}", forceLoad: true);
        }


        private ClientTreeItem CreateClientItem(ClientNavigationContextDto client)
        {
            return new ClientTreeItem { Type = "Client", Name = client.ClientName, Id = client.ClientId };
        }

        private ClientTreeItem CreateSiteItem(ClientSiteNavigationContextDto site)
        {
            return new ClientTreeItem { Type = "Site", Name = site.SiteName, Id = site.SiteId };
        }

        private ClientTreeItem CreateDivisionItem(ClientDivisionNavigationContextDto division)
        {
            return new ClientTreeItem { Type = "Division", Name = division.DivisionName, Id = division.DivisionId };
        }
    }

    public class ClientTreeItem
    {
        public string Type { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int? Id { get; set; }
    }
}