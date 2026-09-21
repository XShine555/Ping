import { useToastStore } from '../store/toastStore'

export function ToastHost() {
  const toasts = useToastStore((s) => s.toasts)
  const dismiss = useToastStore((s) => s.dismiss)

  if (toasts.length === 0) return null

  return (
    <div className="toast-host">
      {toasts.map((toast) => (
        <div key={toast.id} className={`toast toast-${toast.tone}`} onClick={() => dismiss(toast.id)}>
          {toast.message}
        </div>
      ))}
    </div>
  )
}
