using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace EducationWebApi;

[ApiController]
[Route("api/[controller]")]

public class BookingController() : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingController(IBookingService bookingService) : this()
    {
        _bookingService = bookingService;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<BookingDto>> Get(Guid id)
    {
        var booking = await _bookingService.GetBookingByIdAsync(id);

        if (booking is null)
        {
            return new NotFoundResult();
        }

        return booking;
    }
}