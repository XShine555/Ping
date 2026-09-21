import { useState } from 'react'
import type { MessageResponse } from '../types/api'
import { ConfirmModal } from './ConfirmModal'
import { Avatar } from './Avatar'
import { formatMessageTimestamp, formatShortTime } from '../utils/formatTimestamp'

interface Props {
  message: MessageResponse
  isOwn: boolean
  showHeader: boolean
  onEdit: (id: string, content: string) => Promise<void>
  onDelete: (id: string) => Promise<void>
}

export function MessageItem({ message, isOwn, showHeader, onEdit, onDelete }: Props) {
  const [isEditing, setIsEditing] = useState(false)
  const [draft, setDraft] = useState(message.content)
  const [isConfirmingDelete, setIsConfirmingDelete] = useState(false)
  const authorName = message.author.displayName ?? message.author.username

  async function saveEdit() {
    if (draft.trim() && draft !== message.content) {
      await onEdit(message.id, draft.trim())
    }
    setIsEditing(false)
  }

  return (
    <div className={`message-item ${showHeader ? '' : 'compact'}`}>
      {showHeader ? (
        <Avatar className="message-avatar" src={message.author.avatarUrl} fallbackText={authorName.slice(0, 1).toUpperCase()} alt={authorName} />
      ) : (
        <span className="message-timestamp-gutter">{formatShortTime(message.createdAt)}</span>
      )}
      <div className="message-body">
        {showHeader && (
          <div className="message-header">
            <span className="message-author">{authorName}</span>
            <span className="message-timestamp">{formatMessageTimestamp(message.createdAt)}</span>
            {message.editedAt && <span className="message-edited">(edited)</span>}
          </div>
        )}

        {isEditing ? (
          <div className="message-edit-row">
            <input
              value={draft}
              onChange={(e) => setDraft(e.target.value)}
              onKeyDown={(e) => {
                if (e.key === 'Enter') void saveEdit()
                if (e.key === 'Escape') setIsEditing(false)
              }}
              autoFocus
            />
            <button onClick={() => void saveEdit()}>Save</button>
            <button onClick={() => setIsEditing(false)}>Cancel</button>
          </div>
        ) : (
          <p className="message-content">
            {message.content}
            {!showHeader && message.editedAt && <span className="message-edited"> (edited)</span>}
          </p>
        )}

        {message.attachments.length > 0 && (
          <ul className="message-attachments">
            {message.attachments.map((attachment) => (
              <li key={attachment.id}>
                {attachment.kind === 'Audio' ? '🎤' : attachment.kind === 'Image' ? '🖼' : '📎'} {attachment.fileName}
              </li>
            ))}
          </ul>
        )}

        {isOwn && !isEditing && (
          <div className="message-actions">
            <button onClick={() => setIsEditing(true)}>Edit</button>
            <button onClick={() => setIsConfirmingDelete(true)}>Delete</button>
          </div>
        )}
      </div>

      {isConfirmingDelete && (
        <ConfirmModal
          title="Delete message"
          message="This cannot be undone."
          confirmLabel="Delete"
          onCancel={() => setIsConfirmingDelete(false)}
          onConfirm={() => {
            setIsConfirmingDelete(false)
            void onDelete(message.id)
          }}
        />
      )}
    </div>
  )
}
