import { Routes, Route } from 'react-router'
import { RequireAuth } from '@/auth/require-auth'
import { AppLayout } from '@/components/app-layout'
import LoginPage from '@/pages/Login'
import SetupPage from '@/pages/Setup'
import DashboardPage from '@/pages/Dashboard'
import RoutesPage from '@/pages/Routes'
import ClustersPage from '@/pages/Clusters'
import ConfigPage from '@/pages/Config'
import InstancesPage from '@/pages/Instances'
import UsersPage from '@/pages/Users'

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
        <Route index element={<DashboardPage />} />
        <Route path="routes" element={<RoutesPage />} />
        <Route path="clusters" element={<ClustersPage />} />
        <Route path="config" element={<ConfigPage />} />
        <Route path="instances" element={<InstancesPage />} />
        <Route path="users" element={<UsersPage />} />
      </Route>
    </Routes>
  )
}
