using EducationWebApi.Domain;

namespace EducationWebApi.Application;

public record PagingRequestDto(
    int Page = 1,
    int PageSize = 10
)
{
    public PagingRequest ToDomain() => new(Page, PageSize);
}
