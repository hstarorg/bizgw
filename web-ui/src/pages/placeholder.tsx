import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'

function PagePlaceholder({ title, description }: { title: string; description: string }) {
  return (
    <div className="space-y-4">
      <div>
        <h2 className="text-xl font-semibold tracking-tight">{title}</h2>
        <p className="text-muted-foreground text-sm">{description}</p>
      </div>
      <div className="text-muted-foreground flex h-64 items-center justify-center rounded-lg border border-dashed text-sm">
        待接入 API（本次仅 UI）
      </div>
    </div>
  )
}

export function DashboardPage() {
  const stats = [
    { label: '在线实例', value: '—' },
    { label: '当前版本', value: '—' },
    { label: '路由数', value: '—' },
    { label: '集群数', value: '—' },
  ]
  return (
    <div className="space-y-4">
      <div>
        <h2 className="text-xl font-semibold tracking-tight">仪表盘</h2>
        <p className="text-muted-foreground text-sm">网关集群概览</p>
      </div>
      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
        {stats.map((s) => (
          <Card key={s.label}>
            <CardHeader className="pb-2">
              <CardTitle className="text-muted-foreground text-sm font-medium">{s.label}</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-semibold">{s.value}</div>
            </CardContent>
          </Card>
        ))}
      </div>
    </div>
  )
}

export const RoutesPage = () => (
  <PagePlaceholder title="路由" description="管理反向代理路由（match / methods / transforms）" />
)
export const ClustersPage = () => (
  <PagePlaceholder title="集群" description="管理集群与目标（destinations / 健康检查 / 负载策略）" />
)
export const ConfigPage = () => (
  <PagePlaceholder title="配置发布" description="发布 / 回滚版本化快照,查看历史" />
)
export const InstancesPage = () => (
  <PagePlaceholder title="实例" description="各网关实例的存活 / 版本 / 是否落后 / reload 成败" />
)
export const UsersPage = () => <PagePlaceholder title="用户" description="管理登录账户" />
