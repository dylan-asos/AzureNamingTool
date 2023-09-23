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
public class ResourceFunctionsController : ControllerBase
{
    private readonly CacheHelper _cacheHelper;
    private readonly ResourceFunctionService _resourceFunctionService;
    private AdminLogService _adminLogService;

    public ResourceFunctionsController(CacheHelper cacheHelper, ResourceFunctionService resourceFunctionService, AdminLogService adminLogService)
    {
        _cacheHelper = cacheHelper;
        _resourceFunctionService = resourceFunctionService;
        _adminLogService = adminLogService;
    }

    // GET: api/<ResourceFunctionsController>
    /// <summary>
    ///     This function will return the functions data.
    /// </summary>
    /// <returns>json - Current functions data</returns>
    [HttpGet]
    public IActionResult Get()
    {
        var serviceResponse = _resourceFunctionService.GetItems();
        if (serviceResponse.Success)
        {
            return Ok(serviceResponse.ResponseObject);
        }

        return BadRequest(serviceResponse.ResponseObject);
    }

    // GET api/<ResourceFunctionsController>/5
    /// <summary>
    ///     This function will return the specifed function data.
    /// </summary>
    /// <param name="id">int - Function id</param>
    /// <returns>json - Function data</returns>
    [HttpGet("{id}")]
    public IActionResult Get(int id)
    {
        var serviceResponse = _resourceFunctionService.GetItem(id);
        if (serviceResponse.Success)
        {
            return Ok(serviceResponse.ResponseObject);
        }

        return BadRequest(serviceResponse.ResponseObject);
    }

    // POST api/<ResourceFunctionsController>
    /// <summary>
    ///     This function will create/update the specified function data.
    /// </summary>
    /// <param name="item">ResourceFunction (json) - Function data</param>
    /// <returns>bool - PASS/FAIL</returns>
    [HttpPost]
    public IActionResult Post([FromBody] ResourceFunction item)
    {
        var serviceResponse = _resourceFunctionService.PostItem(item);
        if (serviceResponse.Success)
        {
            _adminLogService.PostItem(new AdminLogMessage
            {
                Source = "API", Title = "INFORMATION",
                Message = "Resource Function (" + item.Name + ") added/updated."
            });
            _cacheHelper.InvalidateCacheObject("ResourceFunction");
            return Ok(serviceResponse.ResponseObject);
        }

        return BadRequest(serviceResponse.ResponseObject);
    }

    // POST api/<ResourceFunctionsController>
    /// <summary>
    ///     This function will update all functions data.
    /// </summary>
    /// <param name="items">List - ResourceFunction (json) - All functions data</param>
    /// <returns>bool - PASS/FAIL</returns>
    [HttpPost]
    [Route("[action]")]
    public IActionResult PostConfig([FromBody] List<ResourceFunction> items)
    {
        var serviceResponse = _resourceFunctionService.PostConfig(items);
        if (serviceResponse.Success)
        {
            _adminLogService.PostItem(new AdminLogMessage
                {Source = "API", Title = "INFORMATION", Message = "Resource Functions added/updated."});
            _cacheHelper.InvalidateCacheObject("ResourceFunction");
            return Ok(serviceResponse.ResponseObject);
        }

        return BadRequest(serviceResponse.ResponseObject);
    }

    // DELETE api/<ResourceFunctionsController>/5
    /// <summary>
    ///     This function will delete the specifed function data.
    /// </summary>
    /// <param name="id">int - Function id</param>
    /// <returns>bool - PASS/FAIL</returns>
    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var serviceResponse = _resourceFunctionService.GetItem(id);
        if (serviceResponse.Success)
        {
            var item = (ResourceFunction) serviceResponse.ResponseObject!;
            serviceResponse = _resourceFunctionService.DeleteItem(id);
            if (serviceResponse.Success)
            {
                _adminLogService.PostItem(new AdminLogMessage
                {
                    Source = "API", Title = "INFORMATION",
                    Message = "Resource Function (" + item.Name + ") deleted."
                });
                _cacheHelper.InvalidateCacheObject("ResourceFunction");
                return Ok("Resource Function (" + item.Name + ") deleted.");
            }

            return BadRequest(serviceResponse.ResponseObject);
        }

        return BadRequest(serviceResponse.ResponseObject);
    }
}