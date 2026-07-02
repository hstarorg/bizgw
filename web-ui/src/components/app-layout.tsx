import { Outlet, useNavigate } from 'react-router'
import { LogOut, User } from 'lucide-react'
import { useAuth } from '@/auth/auth-context'
import { AppSidebar } from '@/components/app-sidebar'
import { DraftBanner } from '@/components/draft-banner'
import { Avatar, AvatarFallback } from '@/components/ui/avatar'
import { Separator } from '@/components/ui/separator'
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu'
import { SidebarInset, SidebarProvider, SidebarTrigger } from '@/components/ui/sidebar'

export function AppLayout() {
  const navigate = useNavigate()
  const { user, logout } = useAuth()

  async function onLogout() {
    await logout()
    navigate('/login')
  }

  return (
    <SidebarProvider>
      <AppSidebar />
      <SidebarInset>
        <header className="flex h-14 shrink-0 items-center gap-2 border-b px-4">
          <SidebarTrigger className="-ml-1" />
          <Separator orientation="vertical" className="mr-2 !h-4" />
          <span className="text-sm font-medium">网关配置管理</span>
          <div className="ml-auto flex items-center gap-3">
            <DraftBanner />
            <DropdownMenu>
              <DropdownMenuTrigger className="focus-visible:ring-ring flex items-center gap-2 rounded-md p-1 outline-none focus-visible:ring-2">
                <Avatar className="size-8">
                  <AvatarFallback>
                    <User className="size-4" />
                  </AvatarFallback>
                </Avatar>
                <span className="text-sm">{user?.username ?? '—'}</span>
              </DropdownMenuTrigger>
              <DropdownMenuContent align="end" className="w-44">
                <DropdownMenuLabel>
                  <div className="grid">
                    <span>{user?.username}</span>
                    <span className="text-muted-foreground text-xs font-normal">{user?.role}</span>
                  </div>
                </DropdownMenuLabel>
                <DropdownMenuSeparator />
                <DropdownMenuItem onSelect={onLogout}>
                  <LogOut />
                  退出登录
                </DropdownMenuItem>
              </DropdownMenuContent>
            </DropdownMenu>
          </div>
        </header>
        <main className="flex-1 p-6">
          <Outlet />
        </main>
      </SidebarInset>
    </SidebarProvider>
  )
}
