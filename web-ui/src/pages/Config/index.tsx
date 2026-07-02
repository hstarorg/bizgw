import { useQuery } from '@tanstack/react-query'
import { useViewModel } from 'bizify'
import { Rocket } from 'lucide-react'
import { useAuth } from '@/auth/auth-context'
import { api } from '@/lib/api'
import { fmtTime } from '@/lib/format'
import type { SnapshotDto } from '@/lib/types'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
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
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table'
import { ConfigVM } from './vm'

export default function ConfigPage() {
  const vm = useViewModel(ConfigVM)
  const snap = vm.useSnapshot()
  const { canWrite } = useAuth()

  const versionQ = useQuery({
    queryKey: ['config', 'version'],
    queryFn: () => api.get<{ activeVersion: number }>('/config/version'),
  })
  const draftQ = useQuery({
    queryKey: ['config', 'draft-status'],
    queryFn: () => api.get<{ hasPendingChanges: boolean }>('/config/draft-status'),
  })
  const snapshotsQ = useQuery({
    queryKey: ['config', 'snapshots'],
    queryFn: () => api.get<SnapshotDto[]>('/config/snapshots'),
  })

  return (
    <div className="space-y-4">
      <div>
        <h2 className="text-xl font-semibold tracking-tight">配置发布</h2>
        <p className="text-muted-foreground text-sm">
          编辑表是草稿,发布生成不可变快照;网关实例秒级同步
        </p>
      </div>

      <Card>
        <CardHeader className="pb-2">
          <CardTitle className="text-sm font-medium">当前状态</CardTitle>
        </CardHeader>
        <CardContent className="flex flex-wrap items-center gap-4">
          <div>
            <span className="text-muted-foreground mr-2 text-sm">生效版本</span>
            <span className="font-mono text-2xl font-semibold">
              v{versionQ.data?.activeVersion ?? '—'}
            </span>
          </div>
          {draftQ.data?.hasPendingChanges ? (
            <Badge className="bg-amber-500">有未发布的草稿改动</Badge>
          ) : (
            <Badge variant="secondary">与草稿一致</Badge>
          )}
          {canWrite && (
            <Button className="ml-auto" onClick={vm.publish} disabled={snap.publishing}>
              <Rocket />
              {snap.publishing ? '发布中…' : '发布当前配置'}
            </Button>
          )}
        </CardContent>
      </Card>

      {snap.publishErrors.length > 0 && (
        <div className="border-destructive/50 bg-destructive/5 text-destructive rounded-lg border p-4 text-sm">
          <p className="mb-1 font-medium">发布被校验拦下:</p>
          <ul className="list-disc pl-5">
            {snap.publishErrors.map((e) => (
              <li key={e}>{e}</li>
            ))}
          </ul>
        </div>
      )}

      <div className="rounded-lg border">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>版本</TableHead>
              <TableHead>状态</TableHead>
              <TableHead>发布时间</TableHead>
              <TableHead>发布人</TableHead>
              <TableHead>Schema</TableHead>
              {canWrite && <TableHead className="w-20 text-right">操作</TableHead>}
            </TableRow>
          </TableHeader>
          <TableBody>
            {snapshotsQ.data?.length === 0 && (
              <TableRow>
                <TableCell colSpan={6} className="text-muted-foreground h-24 text-center">
                  尚无发布记录
                </TableCell>
              </TableRow>
            )}
            {snapshotsQ.data?.map((s) => (
              <TableRow key={s.version}>
                <TableCell className="font-mono font-medium">v{s.version}</TableCell>
                <TableCell>
                  {s.isActive ? <Badge className="bg-emerald-600">生效中</Badge> : <Badge variant="outline">历史</Badge>}
                </TableCell>
                <TableCell className="text-muted-foreground text-xs">{fmtTime(s.publishedAt)}</TableCell>
                <TableCell>{s.publishedBy}</TableCell>
                <TableCell className="text-muted-foreground">{s.schemaVersion}</TableCell>
                {canWrite && (
                  <TableCell className="text-right">
                    {!s.isActive && (
                      <Button variant="ghost" size="sm" onClick={() => vm.askRollback(s.version)}>
                        回滚
                      </Button>
                    )}
                  </TableCell>
                )}
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </div>

      <AlertDialog open={snap.rollbackTarget != null} onOpenChange={(o) => !o && vm.cancelRollback()}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>回滚配置</AlertDialogTitle>
            <AlertDialogDescription>
              将把 v{snap.rollbackTarget} 的内容作为新版本重新发布(版本号递增,不覆盖历史),所有网关实例会自动同步。
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel disabled={snap.rollingBack}>取消</AlertDialogCancel>
            <AlertDialogAction onClick={vm.confirmRollback} disabled={snap.rollingBack}>
              {snap.rollingBack ? '回滚中…' : '确认回滚'}
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  )
}
