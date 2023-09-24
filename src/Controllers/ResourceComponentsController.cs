using AzureNamingTool.Attributes;
using AzureNamingTool.Helpers;
using AzureNamingTool.Models;
using AzureNamingTool.Services;
using Microsoft.AspNetCore.Mvc;

namespace AzureNamingTool.Controllers;

[Route("api/[controller]")]
[ApiController]
[ApiKey]
public class ResourceComponentsController : ControllerBase
{
    private readonly CacheHelper _cacheHelper;
    private readonly ResourceComponentService _resourceComponentService;
    private readonly AdminLogService _adminLogService;

    public ResourceComponentsController(CacheHelper cacheHelper, ResourceComponentService resourceComponentService, AdminLogService adminLogService)
    {
        _cacheHelper = cacheHelper;
        _resourceComponentService = resourceComponentService;
        _adminLogService = adminLogService;
    }

    // GET: api/<resourcecomponentsController>
    /// <summary>
    ///     This function will return the components data.
    /// </summary>
    /// <param name="admin">bool - All/Only-enabled components flag</param>
    /// <returns>json - Current components data</returns>
    [HttpGet]
    public async Task<IActionResult> Get(bool admin = false)
    {
        var serviceResponse
            = await _resourceComponentService.GetItems(admin);

        return serviceResponse.Success
            ? (IActionResult) Ok(serviceResponse.ResponseObject)
            : (IActionResult) BadRequest(serviceResponse.ResponseObject);
    }

    // GET api/<resourcecomponentsController>/5
    /// <summary>
    ///     This function will return the specifed resource component data.
    /// </summary>
    /// <param name="id">int - Resource Component id</param>
    /// <returns>json - Resource component data</returns>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var serviceResponse = await _resourceComponentService.GetItem(id);
        return serviceResponse.Success
            ? (IActionResult) Ok(serviceResponse.ResponseObject)
            : (IActionResult) BadRequest(serviceResponse.ResponseObject);
    }

    // POST api/<ResourceComponentsController>
    /// <summary>
    ///     This function will create/update the specified component data.
    /// </summary>
    /// <param name="item">ResourceComponent (json) - Component data</param>
    /// <returns>bool - PASS/FAIL</returns>
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] ResourceComponent item)
    {
        var serviceResponse = await _resourceComponentService.PostItem(item);
        if (!serviceResponse.Success)
            return BadRequest(serviceResponse.ResponseObject);

        await _adminLogService.PostItem(new AdminLogMessage
        {
            Source = "API", Title = "INFORMATION",
            Message = "Resource Component (" + item.Name + ") added/updated."
        });
        _cacheHelper.InvalidateCacheObject("ResourceComponent");
        return Ok(serviceResponse.ResponseObject);
    }

    // POST api/<ResourceComponentsController>
    /// <summary>
    ///     This function will update all components data.
    /// </summary>
    /// <param name="items">List - ResourceComponent (json) - All components configuration data</param>
    /// <returns>bool - PASS/FAIL</returns>
    [HttpPost]
    [Route("[action]")]
    public async Task<IActionResult> PostConfig([FromBody] List<ResourceComponent> items)
    {
        var serviceResponse = await _resourceComponentService.PostConfig(items);
        if (!serviceResponse.Success)
            return BadRequest(serviceResponse.ResponseObject);

        await _adminLogService.PostItem(new AdminLogMessage
            {Source = "API", Title = "INFORMATION", Message = "Resource Components added/updated."});

        _cacheHelper.InvalidateCacheObject("ResourceComponent");
        return Ok(serviceResponse.ResponseObject);
    }
}