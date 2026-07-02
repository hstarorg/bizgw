// transforms 可视化编辑器的领域逻辑:
// 存储格式(route.transforms 列): [{ "key": "备注", "value": { <YARP transform 字典> } }]
// 这里维护「常用规则目录」+ 草稿(draft)与 JSON 的双向转换;未识别的 value 退化为 custom(自定义 JSON)。

export type FieldDef = {
  name: string
  label: string
  kind: 'input' | 'select' | 'json'
  placeholder?: string
  options?: readonly string[]
}

export type TransformTypeDef = {
  value: string
  label: string
  fields: FieldDef[]
  /** 参数 → YARP transform 字典(可抛 Error) */
  build: (p: Record<string, string>) => Record<string, string>
  /** YARP 字典 → 参数;不匹配返回 null */
  detect: (v: Record<string, string>) => Record<string, string> | null
}

const MODES = ['Set', 'Append'] as const
const WHENS = ['Success', 'Always', 'Failure'] as const

function only(v: Record<string, string>, keys: string[]): boolean {
  const ks = Object.keys(v)
  return ks.length === keys.length && keys.every((k) => k in v)
}

export const TRANSFORM_TYPES: TransformTypeDef[] = [
  {
    value: 'PathRemovePrefix',
    label: '去除路径前缀',
    fields: [{ name: 'prefix', label: '前缀', kind: 'input', placeholder: '/apis/foo' }],
    build: (p) => ({ PathRemovePrefix: p.prefix }),
    detect: (v) => (only(v, ['PathRemovePrefix']) ? { prefix: v.PathRemovePrefix } : null),
  },
  {
    value: 'PathPrefix',
    label: '添加路径前缀',
    fields: [{ name: 'prefix', label: '前缀', kind: 'input', placeholder: '/v1' }],
    build: (p) => ({ PathPrefix: p.prefix }),
    detect: (v) => (only(v, ['PathPrefix']) ? { prefix: v.PathPrefix } : null),
  },
  {
    value: 'PathSet',
    label: '重设路径',
    fields: [{ name: 'path', label: '路径', kind: 'input', placeholder: '/fixed/path' }],
    build: (p) => ({ PathSet: p.path }),
    detect: (v) => (only(v, ['PathSet']) ? { path: v.PathSet } : null),
  },
  {
    value: 'PathPattern',
    label: '路径模式重写',
    fields: [{ name: 'pattern', label: '模式', kind: 'input', placeholder: '/api/{**catch-all}' }],
    build: (p) => ({ PathPattern: p.pattern }),
    detect: (v) => (only(v, ['PathPattern']) ? { pattern: v.PathPattern } : null),
  },
  {
    value: 'RequestHeader',
    label: '设置请求头',
    fields: [
      { name: 'name', label: 'Header', kind: 'input', placeholder: 'X-Foo' },
      { name: 'mode', label: '方式', kind: 'select', options: MODES },
      { name: 'value', label: '值', kind: 'input', placeholder: 'bar' },
    ],
    build: (p) => ({ RequestHeader: p.name, [p.mode || 'Set']: p.value }),
    detect: (v) => {
      if (!('RequestHeader' in v)) return null
      const mode = 'Set' in v ? 'Set' : 'Append' in v ? 'Append' : null
      if (!mode || !only(v, ['RequestHeader', mode])) return null
      return { name: v.RequestHeader, mode, value: v[mode] }
    },
  },
  {
    value: 'RequestHeaderRemove',
    label: '移除请求头',
    fields: [{ name: 'name', label: 'Header', kind: 'input', placeholder: 'X-Foo' }],
    build: (p) => ({ RequestHeaderRemove: p.name }),
    detect: (v) => (only(v, ['RequestHeaderRemove']) ? { name: v.RequestHeaderRemove } : null),
  },
  {
    value: 'ResponseHeader',
    label: '设置响应头',
    fields: [
      { name: 'name', label: 'Header', kind: 'input', placeholder: 'X-Foo' },
      { name: 'mode', label: '方式', kind: 'select', options: MODES },
      { name: 'value', label: '值', kind: 'input', placeholder: 'bar' },
      { name: 'when', label: '时机', kind: 'select', options: WHENS },
    ],
    build: (p) => ({ ResponseHeader: p.name, [p.mode || 'Set']: p.value, When: p.when || 'Success' }),
    detect: (v) => {
      if (!('ResponseHeader' in v)) return null
      const mode = 'Set' in v ? 'Set' : 'Append' in v ? 'Append' : null
      if (!mode) return null
      const keys = ['ResponseHeader', mode]
      if ('When' in v) keys.push('When')
      if (!only(v, keys)) return null
      return { name: v.ResponseHeader, mode, value: v[mode], when: v.When || 'Success' }
    },
  },
  {
    value: 'ResponseHeaderRemove',
    label: '移除响应头',
    fields: [{ name: 'name', label: 'Header', kind: 'input', placeholder: 'X-Foo' }],
    build: (p) => ({ ResponseHeaderRemove: p.name }),
    detect: (v) => (only(v, ['ResponseHeaderRemove']) ? { name: v.ResponseHeaderRemove } : null),
  },
  {
    value: 'QueryValueParameter',
    label: '设置查询参数',
    fields: [
      { name: 'name', label: '参数名', kind: 'input', placeholder: 'debug' },
      { name: 'mode', label: '方式', kind: 'select', options: MODES },
      { name: 'value', label: '值', kind: 'input', placeholder: '1' },
    ],
    build: (p) => ({ QueryValueParameter: p.name, [p.mode || 'Set']: p.value }),
    detect: (v) => {
      if (!('QueryValueParameter' in v)) return null
      const mode = 'Set' in v ? 'Set' : 'Append' in v ? 'Append' : null
      if (!mode || !only(v, ['QueryValueParameter', mode])) return null
      return { name: v.QueryValueParameter, mode, value: v[mode] }
    },
  },
  {
    value: 'QueryRemoveParameter',
    label: '移除查询参数',
    fields: [{ name: 'name', label: '参数名', kind: 'input', placeholder: 'debug' }],
    build: (p) => ({ QueryRemoveParameter: p.name }),
    detect: (v) => (only(v, ['QueryRemoveParameter']) ? { name: v.QueryRemoveParameter } : null),
  },
  {
    value: 'custom',
    label: '自定义(JSON)',
    fields: [
      { name: 'json', label: 'value JSON', kind: 'json', placeholder: '{"RequestHeadersCopy":"false"}' },
    ],
    build: (p) => {
      let o: unknown
      try {
        o = JSON.parse(p.json || '')
      } catch {
        throw new Error('不是合法 JSON')
      }
      if (typeof o !== 'object' || o === null || Array.isArray(o)) throw new Error('必须是 JSON 对象')
      return o as Record<string, string>
    },
    detect: () => null, // 兜底类型,不参与识别
  },
]

export function typeDef(type: string): TransformTypeDef {
  return TRANSFORM_TYPES.find((t) => t.value === type) ?? TRANSFORM_TYPES[TRANSFORM_TYPES.length - 1]
}

export type TransformDraft = {
  id: number
  type: string
  /** 存储里的 key 字段(备注);UI 不编辑但往返保留 */
  note: string
  params: Record<string, string>
}

let seq = 1

export function newDraft(type = 'PathRemovePrefix'): TransformDraft {
  return { id: seq++, type, note: '', params: defaultParams(type) }
}

export function defaultParams(type: string): Record<string, string> {
  const p: Record<string, string> = {}
  for (const f of typeDef(type).fields) {
    p[f.name] = f.kind === 'select' && f.options?.length ? f.options[0] : ''
  }
  return p
}

/** 存储 JSON → 草稿列表。整体非法时,把原文塞进单条 custom,交由用户修复。 */
export function parseTransforms(json: string): TransformDraft[] {
  const raw = (json || '').trim()
  if (!raw || raw === '[]') return []
  let arr: unknown
  try {
    arr = JSON.parse(raw)
  } catch {
    return [{ id: seq++, type: 'custom', note: '', params: { json: raw } }]
  }
  if (!Array.isArray(arr)) return [{ id: seq++, type: 'custom', note: '', params: { json: raw } }]

  return arr.map((item) => {
    const it = item as { key?: unknown; value?: unknown }
    const note = typeof it?.key === 'string' ? it.key : ''
    const value = (it?.value ?? item) as Record<string, string>
    if (typeof value === 'object' && value !== null && !Array.isArray(value)) {
      for (const def of TRANSFORM_TYPES) {
        const params = def.detect(value)
        if (params) return { id: seq++, type: def.value, note, params }
      }
    }
    return { id: seq++, type: 'custom', note, params: { json: JSON.stringify(value) } }
  })
}

/** 草稿列表 → 存储 JSON;有非法项抛 Error(带序号的中文信息)。 */
export function serializeTransforms(drafts: TransformDraft[]): string {
  const items = drafts.map((d, i) => {
    const def = typeDef(d.type)
    for (const f of def.fields) {
      if (f.kind !== 'select' && !(d.params[f.name] ?? '').trim()) {
        throw new Error(`第 ${i + 1} 条改写规则:「${f.label}」必填`)
      }
    }
    let value: Record<string, string>
    try {
      value = def.build(d.params)
    } catch (e) {
      throw new Error(`第 ${i + 1} 条改写规则:${e instanceof Error ? e.message : '参数非法'}`)
    }
    return { key: d.note || def.value, value }
  })
  return JSON.stringify(items)
}

/** 尽力而为的预览(编辑半途允许失败)。 */
export function previewTransforms(drafts: TransformDraft[]): string {
  try {
    return JSON.stringify(JSON.parse(serializeTransforms(drafts)), null, 2)
  } catch (e) {
    return `// ${e instanceof Error ? e.message : '存在未完成的规则'}`
  }
}
