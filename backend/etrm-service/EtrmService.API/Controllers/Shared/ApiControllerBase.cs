using Microsoft.AspNetCore.Mvc;

namespace EtrmService.API.Controllers.Shared;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
}
