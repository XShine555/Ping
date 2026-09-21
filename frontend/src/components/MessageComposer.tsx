import { useRef, useState } from 'react'
import { useApi } from '../api/useApi'
import { ApiError, uploadFileToPresignedUrl } from '../api/client'
import type { MessageAttachmentInput } from '../api/client'
import type { AttachmentKind } from '../types/api'
import { useToastStore } from '../store/toastStore'

interface PendingAttachment {
  file: File | Blob
  fileName: string
  contentType: string
  kind: AttachmentKind
}

function guessKind(contentType: string): AttachmentKind {
  if (contentType.startsWith('image/')) return 'Image'
  if (contentType.startsWith('audio/')) return 'Audio'
  return 'File'
}

export function MessageComposer({ onSend }: { onSend: (content: string, attachments: MessageAttachmentInput[]) => Promise<void> }) {
  const api = useApi()
  const pushToast = useToastStore((s) => s.push)
  const [content, setContent] = useState('')
  const [pending, setPending] = useState<PendingAttachment[]>([])
  const [isRecording, setIsRecording] = useState(false)
  const [isSending, setIsSending] = useState(false)
  const fileInputRef = useRef<HTMLInputElement>(null)
  const mediaRecorderRef = useRef<MediaRecorder | null>(null)
  const recordedChunksRef = useRef<Blob[]>([])

  function onFilesPicked(files: FileList | null) {
    if (!files) return
    const additions = Array.from(files).map((file) => ({
      file,
      fileName: file.name,
      contentType: file.type || 'application/octet-stream',
      kind: guessKind(file.type),
    }))
    setPending((prev) => [...prev, ...additions])
  }

  async function toggleRecording() {
    if (isRecording) {
      mediaRecorderRef.current?.stop()
      setIsRecording(false)
      return
    }

    const stream = await navigator.mediaDevices.getUserMedia({ audio: true })
    const recorder = new MediaRecorder(stream)
    recordedChunksRef.current = []

    recorder.ondataavailable = (e) => {
      if (e.data.size > 0) recordedChunksRef.current.push(e.data)
    }
    recorder.onstop = () => {
      stream.getTracks().forEach((track) => track.stop())
      const blob = new Blob(recordedChunksRef.current, { type: 'audio/webm' })
      setPending((prev) => [...prev, { file: blob, fileName: `voice-message-${Date.now()}.webm`, contentType: 'audio/webm', kind: 'Audio' }])
    }

    mediaRecorderRef.current = recorder
    recorder.start()
    setIsRecording(true)
  }

  function removePending(index: number) {
    setPending((prev) => prev.filter((_, i) => i !== index))
  }

  async function handleSend() {
    if (!content.trim() && pending.length === 0) return

    setIsSending(true)
    try {
      const uploaded: MessageAttachmentInput[] = []
      for (const attachment of pending) {
        const { uploadUrl, key } = await api.requestAttachmentUploadUrl(attachment.fileName, attachment.contentType)
        await uploadFileToPresignedUrl(uploadUrl, attachment.file, attachment.contentType)
        uploaded.push({ key, fileName: attachment.fileName, contentType: attachment.contentType, kind: attachment.kind })
      }

      await onSend(content.trim(), uploaded)
      setContent('')
      setPending([])
    } catch (error) {
      pushToast(error instanceof ApiError ? error.message : 'Failed to send message', 'error')
    } finally {
      setIsSending(false)
    }
  }

  return (
    <div className="message-composer">
      {pending.length > 0 && (
        <ul className="pending-attachments">
          {pending.map((attachment, index) => (
            <li key={index}>
              {attachment.fileName}
              <button onClick={() => removePending(index)}>✕</button>
            </li>
          ))}
        </ul>
      )}
      <div className="composer-row">
        <input type="file" ref={fileInputRef} multiple hidden onChange={(e) => onFilesPicked(e.target.files)} />
        <button title="Attach file" onClick={() => fileInputRef.current?.click()}>
          📎
        </button>
        <button title={isRecording ? 'Stop recording' : 'Record voice message'} onClick={() => void toggleRecording()}>
          {isRecording ? '⏹' : '🎙'}
        </button>
        <input
          className="composer-input"
          placeholder="Message…"
          value={content}
          onChange={(e) => setContent(e.target.value)}
          onKeyDown={(e) => {
            if (e.key === 'Enter' && !e.shiftKey) {
              e.preventDefault()
              void handleSend()
            }
          }}
        />
        <button disabled={isSending} onClick={() => void handleSend()}>
          Send
        </button>
      </div>
    </div>
  )
}
