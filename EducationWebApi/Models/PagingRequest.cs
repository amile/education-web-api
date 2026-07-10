namespace EducationWebApi;

public record PagingRequest(
    int Page,
    int PageSize
)
{
    public static PagingRequest FromApi(PagingRequestDto model) => new(model.Page, model.PageSize);
}
