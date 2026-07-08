using EducationWebApi.DAL;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EducationWebApi.Tests;

public class BookingRepositoryTests
{
    private readonly ServiceProvider _serviceProvider;
    private readonly IServiceScope _scope;
    private readonly IBookingRepository _bookingRepository;

    public BookingRepositoryTests()
    {
        var dbName = Guid.NewGuid().ToString();
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase(dbName));
        services.AddScoped<IBookingRepository, BookingRepository>();

        _serviceProvider = services.BuildServiceProvider();
        _scope = _serviceProvider.CreateScope();
        _bookingRepository = _scope.ServiceProvider.GetRequiredService<IBookingRepository>();
    }

    [Fact]
    public async Task ConfirmBooking_Ok()
    {
        //Arrange
        var eventId = Guid.NewGuid();
        var pendingBooking = await _bookingRepository.Add(eventId);
        await _bookingRepository.SaveChanges();

        //Act
        await _bookingRepository.ConfirmBooking(pendingBooking.Id);
        await _bookingRepository.SaveChanges();
        var confirmedBooking = await _bookingRepository.GetById(pendingBooking.Id);

        //Assert
        Assert.NotNull(confirmedBooking);
        Assert.Equal(pendingBooking.Id, confirmedBooking.Id);
        Assert.Equal(BookingStatus.Pending, pendingBooking.Status);
        Assert.Equal(BookingStatus.Confirmed, confirmedBooking.Status);
        Assert.NotNull(confirmedBooking.ProcessedAt);
    }

    [Fact]
    public async Task RejectBooking_Ok()
    {
        //Arrange
        var eventId = Guid.NewGuid();
        var pendingBooking = await _bookingRepository.Add(eventId);
        await _bookingRepository.SaveChanges();

        //Act
        await _bookingRepository.RejectBooking(pendingBooking.Id);
        await _bookingRepository.SaveChanges();
        var rejectedBooking = await _bookingRepository.GetById(pendingBooking.Id);

        //Assert
        Assert.NotNull(rejectedBooking);
        Assert.Equal(pendingBooking.Id, rejectedBooking.Id);
        Assert.Equal(BookingStatus.Pending, pendingBooking.Status);
        Assert.Equal(BookingStatus.Rejected, rejectedBooking.Status);
        Assert.NotNull(rejectedBooking.ProcessedAt);
    }
}
