import { useAuth } from 'react-oidc-context'

export function Login() {
  const auth = useAuth()

  return (
    <div className="centered-screen">
      <div className="login-card">
        <h1>Ping</h1>
        <p>Chat, voice and video, self-hosted.</p>
        <button onClick={() => void auth.signinRedirect()}>Sign in with Zitadel</button>
        {auth.error && <p className="error-text">{auth.error.message}</p>}
      </div>
    </div>
  )
}
