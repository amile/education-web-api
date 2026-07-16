using System.ComponentModel.DataAnnotations;
using System.Data;
using EducationWebApi.Domain;

namespace EducationWebApi.Application;

public class EventsService : IEventsService
{
    private readonly IEventsRepository _eventsRepository;

    public EventsService(IEventsRepository eventsRepository)
    {
        _eventsRepository = eventsRepository;
    }

    public async Task<PaginatedResultDto<EventDto>> GetEventsAsync(EventFilterDto filter, PagingRequestDto pagingRequest, CancellationToken ct = default)
    {
        var domainFilter = filter.ToDomain();
        var domainPaging = pagingRequest.ToDomain();
        var result = await _eventsRepository.GetAllEventsAsync(domainFilter, domainPaging, ct);
        var data = result.Data.Select(EventDto.FromDomain).ToArray();
    
        return new PaginatedResultDto<EventDto>(data, result.TotalCount, result.CurrentPage, result.PageSize);
    }

    public async Task<EventDto> GetEventAsync(Guid id, CancellationToken ct = default)
    {
        var domainEvent = await _eventsRepository.GetEventByIdAsync(id);

        if (domainEvent is null)
        {
            throw new KeyNotFoundException($"Event Id: {id} not found");
        }

        return EventDto.FromDomain(domainEvent);
    }

    public async Task<EventDto> AddEventAsync(CreateEventRequestDto item, CancellationToken ct = default)
    {
        if (item.TotalSeats <= 0)
        {
            throw new ValidationException("Общее количество мест должно быть больше 0");
        }

        var newEvent = new Event(item.Title, item.Description, item.StartAt, item.EndAt, item.TotalSeats);
        await _eventsRepository.AddEventAsync(newEvent, ct);
        await _eventsRepository.SaveChangesAsync(ct);

        return EventDto.FromDomain(newEvent);
    }

    public async Task<EventDto> ChangeEventAsync(Guid id, UpdateEventRequestDto item, CancellationToken ct = default)
    {
        var savedEvent = await _eventsRepository.GetEventByIdAsync(id);

        if (savedEvent is null)
        {
            throw new KeyNotFoundException($"Event Id: {id} not found");
        }

        savedEvent.Title = item.Title;
        savedEvent.Description = item.Description;
        savedEvent.StartAt = item.StartAt;
        savedEvent.EndAt = item.EndAt;

        await _eventsRepository.ChangeEventAsync(savedEvent, ct);
        await _eventsRepository.SaveChangesAsync(ct);

        return EventDto.FromDomain(savedEvent);
    }

    public async Task<bool> RemoveEventAsync(Guid id, CancellationToken ct = default)
    {
        var domainEvent = await _eventsRepository.GetEventByIdAsync(id);

        if (domainEvent is null)
        {
            return false;
        }

        await _eventsRepository.RemoveEventAsync(id);
        await _eventsRepository.SaveChangesAsync();

        return true;
    }
}
