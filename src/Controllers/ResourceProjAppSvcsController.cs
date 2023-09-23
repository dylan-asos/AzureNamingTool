using AzureNamingTool.Attributes;
using AzureNamingTool.Helpers;
using AzureNamingTool.Models;
using AzureNamingTool.Services;
using Microsoft.AspNetCore.Mvc;

namespace AzureNamingTool.Controllers;

[Route("api/[controller]")]
[ApiController]
[ApiKey]
public class ResourceProjAppSvcsController : ControllerBase
{
    private readonly CacheHelper _cacheHelper;
    private readonly ResourceProjAppSvcService _resourceProjAppSvcService;
    private AdminLogService _adminLogService;

    public ResourceProjAppSvcsController(ResourceProjAppSvcService resourceProjAppSvcService, CacheHelper cacheHelper, AdminLogService adminLogService)
    {
        _resourceProjAppSvcService = resourceProjAppSvcService;
        _cacheHelper = cacheHelper;
        _adminLogService = adminLogService;
    }

    // GET: api/<ResourceProjAppSvcsController>
    /// <summary>
    ///     This function will return the projects/apps/services data.
    /// </summary>
    /// <returns>json - Current projects/apps/servicse data</returns>
    [HttpGet]
    public IActionResult Get()
    {
        var serviceResponse = _resourceProjAppSvcService.GetItems();
        if (serviceResponse.Success)
        {
            return Ok(serviceResponse.ResponseObject);
        }

        return BadRequest(serviceResponse.ResponseObject);
    }

    // GET api/<ResourceProjAppSvcsController>/5
    /// <summary>
    ///     This function will return the specifed project/app/service data.
    /// </summary>
    /// <param name="id">int - Project/App/Service id</param>
    /// <returns>json - Project/App/Service data</returns>
    [HttpGet("{id}")]
    public IActionResult Get(int id)
    {
        var serviceResponse = _resourceProjAppSvcService.GetItem(id);
        if (serviceResponse.Success)
        {
            return Ok(serviceResponse.ResponseObject);
        }

        return BadRequest(serviceResponse.ResponseObject);
    }

    // POST api/<ResourceProjAppSvcsController>
    /// <summary>
    ///     This function will create/update the specified project/app/service data.
    /// </summary>
    /// <param name="item">ResourceProjAppSvc (json) - Project/App/Service data</param>
    /// <returns>bool - PASS/FAIL</returns>
    [HttpPost]
    public IActionResult Post([FromBody] ResourceProjAppSvc item)
    {
        var serviceResponse = _resourceProjAppSvcService.PostItem(item);
        if (serviceResponse.Success)
        {
            _adminLogService.PostItem(new AdminLogMessage
            {
                Source = "API", Title = "INFORMATION",
                Message = "Resource Project/App/Service (" + item.Name + ") added/updated."
            });
            _cacheHelper.InvalidateCacheObject("ResourceProjAppSvc");
            return Ok(serviceResponse.ResponseObject);
        }

        return BadRequest(serviceResponse.ResponseObject);
    }

    // POST api/<ResourceProjAppSvcsController>
    /// <summary>
    ///     This function will update all projects/apps/services data.
    /// </summary>
    /// <param name="items">List - ResourceProjAppSvc (json) - All projects/apps/services data</param>
    /// <returns>bool - PASS/FAIL</returns>
    [HttpPost]
    [Route("[action]")]
    public IActionResult PostConfig([FromBody] List<ResourceProjAppSvc> items)
    {
        var serviceResponse = _resourceProjAppSvcService.PostConfig(items);
        if (serviceResponse.Success)
        {
            _adminLogService.PostItem(new AdminLogMessage
                {Source = "API", Title = "INFORMATION", Message = "Resource Projects/Apps/Services added/updated."});
            _cacheHelper.InvalidateCacheObject("ResourceProjAppSvc");
            return Ok(serviceResponse.ResponseObject);
        }

        return BadRequest(serviceResponse.ResponseObject);
    }

    // DELETE api/<ResourceProjAppSvcsController>/5
    /// <summary>
    ///     This function will delete the specifed project/app/service data.
    /// </summary>
    /// <param name="id">int - Project/App?service id</param>
    /// <returns>bool - PASS/FAIL</returns>
    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var serviceResponse = _resourceProjAppSvcService.GetItem(id);
        if (serviceResponse.Success)
        {
            var item = (ResourceProjAppSvc) serviceResponse.ResponseObject!;
            serviceResponse = _resourceProjAppSvcService.DeleteItem(id);
            if (serviceResponse.Success)
            {
                _adminLogService.PostItem(new AdminLogMessage
                {
                    Source = "API", Title = "INFORMATION",
                    Message = "Resource Project/App/Service (" + item.Name + ") deleted."
                });
                _cacheHelper.InvalidateCacheObject("ResourceProjAppSvc");
                return Ok("Resource Project/App/Service (" + item.Name + ") deleted.");
            }

            return BadRequest(serviceResponse.ResponseObject);
        }

        return BadRequest(serviceResponse.ResponseObject);
    }
}