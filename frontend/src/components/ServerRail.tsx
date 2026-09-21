import { useEffect, useState } from 'react'
import { useApi } from '../api/useApi'
import type { ServerResponse } from '../types/api'
import { useUiStore } from '../store/uiStore'
import { Modal } from './Modal'
import { Avatar } from './Avatar'

export function ServerRail() {
  const api = useApi()
  const { view, setView } = useUiStore()
  const [servers, setServers] = useState<ServerResponse[]>([])
  const [isCreating, setIsCreating] = useState(false)
  const [draftName, setDraftName] = useState('')

  useEffect(() => {
    void api.getServers().then(setServers)
  }, [api])

  async function selectServer(server: ServerResponse) {
    const channels = await api.getServerChannels(server.id)
    const firstTextChannel = channels.find((c) => c.type === 'Text') ?? channels[0]
    if (firstTextChannel) {
      setView({ kind: 'channel', serverId: server.id, channelId: firstTextChannel.id })
    }
  }

  async function createServer() {
    if (!draftName.trim()) return
    const server = await api.createServer(draftName.trim())
    setServers((prev) => [...prev, server])
    setDraftName('')
    setIsCreating(false)
    void selectServer(server)
  }

  const activeServerId = view.kind === 'channel' ? view.serverId : null

  return (
    <nav className="server-rail">
      <button
        className={`server-icon ${view.kind === 'friends' ? 'active' : ''}`}
        title="Friends"
        onClick={() => setView({ kind: 'friends' })}
      >
        DM
      </button>
      <div className="server-rail-divider" />
      {servers.map((server) => (
        <button
          key={server.id}
          className={`server-icon ${activeServerId === server.id ? 'active' : ''}`}
          title={server.name}
          onClick={() => void selectServer(server)}
        >
          <Avatar className="server-icon-avatar" src={server.iconUrl} fallbackText={server.name.slice(0, 2).toUpperCase()} alt={server.name} />
        </button>
      ))}
      <button className="server-icon add" title="Create server" onClick={() => setIsCreating(true)}>
        +
      </button>

      {isCreating && (
        <Modal title="Create a server" onClose={() => setIsCreating(false)}>
          <input
            autoFocus
            placeholder="Server name"
            value={draftName}
            onChange={(e) => setDraftName(e.target.value)}
            onKeyDown={(e) => e.key === 'Enter' && void createServer()}
          />
          <div className="modal-actions">
            <button onClick={() => setIsCreating(false)}>Cancel</button>
            <button onClick={() => void createServer()}>Create</button>
          </div>
        </Modal>
      )}
    </nav>
  )
}
