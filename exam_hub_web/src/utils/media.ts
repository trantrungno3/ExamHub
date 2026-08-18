/** Tiện ích phân loại tệp media từ URL — cột image_url có thể chứa ảnh HOẶC pdf. */

export type MediaKind = 'image' | 'pdf' | 'audio' | 'unknown'

const IMAGE_EXT = ['jpg', 'jpeg', 'png', 'gif', 'webp', 'bmp', 'svg']
const AUDIO_EXT = ['mp3', 'wav', 'ogg', 'webm', 'm4a', 'aac', 'mp4']

/** Đọc đuôi file (bỏ query/hash) rồi suy ra loại media để render đúng thẻ. */
export function detectMediaKind(url?: string): MediaKind {
    if (!url) return 'unknown'
    const clean = url.split('?')[0].split('#')[0].toLowerCase()
    const dot = clean.lastIndexOf('.')
    if (dot < 0) return 'unknown'
    const ext = clean.slice(dot + 1)
    if (IMAGE_EXT.includes(ext)) return 'image'
    if (ext === 'pdf') return 'pdf'
    if (AUDIO_EXT.includes(ext)) return 'audio'
    return 'unknown'
}
