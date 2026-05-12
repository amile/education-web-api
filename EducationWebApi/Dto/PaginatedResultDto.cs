using System.Diagnostics.CodeAnalysis;

namespace EducationWebApi;

public class PaginatedResultDto<T>
{
    public required T[] Data { get; set; }
    public required int TotalCount { get; set; }
    public required int CurrentPage { get; set; }
    public required int PageSize { get; set; }

    [SetsRequiredMembers]
    public PaginatedResultDto(T[] data, int totalCount, int currentPage, int pageSize)
    {
        Data = data;
        TotalCount = totalCount;
        CurrentPage = currentPage;
        PageSize = pageSize;
    }
}