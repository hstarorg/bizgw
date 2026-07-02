import { useQuery } from '@tanstack/react-query'
import { Link } from 'react-router'
import { api } from '@/lib/api'
import type { InstanceDto, Paged } from '@/lib/types'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'

export default function DashboardPage() {
  const instancesQ = useQuery({
    queryKey: ['instances'],
    queryFn: () => api.get<InstanceDto[]>('/instances'),
    refetchInterval: 10000,
  })
  const versionQ = useQuery({
    queryKey: ['config', 'version'],
    queryFn: () => api.get<{ activeVersion: number }>('/config/version'),
  })
  const routesQ = useQuery({
    queryKey: ['routes', 'count'],
    queryFn: () => api.get<Paged<unknown>>('/routes?page=1&size=1'),
  })
  const clustersQ = useQuery({
    queryKey: ['clusters', 'count'],
    queryFn: () => api.get<Paged<unknown>>('/clusters?page=1&size=1'),
  })

  const online = instancesQ.data?.filter((i) => i.online).length
  const lagging = instancesQ.data?.filter((i) => i.online && i.lagging).length ?? 0

  const stats = [
    {
      label: '在线实例',
      value: online ?? '—',
      hint: lagging > 0 ? `${lagging} 个版本落后` : undefined,
      to: '/instances',
    },
    { label: '生效版本', value: versionQ.data ? `v${versionQ.data.activeVersion}` : '—', to: '/config' },
    { label: '路由数', value: routesQ.data?.total ?? '—', to: '/routes' },
    { label: '目标组数', value: clustersQ.data?.total ?? '—', to: '/clusters' },
  ]

  return (
    <div className="space-y-4">
      <div>
        <h2 className="text-xl font-semibold tracking-tight">仪表盘</h2>
        <p className="text-muted-foreground text-sm">网关运行概览</p>
      </div>
      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
        {stats.map((s) => (
          <Link key={s.label} to={s.to}>
            <Card className="hover:bg-accent/40 transition-colors">
              <CardHeader className="pb-2">
                <CardTitle className="text-muted-foreground text-sm font-medium">{s.label}</CardTitle>
              </CardHeader>
              <CardContent>
                <div className="text-2xl font-semibold">{s.value}</div>
                {s.hint && <p className="text-destructive mt-1 text-xs">{s.hint}</p>}
              </CardContent>
            </Card>
          </Link>
        ))}
      </div>
    </div>
  )
}
