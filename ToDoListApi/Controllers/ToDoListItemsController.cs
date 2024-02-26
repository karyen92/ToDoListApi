using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ToDoListApi.Handlers.ToDoListItems;

namespace ToDoListApi.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ToDoListItemsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ToDoListItemsController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [HttpPost]
    public async Task<IActionResult> Post([FromBody]CreateToDoListItemRequest request)
    {
        var result = await _mediator.Send(request);
        return Ok(result);
    }
    
    [HttpPut]
    public async Task<IActionResult> Put([FromBody]UpdateToDoListItemRequest request)
    {
        var result = await _mediator.Send(request);
        return Ok(result);
    }
    
    [HttpDelete("{Id}")]
    public async Task<IActionResult> Delete([FromRoute]DeleteToDoListItemRequest request)
    {
        var result = await _mediator.Send(request);
        return Ok(result);
    }
    
    [HttpPost("query")]
    public async Task<IActionResult> Query([FromBody]QueryToDoListItemsRequest request)
    {
        var result = await _mediator.Send(request);
        return Ok(result);
    }
}