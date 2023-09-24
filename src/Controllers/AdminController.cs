using AzureNamingTool.Attributes;
using AzureNamingTool.Helpers;
using AzureNamingTool.Models;
using AzureNamingTool.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AzureNamingTool.Controllers;

[Route("api/[controller]")]
[ApiController]
[ApiKey]
public class AdminController : ControllerBase
{
    private readonly AdminLogService _adminLogService;
    private readonly AdminService _adminService;
    private readonly CacheHelper _cacheHelper;
    private readonly ConfigurationHelper _configurationHelper;
    private readonly SiteConfiguration _config;
    private readonly GeneralHelper _generalHelper;
    private readonly GeneratedNamesService _generatedNamesService;

    public AdminController(
        GeneralHelper generalHelper,
        AdminService adminService,
        AdminLogService adminLogService,
        GeneratedNamesService generatedNamesService,
        CacheHelper cacheHelper, 
        ConfigurationHelper configurationHelper,
        SiteConfiguration config)
    {
        _generalHelper = generalHelper;
        _adminService = adminService;
        _adminLogService = adminLogService;
        _generatedNamesService = generatedNamesService;
        _cacheHelper = cacheHelper;
        _configurationHelper = configurationHelper;
        _config = config;
    }

    // POST api/<AdminController>
    /// <summary>
    ///     This function will update the Global Admin Password.
    /// </summary>
    /// <param name="password">string - New Global Admin Password</param>
    /// <param name="adminpassword">string - Current Global Admin Password</param>
    /// <returns>string - Successful update</returns>
    [HttpPost]
    [Route("[action]")]
    public IActionResult UpdatePassword(
        [BindRequired] [FromHeader(Name = "AdminPassword")] string adminpassword, [FromBody] string password)
    {
        if (string.IsNullOrEmpty(adminpassword))
            return Ok("FAILURE - You must provide the Global Admin Password.");

        if (adminpassword != _generalHelper.DecryptString(_config.AdminPassword!, _config.SaltKey!))
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
    /// <param name="adminpassword">string - Current Global Admin Password</param>
    /// <returns>dttring - Successful update</returns>
    [HttpPost]
    [Route("[action]")]
    public IActionResult UpdateAPIKey(
        [BindRequired] [FromHeader(Name = "AdminPassword")] string adminpassword, [FromBody] string apikey)
    {
        ServiceResponse serviceResponse = new();

        if (adminpassword!= null)
        {
            if (adminpassword == _generalHelper.DecryptString(_config.AdminPassword!, _config.SaltKey!))
            {
                serviceResponse = _adminService.UpdateApiKey(apikey);
                return serviceResponse.Success
                    ? Ok("SUCCESS")
                    : Ok("FAILURE - There was a problem updating the API Key.");
            }

            return Ok("FAILURE - Incorrect Global Admin Password.");
        }

        return Ok("FAILURE - You must provide the Global Admin Password.");
    }

    // POST api/<AdminController>
    /// <summary>
    ///     This function will generate a new API Key.
    /// </summary>
    /// <param name="adminpassword">string - Current Global Admin Password</param>
    /// <returns>string - Successful update / Generated API Key</returns>
    [HttpPost]
    [Route("[action]")]
    public IActionResult GenerateAPIKey(
        [BindRequired] [FromHeader(Name = "AdminPassword")] string adminpassword)
    {
        if (string.IsNullOrEmpty(adminpassword)) 
            return Ok("FAILURE - You must provide the Global Admin Password.");

        if (adminpassword != _generalHelper.DecryptString(_config.AdminPassword!, _config.SaltKey!))
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
    public IActionResult GetAdminLog(
        [BindRequired] [FromHeader(Name = "AdminPassword")] string adminpassword)
    {
        ServiceResponse serviceResponse = new();

        if (string.IsNullOrEmpty(adminpassword))
            return Ok("FAILURE - You must provide the Global Admin Password.");
        
        if (adminpassword == _generalHelper.DecryptString(_config.AdminPassword!, _config.SaltKey!))
        {
            serviceResponse = _adminLogService.GetItems();
            if (serviceResponse.Success)
            {
                return Ok(serviceResponse.ResponseObject);
            }

            return BadRequest(serviceResponse.ResponseObject);
        }

        return Ok("FAILURE - Incorrect Global Admin Password.");

    }

    /// <summary>
    ///     This function will purge the admin log data.
    /// </summary>
    /// <returns>dttring - Successful operation</returns>
    [HttpPost]
    [Route("[action]")]
    public IActionResult PurgeAdminLog(
        [BindRequired] [FromHeader(Name = "AdminPassword")] string adminpassword)
    {
        ServiceResponse serviceResponse = new();

        if (adminpassword!= null)
        {
            if (adminpassword == _generalHelper.DecryptString(_config.AdminPassword!, _config.SaltKey!))
            {
                serviceResponse = _adminLogService.DeleteAllItems();
                if (serviceResponse.Success)
                {
                    return Ok(serviceResponse.ResponseObject);
                }

                return BadRequest(serviceResponse.ResponseObject);
            }

            return Ok("FAILURE - Incorrect Global Admin Password.");
        }

        return Ok("FAILURE - You must provide the Global Admin Password.");
    }

    /// <summary>
    ///     This function will return the generated names data.
    /// </summary>
    /// <returns>json - Current generated names data</returns>
    [HttpGet]
    [Route("[action]")]
    public IActionResult GetGeneratedNamesLog()
    {
        ServiceResponse serviceResponse = new();

        serviceResponse = _generatedNamesService.GetItems();
        if (serviceResponse.Success)
        {
            return Ok(serviceResponse.ResponseObject);
        }

        return BadRequest(serviceResponse.ResponseObject);
    }

    // GET api/<AdminController>/GetGeneratedName/5
    /// <summary>
    ///     This function will return the generated names data by ID.
    /// </summary>
    /// <param name="id">int - Generated Name id</param>
    /// <returns>json - Current generated name data by ID</returns>
    [HttpGet]
    [Route("[action]/{id}")]
    public IActionResult GetGeneratedName(int id)
    {
        ServiceResponse serviceResponse = new();

        serviceResponse = _generatedNamesService.GetItem(id);
        if (serviceResponse.Success)
        {
            return Ok(serviceResponse.ResponseObject);
        }

        return BadRequest(serviceResponse.ResponseObject);
    }

    // DELETE api/<AdminController>/DeleteGeneratedName/5
    /// <summary>
    ///     This function will delete the generated names data by ID.
    /// </summary>
    /// <param name="adminpassword">string - Admin password</param>
    /// <param name="id">int - Generated Name id</param>
    /// <returns>bool - PASS/FAIL</returns>
    [HttpDelete]
    [Route("[action]/{id}")]
    public IActionResult DeleteGeneratedName(
        [BindRequired] [FromHeader(Name = "AdminPassword")] string adminpassword, int id)
    {
        if (string.IsNullOrEmpty(adminpassword)) 
            return Ok("FAILURE - You must provide the Global Admin Password.");

        if (adminpassword != _generalHelper.DecryptString(_config.AdminPassword!, _config.SaltKey!))
            return Ok("FAILURE - Incorrect Global Admin Password.");

        // Get the item details
        var serviceResponse = _generatedNamesService.GetItem(id);
        if (!serviceResponse.Success) 
            return BadRequest(serviceResponse.ResponseObject);
        
        var item = (GeneratedName) serviceResponse.ResponseObject!;
        serviceResponse = _generatedNamesService.DeleteItem(id);
        if (!serviceResponse.Success) 
            return BadRequest(serviceResponse.ResponseObject);
        
        _adminLogService.PostItem(new AdminLogMessage
        {
            Source = "API", Title = "INFORMATION",
            Message = "Generated Name (" + item.ResourceName + ") deleted."
        });
        _cacheHelper.InvalidateCacheObject("GeneratedName");
        return Ok("Generated Name (" + item.ResourceName + ") deleted.");
    }

    /// <summary>
    ///     This function will purge the generated names data.
    /// </summary>
    /// <returns>dttring - Successful operation</returns>
    [HttpPost]
    [Route("[action]")]
    public IActionResult PurgeGeneratedNamesLog(
        [BindRequired] [FromHeader(Name = "AdminPassword")] string adminpassword)
    {
        ServiceResponse serviceResponse = new();

        if (adminpassword!= null)
        {
            if (adminpassword == _generalHelper.DecryptString(_config.AdminPassword!, _config.SaltKey!))
            {
                serviceResponse = _generatedNamesService.DeleteAllItems();
                if (serviceResponse.Success)
                {
                    return Ok(serviceResponse.ResponseObject);
                }

                return BadRequest(serviceResponse.ResponseObject);
            }

            return Ok("FAILURE - Incorrect Global Admin Password.");
        }

        return Ok("FAILURE - You must provide the Global Admin Password.");
    }

    /// <summary>
    ///     This function will reset the site configuration. THIS CANNOT BE UNDONE!
    /// </summary>
    /// <returns>dttring - Successful operation</returns>
    [HttpPost]
    [Route("[action]")]
    public IActionResult ResetSiteConfiguration(
        [BindRequired] [FromHeader(Name = "AdminPassword")] string adminPassword)
    {
        if (string.IsNullOrEmpty(adminPassword))
            return Ok("FAILURE - You must provide the Global Admin Password.");
        
        if (adminPassword == _generalHelper.DecryptString(_config.AdminPassword!, _config.SaltKey!))
        {
            if (_configurationHelper.ResetSiteConfiguration())
            {
                return Ok("Site configuration reset succeeded!");
            }

            return BadRequest("Site configuration reset failed!");
        }

        return Ok("FAILURE - Incorrect Global Admin Password.");

    }
}