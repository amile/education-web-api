using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace EducationWebApi;

[ApiController]
[Route("api/[controller]")]

public class EventsController : ControllerBase
{
    private readonly IEventsService _eventsService;
    private readonly IBookingService _bookingService;
    public EventsController(
        IEventsService eventsService,
        IBookingService bookingService
    )
    {
        _eventsService = eventsService;
        _bookingService = bookingService;
    }

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
    public ActionResult<Guid> Post([FromBody] CreateEventRequestDto item)
    {
        return _eventsService.AddEvent(item).Id;
    }

    [HttpPut("{id}")]
    public ActionResult<EventDto> Put(Guid id, [FromBody] UpdateEventRequestDto item)
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

    [HttpPost("{id}/book")]
    public async Task<ActionResult<BookingDto>> Booking(Guid id)
    {
        var booking = await _bookingService.CreateBookingAsync(id);

        return Accepted($"/api/bookings/{booking.Id}", booking);
    }
}