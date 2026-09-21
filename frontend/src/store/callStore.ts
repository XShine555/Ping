import { create } from 'zustand'

interface CallState {
  activeChannelId: string | null
  activeChannelLabel: string | null
  join: (channelId: string, label: string) => void
  leave: () => void
}

export const useCallStore = create<CallState>((set) => ({
  activeChannelId: null,
  activeChannelLabel: null,
  join: (channelId, label) => set({ activeChannelId: channelId, activeChannelLabel: label }),
  leave: () => set({ activeChannelId: null, activeChannelLabel: null }),
}))
