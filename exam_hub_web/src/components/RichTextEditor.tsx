import { useEffect, useRef, useState } from 'react'
import { useEditor, EditorContent } from '@tiptap/react'
import StarterKit from '@tiptap/starter-kit'
import { Mathematics } from '@tiptap/extension-mathematics'
import { Button, Input, Popover, Tooltip } from 'antd'
import { BoldOutlined, ItalicOutlined } from '@ant-design/icons'
import 'katex/dist/katex.min.css'

interface RichTextEditorProps {
    value?: string
    onChange?: (html: string) => void
    placeholder?: string
    minHeight?: number
}

export default function RichTextEditor({
    value = '',
    onChange,
    placeholder,
    minHeight = 100,
}: RichTextEditorProps) {
    const [mathOpen, setMathOpen] = useState(false)
    const [mathInput, setMathInput] = useState('')
    const skipSync = useRef(false)

    const editor = useEditor({
        extensions: [StarterKit, Mathematics],
        content: value,
        onUpdate: ({ editor }) => {
            if (skipSync.current) return
            const html = editor.getHTML()
            onChange?.(html === '<p></p>' ? '' : html)
        },
        editorProps: {
            attributes: {
                class: 'rich-editor-prosemirror',
            },
        },
    })

    // Sync external value changes (e.g. form.setFieldsValue when loading existing question)
    useEffect(() => {
        if (!editor || editor.isDestroyed) return
        const current = editor.getHTML()
        const next = value || ''
        if (current === next || (current === '<p></p>' && next === '')) return
        skipSync.current = true
        editor.commands.setContent(next, false)
        skipSync.current = false
    }, [value, editor])

    const handleInsertMath = () => {
        const latex = mathInput.trim()
        if (!latex || !editor) return
        editor.chain().focus().insertInlineMath({ latex }).run()
        setMathInput('')
        setMathOpen(false)
    }

    const mathPopoverContent = (
        <div className="flex flex-col gap-2" style={{ width: 260 }}>
            <p className="text-xs text-gray-500 m-0">
                Nhập cú pháp LaTeX, VD: <code>x^2 + y^2</code>, <code>\frac{"{a}"}{"{b}"}</code>
            </p>
            <Input
                size="small"
                value={mathInput}
                onChange={e => setMathInput(e.target.value)}
                placeholder="\sqrt{x^2 + y^2}"
                onPressEnter={handleInsertMath}
                autoFocus
            />
            <div className="flex gap-1.5 justify-end">
                <Button size="small" onClick={() => { setMathOpen(false); setMathInput('') }}>Hủy</Button>
                <Button size="small" type="primary" onClick={handleInsertMath} disabled={!mathInput.trim()}>
                    Chèn
                </Button>
            </div>
        </div>
    )

    return (
        <div className="rich-editor">
            <div className="rich-editor-toolbar">
                <Tooltip title="In đậm (Ctrl+B)">
                    <Button
                        size="small"
                        type={editor?.isActive('bold') ? 'primary' : 'text'}
                        icon={<BoldOutlined />}
                        onMouseDown={e => { e.preventDefault(); editor?.chain().focus().toggleBold().run() }}
                    />
                </Tooltip>
                <Tooltip title="In nghiêng (Ctrl+I)">
                    <Button
                        size="small"
                        type={editor?.isActive('italic') ? 'primary' : 'text'}
                        icon={<ItalicOutlined />}
                        onMouseDown={e => { e.preventDefault(); editor?.chain().focus().toggleItalic().run() }}
                    />
                </Tooltip>
                <div className="rich-editor-divider" />
                <Popover
                    open={mathOpen}
                    onOpenChange={open => { setMathOpen(open); if (!open) setMathInput('') }}
                    content={mathPopoverContent}
                    title="Chèn công thức toán"
                    trigger="click"
                    placement="bottomLeft"
                >
                    <Button size="small" type="text" className="!text-blue-600 !font-medium">
                        ∑ Công thức
                    </Button>
                </Popover>
            </div>
            <EditorContent
                editor={editor}
                className="rich-editor-content"
                style={{
                    '--rte-min-height': `${minHeight}px`,
                    '--rte-placeholder': placeholder ? `"${placeholder}"` : '""',
                } as React.CSSProperties}
            />
        </div>
    )
}
