export interface UserResponse {
  id: string
  username: string
  displayName: string | null
  avatarUrl: string | null
}

export type FriendshipStatus = 'Pending' | 'Accepted' | 'Blocked'

export type PresenceStatus = 'Offline' | 'Online' | 'Busy'

export interface FriendshipResponse {
  id: string
  otherUser: UserResponse
  status: FriendshipStatus
  isIncoming: boolean
  createdAt: string
  presence: PresenceStatus
}

export interface ServerResponse {
  id: string
  name: string
  iconUrl: string | null
  ownerId: string
}

export type ChannelType = 'Text' | 'Voice' | 'DirectMessage'

export interface ChannelResponse {
  id: string
  serverId: string | null
  name: string | null
  type: ChannelType
  position: number
}

export type AttachmentKind = 'File' | 'Image' | 'Audio'

export interface AttachmentResponse {
  id: string
  fileName: string
  contentType: string
  sizeBytes: number
  kind: AttachmentKind
}

export interface MessageResponse {
  id: string
  channelId: string
  author: UserResponse
  content: string
  replyToMessageId: string | null
  createdAt: string
  editedAt: string | null
  attachments: AttachmentResponse[]
}

export interface AttachmentUploadResponse {
  uploadUrl: string
  bucket: string
  key: string
  expiresAt: string
}

export interface CallTokenResponse {
  token: string
  roomName: string
}
