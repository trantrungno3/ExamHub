namespace ExamHub.Core;

/// <summary>
/// Constants for common keys and schema names used in the system.
/// </summary>
public struct AppConst
{
    /// <summary>
    ///     Định nghĩa các khóa liên quan đến xác thực người dùng
    /// </summary>
    public struct AudienceKey
    {
        /// <summary>
        ///     Khóa cho đối tượng audience chính
        /// </summary>
        public const string Audience = nameof(Audience);

        /// <summary>
        ///     Khóa cho đối tượng audience làm mới
        /// </summary>
        public const string AudienceRefresh = nameof(AudienceRefresh);
    }

    /// <summary>
    ///     Định nghĩa các hằng số schema được sử dụng trong hệ thống
    /// </summary>
    public struct Schema
    {
        /// <summary>
        ///     Public
        /// </summary>
        public const string Public = "public";

        /// <summary>
        ///     Danh mục
        /// </summary>
        public const string Category = "category";

        /// <summary>
        ///     Ứng dụng
        /// </summary>
        public const string App = "app";
    }
}