import { ViewModelBase } from 'bizify'
import { toast } from 'sonner'
import { api, ApiError } from '@/lib/api'
import { queryClient } from '@/lib/query-client'
import type { RouteDto, RouteUpsert } from '@/lib/types'

export const HTTP_METHODS = ['GET', 'POST', 'PUT', 'DELETE', 'PATCH', 'HEAD', 'OPTIONS'] as const

const emptyForm = (): RouteUpsert => ({
  routeName: '',
  clusterCode: '',
  matchPath: '',
  matchMethods: ['GET'],
  transforms: '[]',
  remark: '',
})

type RoutesState = {
  // 列表态(进 useQuery 的 key)
  page: number
  size: number
  keyword: string
  keywordInput: string
  // 编辑弹窗
  dialogOpen: boolean
  editingId: number | null
  form: RouteUpsert
  saving: boolean
  formError: string
  // 删除确认
  deleteTarget: RouteDto | null
  deleting: boolean
}

/**
 * Routes 页 ViewModel:持有 UI 态 + 写操作编排;服务端数据由 View 侧 useQuery 订阅,
 * VM 写完通过 queryClient.invalidateQueries 驱动刷新(混用形态的试点)。
 */
export class RoutesVM extends ViewModelBase<RoutesState> {
  protected $data(): RoutesState {
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
    }
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

  // ---- 编辑弹窗 ----
  openCreate() {
    this.data.form = emptyForm()
    this.data.editingId = null
    this.data.formError = ''
    this.data.dialogOpen = true
  }

  openEdit(r: RouteDto) {
    this.data.form = {
      routeName: r.routeName,
      clusterCode: r.clusterCode,
      matchPath: r.matchPath,
      matchMethods: [...r.matchMethods],
      transforms: r.transforms,
      remark: r.remark,
    }
    this.data.editingId = r.id
    this.data.formError = ''
    this.data.dialogOpen = true
  }

  closeDialog() {
    this.data.dialogOpen = false
  }

  setField<K extends keyof RouteUpsert>(key: K, value: RouteUpsert[K]) {
    this.data.form[key] = value
  }

  toggleMethod(m: string) {
    const arr = this.data.form.matchMethods
    const i = arr.indexOf(m)
    if (i >= 0) arr.splice(i, 1)
    else arr.push(m)
  }

  async save() {
    const { form, editingId } = this.data
    // 客户端先校验(后端仍兜底)
    if (!form.clusterCode) return void (this.data.formError = '请选择集群')
    if (!form.matchPath) return void (this.data.formError = '请填写匹配路径')
    if (form.matchMethods.length === 0) return void (this.data.formError = '至少选择一个 Method')
    try {
      const parsed = JSON.parse(form.transforms || '[]')
      if (!Array.isArray(parsed)) throw new Error()
    } catch {
      return void (this.data.formError = 'transforms 必须是 JSON 数组')
    }

    this.data.saving = true
    this.data.formError = ''
    try {
      if (editingId == null) await api.post('/routes', form)
      else await api.put(`/routes/${editingId}`, form)

      // VM 驱动 query 刷新
      queryClient.invalidateQueries({ queryKey: ['routes'] })
      queryClient.invalidateQueries({ queryKey: ['config', 'draft-status'] })

      if (this.$disposed) return
      this.data.dialogOpen = false
      toast.success(editingId == null ? '路由已创建' : '路由已保存')
    } catch (err) {
      if (this.$disposed) return
      this.data.formError = err instanceof ApiError ? err.message : '保存失败'
    } finally {
      if (!this.$disposed) this.data.saving = false
    }
  }

  // ---- 删除 ----
  askDelete(r: RouteDto) {
    this.data.deleteTarget = r
  }

  cancelDelete() {
    this.data.deleteTarget = null
  }

  async confirmDelete() {
    const target = this.data.deleteTarget
    if (!target) return
    this.data.deleting = true
    try {
      await api.del(`/routes/${target.id}`)
      queryClient.invalidateQueries({ queryKey: ['routes'] })
      queryClient.invalidateQueries({ queryKey: ['config', 'draft-status'] })
      toast.success(`已删除路由「${target.routeName || target.matchPath}」`)
    } catch (err) {
      toast.error(err instanceof ApiError ? err.message : '删除失败')
    } finally {
      if (!this.$disposed) {
        this.data.deleting = false
        this.data.deleteTarget = null
      }
    }
  }
}
