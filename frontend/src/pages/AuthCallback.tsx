import { Navigate } from 'react-router-dom'
import { useAuth } from 'react-oidc-context'

export function AuthCallback() {
  const auth = useAuth()

  if (auth.isLoading) {
    return (
      <div className="centered-screen">
        <p>Signing in…</p>
      </div>
    )
  }

  if (auth.error) {
    return (
      <div className="centered-screen">
        <p className="error-text">Sign-in failed: {auth.error.message}</p>
      </div>
    )
  }

  return <Navigate to="/" replace />
}
