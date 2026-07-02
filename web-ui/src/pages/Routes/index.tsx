import { useQuery } from '@tanstack/react-query'
import { useViewModel } from 'bizify'
import { Pencil, Plus, Search, Trash2 } from 'lucide-react'
import { useAuth } from '@/auth/auth-context'
import { api } from '@/lib/api'
import { fmtTime } from '@/lib/format'
import type { ClusterDto, Paged, RouteDto } from '@/lib/types'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import { Checkbox } from '@/components/ui/checkbox'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Textarea } from '@/components/ui/textarea'
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
  DialogContent,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog'
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
import { HTTP_METHODS, RoutesVM } from './vm'

export default function RoutesPage() {
  const vm = useViewModel(RoutesVM)
  const snap = vm.useSnapshot()
  const { canWrite } = useAuth()

  // 服务端数据:View 侧 useQuery 订阅;key 来自 VM 状态,VM 写完 invalidate 自动重拉
  const routesQ = useQuery({
    queryKey: ['routes', snap.page, snap.size, snap.keyword],
    queryFn: () =>
      api.get<Paged<RouteDto>>(
        `/routes?page=${snap.page}&size=${snap.size}&keyword=${encodeURIComponent(snap.keyword)}`,
      ),
  })
  const clustersQ = useQuery({
    queryKey: ['clusters', 'options'],
    queryFn: () => api.get<Paged<ClusterDto>>('/clusters?page=1&size=200'),
  })

  const totalPages = routesQ.data ? Math.max(1, Math.ceil(routesQ.data.total / snap.size)) : 1

  return (
    <div className="space-y-4">
      <div className="flex items-end justify-between gap-4">
        <div>
          <h2 className="text-xl font-semibold tracking-tight">路由</h2>
          <p className="text-muted-foreground text-sm">
            编辑为草稿,需在「配置发布」页发布后生效
          </p>
        </div>
        {canWrite && (
          <Button onClick={vm.openCreate}>
            <Plus /> 新建路由
          </Button>
        )}
      </div>

      <div className="flex gap-2">
        <div className="relative w-64">
          <Search className="text-muted-foreground absolute top-2.5 left-2.5 size-4" />
          <Input
            className="pl-8"
            placeholder="搜索 名称 / 路径 / 集群"
            value={snap.keywordInput}
            onChange={(e) => vm.setKeywordInput(e.target.value)}
            onKeyDown={(e) => e.key === 'Enter' && vm.search()}
          />
        </div>
        <Button variant="secondary" onClick={vm.search}>
          搜索
        </Button>
      </div>

      <div className="rounded-lg border">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>名称</TableHead>
              <TableHead>匹配路径</TableHead>
              <TableHead>Methods</TableHead>
              <TableHead>集群</TableHead>
              <TableHead>最后修改</TableHead>
              {canWrite && <TableHead className="w-24 text-right">操作</TableHead>}
            </TableRow>
          </TableHeader>
          <TableBody>
            {routesQ.isLoading && (
              <TableRow>
                <TableCell colSpan={6} className="text-muted-foreground h-24 text-center">
                  加载中…
                </TableCell>
              </TableRow>
            )}
            {routesQ.isError && (
              <TableRow>
                <TableCell colSpan={6} className="text-destructive h-24 text-center">
                  加载失败
                  <Button variant="link" size="sm" onClick={() => routesQ.refetch()}>
                    重试
                  </Button>
                </TableCell>
              </TableRow>
            )}
            {routesQ.data?.items.length === 0 && (
              <TableRow>
                <TableCell colSpan={6} className="text-muted-foreground h-24 text-center">
                  暂无路由
                </TableCell>
              </TableRow>
            )}
            {routesQ.data?.items.map((r) => (
              <TableRow key={r.id}>
                <TableCell className="font-medium">{r.routeName || '—'}</TableCell>
                <TableCell className="font-mono text-xs">{r.matchPath}</TableCell>
                <TableCell>
                  <div className="flex flex-wrap gap-1">
                    {r.matchMethods.map((m) => (
                      <Badge key={m} variant="secondary">
                        {m}
                      </Badge>
                    ))}
                  </div>
                </TableCell>
                <TableCell>{r.clusterCode}</TableCell>
                <TableCell className="text-muted-foreground text-xs">
                  {fmtTime(r.modifyDate)}
                  <span className="ml-1">{r.modifierName}</span>
                </TableCell>
                {canWrite && (
                  <TableCell className="text-right">
                    <Button variant="ghost" size="icon" onClick={() => vm.openEdit(r)}>
                      <Pencil />
                    </Button>
                    <Button variant="ghost" size="icon" onClick={() => vm.askDelete(r)}>
                      <Trash2 className="text-destructive" />
                    </Button>
                  </TableCell>
                )}
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </div>

      <div className="text-muted-foreground flex items-center justify-between text-sm">
        <span>共 {routesQ.data?.total ?? 0} 条</span>
        <div className="flex items-center gap-2">
          <Button
            variant="outline"
            size="sm"
            disabled={snap.page <= 1}
            onClick={() => vm.setPage(snap.page - 1)}
          >
            上一页
          </Button>
          <span>
            {snap.page} / {totalPages}
          </span>
          <Button
            variant="outline"
            size="sm"
            disabled={snap.page >= totalPages}
            onClick={() => vm.setPage(snap.page + 1)}
          >
            下一页
          </Button>
        </div>
      </div>

      {/* 新建 / 编辑 */}
      <Dialog open={snap.dialogOpen} onOpenChange={(o) => !o && vm.closeDialog()}>
        <DialogContent className="sm:max-w-lg">
          <DialogHeader>
            <DialogTitle>{snap.editingId == null ? '新建路由' : '编辑路由'}</DialogTitle>
          </DialogHeader>
          <div className="grid gap-4">
            <div className="grid gap-2">
              <Label htmlFor="routeName">名称</Label>
              <Input
                id="routeName"
                value={snap.form.routeName}
                onChange={(e) => vm.setField('routeName', e.target.value)}
              />
            </div>
            <div className="grid gap-2">
              <Label>集群</Label>
              <Select
                value={snap.form.clusterCode || undefined}
                onValueChange={(v) => vm.setField('clusterCode', v)}
              >
                <SelectTrigger>
                  <SelectValue placeholder="选择集群" />
                </SelectTrigger>
                <SelectContent>
                  {clustersQ.data?.items.map((c) => (
                    <SelectItem key={c.clusterCode} value={c.clusterCode}>
                      {c.clusterCode}
                      {c.clusterName ? ` (${c.clusterName})` : ''}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
            <div className="grid gap-2">
              <Label htmlFor="matchPath">匹配路径</Label>
              <Input
                id="matchPath"
                placeholder="/apis/foo/{**catch-all}"
                className="font-mono"
                value={snap.form.matchPath}
                onChange={(e) => vm.setField('matchPath', e.target.value)}
              />
            </div>
            <div className="grid gap-2">
              <Label>Methods</Label>
              <div className="flex flex-wrap gap-3">
                {HTTP_METHODS.map((m) => (
                  <label key={m} className="flex items-center gap-1.5 text-sm">
                    <Checkbox
                      checked={snap.form.matchMethods.includes(m)}
                      onCheckedChange={() => vm.toggleMethod(m)}
                    />
                    {m}
                  </label>
                ))}
              </div>
            </div>
            <div className="grid gap-2">
              <Label htmlFor="transforms">Transforms(JSON 数组)</Label>
              <Textarea
                id="transforms"
                rows={3}
                className="font-mono text-xs"
                value={snap.form.transforms}
                onChange={(e) => vm.setField('transforms', e.target.value)}
              />
            </div>
            <div className="grid gap-2">
              <Label htmlFor="remark">备注</Label>
              <Input
                id="remark"
                value={snap.form.remark}
                onChange={(e) => vm.setField('remark', e.target.value)}
              />
            </div>
            {snap.formError && <p className="text-destructive text-sm">{snap.formError}</p>}
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={vm.closeDialog} disabled={snap.saving}>
              取消
            </Button>
            <Button onClick={vm.save} disabled={snap.saving}>
              {snap.saving ? '保存中…' : '保存'}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* 删除确认 */}
      <AlertDialog open={!!snap.deleteTarget} onOpenChange={(o) => !o && vm.cancelDelete()}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>删除路由</AlertDialogTitle>
            <AlertDialogDescription>
              确认删除路由「{snap.deleteTarget?.routeName || snap.deleteTarget?.matchPath}
              」?删除为草稿改动,发布后生效。
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
