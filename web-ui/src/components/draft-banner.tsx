import { useQuery } from '@tanstack/react-query'
import { CircleAlert } from 'lucide-react'
import { Link } from 'react-router'
import { api } from '@/lib/api'

/** 全局草稿提示:有未发布改动时在顶栏亮出,点击去发布页。 */
export function DraftBanner() {
  const q = useQuery({
    queryKey: ['config', 'draft-status'],
    queryFn: () => api.get<{ hasPendingChanges: boolean }>('/config/draft-status'),
    refetchInterval: 30000,
  })

  if (!q.data?.hasPendingChanges) return null
  return (
    <Link
      to="/config"
      className="flex items-center gap-1.5 rounded-md bg-amber-500/15 px-2.5 py-1 text-xs font-medium text-amber-600 hover:bg-amber-500/25 dark:text-amber-400"
    >
      <CircleAlert className="size-3.5" />
      有未发布的草稿改动
    </Link>
  )
}
