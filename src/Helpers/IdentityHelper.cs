using AzureNamingTool.Models;
using AzureNamingTool.Services;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace AzureNamingTool.Helpers;

public class IdentityHelper
{
    private readonly AdminUserService _adminUserService;

    public IdentityHelper(AdminUserService adminUserService)
    {
        _adminUserService = adminUserService;
    }

    public async Task<bool> IsAdminUser(StateContainer state, ProtectedSessionStorage session, string name)
    {
        var result = false;

        // Check if the username is in the list of Admin Users
        var serviceResponse = _adminUserService.GetItems();
        if (!serviceResponse.Success) 
            return result;
        
        if (serviceResponse.ResponseObject != null)
        {
            List<AdminUser> adminusers = serviceResponse.ResponseObject!;
            if (adminusers.Exists(x => string.Equals(x.Name, name, StringComparison.CurrentCultureIgnoreCase)))
            {
                state.SetAdmin(true);
                await session.SetAsync("admin", true);
                result = true;
            }
        }

        return result;
    }

    public static async Task<string> GetCurrentUser(ProtectedSessionStorage session)
    {
        var currentUser = "System";
        var currentUserValue = await session.GetAsync<string>("currentuser");

        if (!string.IsNullOrEmpty(currentUserValue.Value))
        {
            currentUser = currentUserValue.Value;
        }

        return currentUser;
    }
}