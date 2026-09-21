import { create } from 'zustand'
import type { PresenceStatus } from '../types/api'

interface PresenceState {
  statuses: Record<string, PresenceStatus>
  setStatus: (userId: string, status: PresenceStatus) => void
  seed: (entries: Array<[string, PresenceStatus]>) => void
}

export const usePresenceStore = create<PresenceState>((set) => ({
  statuses: {},
  setStatus: (userId, status) =>
    set((s) => ({ statuses: { ...s.statuses, [userId]: status } })),
  seed: (entries) =>
    set((s) => ({ statuses: { ...s.statuses, ...Object.fromEntries(entries) } })),
}))
