import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr'
import { createContext, useContext, useEffect, useState, type ReactNode } from 'react'
import { useAuth } from 'react-oidc-context'

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL as string

const ChatHubContext = createContext<HubConnection | null>(null)

export function ChatHubProvider({ children }: { children: ReactNode }) {
  const auth = useAuth()
  const [connection, setConnection] = useState<HubConnection | null>(null)

  useEffect(() => {
    if (!auth.isAuthenticated) {
      return
    }

    const hubConnection = new HubConnectionBuilder()
      .withUrl(`${API_BASE_URL}/hubs/chat`, {
        accessTokenFactory: () => auth.user?.access_token ?? '',
      })
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Warning)
      .build()

    let cleanedUp = false

    hubConnection
      .start()
      .then(() => {
        if (!cleanedUp) setConnection(hubConnection)
      })
      .catch((error) => {
        if (!cleanedUp) console.error('Failed to connect to chat hub', error)
      })

    return () => {
      cleanedUp = true
      void hubConnection.stop()
      setConnection(null)
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [auth.isAuthenticated, auth.user?.profile.sub])

  return <ChatHubContext.Provider value={connection}>{children}</ChatHubContext.Provider>
}

export function useChatHub(): HubConnection | null {
  return useContext(ChatHubContext)
}
