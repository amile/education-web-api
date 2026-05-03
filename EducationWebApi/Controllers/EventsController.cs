using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace EducationWebApi;

[ApiController]
[Route("api/[controller]")]

public class EventsController(IEventsService _eventsService)
{
    [HttpGet]
    public ActionResult<List<EventDto>> GetAllEvents()
    {
        return _eventsService.GetEvents();
    }

    [HttpGet("{id}")]
    public ActionResult<EventDto> GetBuildingByIndex(Guid id)
    {
        var result = _eventsService.GetEvent(id);

        if (result is null)
        {
            return new NotFoundResult();
        }

        return result;
    }

    [HttpPost]
    public IActionResult Post([FromBody] EventRequestDto item)
    {
        _eventsService.AddEvent(item);

        return new CreatedResult();
    }

    [HttpPut("{id}")]
    public IActionResult Put(Guid id, [FromBody] EventRequestDto item)
    {
        if (!_eventsService.ChangeEvent(id, item))
        {
            return new NotFoundResult();
        }

        return new NoContentResult();
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