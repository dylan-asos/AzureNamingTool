using AzureNamingTool.Attributes;
using AzureNamingTool.Models;
using AzureNamingTool.Services;
using Microsoft.AspNetCore.Mvc;

namespace AzureNamingTool.Controllers;

[Route("api/[controller]")]
[ApiController]
[ApiKey]
public class ImportExportController : ControllerBase
{
    private readonly ImportExportService _importExportService;

    public ImportExportController(ImportExportService importExportService)
    {
        _importExportService = importExportService;
    }
    
    // GET: api/<ImportExportController>
    /// <summary>
    ///     This function will export the current configuration data (all components) as a single JSON file.
    /// </summary>
    /// <returns>json - JSON configuration file</returns>
    [HttpGet]
    [Route("[action]")]
    public IActionResult ExportConfiguration(bool includeAdmin = false)
    {
        var serviceResponse = _importExportService.ExportConfig(includeAdmin);
        
        return serviceResponse.Success 
            ? (IActionResult) Ok(serviceResponse.ResponseObject) 
            : (IActionResult) BadRequest(serviceResponse.ResponseObject);
    }

    // POST api/<ImportExportController>
    /// <summary>
    ///     This function will import the provided configuration data (all components). This will overwrite the existing
    ///     configuration data.
    /// </summary>
    /// <param name="configData">ConfigurationData (json) - Tool configuration File</param>
    /// <returns>bool - PASS/FAIL</returns>
    [HttpPost]
    [Route("[action]")]
    public async Task<IActionResult> ImportConfiguration([FromBody] ConfigurationData configData)
    {
        var serviceResponse = await _importExportService.PostConfig(configData);
        
        return serviceResponse.Success 
            ? (IActionResult) Ok(serviceResponse.ResponseObject) 
            : (IActionResult) BadRequest(serviceResponse.ResponseObject);
    }
}