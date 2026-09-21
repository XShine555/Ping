export function formatMessageTimestamp(iso: string): string {
  const date = new Date(iso)
  const now = new Date()
  const yesterday = new Date(now)
  yesterday.setDate(now.getDate() - 1)
  const time = formatShortTime(iso)

  if (date.toDateString() === now.toDateString()) return `Today at ${time}`
  if (date.toDateString() === yesterday.toDateString()) return `Yesterday at ${time}`
  return `${date.toLocaleDateString()} ${time}`
}

export function formatShortTime(iso: string): string {
  return new Date(iso).toLocaleTimeString(undefined, { hour: 'numeric', minute: '2-digit' })
}
