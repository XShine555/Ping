import { useMemo } from 'react'
import { useAuth } from 'react-oidc-context'
import { createApiClient } from './client'

export function useApi() {
  const auth = useAuth()

  return useMemo(() => createApiClient(() => auth.user?.access_token), [auth.user?.access_token])
}
