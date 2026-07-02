import { QueryClient } from '@tanstack/react-query'

// 模块级单例:React 侧(QueryClientProvider)与 bizify VM 侧共用同一实例,
// VM 经它 invalidate / refetch / setQueryData 驱动服务端数据刷新。
export const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      retry: 1,
      refetchOnWindowFocus: false,
    },
  },
})
