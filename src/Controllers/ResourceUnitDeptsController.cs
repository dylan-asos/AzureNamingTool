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
public class ResourceUnitDeptsController : ControllerBase
{
    private readonly AdminLogService _adminLogService;
    private readonly CacheHelper _cacheHelper;
    private readonly ResourceUnitDeptService _resourceUnitDeptService;

    public ResourceUnitDeptsController(ResourceUnitDeptService resourceUnitDeptService, CacheHelper cacheHelper,
        AdminLogService adminLogService)
    {
        _resourceUnitDeptService = resourceUnitDeptService;
        _cacheHelper = cacheHelper;
        _adminLogService = adminLogService;
    }

    // GET: api/<ResourceUnitDeptsController>
    /// <summary>
    ///     This function will return the units/depts data.
    /// </summary>
    /// <returns>json - Current units/depts data</returns>
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var serviceResponse = await _resourceUnitDeptService.GetItems();
        if (serviceResponse.Success)
        {
            return Ok(serviceResponse.ResponseObject);
        }

        return BadRequest(serviceResponse.ResponseObject);
    }

    // GET api/<ResourceUnitDeptsController>/5
    /// <summary>
    ///     This function will return the specifed unit/dept data.
    /// </summary>
    /// <param name="id">int - Unit/Dept id</param>
    /// <returns>json - Unit/Dept data</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var serviceResponse = await _resourceUnitDeptService.GetItem(id);
        if (serviceResponse.Success)
        {
            return Ok(serviceResponse.ResponseObject);
        }

        return BadRequest(serviceResponse.ResponseObject);
    }

    // POST api/<ResourceUnitDeptsController>
    /// <summary>
    ///     This function will create/update the specified unit/dept data.
    /// </summary>
    /// <param name="item">ResourceUnitDept (json) - Unit/Dept data</param>
    /// <returns>bool - PASS/FAIL</returns>
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] ResourceUnitDept item)
    {
        var serviceResponse = await _resourceUnitDeptService.PostItem(item);

        if (!serviceResponse.Success)
            return BadRequest(serviceResponse.ResponseObject);

        await _adminLogService.PostItem(new AdminLogMessage
        {
            Source = "API", Title = "INFORMATION",
            Message = "Resource Unit/Department (" + item.Name + ") added/updated."
        });

        _cacheHelper.InvalidateCacheObject("ResourceUnitDept");
        return Ok(serviceResponse.ResponseObject);
    }

    // POST api/<ResourceUnitDeptsController>
    /// <summary>
    ///     This function will update all units/depts data.
    /// </summary>
    /// <param name="items">List - ResourceUnitDept (json) - All units/depts data</param>
    /// <returns>bool - PASS/FAIL</returns>
    [HttpPost]
    [Route("[action]")]
    public async Task<IActionResult> PostConfig([FromBody] List<ResourceUnitDept> items)
    {
        var serviceResponse = await _resourceUnitDeptService.PostConfig(items);
        if (!serviceResponse.Success)
            return BadRequest(serviceResponse.ResponseObject);

        await _adminLogService.PostItem(new AdminLogMessage
            {Source = "API", Title = "INFORMATION", Message = "Resource Units/Departments added/updated."});
        _cacheHelper.InvalidateCacheObject("ResourceUnitDept");

        return Ok(serviceResponse.ResponseObject);
    }

    // DELETE api/<ResourceUnitDeptsController>/5
    /// <summary>
    ///     This function will delete the specifed unit/dept data.
    /// </summary>
    /// <param name="id">int - Unit/Dept id</param>
    /// <returns>bool - PASS/FAIL</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var serviceResponse = await _resourceUnitDeptService.GetItem(id);
        if (!serviceResponse.Success)
            return BadRequest(serviceResponse.ResponseObject);

        var item = (ResourceUnitDept) serviceResponse.ResponseObject!;
        serviceResponse = await _resourceUnitDeptService.DeleteItem(id);
        if (!serviceResponse.Success)
            return BadRequest(serviceResponse.ResponseObject);

        await _adminLogService.PostItem(new AdminLogMessage
        {
            Source = "API", Title = "INFORMATION",
            Message = "Resource Unit/Department (" + item.Name + ") deleted."
        });

        _cacheHelper.InvalidateCacheObject("ResourceUnitDept");
        return Ok("Resource Unit/Department (" + item.Name + ") deleted.");
    }
}