import { NavLink, useLocation } from 'react-router'
import { Activity, Boxes, LayoutDashboard, Rocket, Route as RouteIcon, Users } from 'lucide-react'
import { Logo } from '@/components/logo'
import {
  Sidebar,
  SidebarContent,
  SidebarGroup,
  SidebarGroupLabel,
  SidebarHeader,
  SidebarMenu,
  SidebarMenuButton,
  SidebarMenuItem,
  SidebarRail,
} from '@/components/ui/sidebar'

const items = [
  { title: '仪表盘', url: '/', icon: LayoutDashboard },
  { title: '路由', url: '/routes', icon: RouteIcon },
  { title: '集群', url: '/clusters', icon: Boxes },
  { title: '配置发布', url: '/config', icon: Rocket },
  { title: '实例', url: '/instances', icon: Activity },
  { title: '用户', url: '/users', icon: Users },
]

export function AppSidebar() {
  const { pathname } = useLocation()

  return (
    <Sidebar>
      <SidebarHeader>
        <div className="flex items-center gap-2 px-2 py-1.5">
          <Logo className="size-8 shrink-0" />
          <div className="grid leading-tight">
            <span className="text-sm font-semibold">Bizgw</span>
            <span className="text-muted-foreground text-xs">管理后台</span>
          </div>
        </div>
      </SidebarHeader>
      <SidebarContent>
        <SidebarGroup>
          <SidebarGroupLabel>管理</SidebarGroupLabel>
          <SidebarMenu>
            {items.map((item) => {
              const active = item.url === '/' ? pathname === '/' : pathname.startsWith(item.url)
              return (
                <SidebarMenuItem key={item.url}>
                  <SidebarMenuButton asChild isActive={active} tooltip={item.title}>
                    <NavLink to={item.url}>
                      <item.icon />
                      <span>{item.title}</span>
                    </NavLink>
                  </SidebarMenuButton>
                </SidebarMenuItem>
              )
            })}
          </SidebarMenu>
        </SidebarGroup>
      </SidebarContent>
      <SidebarRail />
    </Sidebar>
  )
}
