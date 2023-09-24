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
public class ResourceEnvironmentsController : ControllerBase
{
    private readonly CacheHelper _cacheHelper;
    private readonly ResourceEnvironmentService _resourceEnvironmentService;
    private AdminLogService _adminLogService;

    public ResourceEnvironmentsController(CacheHelper cacheHelper,
        ResourceEnvironmentService resourceEnvironmentService, AdminLogService adminLogService)
    {
        _cacheHelper = cacheHelper;
        _resourceEnvironmentService = resourceEnvironmentService;
        _adminLogService = adminLogService;
    }

    // GET: api/<ResourceEnvironmentsController>
    /// <summary>
    ///     This function will return the environments data.
    /// </summary>
    /// <returns>json - Current environments data</returns>
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var serviceResponse = await _resourceEnvironmentService.GetItems();
        if (serviceResponse.Success)
        {
            return Ok(serviceResponse.ResponseObject);
        }

        return BadRequest(serviceResponse.ResponseObject);
    }

    // GET api/<ResourceEnvironmentsController>/5
    /// <summary>
    ///     This function will return the specifed environment data.
    /// </summary>
    /// <param name="id">int - Environment id</param>
    /// <returns>json - Environment data</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var serviceResponse = await _resourceEnvironmentService.GetItem(id);
        if (serviceResponse.Success)
        {
            return Ok(serviceResponse.ResponseObject);
        }

        return BadRequest(serviceResponse.ResponseObject);
    }

    // POST api/<ResourceEnvironmentsController>
    /// <summary>
    ///     This function will create/update the specified environment data.
    /// </summary>
    /// <param name="item">ResourceEnvironment (json) - Environment data</param>
    /// <returns>bool - PASS/FAIL</returns>
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] ResourceEnvironment item)
    {
        var serviceResponse = await _resourceEnvironmentService.PostItem(item);
        if (!serviceResponse.Success)
            return BadRequest(serviceResponse.ResponseObject);
        
        await _adminLogService.PostItem(new AdminLogMessage
        {
            Source = "API", Title = "INFORMATION",
            Message = "Resource Environment (" + item.Name + ") added/updated."
        });
        _cacheHelper.InvalidateCacheObject("ResourceEnvironment");
        return Ok(serviceResponse.ResponseObject);

    }

    // POST api/<ResourceEnvironmentsController>
    /// <summary>
    ///     This function will update all environments data.
    /// </summary>
    /// <param name="items">List - ResourceEnvironment (json) - All environments data</param>
    /// <returns>bool - PASS/FAIL</returns>
    [HttpPost]
    [Route("[action]")]
    public async Task<IActionResult> PostConfig([FromBody] List<ResourceEnvironment> items)
    {
        var serviceResponse = _resourceEnvironmentService.PostConfig(items);
        if (!serviceResponse.Success)
            return BadRequest(serviceResponse.ResponseObject);
        
        await _adminLogService.PostItem(new AdminLogMessage
            {Source = "API", Title = "INFORMATION", Message = "Resource Environments added/updated."});
        _cacheHelper.InvalidateCacheObject("ResourceEnvironment");
        return Ok(serviceResponse.ResponseObject);

    }

    // DELETE api/<ResourceEnvironmentsController>/5
    /// <summary>
    ///     This function will delete the specifed environment data.
    /// </summary>
    /// <param name="id">int - Environment id</param>
    /// <returns>bool - PASS/FAIL</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var serviceResponse = await _resourceEnvironmentService.GetItem(id);
        if (!serviceResponse.Success) 
            return BadRequest(serviceResponse.ResponseObject);
        
        var item = (ResourceEnvironment) serviceResponse.ResponseObject!;
        serviceResponse = await _resourceEnvironmentService.DeleteItem(id);
        if (!serviceResponse.Success) 
            return BadRequest(serviceResponse.ResponseObject);
        
        await _adminLogService.PostItem(new AdminLogMessage
        {
            Source = "API", Title = "INFORMATION",
            Message = "Resource Environment (" + item.Name + ") deleted."
        });
        _cacheHelper.InvalidateCacheObject("ResourceEnvironment");
        return Ok("Resource Environment (" + item.Name + ") deleted.");
    }
}