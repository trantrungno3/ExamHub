using ExamHub.Core.DataTransferObjects.Common;
using Xunit;

namespace ExamHub.Tests;

/// <summary>
/// Clamp phân trang. Không clamp thì `pageSize=100000` là một request quét cả bảng, còn `page=0`
/// hoặc số âm cho offset âm — PostgreSQL ném lỗi thay vì trả trang đầu.
/// </summary>
public class PaginationTests
{
    [Theory]
    [InlineData(0, 1)]
    [InlineData(-1, 1)]
    [InlineData(-100, 1)]
    [InlineData(1, 1)]
    [InlineData(7, 7)]
    public void Page_is_never_below_one(int requested, int expected)
    {
        var (page, _) = PageRequest.Normalize(requested, 20);

        Assert.Equal(expected, page);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(-5, 1)]
    [InlineData(1, 1)]
    [InlineData(20, 20)]
    [InlineData(100, 100)]
    [InlineData(101, 100)]
    [InlineData(100_000, 100)]
    public void PageSize_stays_between_one_and_one_hundred(int requested, int expected)
    {
        var (_, pageSize) = PageRequest.Normalize(1, requested);

        Assert.Equal(expected, pageSize);
    }

    [Fact]
    public void Offset_uses_the_normalized_values()
    {
        // page 0 → 1 ⇒ offset 0, không phải -20.
        Assert.Equal(0, new PageRequest(0, 20).Normalized().Offset);
        Assert.Equal(40, new PageRequest(3, 20).Normalized().Offset);
        // pageSize 100000 → 100 ⇒ offset theo 100.
        Assert.Equal(100, new PageRequest(2, 100_000).Normalized().Offset);
    }

    [Fact]
    public void Normalized_request_reports_the_clamped_values()
    {
        var normalized = new PageRequest(-3, 500).Normalized();

        Assert.Equal(1, normalized.Page);
        Assert.Equal(100, normalized.PageSize);
    }

    [Fact]
    public void Paged_result_exposes_total_to_match_the_frontend_contract()
    {
        var result = PagedResult<string>.Create(["a", "b"], total: 42, page: 2, pageSize: 20);

        Assert.Equal(42, result.Total);
        Assert.Equal(2, result.Page);
        Assert.Equal(20, result.PageSize);
        Assert.Equal(["a", "b"], result.Items);
        Assert.Equal(3, result.TotalPages);
    }
}
