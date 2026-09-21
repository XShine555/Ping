import { Navigate, Route, Routes } from 'react-router-dom'
import { useAuth } from 'react-oidc-context'
import { ChatHubProvider, useChatHub } from './hub/ChatHubProvider'
import { ServerRail } from './components/ServerRail'
import { ChannelSidebar } from './components/ChannelSidebar'
import { ChatPanel } from './components/ChatPanel'
import { FriendsPanel } from './components/FriendsPanel'
import { CallDock } from './components/CallDock'
import { CallSignaling } from './components/CallSignaling'
import { PresenceListener } from './components/PresenceListener'
import { PresenceDot } from './components/PresenceDot'
import { Avatar } from './components/Avatar'
import { ToastHost } from './components/ToastHost'
import { Login } from './pages/Login'
import { AuthCallback } from './pages/AuthCallback'
import { useUiStore } from './store/uiStore'
import { useCallStore } from './store/callStore'
import { useCallInviteStore } from './store/callInviteStore'
import { useToastStore } from './store/toastStore'

function AppShell() {
  const view = useUiStore((s) => s.view)
  const activeChannelId = useCallStore((s) => s.activeChannelId)
  const join = useCallStore((s) => s.join)
  const connection = useChatHub()
  const setOutgoingCall = useCallInviteStore((s) => s.setOutgoingCall)
  const pushToast = useToastStore((s) => s.push)

  async function startCall(channelId: string, calleeId: string, label: string) {
    join(channelId, label)
    if (!connection) return
    try {
      await connection.invoke('RingUser', channelId, calleeId)
      setOutgoingCall({ channelId, calleeId, calleeName: label })
    } catch (error) {
      console.error('Failed to ring user', error)
      pushToast('Could not ring the other user', 'error')
    }
  }

  return (
    <div className="app-root">
      <div className="app-shell">
        <ServerRail />

        {view.kind === 'friends' && (
          <div className="main-column">
            <FriendsPanel />
          </div>
        )}

        {view.kind === 'channel' && (
          <>
            <ChannelSidebar serverId={view.serverId} />
            <div className="main-column">
              <ChatPanel key={view.channelId} channelId={view.channelId} />
            </div>
          </>
        )}

        {view.kind === 'dm' && (
          <div className="main-column">
            <div className="dm-header">
              <span className="dm-header-user">
                <Avatar
                  className="dm-header-avatar"
                  src={view.otherUser.avatarUrl}
                  fallbackText={(view.otherUser.displayName ?? view.otherUser.username).slice(0, 1).toUpperCase()}
                  alt={view.otherUser.displayName ?? view.otherUser.username}
                />
                {view.otherUser.displayName ?? view.otherUser.username} <PresenceDot userId={view.otherUser.id} showLabel />
              </span>
              {activeChannelId !== view.channelId && (
                <button
                  onClick={() =>
                    void startCall(view.channelId, view.otherUser.id, view.otherUser.displayName ?? view.otherUser.username)
                  }
                >
                  📞 Call
                </button>
              )}
            </div>
            <ChatPanel key={view.channelId} channelId={view.channelId} />
          </div>
        )}
      </div>

      <CallDock />
      <CallSignaling />
      <PresenceListener />
      <ToastHost />
    </div>
  )
}

function MainLayout() {
  return (
    <ChatHubProvider>
      <AppShell />
    </ChatHubProvider>
  )
}

export default function App() {
  const auth = useAuth()

  return (
    <Routes>
      <Route path="/auth/callback" element={<AuthCallback />} />
      <Route
        path="/*"
        element={
          auth.isLoading ? (
            <div className="centered-screen">
              <p>Loading…</p>
            </div>
          ) : auth.isAuthenticated ? (
            <MainLayout />
          ) : (
            <Login />
          )
        }
      />
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  )
}
