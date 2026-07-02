import type { ReactNode } from 'react'
import { Navigate } from 'react-router'
import { Loader2 } from 'lucide-react'
import { useAuth } from '@/auth/auth-context'

function FullScreenSpinner() {
  return (
    <div className="flex min-h-svh items-center justify-center">
      <Loader2 className="text-muted-foreground size-6 animate-spin" />
    </div>
  )
}

/** 已登录才放行;未初始化→/setup,未登录→/login。 */
export function RequireAuth({ children }: { children: ReactNode }) {
  const { status } = useAuth()
  if (status === 'loading') return <FullScreenSpinner />
  if (status === 'needsSetup') return <Navigate to="/setup" replace />
  if (status === 'anonymous') return <Navigate to="/login" replace />
  return children
}
