/**
 * Bảng màu thương hiệu. Giữ đồng bộ với khối @theme trong src/index.css —
 * antd đọc từ đây, Tailwind đọc từ @theme.
 */
export const BRAND = {
    primary: '#3a74f5',
    primarySoft: '#e9ecfe',
    success: '#1ea375',
    successSoft: '#dff5ed',
    danger: '#e74242',
    dangerSoft: '#fee5e5',
    warning: '#d98a00',
    warningSoft: '#fff4e5',
    ink: '#191d27',
    muted: '#6f7788',
    border: '#eceef2',
    surface: '#f5f5f6',
    sidebar: '#191d27',
    /* Các sắc độ phụ — lặp ở nhiều màn, gom về đây thay vì viết hex tại chỗ. */
    primaryTint: '#eef1ff',
    /** Chữ phụ trên nền primary (hero xanh). */
    primaryOn: '#cdd9fb',
    successTint: '#e7f7ef',
    /** Chữ xám nhạt hơn `muted` — placeholder, icon phụ. */
    mutedSoft: '#9aa2b1',
    /** Nền xám trung tính cho chip/tag không trạng thái. */
    neutralSoft: '#eef0f3',
    /** Viền đậm hơn `border` — đường kẻ nhấn, icon mờ. */
    borderStrong: '#c4cad3',
    /** Chữ tiêu đề đậm hơn `ink` một nấc (màn học sinh). */
    inkStrong: '#1d2129',
    /** Nền "bàn thi" của khu vực học sinh. */
    desk: '#f5f4f1',
} as const
