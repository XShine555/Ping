import { useCallback, useEffect, useState } from 'react'
import { useAuth } from 'react-oidc-context'
import { useApi } from '../api/useApi'
import type { MessageAttachmentInput } from '../api/client'
import type { MessageResponse } from '../types/api'
import { useChatHub } from '../hub/ChatHubProvider'
import { useHubEvent } from '../hub/useHubEvent'
import { MessageItem } from './MessageItem'
import { MessageComposer } from './MessageComposer'

const GROUP_WINDOW_MS = 5 * 60 * 1000

export function ChatPanel({ channelId }: { channelId: string }) {
  const api = useApi()
  const auth = useAuth()
  const connection = useChatHub()
  const [messages, setMessages] = useState<MessageResponse[]>([])
  const currentUserId = auth.user?.profile.sub

  useEffect(() => {
    let cancelled = false
    void api.getChannelMessages(channelId).then((page) => {
      if (!cancelled) setMessages([...page].reverse())
    })
    return () => {
      cancelled = true
    }
  }, [api, channelId])

  useEffect(() => {
    if (!connection) return
    void connection.invoke('JoinChannel', channelId).catch((error) => console.error('JoinChannel failed', error))
    return () => {
      void connection.invoke('LeaveChannel', channelId).catch(() => undefined)
    }
  }, [connection, channelId])

  const onMessageCreated = useCallback(
    (message: MessageResponse) => {
      if (message.channelId !== channelId) return
      setMessages((prev) => [...prev, message])
    },
    [channelId],
  )

  const onMessageUpdated = useCallback(
    (message: MessageResponse) => {
      if (message.channelId !== channelId) return
      setMessages((prev) => prev.map((m) => (m.id === message.id ? message : m)))
    },
    [channelId],
  )

  const onMessageDeleted = useCallback((messageId: string) => {
    setMessages((prev) => prev.filter((m) => m.id !== messageId))
  }, [])

  useHubEvent('MessageCreated', onMessageCreated)
  useHubEvent('MessageUpdated', onMessageUpdated)
  useHubEvent('MessageDeleted', onMessageDeleted)

  async function handleSend(content: string, attachments: MessageAttachmentInput[]) {
    await api.sendMessage(channelId, content, attachments)
  }

  async function handleEdit(id: string, newContent: string) {
    await api.editMessage(id, newContent)
  }

  async function handleDelete(id: string) {
    await api.deleteMessage(id)
  }

  return (
    <div className="chat-panel">
      <div className="message-list">
        {messages.length === 0 && <p className="empty-state">No messages yet. Say hi!</p>}
        {messages.map((message, index) => {
          const previous = messages[index - 1]
          const showHeader =
            !previous ||
            previous.author.id !== message.author.id ||
            new Date(message.createdAt).getTime() - new Date(previous.createdAt).getTime() > GROUP_WINDOW_MS

          return (
            <MessageItem
              key={message.id}
              message={message}
              isOwn={message.author.id === currentUserId}
              showHeader={showHeader}
              onEdit={handleEdit}
              onDelete={handleDelete}
            />
          )
        })}
      </div>
      <MessageComposer onSend={handleSend} />
    </div>
  )
}
