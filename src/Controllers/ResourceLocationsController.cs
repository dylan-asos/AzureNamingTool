using AzureNamingTool.Attributes;
using AzureNamingTool.Helpers;
using AzureNamingTool.Models;
using AzureNamingTool.Services;
using Microsoft.AspNetCore.Mvc;

namespace AzureNamingTool.Controllers;

[Route("api/[controller]")]
[ApiController]
[ApiKey]
public class ResourceLocationsController : ControllerBase
{
    private readonly CacheHelper _cacheHelper;
    private readonly ResourceLocationService _resourceLocationService;
    private readonly AdminLogService _adminLogService;

    public ResourceLocationsController(CacheHelper cacheHelper, ResourceLocationService resourceLocationService, AdminLogService adminLogService)
    {
        _cacheHelper = cacheHelper;
        _resourceLocationService = resourceLocationService;
        _adminLogService = adminLogService;
    }

    // GET: api/<ResourceLocationsController>
    /// <summary>
    ///     This function will return the locations data.
    /// </summary>
    /// <returns>json - Current locations data</returns>
    [HttpGet]
    public IActionResult Get(bool admin = false)
    {
        var serviceResponse = _resourceLocationService.GetItems(admin);
        return serviceResponse.Success 
            ? (IActionResult) Ok(serviceResponse.ResponseObject) 
            : (IActionResult) BadRequest(serviceResponse.ResponseObject);
    }

    // GET api/<ResourceLocationsController>/5
    /// <summary>
    ///     This function will return the specifed location data.
    /// </summary>
    /// <param name="id">int - Location id</param>
    /// <returns>json - Location data</returns>
    [HttpGet("{id}")]
    public IActionResult Get(int id)
    {
        var serviceResponse = _resourceLocationService.GetItem(id);
        
        return serviceResponse.Success 
            ? (IActionResult) Ok(serviceResponse.ResponseObject) 
            : (IActionResult) BadRequest(serviceResponse.ResponseObject);
    }

    // POST api/<ResourceLocationsController>
    /// <summary>
    ///     This function will update all locations data.
    /// </summary>
    /// <param name="items">List - ResourceLocation (json) - All locations data</param>
    /// <returns>bool - PASS/FAIL</returns>
    [HttpPost]
    [Route("[action]")]
    public IActionResult PostConfig([FromBody] List<ResourceLocation> items)
    {
        var serviceResponse = _resourceLocationService.PostConfig(items);
        if (!serviceResponse.Success) 
            return BadRequest(serviceResponse.ResponseObject);
        
        _adminLogService.PostItem(new AdminLogMessage
            {Source = "API", Title = "INFORMATION", Message = "Resource Locations added/updated."});
        _cacheHelper.InvalidateCacheObject("ResourceLocation");
        return Ok(serviceResponse.ResponseObject);
    }
}