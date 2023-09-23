using AzureNamingTool.Attributes;
using AzureNamingTool.Helpers;
using AzureNamingTool.Models;
using AzureNamingTool.Services;
using Microsoft.AspNetCore.Mvc;

namespace AzureNamingTool.Controllers;

[Route("api/[controller]")]
[ApiController]
[ApiKey]
public class ResourceNamingRequestsController : ControllerBase
{
    private readonly ResourceTypeService _resourceTypeService;
    private readonly ResourceNamingRequestService _resourceNamingRequestService;

    public ResourceNamingRequestsController(ResourceTypeService resourceTypeService, ResourceNamingRequestService resourceNamingRequestService)
    {
        _resourceTypeService = resourceTypeService;
        _resourceNamingRequestService = resourceNamingRequestService;
    }
    
    // POST api/<ResourceNamingRequestsController>
    /// <summary>
    ///     This function will generate a resoure type name for specifed component values. This function requires full
    ///     definition for all components. It is recommended to use the RequestName API function for name generation.
    /// </summary>
    /// <param name="request">ResourceNameRequestWithComponents (json) - Resource Name Request data</param>
    /// <returns>string - Name generation response</returns>
    [HttpPost]
    [Route("[action]")]
    public IActionResult RequestNameWithComponents([FromBody] ResourceNameRequestWithComponents request)
    {
        var resourceNameRequestResponse = _resourceNamingRequestService.RequestNameWithComponents(request);
        if (resourceNameRequestResponse.Success)
        {
            return Ok(resourceNameRequestResponse);
        }

        return BadRequest(resourceNameRequestResponse);
    }

    // POST api/<ResourceNamingRequestsController>
    /// <summary>
    ///     This function will generate a resoure type name for specifed component values, using a simple data format.
    /// </summary>
    /// <param name="request">ResourceNameRequest (json) - Resource Name Request data</param>
    /// <returns>string - Name generation response</returns>
    [HttpPost]
    [Route("[action]")]
    public async Task<IActionResult> RequestName([FromBody] ResourceNameRequest request)
    {
        request.CreatedBy = "API";
        var resourceNameRequestResponse = await _resourceNamingRequestService.RequestName(request);
        if (resourceNameRequestResponse.Success)
        {
            return Ok(resourceNameRequestResponse);
        }

        return BadRequest(resourceNameRequestResponse);
    }

    // POST api/<ResourceNamingRequestsController>
    /// <summary>
    ///     This function will validate the name for the specified resource type. NOTE: This function does not validate using
    ///     the tool configuration, only the regex for the specified resource type. Use the RequestName function to validate
    ///     using the tool configuration.
    /// </summary>
    /// <param name="validateNameRequest">ValidateNameRequest (json) - Validate Name Request data</param>
    /// <returns>ValidateNameResponse - Name validation response</returns>
    [HttpPost]
    [Route("[action]")]
    public IActionResult ValidateName([FromBody] ValidateNameRequest validateNameRequest)
    {
        var serviceResponse = _resourceTypeService.ValidateResourceTypeName(validateNameRequest);
        if (!serviceResponse.Success) 
            return BadRequest("There was a problem validating the name.");
        
        if (serviceResponse.ResponseObject != null)
        {
            var validateNameResponse = (ValidateNameResponse) serviceResponse.ResponseObject!;
            return Ok(validateNameResponse);
        }

        return BadRequest("There was a problem validating the name.");
    }
}