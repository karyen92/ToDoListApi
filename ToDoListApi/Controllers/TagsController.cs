using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    public async Task<IActionResult> Post([FromBody]CreateTagRequest request)
    {
        var result = await _mediator.Send(request);
        return Ok(result);
    }
    
    [HttpPut]
    public async Task<IActionResult> Put([FromBody]UpdateTagRequest request)
    {
        var result = await _mediator.Send(request);
        return Ok(result);
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete([FromRoute]DeleteTagRequest request)
    {
        var result = await _mediator.Send(request);
        return Ok(result);
    }
    
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var result = await _mediator.Send(new GetAllTagsRequest());
        return Ok(result);
    }
}