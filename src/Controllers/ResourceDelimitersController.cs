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
public class ResourceDelimitersController : ControllerBase
{
    private readonly CacheHelper _cacheHelper;
    private readonly ResourceDelimiterService _resourceDelimiterService;
    private AdminLogService _adminLogService;

    public ResourceDelimitersController(CacheHelper cacheHelper, ResourceDelimiterService resourceDelimiterService, AdminLogService adminLogService)
    {
        _cacheHelper = cacheHelper;
        _resourceDelimiterService = resourceDelimiterService;
        _adminLogService = adminLogService;
    }
    
    // GET api/<ResourceDelimitersController>
    /// <summary>
    ///     This function will return the delimiters data.
    /// </summary>
    /// <param name="admin">bool - All/Only-enabled delimiters flag</param>
    /// <returns>json - Current delimiters data</returns>
    [HttpGet]
    public IActionResult Get(bool admin = false)
    {
        ServiceResponse serviceResponse = new();

        serviceResponse = _resourceDelimiterService.GetItems(admin);
        if (serviceResponse.Success)
        {
            return Ok(serviceResponse.ResponseObject);
        }

        return BadRequest(serviceResponse.ResponseObject);
    }

    // GET api/<ResourceDelimitersController>/5
    /// <summary>
    ///     This function will return the specifed resource delimiter data.
    /// </summary>
    /// <param name="id">int - Resource Delimiter id</param>
    /// <returns>json - Resource delimiter data</returns>
    [HttpGet("{id:int}")]
    public IActionResult Get(int id)
    {
        ServiceResponse serviceResponse = new();

        // Get list of items
        serviceResponse = _resourceDelimiterService.GetItem(id);
        if (serviceResponse.Success)
        {
            return Ok(serviceResponse.ResponseObject);
        }

        return BadRequest(serviceResponse.ResponseObject);
    }

    // POST api/<ResourceDelimitersController>
    /// <summary>
    ///     This function will create/update the specified delimiter data.
    /// </summary>
    /// <param name="item">ResourceDelimiter (json) - Delimiter data</param>
    /// <returns>bool - PASS/FAIL</returns>
    [HttpPost]
    public IActionResult Post([FromBody] ResourceDelimiter item)
    {
        ServiceResponse serviceResponse = new();

        serviceResponse = _resourceDelimiterService.PostItem(item);
        if (serviceResponse.Success)
        {
            _adminLogService.PostItem(new AdminLogMessage
            {
                Source = "API", Title = "INFORMATION", Message = "Resource Delimiter (" + item.Name + ") added/updated."
            });
            _cacheHelper.InvalidateCacheObject("ResourceDelimiter");
            return Ok(serviceResponse.ResponseObject);
        }

        return BadRequest(serviceResponse.ResponseObject);
    }

    // POST api/<resourcedelimitersController>
    /// <summary>
    ///     This function will update all delimiters data.
    /// </summary>
    /// <param name="items">List - ResourceDelimiter (json) - All delimiters data</param>
    /// <returns>bool - PASS/FAIL</returns>
    [HttpPost]
    [Route("[action]")]
    public IActionResult PostConfig([FromBody] List<ResourceDelimiter> items)
    {
        var serviceResponse = _resourceDelimiterService.PostConfig(items);
        if (serviceResponse.Success)
        {
            _adminLogService.PostItem(new AdminLogMessage
                {Source = "API", Title = "INFORMATION", Message = "Resource Delimiters added/updated."});
            _cacheHelper.InvalidateCacheObject("ResourceDelimiter");
            return Ok(serviceResponse.ResponseObject);
        }

        return BadRequest(serviceResponse.ResponseObject);
    }
}