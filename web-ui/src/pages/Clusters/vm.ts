import { ViewModelBase } from 'bizify'
import { toast } from 'sonner'
import { api, ApiError } from '@/lib/api'
import { queryClient } from '@/lib/query-client'
import type { ClusterDto, ClusterUpsert, DestinationDto, DestinationUpsert } from '@/lib/types'

export type PolicyOption = { value: string; label: string; desc: string }

/** YARP 内置负载均衡策略(value = Yarp.ReverseProxy.LoadBalancing.LoadBalancingPolicies 常量)。 */
export const LOAD_BALANCING_POLICIES: PolicyOption[] = [
  {
    value: 'PowerOfTwoChoices',
    label: '二选一(YARP 默认)',
    desc: '随机抽两个目标,转发给并发请求更少的那个;均匀且开销低',
  },
  { value: 'RoundRobin', label: '轮询', desc: '按顺序依次分配到各目标' },
  {
    value: 'LeastRequests',
    label: '最少请求',
    desc: '总是转发给当前并发请求最少的目标;最均匀,遍历所有目标开销稍高',
  },
  { value: 'Random', label: '随机', desc: '每次随机挑选一个目标' },
  { value: 'First', label: '固定首个', desc: '总是使用列表中第一个目标;适合主备(failover 由健康检查摘除)' },
]

/** YARP 内置主动健康检查策略。 */
export const HEALTH_CHECK_POLICIES: PolicyOption[] = [
  {
    value: 'ConsecutiveFailures',
    label: '连续失败判定',
    desc: '主动探测连续失败达到阈值后,把目标标记为不健康并摘除;探测恢复后重新纳入',
  },
]

/** value → 中文 label(找不到时原样返回,兼容旧/自定义值)。 */
export function policyLabel(options: PolicyOption[], value: string): string {
  return options.find((o) => o.value === value)?.label ?? value
}

/** 名称 → code 自动派生:ascii 小写 kebab(非 ascii 字符丢弃,纯中文名得空串,由调用方兜底)。 */
const slugify = (name: string): string =>
  name
    .normalize('NFKD')
    .toLowerCase()
    .replace(/[^a-z0-9]+/g, '-')
    .replace(/^-+|-+$/g, '')
    .slice(0, 40)

const emptyForm = (): ClusterUpsert => ({
  clusterCode: '',
  clusterName: '',
  loadBalancingPolicy: 'RoundRobin',
  enabledHealthCheck: false,
  healthCheckInterval: 15,
  healthCheckTimeout: 5,
  healthCheckPolicy: 'ConsecutiveFailures',
  healthCheckPath: '',
  remark: '',
})

const emptyDestForm = (): DestinationUpsert => ({ address: '', healthCheckPath: '', name: '' })

/** 完整 http(s) URL 校验(目标的专用探测地址映射 YARP DestinationConfig.Health,语义是基地址而非路径)。 */
const isHttpUrl = (s: string): boolean => {
  try {
    const u = new URL(s)
    return u.protocol === 'http:' || u.protocol === 'https:'
  } catch {
    return false
  }
}

type ClustersState = {
  page: number
  size: number
  keyword: string
  keywordInput: string
  // 目标组编辑弹窗
  dialogOpen: boolean
  editingId: number | null
  form: ClusterUpsert
  saving: boolean
  formError: string
  // 删除确认
  deleteTarget: ClusterDto | null
  deleting: boolean
  // 目标管理弹窗
  destCluster: string | null
  destClusterName: string
  destForm: DestinationUpsert
  destEditingId: number | null
  destSaving: boolean
  destError: string
}

export class ClustersVM extends ViewModelBase<ClustersState> {
  /** 创建弹窗内:用户是否手动改过 code(改过则不再随名称联动;清空则恢复联动)。 */
  private codeTouched = false
  /** 纯中文等 slug 不出内容的名称,退化用的随机 code(一次弹窗内保持稳定,避免键入时抖动)。 */
  private fallbackCode = ''

  protected $data(): ClustersState {
    return {
      page: 1,
      size: 20,
      keyword: '',
      keywordInput: '',
      dialogOpen: false,
      editingId: null,
      form: emptyForm(),
      saving: false,
      formError: '',
      deleteTarget: null,
      deleting: false,
      destCluster: null,
      destClusterName: '',
      destForm: emptyDestForm(),
      destEditingId: null,
      destSaving: false,
      destError: '',
    }
  }

  private invalidate() {
    queryClient.invalidateQueries({ queryKey: ['clusters'] })
    queryClient.invalidateQueries({ queryKey: ['config', 'draft-status'] })
  }

  // ---- 列表 ----
  setKeywordInput(v: string) {
    this.data.keywordInput = v
  }
  search() {
    this.data.keyword = this.data.keywordInput.trim()
    this.data.page = 1
  }
  setPage(p: number) {
    this.data.page = p
  }

  // ---- 目标组编辑 ----
  openCreate() {
    this.data.form = emptyForm()
    this.data.editingId = null
    this.data.formError = ''
    this.data.dialogOpen = true
    this.codeTouched = false
    this.fallbackCode = ''
  }

  openEdit(c: ClusterDto) {
    this.data.form = {
      clusterCode: c.clusterCode,
      clusterName: c.clusterName,
      // 旧数据可能为空串:兜底到 YARP 默认,下拉才有可选中的值
      loadBalancingPolicy: c.loadBalancingPolicy || 'PowerOfTwoChoices',
      enabledHealthCheck: c.enabledHealthCheck,
      healthCheckInterval: c.healthCheckInterval,
      healthCheckTimeout: c.healthCheckTimeout,
      healthCheckPolicy: c.healthCheckPolicy || 'ConsecutiveFailures',
      healthCheckPath: c.healthCheckPath,
      remark: c.remark,
    }
    this.data.editingId = c.id
    this.data.formError = ''
    this.data.dialogOpen = true
  }

  closeDialog() {
    this.data.dialogOpen = false
  }

  setField<K extends keyof ClusterUpsert>(key: K, value: ClusterUpsert[K]) {
    this.data.form[key] = value
    // 创建时 code 随名称自动派生;用户手动改过 code 则停止联动(清空恢复)
    if (this.data.editingId != null) return
    if (key === 'clusterCode') {
      this.codeTouched = (value as string) !== ''
    } else if (key === 'clusterName' && !this.codeTouched) {
      const name = (value as string).trim()
      const slug = slugify(name)
      if (slug) this.data.form.clusterCode = slug
      else if (name) {
        if (!this.fallbackCode) this.fallbackCode = `grp-${Math.random().toString(36).slice(2, 6)}`
        this.data.form.clusterCode = this.fallbackCode
      } else this.data.form.clusterCode = ''
    }
  }

  async save() {
    const { form, editingId } = this.data
    if (editingId == null && !form.clusterCode.trim())
      return void (this.data.formError = '组标识 Code 不能为空(输入名称可自动生成)')

    this.data.saving = true
    this.data.formError = ''
    try {
      if (editingId == null) await api.post('/clusters', form)
      else await api.put(`/clusters/${editingId}`, form)
      this.invalidate()
      if (this.$disposed) return
      this.data.dialogOpen = false
      toast.success(editingId == null ? '目标组已创建' : '目标组已保存')
    } catch (err) {
      if (this.$disposed) return
      this.data.formError = err instanceof ApiError ? err.message : '保存失败'
    } finally {
      if (!this.$disposed) this.data.saving = false
    }
  }

  // ---- 删除 ----
  askDelete(c: ClusterDto) {
    this.data.deleteTarget = c
  }
  cancelDelete() {
    this.data.deleteTarget = null
  }

  async confirmDelete() {
    const target = this.data.deleteTarget
    if (!target) return
    this.data.deleting = true
    try {
      await api.del(`/clusters/${target.id}`)
      this.invalidate()
      toast.success(`已删除目标组「${target.clusterName || target.clusterCode}」`)
    } catch (err) {
      toast.error(err instanceof ApiError ? err.message : '删除失败')
    } finally {
      if (!this.$disposed) {
        this.data.deleting = false
        this.data.deleteTarget = null
      }
    }
  }

  // ---- 目标管理 ----
  openDests(c: ClusterDto) {
    this.data.destCluster = c.clusterCode
    this.data.destClusterName = c.clusterName
    this.data.destForm = emptyDestForm()
    this.data.destEditingId = null
    this.data.destError = ''
  }

  closeDests() {
    this.data.destCluster = null
  }

  destSetField<K extends keyof DestinationUpsert>(key: K, value: DestinationUpsert[K]) {
    this.data.destForm[key] = value
  }

  destEdit(d: DestinationDto) {
    this.data.destForm = { address: d.address, healthCheckPath: d.healthCheckPath, name: d.name }
    this.data.destEditingId = d.id
    this.data.destError = ''
  }

  destResetForm() {
    this.data.destForm = emptyDestForm()
    this.data.destEditingId = null
    this.data.destError = ''
  }

  async destSave() {
    const code = this.data.destCluster
    if (!code) return
    if (!this.data.destForm.address.trim()) return void (this.data.destError = '请填写地址')
    const probe = this.data.destForm.healthCheckPath.trim()
    if (probe && !isHttpUrl(probe))
      return void (this.data.destError =
        '专用探测地址必须是完整 URL(如 http://host:9090);留空则直接用转发地址探测')

    this.data.destSaving = true
    this.data.destError = ''
    try {
      if (this.data.destEditingId == null)
        await api.post(`/clusters/${code}/destinations`, this.data.destForm)
      else await api.put(`/clusters/${code}/destinations/${this.data.destEditingId}`, this.data.destForm)
      queryClient.invalidateQueries({ queryKey: ['destinations', code] })
      queryClient.invalidateQueries({ queryKey: ['config', 'draft-status'] })
      if (this.$disposed) return
      this.destResetForm()
    } catch (err) {
      if (this.$disposed) return
      this.data.destError = err instanceof ApiError ? err.message : '保存失败'
    } finally {
      if (!this.$disposed) this.data.destSaving = false
    }
  }

  async destDelete(id: number) {
    const code = this.data.destCluster
    if (!code) return
    try {
      await api.del(`/clusters/${code}/destinations/${id}`)
      queryClient.invalidateQueries({ queryKey: ['destinations', code] })
      queryClient.invalidateQueries({ queryKey: ['config', 'draft-status'] })
    } catch (err) {
      toast.error(err instanceof ApiError ? err.message : '删除失败')
    }
  }
}
