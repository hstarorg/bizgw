import { ViewModelBase } from 'bizify'
import { toast } from 'sonner'
import { api, ApiError } from '@/lib/api'
import { queryClient } from '@/lib/query-client'
import type { ClusterDto, ClusterUpsert, DestinationDto, DestinationUpsert } from '@/lib/types'

const emptyForm = (): ClusterUpsert => ({
  clusterCode: '',
  clusterName: '',
  loadBalancingPolicy: 'RoundRobin',
  enabledHealthCheck: false,
  healthCheckInterval: 15,
  healthCheckTimeout: 5,
  healthCheckPolicy: '',
  healthCheckPath: '',
  remark: '',
})

const emptyDestForm = (): DestinationUpsert => ({ address: '', healthCheckPath: '', name: '' })

type ClustersState = {
  page: number
  size: number
  keyword: string
  keywordInput: string
  // 集群编辑弹窗
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
  destForm: DestinationUpsert
  destEditingId: number | null
  destSaving: boolean
  destError: string
}

export class ClustersVM extends ViewModelBase<ClustersState> {
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

  // ---- 集群编辑 ----
  openCreate() {
    this.data.form = emptyForm()
    this.data.editingId = null
    this.data.formError = ''
    this.data.dialogOpen = true
  }

  openEdit(c: ClusterDto) {
    this.data.form = {
      clusterCode: c.clusterCode,
      clusterName: c.clusterName,
      loadBalancingPolicy: c.loadBalancingPolicy,
      enabledHealthCheck: c.enabledHealthCheck,
      healthCheckInterval: c.healthCheckInterval,
      healthCheckTimeout: c.healthCheckTimeout,
      healthCheckPolicy: c.healthCheckPolicy,
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
  }

  async save() {
    const { form, editingId } = this.data
    if (editingId == null && !form.clusterCode.trim())
      return void (this.data.formError = '请填写 cluster_code(创建后不可改)')

    this.data.saving = true
    this.data.formError = ''
    try {
      if (editingId == null) await api.post('/clusters', form)
      else await api.put(`/clusters/${editingId}`, form)
      this.invalidate()
      if (this.$disposed) return
      this.data.dialogOpen = false
      toast.success(editingId == null ? '集群已创建' : '集群已保存')
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
      toast.success(`已删除集群「${target.clusterCode}」`)
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
  openDests(clusterCode: string) {
    this.data.destCluster = clusterCode
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
