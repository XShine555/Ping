import { useState } from 'react'

interface AvatarProps {
  src?: string | null
  fallbackText: string
  alt: string
  className?: string
}

export function Avatar({ src, fallbackText, alt, className = '' }: AvatarProps) {
  const [failed, setFailed] = useState(false)

  if (src && !failed) {
    return <img className={`avatar-img ${className}`} src={src} alt={alt} onError={() => setFailed(true)} />
  }

  return (
    <div className={`avatar-fallback ${className}`} title={alt}>
      {fallbackText}
    </div>
  )
}
