using System.Security.Claims;
using Bookings.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bookings.Presentation;

[Authorize]
[ApiController]
[Route("api/[controller]")]

public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingsController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<BookingDto>> Get(Guid id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
        if (userId == null)
        {
            return Unauthorized();
        }

        var booking = await _bookingService.GetBookingByIdAsync(id, Guid.Parse(userId), userRole);

        return booking;
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<BookingDto>> Cancel(Guid id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
        if (userId == null)
        {
            return Unauthorized();
        }
        var booking = await _bookingService.CancelBookingAsync(id, Guid.Parse(userId), userRole);

        return booking;
    }

    [HttpPost("/events/{id}/book")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<BookingDto>> Booking(Guid id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
        {
            return Unauthorized();
        }
        var booking = await _bookingService.CreateBookingAsync(id, Guid.Parse(userId));

        return Accepted($"/api/bookings/{booking.Id}", booking);
    }
}