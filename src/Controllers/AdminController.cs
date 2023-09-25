using AzureNamingTool.Attributes;
using AzureNamingTool.Helpers;
using AzureNamingTool.Models;
using AzureNamingTool.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace AzureNamingTool.Controllers;

[Route("api/[controller]")]
[ApiController]
[ApiKey]
public class AdminController : ControllerBase
{
    private readonly AdminLogService _adminLogService;
    private readonly AdminService _adminService;
    private readonly CacheHelper _cacheHelper;
    private readonly SiteConfiguration _config;
    private readonly ConfigurationHelper _configurationHelper;
    private readonly EncryptionHelper _encryptionHelper;
    private readonly GeneratedNamesService _generatedNamesService;

    public AdminController(
        AdminService adminService,
        AdminLogService adminLogService,
        GeneratedNamesService generatedNamesService,
        CacheHelper cacheHelper,
        ConfigurationHelper configurationHelper,
        SiteConfiguration config,
        EncryptionHelper encryptionHelper)
    {
        _adminService = adminService;
        _adminLogService = adminLogService;
        _generatedNamesService = generatedNamesService;
        _cacheHelper = cacheHelper;
        _configurationHelper = configurationHelper;
        _config = config;
        _encryptionHelper = encryptionHelper;
    }

    // POST api/<AdminController>
    /// <summary>
    ///     This function will update the Global Admin Password.
    /// </summary>
    /// <param name="password">string - New Global Admin Password</param>
    /// <param name="adminPassword">string - Current Global Admin Password</param>
    /// <returns>string - Successful update</returns>
    [HttpPost]
    [Route("[action]")]
    public IActionResult UpdatePassword(
        [BindRequired] [FromHeader(Name = "AdminPassword")]
        string adminPassword, [FromBody] string password)
    {
        if (string.IsNullOrEmpty(adminPassword))
            return Ok("FAILURE - You must provide the Global Admin Password.");

        if (adminPassword != _encryptionHelper.DecryptString(_config.AdminPassword!, _config.SaltKey!))
            return Ok("FAILURE - Incorrect Global Admin Password.");

        var serviceResponse = _adminService.UpdatePassword(password);
        return serviceResponse.Success
            ? Ok("SUCCESS")
            : Ok("FAILURE - There was a problem updating the password.");
    }

    // POST api/<AdminController>
    /// <summary>
    ///     This function will update the API Key.
    /// </summary>
    /// <param name="apikey">string - New API Key</param>
    /// <param name="adminPassword">string - Current Global Admin Password</param>
    /// <returns>dttring - Successful update</returns>
    [HttpPost]
    [Route("[action]")]
    public IActionResult UpdateApiKey(
        [BindRequired] [FromHeader(Name = "AdminPassword")]
        string adminPassword, [FromBody] string apikey)
    {
        if (string.IsNullOrEmpty(adminPassword))
            return Ok("FAILURE - You must provide the Global Admin Password.");

        if (adminPassword != _encryptionHelper.DecryptString(_config.AdminPassword!, _config.SaltKey!))
            return Ok("FAILURE - Incorrect Global Admin Password.");

        var serviceResponse = _adminService.UpdateApiKey(apikey);

        return serviceResponse.Success
            ? Ok("SUCCESS")
            : Ok("FAILURE - There was a problem updating the API Key.");
    }

    // POST api/<AdminController>
    /// <summary>
    ///     This function will generate a new API Key.
    /// </summary>
    /// <param name="adminPassword">string - Current Global Admin Password</param>
    /// <returns>string - Successful update / Generated API Key</returns>
    [HttpPost]
    [Route("[action]")]
    public IActionResult GenerateAPIKey(
        [BindRequired] [FromHeader(Name = "AdminPassword")]
        string adminPassword)
    {
        if (string.IsNullOrEmpty(adminPassword))
            return Ok("FAILURE - You must provide the Global Admin Password.");

        if (adminPassword != _encryptionHelper.DecryptString(_config.AdminPassword!, _config.SaltKey!))
            return Ok("FAILURE - Incorrect Global Admin Password.");

        var serviceResponse = _adminService.GenerateApiKey();
        return serviceResponse.Success
            ? Ok("SUCCESS")
            : Ok("FAILURE - There was a problem generating the API Key.");
    }

    /// <summary>
    ///     This function will return the admin log data.
    /// </summary>
    /// <returns>json - Current admin log data</returns>
    [HttpGet]
    [Route("[action]")]
    public async Task<IActionResult> GetAdminLog(
        [BindRequired] [FromHeader(Name = "AdminPassword")]
        string adminPassword)
    {
        if (string.IsNullOrEmpty(adminPassword))
            return Ok("FAILURE - You must provide the Global Admin Password.");

        if (adminPassword != _encryptionHelper.DecryptString(_config.AdminPassword!, _config.SaltKey!))
            return Ok("FAILURE - Incorrect Global Admin Password.");

        var serviceResponse = await _adminLogService.GetItems();

        return serviceResponse.Success
            ? (IActionResult) Ok(serviceResponse.ResponseObject)
            : (IActionResult) BadRequest(serviceResponse.ResponseObject);
    }

    /// <summary>
    ///     This function will purge the admin log data.
    /// </summary>
    /// <returns>dttring - Successful operation</returns>
    [HttpPost]
    [Route("[action]")]
    public async Task<IActionResult> PurgeAdminLog(
        [BindRequired] [FromHeader(Name = "AdminPassword")]
        string adminPassword)
    {
        if (string.IsNullOrEmpty(adminPassword))
            return Ok("FAILURE - You must provide the Global Admin Password.");

        if (adminPassword != _encryptionHelper.DecryptString(_config.AdminPassword!, _config.SaltKey!))
            return Ok("FAILURE - Incorrect Global Admin Password.");

        var serviceResponse = await _adminLogService.DeleteAllItems();

        return serviceResponse.Success
            ? Ok(serviceResponse.ResponseObject)
            : BadRequest(serviceResponse.ResponseObject);
    }

    /// <summary>
    ///     This function will return the generated names data.
    /// </summary>
    /// <returns>json - Current generated names data</returns>
    [HttpGet]
    [Route("[action]")]
    public async Task<IActionResult> GetGeneratedNamesLog()
    {
        var serviceResponse = await _generatedNamesService.GetItems();

        return serviceResponse.Success
            ? (IActionResult) Ok(serviceResponse.ResponseObject)
            : (IActionResult) BadRequest(serviceResponse.ResponseObject);
    }

    // GET api/<AdminController>/GetGeneratedName/5
    /// <summary>
    ///     This function will return the generated names data by ID.
    /// </summary>
    /// <param name="id">int - Generated Name id</param>
    /// <returns>json - Current generated name data by ID</returns>
    [HttpGet]
    [Route("[action]/{id}")]
    public async Task<IActionResult> GetGeneratedName(int id)
    {
        var serviceResponse = await _generatedNamesService.GetItem(id);

        return serviceResponse.Success
            ? (IActionResult) Ok(serviceResponse.ResponseObject)
            : (IActionResult) BadRequest(serviceResponse.ResponseObject);
    }

    // DELETE api/<AdminController>/DeleteGeneratedName/5
    /// <summary>
    ///     This function will delete the generated names data by ID.
    /// </summary>
    /// <param name="adminPassword">string - Admin password</param>
    /// <param name="id">int - Generated Name id</param>
    /// <returns>bool - PASS/FAIL</returns>
    [HttpDelete]
    [Route("[action]/{id}")]
    public async Task<IActionResult> DeleteGeneratedName(
        [BindRequired] [FromHeader(Name = "AdminPassword")]
        string adminPassword, int id)
    {
        if (string.IsNullOrEmpty(adminPassword))
            return Ok("FAILURE - You must provide the Global Admin Password.");

        if (adminPassword != _encryptionHelper.DecryptString(_config.AdminPassword!, _config.SaltKey!))
            return Ok("FAILURE - Incorrect Global Admin Password.");

        // Get the item details
        var serviceResponse = await _generatedNamesService.GetItem(id);
        if (!serviceResponse.Success)
            return BadRequest(serviceResponse.ResponseObject);

        var item = (GeneratedName) serviceResponse.ResponseObject!;
        serviceResponse = await _generatedNamesService.DeleteItem(id);
        if (!serviceResponse.Success)
            return BadRequest(serviceResponse.ResponseObject);

        await _adminLogService.PostItem(new AdminLogMessage
        {
            Source = "API", Title = "INFORMATION",
            Message = "Generated Name (" + item.ResourceName + ") deleted."
        });
        _cacheHelper.InvalidateCacheObject("GeneratedName");
        return Ok("Generated Name (" + item.ResourceName + ") deleted.");
    }

    [HttpPost]
    [Route("[action]")]
    public async Task<IActionResult> PurgeGeneratedNamesLog(
        [BindRequired] [FromHeader(Name = "AdminPassword")]
        string adminPassword)
    {
        if (string.IsNullOrEmpty(adminPassword))
            return Ok("FAILURE - You must provide the Global Admin Password.");

        if (adminPassword != _encryptionHelper.DecryptString(_config.AdminPassword!, _config.SaltKey!))
            return Ok("FAILURE - Incorrect Global Admin Password.");

        var serviceResponse = await _generatedNamesService.DeleteAllItems();

        return serviceResponse.Success
            ? Ok(serviceResponse.ResponseObject)
            : BadRequest(serviceResponse.ResponseObject);
    }

    /// <summary>
    ///     This function will reset the site configuration. THIS CANNOT BE UNDONE!
    /// </summary>
    /// <returns>dttring - Successful operation</returns>
    [HttpPost]
    [Route("[action]")]
    public IActionResult ResetSiteConfiguration(
        [BindRequired] [FromHeader(Name = "AdminPassword")]
        string adminPassword)
    {
        if (string.IsNullOrEmpty(adminPassword))
            return Ok("FAILURE - You must provide the Global Admin Password.");

        if (adminPassword != _encryptionHelper.DecryptString(_config.AdminPassword!, _config.SaltKey!))
            return Ok("FAILURE - Incorrect Global Admin Password.");

        if (_configurationHelper.ResetSiteConfiguration())
        {
            return Ok("Site configuration reset succeeded!");
        }

        return BadRequest("Site configuration reset failed!");
    }
}