import type {
  AttachmentKind,
  AttachmentUploadResponse,
  CallTokenResponse,
  ChannelResponse,
  ChannelType,
  FriendshipResponse,
  MessageResponse,
  ServerResponse,
  UserResponse,
} from '../types/api'

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL as string

export class ApiError extends Error {
  status: number

  constructor(status: number, message: string) {
    super(message)
    this.status = status
  }
}

export interface MessageAttachmentInput {
  key: string
  fileName: string
  contentType: string
  kind: AttachmentKind
}

export function createApiClient(getAccessToken: () => string | undefined) {
  async function request<T>(path: string, init?: RequestInit): Promise<T> {
    const token = getAccessToken()
    const response = await fetch(`${API_BASE_URL}${path}`, {
      ...init,
      headers: {
        ...(init?.body ? { 'Content-Type': 'application/json' } : {}),
        ...(token ? { Authorization: `Bearer ${token}` } : {}),
        ...init?.headers,
      },
    })

    if (!response.ok) {
      const body = await response.text().catch(() => '')
      throw new ApiError(response.status, body || response.statusText)
    }

    if (response.status === 204) {
      return undefined as T
    }

    return (await response.json()) as T
  }

  return {
    getCurrentUser: () => request<UserResponse>('/users/me'),
    getUserById: (id: string) => request<UserResponse>(`/users/${id}`),
    searchUsers: (query: string) => request<UserResponse[]>(`/users/search?query=${encodeURIComponent(query)}`),

    getFriends: () => request<FriendshipResponse[]>('/friends'),
    getPendingFriendRequests: () => request<FriendshipResponse[]>('/friends/requests'),
    sendFriendRequest: (addresseeId: string) =>
      request<FriendshipResponse>('/friends/requests', { method: 'POST', body: JSON.stringify({ addresseeId }) }),
    acceptFriendRequest: (id: string) => request<FriendshipResponse>(`/friends/requests/${id}/accept`, { method: 'POST' }),
    rejectFriendRequest: (id: string) => request<void>(`/friends/requests/${id}/reject`, { method: 'POST' }),
    removeFriend: (id: string) => request<void>(`/friends/${id}`, { method: 'DELETE' }),
    blockUser: (userId: string) => request<void>(`/friends/block/${userId}`, { method: 'POST' }),

    getServers: () => request<ServerResponse[]>('/servers'),
    createServer: (name: string) => request<ServerResponse>('/servers', { method: 'POST', body: JSON.stringify({ name }) }),

    getServerChannels: (serverId: string) => request<ChannelResponse[]>(`/servers/${serverId}/channels`),
    createChannel: (serverId: string, name: string, type: ChannelType) =>
      request<ChannelResponse>(`/servers/${serverId}/channels`, { method: 'POST', body: JSON.stringify({ name, type }) }),
    getOrCreateDirectMessageChannel: (otherUserId: string) =>
      request<ChannelResponse>(`/channels/dm/${otherUserId}`, { method: 'POST' }),

    getChannelMessages: (channelId: string, before?: string, take = 50) =>
      request<MessageResponse[]>(
        `/channels/${channelId}/messages?take=${take}${before ? `&before=${before}` : ''}`,
      ),
    sendMessage: (channelId: string, content: string, attachments: MessageAttachmentInput[] = [], replyToMessageId?: string) =>
      request<MessageResponse>(`/channels/${channelId}/messages`, {
        method: 'POST',
        body: JSON.stringify({ content, replyToMessageId, attachments }),
      }),
    editMessage: (id: string, content: string) =>
      request<MessageResponse>(`/messages/${id}`, { method: 'PATCH', body: JSON.stringify({ content }) }),
    deleteMessage: (id: string) => request<void>(`/messages/${id}`, { method: 'DELETE' }),

    requestAttachmentUploadUrl: (fileName: string, contentType: string) =>
      request<AttachmentUploadResponse>('/attachments/upload-url', {
        method: 'POST',
        body: JSON.stringify({ fileName, contentType }),
      }),

    requestCallToken: (channelId: string) =>
      request<CallTokenResponse>(`/channels/${channelId}/call-token`, { method: 'POST' }),
  }
}

export async function uploadFileToPresignedUrl(uploadUrl: string, file: File | Blob, contentType: string): Promise<void> {
  const response = await fetch(uploadUrl, {
    method: 'PUT',
    headers: { 'Content-Type': contentType },
    body: file,
  })

  if (!response.ok) {
    throw new ApiError(response.status, `Failed to upload file: ${response.statusText}`)
  }
}

export type ApiClient = ReturnType<typeof createApiClient>
