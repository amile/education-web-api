using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace EducationWebApi;

[ApiController]
[Route("api/[controller]")]

public class EventsController(IEventsService _eventsService)
{
    [HttpGet]
    public ActionResult<PaginatedResultDto<EventDto>> GetAllEvents(
        [FromQuery] EventFilterDto filter,
        [FromQuery] PagingRequestDto pagingRequest
    )
    {
        return _eventsService.GetEvents(filter, pagingRequest);
    }

    [HttpGet("{id}")]
    public ActionResult<EventDto> GetEvent(Guid id)
    {
        var result = _eventsService.GetEvent(id);

        return result;
    }

    [HttpPost]
    public ActionResult<Guid> Post([FromBody] EventRequestDto item)
    {
        return _eventsService.AddEvent(item).Id;
    }

    [HttpPut("{id}")]
    public ActionResult<EventDto> Put(Guid id, [FromBody] EventRequestDto item)
    {
        var result = _eventsService.ChangeEvent(id, item);

        return result;
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(Guid id)
    {

        if (!_eventsService.RemoveEvent(id))
        {
            return new NotFoundResult();
        }

        return new OkResult(); 
    }
}