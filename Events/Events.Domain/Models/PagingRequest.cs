namespace Events.Domain;

public record PagingRequest(
    int Page,
    int PageSize
);
