import { Routes, Route } from 'react-router'
import { AppLayout } from '@/components/app-layout'
import LoginPage from '@/pages/login'
import {
  ClustersPage,
  ConfigPage,
  DashboardPage,
  InstancesPage,
  RoutesPage,
  UsersPage,
} from '@/pages/placeholder'

export default function App() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route element={<AppLayout />}>
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
