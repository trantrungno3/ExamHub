namespace ExamHub.Core.Application.Services;

/// <summary>Tính năm học hiện tại của một khoá dựa trên ngày hôm nay (năm học bắt đầu tháng 9).</summary>
public static class SchoolYearCalculator
{
    /// <summary>YearIndex (1-based, khớp với cohort_classes.year_index) của khoá tại thời điểm <paramref name="today"/>.</summary>
    public static int GetCurrentYearIndex(short cohortStartYear, DateOnly today)
    {
        var academicYearStart = today.Month >= 9 ? today.Year : today.Year - 1;
        return academicYearStart - cohortStartYear + 1;
    }
}
