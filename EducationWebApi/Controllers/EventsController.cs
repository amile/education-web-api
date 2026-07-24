using EducationWebApi.Application;
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

    [HttpPost]
    public async Task<ActionResult<Guid>> Post([FromBody] CreateEventRequestDto item)
    {
        var result = await _eventsService.AddEventAsync(item);
        return result.Id;
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<EventDto>> Put(Guid id, [FromBody] UpdateEventRequestDto item)
    {
        var result = await _eventsService.ChangeEventAsync(id, item);

        return result;
    }

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

    [HttpPost("{id}/book")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<BookingDto>> Booking(Guid id)
    {
        var booking = await _bookingService.CreateBookingAsync(id);

        return Accepted($"/api/bookings/{booking.Id}", booking);
    }
}