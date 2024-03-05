using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using ToDoListApi.Handlers.Tags;

namespace ToDoListApi.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TagsController : ControllerBase
{
    private readonly IMediator _mediator;

    public TagsController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [HttpPost]
    [SwaggerOperation("Create Tag")]
    [ProducesResponseType(typeof(CreateTagResponse), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Post([FromBody]CreateTagRequest request)
    {
        var result = await _mediator.Send(request);
        return Ok(result);
    }
    
    [HttpPut]
    [SwaggerOperation("Edit Tag")]
    [ProducesResponseType(typeof(UpdateTagResponse), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Put([FromBody]UpdateTagRequest request)
    {
        var result = await _mediator.Send(request);
        return Ok(result);
    }
    
    [HttpDelete("{Id}")]
    [SwaggerOperation("Delete Tag")]
    [ProducesResponseType(typeof(DeleteTagResponse), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Delete([FromRoute]DeleteTagRequest request)
    {
        var result = await _mediator.Send(request);
        return Ok(result);
    }
    
    [HttpGet]
    [SwaggerOperation("Get All Tags Created By Current User")]
    [ProducesResponseType(typeof(GetAllTagsResponse), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Get()
    {
        var result = await _mediator.Send(new GetAllTagsRequest());
        return Ok(result);
    }
}