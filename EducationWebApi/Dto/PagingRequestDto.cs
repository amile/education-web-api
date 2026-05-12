namespace EducationWebApi;

public record PagingRequestDto(
    int Page = 1,
    int PageSize = 10
);
