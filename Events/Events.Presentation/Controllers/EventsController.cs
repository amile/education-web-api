using Events.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Events.Presentation;

[Authorize]
[ApiController]
[Route("api/[controller]")]

public class EventsController : ControllerBase
{
    private readonly IEventsService _eventsService;

    public EventsController(
        IEventsService eventsService
    )
    {
        _eventsService = eventsService;
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedResultDto<EventDto>>> GetAllEvents(
        [FromQuery] EventFilterDto filter,
        [FromQuery] PagingRequestDto pagingRequest
    )
    {
        return await _eventsService.GetEventsAsync(filter, pagingRequest);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<EventDto>> GetEventAsync(Guid id)
    {
        var result = await _eventsService.GetEventAsync(id);

        return result;
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<Guid>> Post([FromBody] CreateEventRequestDto item)
    {
        var result = await _eventsService.AddEventAsync(item);
        return result.Id;
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<ActionResult<EventDto>> Put(Guid id, [FromBody] UpdateEventRequestDto item)
    {
        var result = await _eventsService.ChangeEventAsync(id, item);

        return result;
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _eventsService.RemoveEventAsync(id);
        if (!result)
        {
            return new NotFoundResult();
        }

        return new OkResult(); 
    }
}
