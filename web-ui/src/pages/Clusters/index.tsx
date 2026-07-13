import { useQuery } from '@tanstack/react-query'
import { useViewModel } from 'bizify'
import { Network, Pencil, Plus, Search, Trash2 } from 'lucide-react'
import { useAuth } from '@/auth/auth-context'
import { api } from '@/lib/api'
import { fmtTime } from '@/lib/format'
import type { ClusterDto, DestinationDto, Paged } from '@/lib/types'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import { Checkbox } from '@/components/ui/checkbox'
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
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table'
import {
  Select,
  SelectContent,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'
import { RichSelectItem } from '@/components/rich-select-item'
import { ClustersVM, HEALTH_CHECK_POLICIES, LOAD_BALANCING_POLICIES, policyLabel } from './vm'

export default function ClustersPage() {
  const vm = useViewModel(ClustersVM)
  const snap = vm.useSnapshot()
  const { canWrite } = useAuth()

  const clustersQ = useQuery({
    queryKey: ['clusters', snap.page, snap.size, snap.keyword],
    queryFn: () =>
      api.get<Paged<ClusterDto>>(
        `/clusters?page=${snap.page}&size=${snap.size}&keyword=${encodeURIComponent(snap.keyword)}`,
      ),
  })
  const destsQ = useQuery({
    queryKey: ['destinations', snap.destCluster],
    queryFn: () => api.get<DestinationDto[]>(`/clusters/${snap.destCluster}/destinations`),
    enabled: !!snap.destCluster,
  })

  const totalPages = clustersQ.data ? Math.max(1, Math.ceil(clustersQ.data.total / snap.size)) : 1

  return (
    <div className="space-y-4">
      <div className="flex items-end justify-between gap-4">
        <div>
          <h2 className="text-xl font-semibold tracking-tight">目标组</h2>
          <p className="text-muted-foreground text-sm">
            目标组(YARP Cluster):一组转发目标 + 负载策略 + 健康检查;编辑为草稿,发布后生效
          </p>
        </div>
        {canWrite && (
          <Button onClick={vm.openCreate}>
            <Plus /> 新建目标组
          </Button>
        )}
      </div>

      <div className="flex gap-2">
        <div className="relative w-64">
          <Search className="text-muted-foreground absolute top-2.5 left-2.5 size-4" />
          <Input
            className="pl-8"
            placeholder="搜索 code / 名称"
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
              <TableHead>Code</TableHead>
              <TableHead>转发目标</TableHead>
              <TableHead>负载策略</TableHead>
              <TableHead>健康检查</TableHead>
              <TableHead>被路由引用</TableHead>
              <TableHead>最后修改</TableHead>
              <TableHead className="w-24 text-right">操作</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {clustersQ.isLoading && (
              <TableRow>
                <TableCell colSpan={8} className="text-muted-foreground h-24 text-center">
                  加载中…
                </TableCell>
              </TableRow>
            )}
            {clustersQ.data?.items.length === 0 && (
              <TableRow>
                <TableCell colSpan={8} className="text-muted-foreground h-24 text-center">
                  暂无目标组
                </TableCell>
              </TableRow>
            )}
            {clustersQ.data?.items.map((c) => (
              <TableRow key={c.id}>
                <TableCell className="font-medium">{c.clusterName || c.clusterCode}</TableCell>
                <TableCell className="text-muted-foreground font-mono text-xs">{c.clusterCode}</TableCell>
                <TableCell>
                  <Button
                    variant="link"
                    size="sm"
                    className={`h-auto p-0 ${c.destinationCount === 0 ? 'text-destructive' : ''}`}
                    onClick={() => vm.openDests(c)}
                  >
                    <Network className="size-3.5" />
                    {c.destinationCount > 0 ? `${c.destinationCount} 个目标` : '无目标,去添加'}
                  </Button>
                </TableCell>
                <TableCell>{c.loadBalancingPolicy ? policyLabel(LOAD_BALANCING_POLICIES, c.loadBalancingPolicy) : '—'}</TableCell>
                <TableCell>
                  {c.enabledHealthCheck ? (
                    <Badge variant="secondary">{c.healthCheckPath || 'on'}</Badge>
                  ) : (
                    <span className="text-muted-foreground">关</span>
                  )}
                </TableCell>
                <TableCell>
                  {c.usedByRouteCount > 0 ? `${c.usedByRouteCount} 条路由` : '—'}
                </TableCell>
                <TableCell className="text-muted-foreground text-xs">
                  {fmtTime(c.modifyDate)} {c.modifierName}
                </TableCell>
                <TableCell className="text-right whitespace-nowrap">
                  {canWrite && (
                    <>
                      <Button variant="ghost" size="icon" onClick={() => vm.openEdit(c)}>
                        <Pencil />
                      </Button>
                      <Button variant="ghost" size="icon" onClick={() => vm.askDelete(c)}>
                        <Trash2 className="text-destructive" />
                      </Button>
                    </>
                  )}
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </div>

      <div className="text-muted-foreground flex items-center justify-between text-sm">
        <span>共 {clustersQ.data?.total ?? 0} 条</span>
        <div className="flex items-center gap-2">
          <Button variant="outline" size="sm" disabled={snap.page <= 1} onClick={() => vm.setPage(snap.page - 1)}>
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

      {/* 新建 / 编辑目标组 */}
      <Dialog open={snap.dialogOpen} onOpenChange={(o) => !o && vm.closeDialog()}>
        <FormDialogContent className="sm:max-w-lg">
          <DialogHeader>
            <DialogTitle>
              {snap.editingId == null ? '新建目标组' : `编辑目标组 ${snap.form.clusterName || snap.form.clusterCode}`}
            </DialogTitle>
          </DialogHeader>
          <div className="grid gap-4">
            <div className="grid grid-cols-2 gap-4">
              <div className="grid gap-2">
                <Label htmlFor="clusterName">名称</Label>
                <Input
                  id="clusterName"
                  value={snap.form.clusterName}
                  onChange={(e) => vm.setField('clusterName', e.target.value)}
                />
              </div>
              <div className="grid gap-2">
                <Label>负载策略</Label>
                <Select
                  value={snap.form.loadBalancingPolicy || undefined}
                  onValueChange={(v) => vm.setField('loadBalancingPolicy', v)}
                >
                  <SelectTrigger>
                    <SelectValue placeholder="选择策略" />
                  </SelectTrigger>
                  <SelectContent>
                    {LOAD_BALANCING_POLICIES.map((p) => (
                      <RichSelectItem key={p.value} value={p.value} label={p.label} code={p.value} desc={p.desc} />
                    ))}
                  </SelectContent>
                </Select>
              </div>
            </div>
            {snap.editingId == null && (
              <div className="grid gap-1.5">
                <Label htmlFor="clusterCode" className="text-muted-foreground text-xs font-normal">
                  组标识 Code —— 由名称自动生成,可改;创建后不可改,路由/目标与网关日志经它关联
                </Label>
                <Input
                  id="clusterCode"
                  className="h-8 font-mono text-sm"
                  value={snap.form.clusterCode}
                  onChange={(e) => vm.setField('clusterCode', e.target.value)}
                />
              </div>
            )}
            <label className="flex items-center gap-2 text-sm">
              <Checkbox
                checked={snap.form.enabledHealthCheck}
                onCheckedChange={(v) => vm.setField('enabledHealthCheck', v === true)}
              />
              启用主动健康检查
            </label>
            {snap.form.enabledHealthCheck && (
              <div className="grid grid-cols-2 gap-4">
                <div className="grid gap-2">
                  <Label>检查路径</Label>
                  <Input
                    placeholder="/healthz"
                    value={snap.form.healthCheckPath}
                    onChange={(e) => vm.setField('healthCheckPath', e.target.value)}
                  />
                </div>
                <div className="grid gap-2">
                  <Label>判定策略</Label>
                  <Select
                    value={snap.form.healthCheckPolicy || undefined}
                    onValueChange={(v) => vm.setField('healthCheckPolicy', v)}
                  >
                    <SelectTrigger>
                      <SelectValue placeholder="选择策略" />
                    </SelectTrigger>
                    <SelectContent>
                      {HEALTH_CHECK_POLICIES.map((p) => (
                        <RichSelectItem key={p.value} value={p.value} label={p.label} code={p.value} desc={p.desc} />
                      ))}
                    </SelectContent>
                  </Select>
                </div>
                <div className="grid gap-2">
                  <Label>间隔(秒)</Label>
                  <Input
                    type="number"
                    value={snap.form.healthCheckInterval}
                    onChange={(e) => vm.setField('healthCheckInterval', Number(e.target.value) || 0)}
                  />
                </div>
                <div className="grid gap-2">
                  <Label>超时(秒)</Label>
                  <Input
                    type="number"
                    value={snap.form.healthCheckTimeout}
                    onChange={(e) => vm.setField('healthCheckTimeout', Number(e.target.value) || 0)}
                  />
                </div>
              </div>
            )}
            <div className="grid gap-2">
              <Label htmlFor="remark">备注</Label>
              <Input id="remark" value={snap.form.remark} onChange={(e) => vm.setField('remark', e.target.value)} />
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
        </FormDialogContent>
      </Dialog>

      {/* 目标管理 */}
      <Dialog open={!!snap.destCluster} onOpenChange={(o) => !o && vm.closeDests()}>
        <FormDialogContent className="sm:max-w-xl">
          <DialogHeader>
            <DialogTitle>目标管理 — {snap.destClusterName || snap.destCluster}</DialogTitle>
          </DialogHeader>
          <div className="space-y-3">
            <div className="rounded-lg border">
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>地址</TableHead>
                    <TableHead>名称</TableHead>
                    <TableHead>专用探测地址</TableHead>
                    {canWrite && <TableHead className="w-20 text-right">操作</TableHead>}
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {destsQ.data?.length === 0 && (
                    <TableRow>
                      <TableCell colSpan={4} className="text-muted-foreground h-16 text-center">
                        暂无目标
                      </TableCell>
                    </TableRow>
                  )}
                  {destsQ.data?.map((d) => (
                    <TableRow key={d.id}>
                      <TableCell className="font-mono text-xs">{d.address}</TableCell>
                      <TableCell>{d.name || '—'}</TableCell>
                      <TableCell className="font-mono text-xs">{d.healthCheckPath || '—'}</TableCell>
                      {canWrite && (
                        <TableCell className="text-right whitespace-nowrap">
                          <Button variant="ghost" size="icon" onClick={() => vm.destEdit(d)}>
                            <Pencil />
                          </Button>
                          <Button variant="ghost" size="icon" onClick={() => vm.destDelete(d.id)}>
                            <Trash2 className="text-destructive" />
                          </Button>
                        </TableCell>
                      )}
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </div>

            {canWrite && (
              <div className="space-y-2 rounded-lg border p-3">
                <p className="text-sm font-medium">{snap.destEditingId == null ? '添加目标' : '编辑目标'}</p>
                <div className="grid grid-cols-3 gap-2">
                  <Input
                    placeholder="http://host:port/"
                    className="col-span-3 font-mono text-xs"
                    value={snap.destForm.address}
                    onChange={(e) => vm.destSetField('address', e.target.value)}
                  />
                  <Input
                    placeholder="名称(可选)"
                    value={snap.destForm.name}
                    onChange={(e) => vm.destSetField('name', e.target.value)}
                  />
                  <Input
                    placeholder="专用探测地址(可选,默认用转发地址)"
                    className="col-span-2 font-mono text-xs"
                    value={snap.destForm.healthCheckPath}
                    onChange={(e) => vm.destSetField('healthCheckPath', e.target.value)}
                  />
                </div>
                {snap.destError && <p className="text-destructive text-sm">{snap.destError}</p>}
                <div className="flex justify-end gap-2">
                  {snap.destEditingId != null && (
                    <Button variant="outline" size="sm" onClick={vm.destResetForm}>
                      取消编辑
                    </Button>
                  )}
                  <Button size="sm" onClick={vm.destSave} disabled={snap.destSaving}>
                    {snap.destSaving ? '保存中…' : snap.destEditingId == null ? '添加' : '保存'}
                  </Button>
                </div>
              </div>
            )}
          </div>
        </FormDialogContent>
      </Dialog>

      {/* 删除目标组确认 */}
      <AlertDialog open={!!snap.deleteTarget} onOpenChange={(o) => !o && vm.cancelDelete()}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>删除目标组</AlertDialogTitle>
            <AlertDialogDescription>
              确认删除目标组「{snap.deleteTarget?.clusterName || snap.deleteTarget?.clusterCode}」?
              {snap.deleteTarget && snap.deleteTarget.usedByRouteCount > 0 && (
                <span className="text-destructive block font-medium">
                  ⚠ 它正被 {snap.deleteTarget.usedByRouteCount} 条路由引用,删除后这些路由将无法通过发布校验。
                </span>
              )}
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
