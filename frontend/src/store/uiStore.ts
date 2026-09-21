import { create } from 'zustand'
import type { UserResponse } from '../types/api'

export type View =
  | { kind: 'friends' }
  | { kind: 'channel'; serverId: string; channelId: string }
  | { kind: 'dm'; channelId: string; otherUser: UserResponse }

interface UiState {
  view: View
  setView: (view: View) => void
}

export const useUiStore = create<UiState>((set) => ({
  view: { kind: 'friends' },
  setView: (view) => set({ view }),
}))
