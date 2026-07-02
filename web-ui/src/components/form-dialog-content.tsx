import type { ComponentProps } from 'react'
import { DialogContent } from '@/components/ui/dialog'

/**
 * 表单弹窗内容:禁止点击遮罩关闭(防误点丢失输入);
 * Esc、右上角 X、显式「取消」按钮仍可关闭。
 */
export function FormDialogContent(props: ComponentProps<typeof DialogContent>) {
  return <DialogContent onInteractOutside={(e) => e.preventDefault()} {...props} />
}
