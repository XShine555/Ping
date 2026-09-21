import { usePresenceStore } from '../store/presenceStore'
import type { PresenceStatus } from '../types/api'

const LABELS: Record<PresenceStatus, string> = {
  Online: 'Online',
  Busy: 'In a call',
  Offline: 'Offline',
}

export function PresenceDot({ userId, showLabel = false }: { userId: string; showLabel?: boolean }) {
  const status = usePresenceStore((s) => s.statuses[userId] ?? 'Offline')

  return (
    <span className="presence" title={LABELS[status]}>
      <span className={`presence-dot presence-dot-${status.toLowerCase()}`} />
      {showLabel && <span className="presence-label">{LABELS[status]}</span>}
    </span>
  )
}
