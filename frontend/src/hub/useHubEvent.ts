import { useEffect } from 'react'
import { useChatHub } from './ChatHubProvider'

export function useHubEvent<TArgs extends unknown[]>(eventName: string, handler: (...args: TArgs) => void) {
  const connection = useChatHub()

  useEffect(() => {
    if (!connection) {
      return
    }

    connection.on(eventName, handler as (...args: unknown[]) => void)
    return () => {
      connection.off(eventName, handler as (...args: unknown[]) => void)
    }
  }, [connection, eventName, handler])
}
