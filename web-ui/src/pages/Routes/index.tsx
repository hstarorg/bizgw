import { useQuery } from '@tanstack/react-query'
import { useViewModel } from 'bizify'
import { ChevronDown, ChevronUp, Pencil, Plus, Search, Trash2, X } from 'lucide-react'
import { useAuth } from '@/auth/auth-context'
import { api } from '@/lib/api'
import { fmtTime } from '@/lib/format'
import type { ClusterDto, DestinationDto, Paged, RouteDto } from '@/lib/types'
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
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog'
import { FormDialogContent } from '@/components/form-dialog-content'
import { RichSelectItem } from '@/components/rich-select-item'
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
import { previewTransforms, TRANSFORM_TYPES, typeDef } from './transforms'

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
  // 弹窗内选中目标组后,展示其转发目标(让人知道流量最终去哪)
  const dialogDestsQ = useQuery({
    queryKey: ['destinations', snap.form.clusterCode],
    queryFn: () => api.get<DestinationDto[]>(`/clusters/${snap.form.clusterCode}/destinations`),
    enabled: snap.dialogOpen && !!snap.form.clusterCode,
  })

  const totalPages = routesQ.data ? Math.max(1, Math.ceil(routesQ.data.total / snap.size)) : 1

  // 列表里目标组显示名称(code 是内部连接键,查不到名称时才回退显示 code)
  const clusterNameOf = (code: string) =>
    clustersQ.data?.items.find((c) => c.clusterCode === code)?.clusterName || code

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
            placeholder="搜索 名称 / 路径 / 目标组"
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
              <TableHead>目标组</TableHead>
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
                <TableCell>{clusterNameOf(r.clusterCode)}</TableCell>
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
        <FormDialogContent className="max-h-[85vh] overflow-y-auto sm:max-w-xl">
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
              <Label>目标组</Label>
              <Select
                value={snap.form.clusterCode || undefined}
                onValueChange={(v) => vm.setField('clusterCode', v)}
              >
                <SelectTrigger>
                  <SelectValue placeholder="选择目标组" />
                </SelectTrigger>
                <SelectContent>
                  {clustersQ.data?.items.map((c) => (
                    <RichSelectItem
                      key={c.clusterCode}
                      value={c.clusterCode}
                      label={c.clusterName || c.clusterCode}
                      code={c.clusterName ? c.clusterCode : undefined}
                    />
                  ))}
                </SelectContent>
              </Select>
              {snap.form.clusterCode && dialogDestsQ.data && (
                dialogDestsQ.data.length > 0 ? (
                  <div className="text-muted-foreground text-xs">
                    转发目标:
                    <span className="ml-1 font-mono">
                      {dialogDestsQ.data.map((d) => d.address).join('、')}
                    </span>
                  </div>
                ) : (
                  <p className="text-destructive text-xs">
                    ⚠ 该目标组还没有转发目标,请先到「目标组」页为它添加目标,否则发布后无法转发
                  </p>
                )
              )}
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
              <Label>请求/响应改写(Transforms,按序生效)</Label>
              <div className="space-y-2">
                {snap.transforms.length === 0 && (
                  <p className="text-muted-foreground text-xs">
                    未配置改写规则,请求将原样转发(常见需求:上游不认识路由前缀时用「去除路径前缀」)
                  </p>
                )}
                {snap.transforms.map((t, i) => {
                  const def = typeDef(t.type)
                  return (
                    <div key={t.id} className="space-y-2 rounded-md border p-2">
                      <div className="flex items-center gap-2">
                        <Select value={t.type} onValueChange={(v) => vm.setTransformType(t.id, v)}>
                          <SelectTrigger className="h-8 w-44">
                            <SelectValue />
                          </SelectTrigger>
                          <SelectContent>
                            {TRANSFORM_TYPES.map((tt) => (
                              <SelectItem key={tt.value} value={tt.value}>
                                {tt.label}
                              </SelectItem>
                            ))}
                          </SelectContent>
                        </Select>
                        <div className="ml-auto flex items-center">
                          <Button
                            variant="ghost"
                            size="icon"
                            className="size-7"
                            disabled={i === 0}
                            onClick={() => vm.moveTransform(t.id, -1)}
                          >
                            <ChevronUp />
                          </Button>
                          <Button
                            variant="ghost"
                            size="icon"
                            className="size-7"
                            disabled={i === snap.transforms.length - 1}
                            onClick={() => vm.moveTransform(t.id, 1)}
                          >
                            <ChevronDown />
                          </Button>
                          <Button
                            variant="ghost"
                            size="icon"
                            className="size-7"
                            onClick={() => vm.removeTransform(t.id)}
                          >
                            <X className="text-destructive" />
                          </Button>
                        </div>
                      </div>
                      <div className="flex flex-wrap gap-2">
                        {def.fields.map((f) =>
                          f.kind === 'select' ? (
                            <Select
                              key={f.name}
                              value={t.params[f.name] || f.options?.[0]}
                              onValueChange={(v) => vm.setTransformParam(t.id, f.name, v)}
                            >
                              <SelectTrigger className="h-8 w-28" title={f.label}>
                                <SelectValue />
                              </SelectTrigger>
                              <SelectContent>
                                {f.options?.map((o) => (
                                  <SelectItem key={o} value={o}>
                                    {o}
                                  </SelectItem>
                                ))}
                              </SelectContent>
                            </Select>
                          ) : f.kind === 'json' ? (
                            <Textarea
                              key={f.name}
                              rows={2}
                              className="w-full font-mono text-xs"
                              placeholder={f.placeholder}
                              value={t.params[f.name] ?? ''}
                              onChange={(e) => vm.setTransformParam(t.id, f.name, e.target.value)}
                            />
                          ) : (
                            <Input
                              key={f.name}
                              className="h-8 min-w-32 flex-1 font-mono text-xs"
                              placeholder={`${f.label} 如 ${f.placeholder ?? ''}`}
                              value={t.params[f.name] ?? ''}
                              onChange={(e) => vm.setTransformParam(t.id, f.name, e.target.value)}
                            />
                          ),
                        )}
                      </div>
                    </div>
                  )
                })}
                <div className="flex items-center gap-3">
                  <Button variant="outline" size="sm" onClick={vm.addTransform}>
                    <Plus /> 添加改写规则
                  </Button>
                  {snap.transforms.length > 0 && (
                    <details className="text-muted-foreground text-xs">
                      <summary className="cursor-pointer select-none">JSON 预览</summary>
                      <pre className="bg-muted mt-1 max-h-40 overflow-auto rounded p-2">
                        {previewTransforms(snap.transforms)}
                      </pre>
                    </details>
                  )}
                </div>
              </div>
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
        </FormDialogContent>
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
