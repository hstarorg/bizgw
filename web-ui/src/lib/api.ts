// 统一信封拆封:HTTP ok 且 code==0 → 返回 data;否则抛 ApiError。401 触发全局回调(跳登录)。
const BASE = '/api'

export class ApiError extends Error {
  constructor(
    public status: number,
    public code: number,
    message: string,
    /** 信封里的 data(如发布 422 的 { errors: string[] }) */
    public data?: unknown,
  ) {
    super(message)
  }
}

let onUnauthorized: (() => void) | null = null
export function setOnUnauthorized(cb: (() => void) | null) {
  onUnauthorized = cb
}

type Envelope<T> = { code: number; message: string; data: T }

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const res = await fetch(BASE + path, {
    credentials: 'same-origin',
    headers: { 'content-type': 'application/json', ...init?.headers },
    ...init,
  })

  let body: Envelope<T> | null = null
  try {
    body = (await res.json()) as Envelope<T>
  } catch {
    // 非 JSON(理论上不该出现,信封覆盖全系统)
  }

  if (res.status === 401) {
    onUnauthorized?.()
    throw new ApiError(401, 401, body?.message ?? '未登录')
  }
  if (!res.ok || !body || body.code !== 0) {
    throw new ApiError(res.status, body?.code ?? res.status, body?.message ?? '请求失败', body?.data)
  }
  return body.data
}

export const api = {
  get: <T>(path: string) => request<T>(path),
  post: <T>(path: string, data?: unknown) =>
    request<T>(path, { method: 'POST', body: data === undefined ? undefined : JSON.stringify(data) }),
  put: <T>(path: string, data?: unknown) =>
    request<T>(path, { method: 'PUT', body: data === undefined ? undefined : JSON.stringify(data) }),
  del: <T>(path: string) => request<T>(path, { method: 'DELETE' }),
}
