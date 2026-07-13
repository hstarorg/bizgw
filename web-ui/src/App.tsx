import { lazy, Suspense } from 'react'
import { Routes, Route } from 'react-router'
import { RequireAuth } from '@/auth/require-auth'
import { AppLayout } from '@/components/app-layout'
import LoginPage from '@/pages/Login'
import SetupPage from '@/pages/Setup'

// 业务页按路由分包(登录/初始化保持同步加载,保证首屏)
const DashboardPage = lazy(() => import('@/pages/Dashboard'))
const RoutesPage = lazy(() => import('@/pages/Routes'))
const ClustersPage = lazy(() => import('@/pages/Clusters'))
const ConfigPage = lazy(() => import('@/pages/Config'))
const InstancesPage = lazy(() => import('@/pages/Instances'))
const UsersPage = lazy(() => import('@/pages/Users'))

function PageLoading() {
  return <div className="text-muted-foreground flex h-full min-h-40 items-center justify-center text-sm">加载中…</div>
}

export default function App() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route path="/setup" element={<SetupPage />} />
      <Route
        element={
          <RequireAuth>
            <AppLayout />
          </RequireAuth>
        }
      >
        <Route
          index
          element={
            <Suspense fallback={<PageLoading />}>
              <DashboardPage />
            </Suspense>
          }
        />
        <Route
          path="routes"
          element={
            <Suspense fallback={<PageLoading />}>
              <RoutesPage />
            </Suspense>
          }
        />
        <Route
          path="clusters"
          element={
            <Suspense fallback={<PageLoading />}>
              <ClustersPage />
            </Suspense>
          }
        />
        <Route
          path="config"
          element={
            <Suspense fallback={<PageLoading />}>
              <ConfigPage />
            </Suspense>
          }
        />
        <Route
          path="instances"
          element={
            <Suspense fallback={<PageLoading />}>
              <InstancesPage />
            </Suspense>
          }
        />
        <Route
          path="users"
          element={
            <Suspense fallback={<PageLoading />}>
              <UsersPage />
            </Suspense>
          }
        />
      </Route>
    </Routes>
  )
}
