import { LiveKitRoom, VideoConference, useRemoteParticipants } from '@livekit/components-react'
import { AudioPresets, ScreenSharePresets, type RoomOptions } from 'livekit-client'
import { useEffect, useState } from 'react'
import { useApi } from '../api/useApi'
import { ApiError } from '../api/client'
import { useChatHub } from '../hub/ChatHubProvider'
import { useCallStore } from '../store/callStore'
import { useCallInviteStore } from '../store/callInviteStore'
import { useToastStore } from '../store/toastStore'

const LIVEKIT_URL = import.meta.env.VITE_LIVEKIT_URL

const roomOptions: RoomOptions = {
  adaptiveStream: true,
  dynacast: true,
  publishDefaults: {
    videoCodec: 'vp9',
    screenShareEncoding: ScreenSharePresets.h1080fps30.encoding,
    audioPreset: AudioPresets.musicHighQualityStereo,
    dtx: false,
  },
}

function RingingWatcher() {
  const remoteParticipants = useRemoteParticipants()
  const outgoingCall = useCallInviteStore((s) => s.outgoingCall)
  const setOutgoingCall = useCallInviteStore((s) => s.setOutgoingCall)

  useEffect(() => {
    if (outgoingCall && remoteParticipants.length > 0) {
      setOutgoingCall(null)
    }
  }, [remoteParticipants.length, outgoingCall, setOutgoingCall])

  return null
}

export function CallDock() {
  const api = useApi()
  const connection = useChatHub()
  const activeChannelId = useCallStore((s) => s.activeChannelId)
  const activeChannelLabel = useCallStore((s) => s.activeChannelLabel)
  const leave = useCallStore((s) => s.leave)
  const outgoingCall = useCallInviteStore((s) => s.outgoingCall)
  const setOutgoingCall = useCallInviteStore((s) => s.setOutgoingCall)
  const pushToast = useToastStore((s) => s.push)
  const [token, setToken] = useState<string | null>(null)

  useEffect(() => {
    if (!activeChannelId) {
      setToken(null)
      return
    }

    let cancelled = false
    setToken(null)
    void api
      .requestCallToken(activeChannelId)
      .then((response) => {
        if (!cancelled) setToken(response.token)
      })
      .catch((error) => {
        if (cancelled) return
        pushToast(error instanceof ApiError ? error.message : 'Failed to join the call', 'error')
        leave()
      })
    return () => {
      cancelled = true
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [api, activeChannelId])

  useEffect(() => {
    if (!activeChannelId || !connection) return
    void connection.invoke('SetBusy', true).catch(() => undefined)
    return () => {
      void connection.invoke('SetBusy', false).catch(() => undefined)
    }
  }, [activeChannelId, connection])

  if (!activeChannelId) {
    return null
  }

  const isRinging = outgoingCall?.channelId === activeChannelId

  function handleLeave() {
    if (outgoingCall?.channelId === activeChannelId) {
      void connection?.invoke('CancelCall', outgoingCall.channelId, outgoingCall.calleeId).catch(() => undefined)
      setOutgoingCall(null)
    }
    leave()
  }

  return (
    <div className="call-dock">
      <div className="call-dock-header">
        <span>
          🔊 {activeChannelLabel}
          {isRinging && <span className="call-ringing-badge">Ringing…</span>}
        </span>
        <button onClick={handleLeave}>Leave call</button>
      </div>
      {token ? (
        <LiveKitRoom
          serverUrl={LIVEKIT_URL}
          token={token}
          connect
          audio
          video={false}
          options={roomOptions}
          onDisconnected={handleLeave}
          className="call-dock-room"
        >
          <RingingWatcher />
          <VideoConference />
        </LiveKitRoom>
      ) : (
        <p className="call-dock-connecting">Connecting…</p>
      )}
    </div>
  )
}
