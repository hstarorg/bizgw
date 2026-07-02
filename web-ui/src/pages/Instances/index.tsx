import { useQuery } from '@tanstack/react-query'
import { useAuth } from '@/auth/auth-context'
import { api } from '@/lib/api'
import { fmtTime } from '@/lib/format'
import type { InstanceDto } from '@/lib/types'
import { Badge } from '@/components/ui/badge'
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table'

// 纯读页面:无 VM,useQuery + 轮询即可
export default function InstancesPage() {
  const { user } = useAuth()
  const q = useQuery({
    queryKey: ['instances'],
    queryFn: () => api.get<InstanceDto[]>('/instances'),
    refetchInterval: 5000,
    enabled: !!user,
  })

  return (
    <div className="space-y-4">
      <div>
        <h2 className="text-xl font-semibold tracking-tight">实例</h2>
        <p className="text-muted-foreground text-sm">网关实例的存活与配置版本(每 5s 自动刷新)</p>
      </div>

      <div className="rounded-lg border">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>实例</TableHead>
              <TableHead>主机</TableHead>
              <TableHead>状态</TableHead>
              <TableHead>版本(已应用 / 已发布)</TableHead>
              <TableHead>上次 Reload</TableHead>
              <TableHead>心跳</TableHead>
              <TableHead>启动于</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {q.isLoading && (
              <TableRow>
                <TableCell colSpan={7} className="text-muted-foreground h-24 text-center">
                  加载中…
                </TableCell>
              </TableRow>
            )}
            {q.data?.length === 0 && (
              <TableRow>
                <TableCell colSpan={7} className="text-muted-foreground h-24 text-center">
                  暂无实例上报(启动 GatewayServer 后会自动心跳注册)
                </TableCell>
              </TableRow>
            )}
            {q.data?.map((it) => (
              <TableRow key={it.instanceId}>
                <TableCell className="font-medium">{it.instanceId}</TableCell>
                <TableCell>{it.hostname}</TableCell>
                <TableCell>
                  {it.online ? (
                    <Badge className="bg-emerald-600">在线</Badge>
                  ) : (
                    <Badge variant="destructive">离线</Badge>
                  )}
                </TableCell>
                <TableCell>
                  <span className="font-mono">
                    v{it.appliedVersion} / v{it.activeVersion}
                  </span>
                  {it.lagging && (
                    <Badge variant="destructive" className="ml-2">
                      落后
                    </Badge>
                  )}
                </TableCell>
                <TableCell>
                  {it.lastReloadOk ? (
                    <span className="text-emerald-600">成功</span>
                  ) : (
                    <span className="text-destructive">失败</span>
                  )}
                  <span className="text-muted-foreground ml-1 text-xs">{fmtTime(it.lastReloadAt)}</span>
                </TableCell>
                <TableCell className="text-muted-foreground text-xs">{fmtTime(it.lastHeartbeatAt)}</TableCell>
                <TableCell className="text-muted-foreground text-xs">{fmtTime(it.startedAt)}</TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </div>
    </div>
  )
}
