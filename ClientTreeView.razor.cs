using Intent.RoslynWeaver.Attributes;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.WebUtilities;
using MudBlazor;
using MXAdmin.Frontend.Client.Components.Pages.Clients;
using MXAdmin.Frontend.Client.Contracts.MXAdminApp.Services.Clients;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static MudBlazor.Colors;

[assembly: DefaultIntentManaged(Mode.Merge)]
[assembly: IntentTemplate("Intent.Blazor.Templates.Client.RazorComponentCodeBehindTemplate", Version = "1.0")]

namespace MXAdmin.Frontend.Client.Components
{
    public partial class ClientTreeView : ComponentBase
    {
        [Parameter]
        public EventCallback<ClientTreeItem> OnItemSelected { get; set; }
        [Parameter]
        public int ClientIdParameter { get; set; }
        private int _previousClientIdParameter;
        [Inject]
        public AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;
        [Inject] NavigationManager? NavManager { get; set; }
        public List<ClientDto>? ClientModel { get; set; }
        public List<ClientNavigationContextDto>? Model { get; set; }
        [Inject]
        public IClientsService ClientService { get; set; } = default!;
        [Inject]
        public IClientTreeService ClientTreeService { get; set; } = default!;

        [Inject]
        public NavigationManager NavigationManager { get; set; } = default!;
        [Inject]
        public ISnackbar Snackbar { get; set; } = default!;

        private MudMenu _clientContextMenu = null!;
        private MudMenu _siteContextMenu = null!;
        private MudMenu _divisionContextMenu = null!;

        private ClientNavigationContextDto? _selectedClient;
        private ClientSiteNavigationContextDto? _selectedSite;
        private ClientDivisionNavigationContextDto? _selectedDivision;

        private async Task OpenClientContextMenu(MouseEventArgs e, ClientNavigationContextDto client)
        {
            _selectedClient = client;
            await _clientContextMenu.OpenMenuAsync(e);
        }

        private async Task OpenSiteContextMenu(MouseEventArgs e, ClientSiteNavigationContextDto site)
        {
            _selectedSite = site;
            await _siteContextMenu.OpenMenuAsync(e);
        }

        private async Task OpenDivisionContextMenu(MouseEventArgs e, ClientDivisionNavigationContextDto division)
        {
            _selectedDivision = division;
            await _divisionContextMenu.OpenMenuAsync(e);
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

            // Snackbar.Add(state.User.Identity?.Name + " " + state.User.Identity?.IsAuthenticated, Severity.Info);

            if (state.User.Identity?.IsAuthenticated != true)
            {
                // var returnUrl = "~/" + NavigationManager.ToBaseRelativePath(NavigationManager.Uri);
                // NavigationManager.NavigateTo($"/account/login", forceLoad: true);
                return;
            }
            // Snackbar.Add(state.User.Identity?.Name + " " + state.User.Identity?.IsAuthenticated, Severity.Info);

            //foreach (var claim in state.User.Claims)
            //{
            //    Snackbar.Add($"{claim.Type}: {claim.Value}", Severity.Info);
            //}

            // AuthenticationStateProvider.

            //if (string.IsNullOrEmpty(state.User.Identity?.Name))
            //{
            //    // var returnUrl = "~/" + NavigationManager.ToBaseRelativePath(NavigationManager.Uri);
            //    //NavigationManager.NavigateTo($"/account/login", forceLoad: true);
            //    return;
            //}
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

        private async Task OnClientClick(ClientNavigationContextDto client)
        {
            if (client == null)
            {
                return;
            }
            var item = CreateClientItem(client);

            await OnItemSelected.InvokeAsync(item);
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

        private async Task OnSiteClick(ClientNavigationContextDto client, int siteId)
        {
            if (client == null)
            {
                return;
            }

            var site = client.Sites?.FirstOrDefault(s => s.SiteId == siteId);

            if (site == null)
            {
                return;
            }

            // Pass clientId when creating the item
            await OnItemSelected.InvokeAsync(CreateSiteItem(site, client.ClientId));
        }

        private void EditSite(int clientId, int siteId)
        {
            // Example: navigate to edit page by Id
            NavigationManager.NavigateTo($"/clients/{clientId}/sites/{siteId}", forceLoad: true);
        }

        private async Task OnDivisionClick(ClientNavigationContextDto client, int siteId, int divisionId)
        {
            if (client == null)
            {
                return;
            }

            var site = client.Sites?.FirstOrDefault(s => s.SiteId == siteId);
            var division = site?.Divisions?.FirstOrDefault(d => d.DivisionId == divisionId);
            if (division == null)
            {
                return;
            }

            // Pass both IDs when creating the item
            await OnItemSelected.InvokeAsync(CreateDivisionItem(division, client.ClientId, siteId));
        }

        private void EditDivision(int clientId, int siteId, int divisionId)
        {
            // Example: navigate to edit page by Id
            NavigationManager.NavigateTo($"/clients/{clientId}/sites/{siteId}/divisions/{divisionId}", forceLoad: true);
        }


        private ClientTreeItem CreateClientItem(ClientNavigationContextDto client)
        {
            return new ClientTreeItem
            {
                Type = "Client",
                Name = client.ClientName,
                Id = client.ClientId,
                ClientId = client.ClientId // Set for consistency
            };
        }

        private ClientTreeItem CreateSiteItem(ClientSiteNavigationContextDto site, int clientId)
        {
            return new ClientTreeItem
            {
                Type = "Site",
                Name = site.SiteName,
                Id = site.SiteId,
                ClientId = clientId
            };
        }

        private ClientTreeItem CreateDivisionItem(ClientDivisionNavigationContextDto division, int clientId, int siteId)
        {
            return new ClientTreeItem
            {
                Type = "Division",
                Name = division.DivisionName,
                Id = division.DivisionId,
                ClientId = clientId,
                SiteId = siteId
            };
        }
    }

    public class ClientTreeItem
    {
        public string Type { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int? Id { get; set; } // Primary ID (ClientId, SiteId, or DivisionId)
        public int? ClientId { get; set; } // Set for Site and Division items
        public int? SiteId { get; set; } // Set only for Division items
    }
}