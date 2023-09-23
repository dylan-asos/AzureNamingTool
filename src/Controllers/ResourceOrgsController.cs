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
public class ResourceOrgsController : ControllerBase
{
    private readonly CacheHelper _cacheHelper;
    private readonly ResourceOrgService _resourceOrgService;
    private readonly AdminLogService _adminLogService;

    public ResourceOrgsController(ResourceOrgService resourceOrgService, CacheHelper cacheHelper, AdminLogService adminLogService)
    {
        _resourceOrgService = resourceOrgService;
        _cacheHelper = cacheHelper;
        _adminLogService = adminLogService;
    }

    // GET: api/<ResourceOrgsController>
    /// <summary>
    ///     This function will return the orgs data.
    /// </summary>
    /// <returns>json - Current orgs data</returns>
    [HttpGet]
    public IActionResult Get()
    {
        ServiceResponse serviceResponse = new();

        // Get list of items
        serviceResponse = _resourceOrgService.GetItems();
        if (serviceResponse.Success)
        {
            return Ok(serviceResponse.ResponseObject);
        }

        return BadRequest(serviceResponse.ResponseObject);
    }

    // GET api/<ResourceOrgsController>/5
    /// <summary>
    ///     This function will return the specifed org data.
    /// </summary>
    /// <param name="id">int - Org id</param>
    /// <returns>json - Org data</returns>
    [HttpGet("{id}")]
    public IActionResult Get(int id)
    {
        ServiceResponse serviceResponse = new();

        // Get list of items
        serviceResponse = _resourceOrgService.GetItem(id);
        if (serviceResponse.Success)
        {
            return Ok(serviceResponse.ResponseObject);
        }

        return BadRequest(serviceResponse.ResponseObject);
    }

    // POST api/<ResourceOrgsController>
    /// <summary>
    ///     This function will create/update the specified org data.
    /// </summary>
    /// <param name="item">ResourceOrg (json) - Org data</param>
    /// <returns>bool - PASS/FAIL</returns>
    [HttpPost]
    public IActionResult Post([FromBody] ResourceOrg item)
    {
        ServiceResponse serviceResponse = new();

        serviceResponse = _resourceOrgService.PostItem(item);
        if (serviceResponse.Success)
        {
            _adminLogService.PostItem(new AdminLogMessage
                {Source = "API", Title = "INFORMATION", Message = "Resource Org (" + item.Name + ") added/updated."});
            _cacheHelper.InvalidateCacheObject("ResourceOrg");
            return Ok(serviceResponse.ResponseObject);
        }

        return BadRequest(serviceResponse.ResponseObject);
    }

    // POST api/<ResourceOrgsController>
    /// <summary>
    ///     This function will update all orgs data.
    /// </summary>
    /// <param name="items">List - ResourceOrg (json) - All orgs data</param>
    /// <returns>bool - PASS/FAIL</returns>
    [HttpPost]
    [Route("[action]")]
    public IActionResult PostConfig([FromBody] List<ResourceOrg> items)
    {
        ServiceResponse serviceResponse = new();
        serviceResponse = _resourceOrgService.PostConfig(items);
        if (serviceResponse.Success)
        {
            _adminLogService.PostItem(new AdminLogMessage
                {Source = "API", Title = "INFORMATION", Message = "Resource Orgs added/updated."});
            _cacheHelper.InvalidateCacheObject("ResourceOrg");
            return Ok(serviceResponse.ResponseObject);
        }

        return BadRequest(serviceResponse.ResponseObject);
    }

    // DELETE api/<ResourceOrgsController>/5
    /// <summary>
    ///     This function will delete the specifed org data.
    /// </summary>
    /// <param name="id">int - Org id</param>
    /// <returns>bool - PASS/FAIL</returns>
    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        ServiceResponse serviceResponse = new();
        
        serviceResponse = _resourceOrgService.GetItem(id);
        if (serviceResponse.Success)
        {
            var item = (ResourceOrg) serviceResponse.ResponseObject!;
            serviceResponse = _resourceOrgService.DeleteItem(id);
            if (serviceResponse.Success)
            {
                _adminLogService.PostItem(new AdminLogMessage
                    {Source = "API", Title = "INFORMATION", Message = "Resource Org (" + item.Name + ") deleted."});
                _cacheHelper.InvalidateCacheObject("ResourceOrg");
                return Ok("Resource Org (" + item.Name + ") deleted.");
            }

            return BadRequest(serviceResponse.ResponseObject);
        }

        return BadRequest(serviceResponse.ResponseObject);
    }
}