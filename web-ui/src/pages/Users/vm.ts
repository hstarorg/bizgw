import { ViewModelBase } from 'bizify'
import { toast } from 'sonner'
import { api, ApiError } from '@/lib/api'
import { queryClient } from '@/lib/query-client'
import type { UserDto } from '@/lib/types'

type UsersState = {
  // 新建
  createOpen: boolean
  form: { username: string; password: string; role: string }
  saving: boolean
  formError: string
  // 重置密码
  resetTarget: UserDto | null
  resetPassword: string
  resetting: boolean
  resetError: string
  // 删除
  deleteTarget: UserDto | null
  deleting: boolean
}

export class UsersVM extends ViewModelBase<UsersState> {
  protected $data(): UsersState {
    return {
      createOpen: false,
      form: { username: '', password: '', role: 'Viewer' },
      saving: false,
      formError: '',
      resetTarget: null,
      resetPassword: '',
      resetting: false,
      resetError: '',
      deleteTarget: null,
      deleting: false,
    }
  }

  private invalidate() {
    queryClient.invalidateQueries({ queryKey: ['users'] })
  }

  // ---- 新建 ----
  openCreate() {
    this.data.form = { username: '', password: '', role: 'Viewer' }
    this.data.formError = ''
    this.data.createOpen = true
  }
  closeCreate() {
    this.data.createOpen = false
  }
  setField<K extends keyof UsersState['form']>(key: K, value: string) {
    this.data.form[key] = value
  }

  async create() {
    const { username, password, role } = this.data.form
    if (username.trim().length < 3) return void (this.data.formError = '用户名至少 3 个字符')
    if (password.length < 6) return void (this.data.formError = '密码至少 6 位')

    this.data.saving = true
    this.data.formError = ''
    try {
      await api.post('/users', { username: username.trim(), password, role })
      this.invalidate()
      if (this.$disposed) return
      this.data.createOpen = false
      toast.success(`用户「${username.trim()}」已创建`)
    } catch (err) {
      if (this.$disposed) return
      this.data.formError = err instanceof ApiError ? err.message : '创建失败'
    } finally {
      if (!this.$disposed) this.data.saving = false
    }
  }

  // ---- 改角色(行内 Select,立即生效) ----
  async changeRole(u: UserDto, role: string) {
    try {
      await api.put(`/users/${u.id}`, { role })
      this.invalidate()
      toast.success(`「${u.username}」角色已改为 ${role}`)
    } catch (err) {
      toast.error(err instanceof ApiError ? err.message : '修改失败')
      this.invalidate() // 失败也重拉,恢复行内选择的显示
    }
  }

  // ---- 重置密码 ----
  askReset(u: UserDto) {
    this.data.resetTarget = u
    this.data.resetPassword = ''
    this.data.resetError = ''
  }
  cancelReset() {
    this.data.resetTarget = null
  }
  setResetPassword(v: string) {
    this.data.resetPassword = v
  }

  async confirmReset() {
    const target = this.data.resetTarget
    if (!target) return
    if (this.data.resetPassword.length < 6) return void (this.data.resetError = '密码至少 6 位')
    this.data.resetting = true
    this.data.resetError = ''
    try {
      await api.post(`/users/${target.id}/reset-password`, { password: this.data.resetPassword })
      if (this.$disposed) return
      this.data.resetTarget = null
      toast.success(`「${target.username}」密码已重置`)
    } catch (err) {
      if (this.$disposed) return
      this.data.resetError = err instanceof ApiError ? err.message : '重置失败'
    } finally {
      if (!this.$disposed) this.data.resetting = false
    }
  }

  // ---- 删除 ----
  askDelete(u: UserDto) {
    this.data.deleteTarget = u
  }
  cancelDelete() {
    this.data.deleteTarget = null
  }

  async confirmDelete() {
    const target = this.data.deleteTarget
    if (!target) return
    this.data.deleting = true
    try {
      await api.del(`/users/${target.id}`)
      this.invalidate()
      toast.success(`用户「${target.username}」已删除`)
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
