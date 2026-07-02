import { ViewModelBase } from 'bizify'
import { toast } from 'sonner'
import { api, ApiError } from '@/lib/api'
import { queryClient } from '@/lib/query-client'

type ConfigState = {
  publishing: boolean
  publishErrors: string[]
  rollbackTarget: number | null
  rollingBack: boolean
}

/** 配置发布页 VM:发布 / 回滚编排;数据(版本/草稿态/快照)由 View 侧 useQuery 订阅。 */
export class ConfigVM extends ViewModelBase<ConfigState> {
  protected $data(): ConfigState {
    return { publishing: false, publishErrors: [], rollbackTarget: null, rollingBack: false }
  }

  private invalidateAll() {
    queryClient.invalidateQueries({ queryKey: ['config'] })
    queryClient.invalidateQueries({ queryKey: ['instances'] })
  }

  async publish() {
    this.data.publishing = true
    this.data.publishErrors = []
    try {
      const r = await api.post<{ version: number }>('/config/publish')
      this.invalidateAll()
      toast.success(`已发布版本 v${r.version}`)
    } catch (err) {
      if (this.$disposed) return
      if (err instanceof ApiError) {
        const errors = (err.data as { errors?: string[] } | undefined)?.errors
        this.data.publishErrors = errors?.length ? errors : [err.message]
      } else {
        this.data.publishErrors = ['发布失败']
      }
    } finally {
      if (!this.$disposed) this.data.publishing = false
    }
  }

  askRollback(version: number) {
    this.data.rollbackTarget = version
  }

  cancelRollback() {
    this.data.rollbackTarget = null
  }

  async confirmRollback() {
    const target = this.data.rollbackTarget
    if (target == null) return
    this.data.rollingBack = true
    try {
      const r = await api.post<{ version: number }>(`/config/rollback/${target}`)
      this.invalidateAll()
      toast.success(`已回滚到 v${target} 的内容(新版本 v${r.version})`)
    } catch (err) {
      toast.error(err instanceof ApiError ? err.message : '回滚失败')
    } finally {
      if (!this.$disposed) {
        this.data.rollingBack = false
        this.data.rollbackTarget = null
      }
    }
  }
}
