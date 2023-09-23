using AzureNamingTool.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace AzureNamingTool.Controllers;

[Route("api/[controller]")]
[ApiController]
[ApiKey]
public class PolicyController : ControllerBase
{
    // the policy controller is used to create an azure policy definition that can be used to enforce
    // the naming conventions created by this application. 

    // this allows you to define the names and make them available to engineers at design time, but also
    // enforce the same logic at deployment time.
}