using Bookings.Domain;

namespace Bookings.Application;

public interface IBookingProviderService
{
    Task Publish(Booking booking, CancellationToken cancellationToken = default);
}
