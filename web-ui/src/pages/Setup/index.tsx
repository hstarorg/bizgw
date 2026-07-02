import { useState } from 'react'
import { Navigate, useNavigate } from 'react-router'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Logo } from '@/components/logo'
import { useAuth } from '@/auth/auth-context'
import { ApiError } from '@/lib/api'

export default function SetupPage() {
  const { status, setup } = useAuth()
  const navigate = useNavigate()
  const [error, setError] = useState('')
  const [busy, setBusy] = useState(false)

  if (status === 'authenticated') return <Navigate to="/" replace />
  if (status === 'anonymous') return <Navigate to="/login" replace />

  async function onSubmit(e: React.FormEvent<HTMLFormElement>) {
    e.preventDefault()
    const form = new FormData(e.currentTarget)
    setBusy(true)
    setError('')
    try {
      await setup(String(form.get('username')), String(form.get('password')))
      navigate('/')
    } catch (err) {
      setError(err instanceof ApiError ? err.message : '初始化失败')
    } finally {
      setBusy(false)
    }
  }

  return (
    <div className="bg-muted/40 flex min-h-svh items-center justify-center p-4">
      <Card className="w-full max-w-sm">
        <CardHeader className="text-center">
          <Logo className="mx-auto mb-2 size-12" />
          <CardTitle className="text-xl">初始化管理员</CardTitle>
          <CardDescription>系统首次运行,请创建首位管理员(Owner)</CardDescription>
        </CardHeader>
        <CardContent>
          <form onSubmit={onSubmit} className="grid gap-4">
            <div className="grid gap-2">
              <Label htmlFor="username">用户名</Label>
              <Input id="username" name="username" required minLength={3} autoComplete="username" />
            </div>
            <div className="grid gap-2">
              <Label htmlFor="password">密码</Label>
              <Input id="password" name="password" type="password" required minLength={6} autoComplete="new-password" />
            </div>
            {error && <p className="text-destructive text-sm">{error}</p>}
            <Button type="submit" className="w-full" disabled={busy}>
              {busy ? '创建中…' : '创建并登录'}
            </Button>
          </form>
        </CardContent>
      </Card>
    </div>
  )
}
