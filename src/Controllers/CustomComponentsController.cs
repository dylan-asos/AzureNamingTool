using AzureNamingTool.Attributes;
using AzureNamingTool.Helpers;
using AzureNamingTool.Models;
using AzureNamingTool.Services;
using Microsoft.AspNetCore.Mvc;

namespace AzureNamingTool.Controllers;

[Route("api/[controller]")]
[ApiController]
[ApiKey]
public class CustomComponentsController : ControllerBase
{
    private readonly CacheHelper _cacheHelper;
    private readonly CustomComponentService _customComponentService;
    private readonly GeneralHelper _generalHelper;
    private readonly ResourceComponentService _resourceComponentService;
    private readonly AdminLogService _adminLogService;

    public CustomComponentsController(
        CustomComponentService customComponentService,
        GeneralHelper generalHelper,
        CacheHelper cacheHelper,
        ResourceComponentService resourceComponentService, AdminLogService adminLogService)
    {
        _customComponentService = customComponentService;
        _generalHelper = generalHelper;
        _cacheHelper = cacheHelper;
        _resourceComponentService = resourceComponentService;
        _adminLogService = adminLogService;
    }

    // GET: api/<CustomComponentsController>
    /// <summary>
    ///     This function will return the custom components data.
    /// </summary>
    /// <returns>json - Current custom components data</returns>
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var serviceResponse = await _customComponentService.GetItems();
        
        if (serviceResponse.Success)
        {
            return Ok(serviceResponse.ResponseObject);
        }

        return BadRequest(serviceResponse.ResponseObject);
    }

    // GET api/<CustomComponentsController>/sample
    /// <summary>
    ///     This function will return the custom components data for the specifc parent component type.
    /// </summary>
    /// <param name="parenttype">string - Parent Component Type Name</param>
    /// <returns>json - Current custom components data</returns>
    [Route("[action]/{parenttype}")]
    [HttpGet]
    public async Task<IActionResult> GetByParentType(string parenttype)
    {
        var serviceResponse = await _customComponentService
            .GetItemsByParentType(_generalHelper.NormalizeName(parenttype, true));
        
        if (serviceResponse.Success)
        {
            return Ok(serviceResponse.ResponseObject);
        }

        return BadRequest(serviceResponse.ResponseObject);
    }

    // GET api/<CustomComponentsController>/5
    /// <summary>
    ///     This function will return the specifed custom component data.
    /// </summary>
    /// <param name="id">int - Custom Component id</param>
    /// <returns>json - Custom component data</returns>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var serviceResponse =
            // Get list of items
            await _customComponentService.GetItem(id);
        
        if (serviceResponse.Success)
        {
            return Ok(serviceResponse.ResponseObject);
        }

        return BadRequest(serviceResponse.ResponseObject);
    }

    // POST api/<CustomComponentsController>
    /// <summary>
    ///     This function will create/update the specified custom component data.
    /// </summary>
    /// <param name="item">CustomComponent (json) - Custom component data</param>
    /// <returns>bool - PASS/FAIL</returns>
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] CustomComponent item)
    {
        var serviceResponse = await _customComponentService.PostItem(item);
        if (!serviceResponse.Success)
            return BadRequest(serviceResponse.ResponseObject);
        
        await _adminLogService.PostItem(new AdminLogMessage
            {Source = "API", Title = "INFORMATION", Message = "Custom Component (" + item.Name + ") updated."});
        _cacheHelper.InvalidateCacheObject("CustomComponent");
        
        return Ok(serviceResponse.ResponseObject);
    }

    // POST api/<CustomComponentsController>
    /// <summary>
    ///     This function will update all custom components data.
    /// </summary>
    /// <param name="items">List-CustomComponent (json) - All custom components data. (Legacy functionality).</param>
    /// <returns>bool - PASS/FAIL</returns>
    [HttpPost]
    [Route("[action]")]
    public async Task<IActionResult> PostConfig([FromBody] List<CustomComponent> items)
    {
        var serviceResponse = _customComponentService.PostConfig(items);
        if (!serviceResponse.Success) 
            return BadRequest(serviceResponse.ResponseObject);
        
        await _adminLogService.PostItem(new AdminLogMessage
            {Source = "API", Title = "INFORMATION", Message = "Custom Components updated."});
        _cacheHelper.InvalidateCacheObject("CustomComponent");
        return Ok(serviceResponse.ResponseObject);
    }

    // POST api/<CustomComponentsController>
    /// <summary>
    ///     This function will update all custom components data.
    /// </summary>
    /// <param name="config">CustomComponmentConfig (json) - Full custom components data with parent component data.</param>
    /// <returns>bool - PASS/FAIL</returns>
    [HttpPost]
    [Route("[action]")]
    public async Task<IActionResult> PostConfigWithParentData([FromBody] CustomComponentConfig config)
    {
        List<ResourceComponent> currentresourcecomponents = new();
        List<CustomComponent> newcustomcomponents = new();
        // Get the current resource components
        var serviceResponse = await _resourceComponentService.GetItems(true);

        if (!serviceResponse.Success) 
        return BadRequest(serviceResponse.ResponseObject);
        
        if (serviceResponse.ResponseObject != null)
        {
            currentresourcecomponents = serviceResponse.ResponseObject!;

            // Loop through the posted components
            if (config.ParentComponents!= null)
            {
                foreach (var thisparentcomponent in config.ParentComponents)
                {
                    // Check if the posted component exists in the current components
                    if (!currentresourcecomponents.Exists(x => x.Name == thisparentcomponent.Name))
                    {
                        // Add the custom component
                        ResourceComponent newcustomcomponent = new()
                        {
                            Name = thisparentcomponent.Name,
                            DisplayName = thisparentcomponent.Name,
                            IsCustom = true
                        };
                        serviceResponse = await _resourceComponentService.PostItem(newcustomcomponent);

                        if (serviceResponse.Success)
                        {
                            // Add the new custom component to the list
                            currentresourcecomponents.Add(newcustomcomponent);
                        }
                        else
                        {
                            return BadRequest(serviceResponse.ResponseObject);
                        }
                    }
                }
            }
        }

        if (config.CustomComponents!= null)
        {
            if (config.CustomComponents.Count > 0)
            {
                // Loop through custom components to make sure the parent exists
                foreach (var thiscustomcomponent in config.CustomComponents)
                {
                    if (currentresourcecomponents.Any(x =>
                            _generalHelper.NormalizeName(x.Name, true) == thiscustomcomponent.ParentComponent))
                    {
                        newcustomcomponents.Add(thiscustomcomponent);
                    }
                }

                // Update the custom component options
                serviceResponse = _customComponentService.PostConfig(newcustomcomponents);
                if (!serviceResponse.Success)
                {
                    return BadRequest(serviceResponse.ResponseObject);
                }
            }
        }

        await _adminLogService.PostItem(new AdminLogMessage
            {Source = "API", Title = "INFORMATION", Message = "Custom Components updated."});
        _cacheHelper.InvalidateCacheObject("CustomComponent");
        return Ok("Custom Component configuration updated!");

    }

    // DELETE api/<CustomComponentsController>/5
    /// <summary>
    ///     This function will delete the specifed custom component data.
    /// </summary>
    /// <param name="id">int - Custom component id</param>
    /// <returns>bool - PASS/FAIL</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var serviceResponse = await _customComponentService.GetItem(id);

        if (!serviceResponse.Success) 
            return BadRequest(serviceResponse.ResponseObject);
        
        var item = (CustomComponent) serviceResponse.ResponseObject!;
        serviceResponse = await _customComponentService.DeleteItem(id);

        if (!serviceResponse.Success) 
            return BadRequest(serviceResponse.ResponseObject);
        
        await _adminLogService.PostItem(new AdminLogMessage
            {Source = "API", Title = "INFORMATION", Message = "Custom Component (" + item.Name + ") deleted."});
        _cacheHelper.InvalidateCacheObject("GeneratedName");
        return Ok("Custom Component (" + item.Name + ") deleted.");
    }
}