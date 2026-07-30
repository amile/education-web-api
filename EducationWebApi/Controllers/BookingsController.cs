using System.Security.Claims;
using EducationWebApi.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EducationWebApi;

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
}