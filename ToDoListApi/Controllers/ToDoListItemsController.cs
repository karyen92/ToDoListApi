using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
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
    [SwaggerOperation("Create a To Do List Item")]
    public async Task<IActionResult> Post([FromBody]CreateToDoListItemRequest request)
    {
        var result = await _mediator.Send(request);
        return Ok(result);
    }
    
    [HttpPut]
    [SwaggerOperation("Edit a To Do List Item")]
    public async Task<IActionResult> Put([FromBody]UpdateToDoListItemRequest request)
    {
        var result = await _mediator.Send(request);
        return Ok(result);
    }
    
    [HttpDelete("{Id}")]
    [SwaggerOperation("Delete To Do List Item")]
    public async Task<IActionResult> Delete([FromRoute]DeleteToDoListItemRequest request)
    {
        var result = await _mediator.Send(request);
        return Ok(result);
    }
    
    [HttpPost("query")]
    [SwaggerOperation("Query To Do List Items")]
    public async Task<IActionResult> Query([FromBody]QueryToDoListItemsRequest request)
    {
        var result = await _mediator.Send(request);
        return Ok(result);
    }
}