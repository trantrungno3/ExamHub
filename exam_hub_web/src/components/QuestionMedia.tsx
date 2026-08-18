import {Button} from 'antd'
import {FilePdfOutlined} from '@ant-design/icons'
import {detectMediaKind} from '../utils/media'

type Props = {
    /** URL tệp đính kèm — có thể là ảnh HOẶC pdf (cùng cột image_url). */
    imageUrl?: string
    /** URL tệp audio. */
    audioUrl?: string
    className?: string
}

/** Hiển thị tệp đính kèm của câu hỏi, tự phân loại ảnh / pdf / audio để render đúng. */
export default function QuestionMedia({imageUrl, audioUrl, className}: Props) {
    const imageKind = detectMediaKind(imageUrl)
    const hasImage = imageUrl && imageKind === 'image'
    const hasPdf = imageUrl && imageKind === 'pdf'
    const hasAudio = !!audioUrl

    if (!hasImage && !hasPdf && !hasAudio) return null

    return (
        <div className={`flex flex-col gap-3 ${className ?? ''}`}>
            {hasImage && (
                <img
                    src={imageUrl}
                    alt="Hình ảnh câu hỏi"
                    className="max-w-full max-h-80 rounded-lg border border-black/10 object-contain"
                />
            )}

            {hasPdf && (
                <div className="flex flex-col gap-2">
                    <iframe
                        src={imageUrl}
                        title="Tệp PDF câu hỏi"
                        className="w-full h-80 rounded-lg border border-black/10 bg-white"
                    />
                    <Button
                        type="link"
                        size="small"
                        icon={<FilePdfOutlined/>}
                        href={imageUrl}
                        target="_blank"
                        rel="noreferrer"
                        className="self-start !px-0"
                    >
                        Mở PDF trong tab mới
                    </Button>
                </div>
            )}

            {hasAudio && (
                <audio controls src={audioUrl} className="w-full max-w-md">
                    Trình duyệt không hỗ trợ phát audio.
                </audio>
            )}
        </div>
    )
}
