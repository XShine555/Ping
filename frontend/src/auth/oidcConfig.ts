import type { AuthProviderProps } from 'react-oidc-context'
import { WebStorageStateStore } from 'oidc-client-ts'

export const oidcConfig: AuthProviderProps = {
  authority: import.meta.env.VITE_AUTH_AUTHORITY as string,
  client_id: import.meta.env.VITE_AUTH_CLIENT_ID as string,
  redirect_uri: import.meta.env.VITE_AUTH_REDIRECT_URI as string,
  post_logout_redirect_uri: window.location.origin,
  scope: 'openid profile email offline_access',
  userStore: new WebStorageStateStore({ store: window.localStorage }),
  onSigninCallback: () => {
    window.history.replaceState({}, document.title, window.location.pathname.replace(/\/auth\/callback\/?$/, '/'))
  },
}
