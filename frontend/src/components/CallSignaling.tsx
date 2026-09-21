import { useCallback } from 'react'
import { useApi } from '../api/useApi'
import { useChatHub } from '../hub/ChatHubProvider'
import { useHubEvent } from '../hub/useHubEvent'
import { useCallStore } from '../store/callStore'
import { useCallInviteStore } from '../store/callInviteStore'
import { useToastStore } from '../store/toastStore'
import { Modal } from './Modal'
import { Avatar } from './Avatar'

export function CallSignaling() {
  const api = useApi()
  const connection = useChatHub()
  const activeChannelId = useCallStore((s) => s.activeChannelId)
  const join = useCallStore((s) => s.join)
  const leave = useCallStore((s) => s.leave)
  const incomingCall = useCallInviteStore((s) => s.incomingCall)
  const outgoingCall = useCallInviteStore((s) => s.outgoingCall)
  const setIncomingCall = useCallInviteStore((s) => s.setIncomingCall)
  const setOutgoingCall = useCallInviteStore((s) => s.setOutgoingCall)
  const push = useToastStore((s) => s.push)

  const onIncomingCall = useCallback(
    (channelId: string, callerId: string) => {
      void api.getUserById(callerId).then((caller) => {
        setIncomingCall({
          channelId,
          callerId,
          callerName: caller.displayName ?? caller.username,
          callerAvatarUrl: caller.avatarUrl,
        })
      })
    },
    [api, setIncomingCall],
  )

  const onCallDeclined = useCallback(
    (channelId: string) => {
      if (outgoingCall?.channelId !== channelId) return
      push(`${outgoingCall.calleeName} declined the call`, 'info')
      setOutgoingCall(null)
      if (activeChannelId === channelId) leave()
    },
    [outgoingCall, push, setOutgoingCall, activeChannelId, leave],
  )

  const onCallCancelled = useCallback(
    (channelId: string) => {
      if (incomingCall?.channelId === channelId) setIncomingCall(null)
    },
    [incomingCall, setIncomingCall],
  )

  useHubEvent('IncomingCall', onIncomingCall)
  useHubEvent('CallDeclined', onCallDeclined)
  useHubEvent('CallCancelled', onCallCancelled)

  function acceptCall() {
    if (!incomingCall) return
    join(incomingCall.channelId, incomingCall.callerName)
    setIncomingCall(null)
  }

  async function declineCall() {
    if (!incomingCall) return
    const { channelId, callerId } = incomingCall
    setIncomingCall(null)
    try {
      await connection?.invoke('DeclineCall', channelId, callerId)
    } catch (error) {
      console.error('Failed to send DeclineCall', error)
    }
  }

  if (!incomingCall) return null

  return (
    <Modal title="Incoming call" onClose={() => void declineCall()}>
      <div className="incoming-call-caller">
        <Avatar
          className="incoming-call-avatar"
          src={incomingCall.callerAvatarUrl}
          fallbackText={incomingCall.callerName.slice(0, 1).toUpperCase()}
          alt={incomingCall.callerName}
        />
        <p>{incomingCall.callerName} is calling you…</p>
      </div>
      <div className="modal-actions">
        <button onClick={() => void declineCall()}>Decline</button>
        <button className="accept-button" onClick={acceptCall}>
          Accept
        </button>
      </div>
    </Modal>
  )
}
