import { useEffect, useState } from 'react'
import { useAuth } from 'react-oidc-context'
import { useApi } from '../api/useApi'
import type { ChannelResponse, ChannelType } from '../types/api'
import { useUiStore } from '../store/uiStore'
import { useCallStore } from '../store/callStore'
import { Modal } from './Modal'
import { Avatar } from './Avatar'

export function ChannelSidebar({ serverId }: { serverId: string }) {
  const api = useApi()
  const auth = useAuth()
  const { view, setView } = useUiStore()
  const { activeChannelId, join, leave } = useCallStore()
  const [channels, setChannels] = useState<ChannelResponse[]>([])
  const [isCreating, setIsCreating] = useState(false)
  const [draftName, setDraftName] = useState('')
  const [draftType, setDraftType] = useState<ChannelType>('Text')

  useEffect(() => {
    void api.getServerChannels(serverId).then(setChannels)
  }, [api, serverId])

  async function createChannel() {
    if (!draftName.trim()) return
    const channel = await api.createChannel(serverId, draftName.trim(), draftType)
    setChannels((prev) => [...prev, channel])
    setDraftName('')
    setDraftType('Text')
    setIsCreating(false)
  }

  return (
    <aside className="channel-sidebar">
      <div className="channel-sidebar-header">Channels</div>
      <div className="channel-list">
        {channels.map((channel) => (
          <div key={channel.id} className="channel-row">
            <button
              className={`channel-item ${view.kind === 'channel' && view.channelId === channel.id ? 'active' : ''}`}
              onClick={() => setView({ kind: 'channel', serverId, channelId: channel.id })}
            >
              {channel.type === 'Voice' ? '🔊' : '#'} {channel.name}
            </button>
            {channel.type === 'Voice' &&
              (activeChannelId === channel.id ? (
                <button className="channel-call-toggle" onClick={leave}>
                  Leave
                </button>
              ) : (
                <button className="channel-call-toggle" onClick={() => join(channel.id, `${channel.name}`)}>
                  Join
                </button>
              ))}
          </div>
        ))}
        <button className="channel-item add" onClick={() => setIsCreating(true)}>
          + Add channel
        </button>
      </div>
      <div className="current-user-footer">
        <span className="current-user-info">
          <Avatar
            className="current-user-avatar"
            src={auth.user?.profile.picture}
            fallbackText={(auth.user?.profile.preferred_username ?? auth.user?.profile.name ?? '?').slice(0, 1).toUpperCase()}
            alt=""
          />
          {auth.user?.profile.preferred_username ?? auth.user?.profile.name}
        </span>
        <button onClick={() => void auth.signoutRedirect()}>Sign out</button>
      </div>

      {isCreating && (
        <Modal title="Create a channel" onClose={() => setIsCreating(false)}>
          <input
            autoFocus
            placeholder="Channel name"
            value={draftName}
            onChange={(e) => setDraftName(e.target.value)}
            onKeyDown={(e) => e.key === 'Enter' && void createChannel()}
          />
          <div className="modal-radio-row">
            <label>
              <input type="radio" checked={draftType === 'Text'} onChange={() => setDraftType('Text')} /># Text
            </label>
            <label>
              <input type="radio" checked={draftType === 'Voice'} onChange={() => setDraftType('Voice')} /> 🔊 Voice
            </label>
          </div>
          <div className="modal-actions">
            <button onClick={() => setIsCreating(false)}>Cancel</button>
            <button onClick={() => void createChannel()}>Create</button>
          </div>
        </Modal>
      )}
    </aside>
  )
}
