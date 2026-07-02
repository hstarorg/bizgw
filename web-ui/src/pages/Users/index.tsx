import { useQuery } from '@tanstack/react-query'
import { useViewModel } from 'bizify'
import { KeyRound, Plus, Trash2 } from 'lucide-react'
import { Navigate } from 'react-router'
import { useAuth } from '@/auth/auth-context'
import { api } from '@/lib/api'
import { fmtTime } from '@/lib/format'
import type { UserDto } from '@/lib/types'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from '@/components/ui/alert-dialog'
import {
  Dialog,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog'
import { FormDialogContent } from '@/components/form-dialog-content'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table'
import { UsersVM } from './vm'

const ROLES = ['Owner', 'Editor', 'Viewer'] as const

export default function UsersPage() {
  const vm = useViewModel(UsersVM)
  const snap = vm.useSnapshot()
  const { user, canManage } = useAuth()

  const usersQ = useQuery({
    queryKey: ['users'],
    queryFn: () => api.get<UserDto[]>('/users'),
    enabled: canManage,
  })

  // 页面级门控:非 Owner 直接回仪表盘(后端仍强制 403)
  if (!canManage) return <Navigate to="/" replace />

  return (
    <div className="space-y-4">
      <div className="flex items-end justify-between gap-4">
        <div>
          <h2 className="text-xl font-semibold tracking-tight">用户</h2>
          <p className="text-muted-foreground text-sm">
            登录账户与角色(Owner 全权 / Editor 可改配置 / Viewer 只读)
          </p>
        </div>
        <Button onClick={vm.openCreate}>
          <Plus /> 新建用户
        </Button>
      </div>

      <div className="rounded-lg border">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>用户名</TableHead>
              <TableHead>角色</TableHead>
              <TableHead>创建</TableHead>
              <TableHead>最后修改</TableHead>
              <TableHead className="w-24 text-right">操作</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {usersQ.isLoading && (
              <TableRow>
                <TableCell colSpan={5} className="text-muted-foreground h-24 text-center">
                  加载中…
                </TableCell>
              </TableRow>
            )}
            {usersQ.data?.map((u) => (
              <TableRow key={u.id}>
                <TableCell className="font-medium">
                  {u.username}
                  {u.username === user?.username && (
                    <Badge variant="outline" className="ml-2">
                      我
                    </Badge>
                  )}
                </TableCell>
                <TableCell>
                  <Select value={u.role} onValueChange={(v) => vm.changeRole(u, v)}>
                    <SelectTrigger className="h-8 w-28">
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      {ROLES.map((r) => (
                        <SelectItem key={r} value={r}>
                          {r}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </TableCell>
                <TableCell className="text-muted-foreground text-xs">
                  {fmtTime(u.createDate)} {u.creatorName}
                </TableCell>
                <TableCell className="text-muted-foreground text-xs">
                  {fmtTime(u.modifyDate)} {u.modifierName}
                </TableCell>
                <TableCell className="text-right whitespace-nowrap">
                  <Button variant="ghost" size="icon" title="重置密码" onClick={() => vm.askReset(u)}>
                    <KeyRound />
                  </Button>
                  <Button
                    variant="ghost"
                    size="icon"
                    title="删除"
                    disabled={u.username === user?.username}
                    onClick={() => vm.askDelete(u)}
                  >
                    <Trash2 className="text-destructive" />
                  </Button>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </div>

      {/* 新建用户 */}
      <Dialog open={snap.createOpen} onOpenChange={(o) => !o && vm.closeCreate()}>
        <FormDialogContent className="sm:max-w-sm">
          <DialogHeader>
            <DialogTitle>新建用户</DialogTitle>
          </DialogHeader>
          <div className="grid gap-4">
            <div className="grid gap-2">
              <Label htmlFor="nu">用户名</Label>
              <Input id="nu" value={snap.form.username} onChange={(e) => vm.setField('username', e.target.value)} />
            </div>
            <div className="grid gap-2">
              <Label htmlFor="np">初始密码</Label>
              <Input
                id="np"
                type="password"
                value={snap.form.password}
                onChange={(e) => vm.setField('password', e.target.value)}
              />
            </div>
            <div className="grid gap-2">
              <Label>角色</Label>
              <Select value={snap.form.role} onValueChange={(v) => vm.setField('role', v)}>
                <SelectTrigger>
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  {ROLES.map((r) => (
                    <SelectItem key={r} value={r}>
                      {r}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
            {snap.formError && <p className="text-destructive text-sm">{snap.formError}</p>}
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={vm.closeCreate} disabled={snap.saving}>
              取消
            </Button>
            <Button onClick={vm.create} disabled={snap.saving}>
              {snap.saving ? '创建中…' : '创建'}
            </Button>
          </DialogFooter>
        </FormDialogContent>
      </Dialog>

      {/* 重置密码 */}
      <Dialog open={!!snap.resetTarget} onOpenChange={(o) => !o && vm.cancelReset()}>
        <FormDialogContent className="sm:max-w-sm">
          <DialogHeader>
            <DialogTitle>重置密码 — {snap.resetTarget?.username}</DialogTitle>
          </DialogHeader>
          <div className="grid gap-2">
            <Label htmlFor="rp">新密码</Label>
            <Input
              id="rp"
              type="password"
              value={snap.resetPassword}
              onChange={(e) => vm.setResetPassword(e.target.value)}
            />
            {snap.resetError && <p className="text-destructive text-sm">{snap.resetError}</p>}
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={vm.cancelReset} disabled={snap.resetting}>
              取消
            </Button>
            <Button onClick={vm.confirmReset} disabled={snap.resetting}>
              {snap.resetting ? '重置中…' : '重置'}
            </Button>
          </DialogFooter>
        </FormDialogContent>
      </Dialog>

      {/* 删除用户 */}
      <AlertDialog open={!!snap.deleteTarget} onOpenChange={(o) => !o && vm.cancelDelete()}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>删除用户</AlertDialogTitle>
            <AlertDialogDescription>
              确认删除用户「{snap.deleteTarget?.username}」?删除后同名可重新创建。
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel disabled={snap.deleting}>取消</AlertDialogCancel>
            <AlertDialogAction onClick={vm.confirmDelete} disabled={snap.deleting}>
              {snap.deleting ? '删除中…' : '删除'}
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  )
}
