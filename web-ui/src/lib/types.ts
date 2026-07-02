// 与 ControlPlane API 的共享契约(信封拆封后的 data 形状)

export type Paged<T> = { items: T[]; total: number; page: number; size: number }

export type RouteDto = {
  id: number
  routeName: string
  clusterCode: string
  matchPath: string
  matchMethods: string[]
  transforms: string
  remark: string
  createDate: number
  modifyDate: number
  creatorName: string
  modifierName: string
}

export type RouteUpsert = {
  routeName: string
  clusterCode: string
  matchPath: string
  matchMethods: string[]
  transforms: string
  remark: string
}

export type ClusterDto = {
  id: number
  clusterCode: string
  clusterName: string
  loadBalancingPolicy: string
  enabledHealthCheck: boolean
  healthCheckInterval: number
  healthCheckTimeout: number
  healthCheckPolicy: string
  healthCheckPath: string
  remark: string
  usedByRouteCount: number
  createDate: number
  modifyDate: number
  creatorName: string
  modifierName: string
}
