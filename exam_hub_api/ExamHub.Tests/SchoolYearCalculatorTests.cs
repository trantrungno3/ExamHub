using ExamHub.Core.Application.Services;
using Xunit;

namespace ExamHub.Tests;

public class SchoolYearCalculatorTests
{
    [Fact]
    public void GetCurrentYearIndex_SeptemberOnwards_UsesCurrentCalendarYearAsAcademicStart()
    {
        // Khoá bắt đầu 2023, hôm nay 2025-09-01 -> năm học 2025-2026 -> year index 3
        var result = SchoolYearCalculator.GetCurrentYearIndex(2023, new DateOnly(2025, 9, 1));
        Assert.Equal(3, result);
    }

    [Fact]
    public void GetCurrentYearIndex_BeforeSeptember_UsesPreviousCalendarYearAsAcademicStart()
    {
        // Hôm nay 2025-08-31 (trước mốc tháng 9) -> vẫn thuộc năm học 2024-2025 -> year index 2
        var result = SchoolYearCalculator.GetCurrentYearIndex(2023, new DateOnly(2025, 8, 31));
        Assert.Equal(2, result);
    }

    [Fact]
    public void GetCurrentYearIndex_FirstYearOfCohort_ReturnsOne()
    {
        var result = SchoolYearCalculator.GetCurrentYearIndex(2025, new DateOnly(2025, 9, 15));
        Assert.Equal(1, result);
    }

    [Fact]
    public void GetCurrentYearIndex_BeforeCohortStarts_ReturnsNonPositive()
    {
        // Khoá 2026-... nhưng hôm nay mới 2025 -> year index <= 0, caller phải tự lọc kết quả không khớp
        var result = SchoolYearCalculator.GetCurrentYearIndex(2026, new DateOnly(2025, 9, 15));
        Assert.Equal(0, result);
    }
}
