import { useCallback, useEffect, useState } from 'react'
import { useApi } from '../api/useApi'
import { ApiError } from '../api/client'
import type { FriendshipResponse, UserResponse } from '../types/api'
import { useHubEvent } from '../hub/useHubEvent'
import { useUiStore } from '../store/uiStore'
import { usePresenceStore } from '../store/presenceStore'
import { ConfirmModal } from './ConfirmModal'
import { PresenceDot } from './PresenceDot'
import { Avatar } from './Avatar'

function userLabel(user: UserResponse) {
  return user.displayName ?? user.username
}

interface PendingConfirm {
  kind: 'remove' | 'block'
  friendshipId: string
  userId: string
  label: string
}

export function FriendsPanel() {
  const api = useApi()
  const setView = useUiStore((s) => s.setView)
  const [friends, setFriends] = useState<FriendshipResponse[]>([])
  const [requests, setRequests] = useState<FriendshipResponse[]>([])
  const [query, setQuery] = useState('')
  const [results, setResults] = useState<UserResponse[]>([])
  const [errorMessage, setErrorMessage] = useState<string | null>(null)
  const [pendingConfirm, setPendingConfirm] = useState<PendingConfirm | null>(null)
  const seedPresence = usePresenceStore((s) => s.seed)

  const refresh = useCallback(() => {
    void api.getFriends().then((friendships) => {
      setFriends(friendships)
      seedPresence(friendships.map((f) => [f.otherUser.id, f.presence]))
    })
    void api.getPendingFriendRequests().then(setRequests)
  }, [api, seedPresence])

  useEffect(refresh, [refresh])
  useHubEvent('FriendRequestReceived', refresh)
  useHubEvent('FriendshipUpdated', refresh)

  useEffect(() => {
    if (query.trim().length < 2) {
      setResults([])
      return
    }
    const handle = setTimeout(() => void api.searchUsers(query.trim()).then(setResults), 300)
    return () => clearTimeout(handle)
  }, [api, query])

  async function runAction(action: Promise<unknown>) {
    setErrorMessage(null)
    try {
      await action
      refresh()
    } catch (error) {
      setErrorMessage(error instanceof ApiError ? error.message : 'Something went wrong')
    }
  }

  async function openDirectMessage(user: UserResponse) {
    try {
      const channel = await api.getOrCreateDirectMessageChannel(user.id)
      setView({ kind: 'dm', channelId: channel.id, otherUser: user })
    } catch (error) {
      setErrorMessage(error instanceof ApiError ? error.message : 'Something went wrong')
    }
  }

  const incoming = requests.filter((r) => r.isIncoming)
  const outgoing = requests.filter((r) => !r.isIncoming)

  return (
    <div className="friends-panel">
      {errorMessage && <p className="error-text">{errorMessage}</p>}

      <section>
        <h2>Add friend</h2>
        <input placeholder="Search by username…" value={query} onChange={(e) => setQuery(e.target.value)} />
        <ul className="user-list">
          {results.map((user) => (
            <li key={user.id}>
              <Avatar className="user-avatar-sm" src={user.avatarUrl} fallbackText={userLabel(user).slice(0, 1).toUpperCase()} alt={userLabel(user)} />
              <span>{userLabel(user)}</span>
              <button onClick={() => void runAction(api.sendFriendRequest(user.id))}>Add friend</button>
            </li>
          ))}
        </ul>
      </section>

      {incoming.length > 0 && (
        <section>
          <h2>Incoming requests</h2>
          <ul className="user-list">
            {incoming.map((r) => (
              <li key={r.id}>
                <Avatar
                  className="user-avatar-sm"
                  src={r.otherUser.avatarUrl}
                  fallbackText={userLabel(r.otherUser).slice(0, 1).toUpperCase()}
                  alt={userLabel(r.otherUser)}
                />
                <span>{userLabel(r.otherUser)}</span>
                <button onClick={() => void runAction(api.acceptFriendRequest(r.id))}>Accept</button>
                <button onClick={() => void runAction(api.rejectFriendRequest(r.id))}>Reject</button>
              </li>
            ))}
          </ul>
        </section>
      )}

      {outgoing.length > 0 && (
        <section>
          <h2>Outgoing requests</h2>
          <ul className="user-list">
            {outgoing.map((r) => (
              <li key={r.id}>
                <Avatar
                  className="user-avatar-sm"
                  src={r.otherUser.avatarUrl}
                  fallbackText={userLabel(r.otherUser).slice(0, 1).toUpperCase()}
                  alt={userLabel(r.otherUser)}
                />
                <span>{userLabel(r.otherUser)}</span>
                <em>Pending</em>
              </li>
            ))}
          </ul>
        </section>
      )}

      <section>
        <h2>Friends</h2>
        {friends.length === 0 ? (
          <p className="empty-state">No friends yet — search a username above to add one.</p>
        ) : (
          <ul className="user-list">
            {friends.map((f) => (
              <li key={f.id}>
                <Avatar
                  className="user-avatar-sm"
                  src={f.otherUser.avatarUrl}
                  fallbackText={userLabel(f.otherUser).slice(0, 1).toUpperCase()}
                  alt={userLabel(f.otherUser)}
                />
                <PresenceDot userId={f.otherUser.id} />
                <span>{userLabel(f.otherUser)}</span>
                <button onClick={() => void openDirectMessage(f.otherUser)}>Message</button>
                <button
                  onClick={() =>
                    setPendingConfirm({
                      kind: 'remove',
                      friendshipId: f.id,
                      userId: f.otherUser.id,
                      label: userLabel(f.otherUser),
                    })
                  }
                >
                  Remove
                </button>
                <button
                  onClick={() =>
                    setPendingConfirm({
                      kind: 'block',
                      friendshipId: f.id,
                      userId: f.otherUser.id,
                      label: userLabel(f.otherUser),
                    })
                  }
                >
                  Block
                </button>
              </li>
            ))}
          </ul>
        )}
      </section>

      {pendingConfirm && (
        <ConfirmModal
          title={pendingConfirm.kind === 'remove' ? 'Remove friend' : 'Block user'}
          message={
            pendingConfirm.kind === 'remove'
              ? `Remove ${pendingConfirm.label} from your friends?`
              : `Block ${pendingConfirm.label}? They won't be able to message you, and this also removes them as a friend.`
          }
          confirmLabel={pendingConfirm.kind === 'remove' ? 'Remove' : 'Block'}
          onCancel={() => setPendingConfirm(null)}
          onConfirm={() => {
            const action = pendingConfirm
            setPendingConfirm(null)
            void runAction(action.kind === 'remove' ? api.removeFriend(action.friendshipId) : api.blockUser(action.userId))
          }}
        />
      )}
    </div>
  )
}
