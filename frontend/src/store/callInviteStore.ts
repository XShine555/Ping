import { create } from 'zustand'

export interface IncomingCall {
  channelId: string
  callerId: string
  callerName: string
  callerAvatarUrl: string | null
}

export interface OutgoingCall {
  channelId: string
  calleeId: string
  calleeName: string
}

interface CallInviteState {
  incomingCall: IncomingCall | null
  outgoingCall: OutgoingCall | null
  setIncomingCall: (call: IncomingCall | null) => void
  setOutgoingCall: (call: OutgoingCall | null) => void
}

export const useCallInviteStore = create<CallInviteState>((set) => ({
  incomingCall: null,
  outgoingCall: null,
  setIncomingCall: (call) => set({ incomingCall: call }),
  setOutgoingCall: (call) => set({ outgoingCall: call }),
}))
