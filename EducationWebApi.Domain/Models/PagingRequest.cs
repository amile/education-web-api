namespace EducationWebApi.Domain;

public record PagingRequest(
    int Page,
    int PageSize
);
