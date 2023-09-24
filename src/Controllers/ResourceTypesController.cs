using AzureNamingTool.Attributes;
using AzureNamingTool.Helpers;
using AzureNamingTool.Models;
using AzureNamingTool.Services;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AzureNamingTool.Controllers;

[Route("api/[controller]")]
[ApiController]
[ApiKey]
public class ResourceTypesController : ControllerBase
{
    private readonly CacheHelper _cacheHelper;
    private readonly ResourceTypeService _resourceTypeService;
    private readonly ResourceTypeUpdater _resourceTypeUpdater;
    private readonly AdminLogService _adminLogService;

    public ResourceTypesController(
        ResourceTypeService resourceTypeService, 
        ResourceTypeUpdater resourceTypeUpdater,
        CacheHelper cacheHelper, 
        AdminLogService adminLogService)
    {
        _resourceTypeService = resourceTypeService;
        _resourceTypeUpdater = resourceTypeUpdater;
        _cacheHelper = cacheHelper;
        _adminLogService = adminLogService;
    }

    // GET: api/<ResourceTypesController>
    /// <summary>
    ///     This function will return the resource types data.
    /// </summary>
    /// <returns>json - Current resource types data</returns>
    [HttpGet]
    public async Task<IActionResult> Get(bool admin = false)
    {
        var serviceResponse =
            // Get list of items
            await _resourceTypeService.GetItems(admin);
        
        if (serviceResponse.Success)
        {
            return Ok(serviceResponse.ResponseObject);
        }

        return BadRequest(serviceResponse.ResponseObject);
    }

    // GET api/<ResourceTypesController>/5
    /// <summary>
    ///     This function will return the specifed resource type data.
    /// </summary>
    /// <param name="id">int - Resource Type id</param>
    /// <returns>json - Resource Type data</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var serviceResponse =
            // Get list of items
            await _resourceTypeService.GetItem(id);
        
        if (serviceResponse.Success)
        {
            return Ok(serviceResponse.ResponseObject);
        }

        return BadRequest(serviceResponse.ResponseObject);
    }

    // POST api/<ResourceTypesController>
    /// <summary>
    ///     This function will update all resource types data.
    /// </summary>
    /// <param name="items">List - ResourceType (json) - All resource types data</param>
    /// <returns>bool - PASS/FAIL</returns>
    [HttpPost]
    [Route("[action]")]
    public async Task<IActionResult> PostConfig([FromBody] List<ResourceType> items)
    {
        var serviceResponse = await _resourceTypeService.PostConfig(items);

        if (!serviceResponse.Success) 
            return BadRequest(serviceResponse.ResponseObject);
        
        await _adminLogService.PostItem(new AdminLogMessage
            {Source = "API", Title = "INFORMATION", Message = "Resource Types updated."});
        _cacheHelper.InvalidateCacheObject("ResourceType");
        return Ok(serviceResponse.ResponseObject);

    }

    // POST api/<ResourceTypesController>
    /// <summary>
    ///     This function will update all resource types for the specifed component
    /// </summary>
    /// <param name="operation">string - Operation type  (optional-add, optional-remove, exlcude-add, exclude-remove)</param>
    /// <param name="componentId">int - Component ID</param>
    /// <returns>bool - PASS/FAIL</returns>
    [HttpPost]
    [Route("[action]")]
    public async Task<IActionResult> UpdateTypeComponents(string operation, int componentId)
    {
        var serviceResponse = await _resourceTypeUpdater.UpdateTypeComponents(operation, componentId);
        if (!serviceResponse.Success) 
            return BadRequest(serviceResponse.ResponseObject);
        
        await _adminLogService.PostItem(new AdminLogMessage
            {Source = "API", Title = "INFORMATION", Message = "Resource Types updated."});
        _cacheHelper.InvalidateCacheObject("ResourceType");
        return Ok(serviceResponse.ResponseObject);
    }
}