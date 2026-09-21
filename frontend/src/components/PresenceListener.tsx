import { useCallback } from 'react'
import { useHubEvent } from '../hub/useHubEvent'
import { usePresenceStore } from '../store/presenceStore'
import type { PresenceStatus } from '../types/api'

export function PresenceListener() {
  const setStatus = usePresenceStore((s) => s.setStatus)

  const onPresenceChanged = useCallback(
    (userId: string, status: PresenceStatus) => setStatus(userId, status),
    [setStatus],
  )

  useHubEvent('PresenceChanged', onPresenceChanged)

  return null
}
