import { createContext, use, useCallback, useEffect, useState, type ReactNode } from 'react'
import { api, setOnUnauthorized } from '@/lib/api'

export type Role = 'Owner' | 'Editor' | 'Viewer'
export type User = { username: string; role: Role }

type Status = 'loading' | 'needsSetup' | 'anonymous' | 'authenticated'

type StatusResponse = { needsSetup: boolean; authenticated: boolean; user: User | null }

type AuthValue = {
  status: Status
  user: User | null
  canWrite: boolean
  canManage: boolean
  login: (username: string, password: string) => Promise<void>
  setup: (username: string, password: string) => Promise<void>
  logout: () => Promise<void>
}

const AuthContext = createContext<AuthValue | null>(null)

export function AuthProvider({ children }: { children: ReactNode }) {
  const [status, setStatus] = useState<Status>('loading')
  const [user, setUser] = useState<User | null>(null)

  const bootstrap = useCallback(async () => {
    try {
      const s = await api.get<StatusResponse>('/auth/status')
      if (s.needsSetup) setStatus('needsSetup')
      else if (s.authenticated && s.user) {
        setUser(s.user)
        setStatus('authenticated')
      } else setStatus('anonymous')
    } catch {
      setStatus('anonymous')
    }
  }, [])

  useEffect(() => {
    bootstrap()
  }, [bootstrap])

  // 会话过期等 401:回到未登录态(守卫据此跳登录)
  useEffect(() => {
    setOnUnauthorized(() => {
      setUser(null)
      setStatus('anonymous')
    })
    return () => setOnUnauthorized(null)
  }, [])

  const login = useCallback(async (username: string, password: string) => {
    const u = await api.post<User>('/auth/login', { username, password })
    setUser(u)
    setStatus('authenticated')
  }, [])

  const setup = useCallback(async (username: string, password: string) => {
    const u = await api.post<User>('/auth/setup', { username, password })
    setUser(u)
    setStatus('authenticated')
  }, [])

  const logout = useCallback(async () => {
    await api.post('/auth/logout')
    setUser(null)
    setStatus('anonymous')
  }, [])

  const value: AuthValue = {
    status,
    user,
    canWrite: user?.role === 'Owner' || user?.role === 'Editor',
    canManage: user?.role === 'Owner',
    login,
    setup,
    logout,
  }
  return <AuthContext value={value}>{children}</AuthContext>
}

export function useAuth() {
  const ctx = use(AuthContext)
  if (!ctx) throw new Error('useAuth must be used within AuthProvider')
  return ctx
}
