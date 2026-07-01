import { cn } from '@/lib/utils'

/**
 * Bizgw 品牌标（PNG）。图形语义:一道网关竖闸把入口流量扇出到多个后端集群 —— 反向代理。
 * 源文件见 web-ui/brand/logo-master.svg;PNG 由它导出到 public/。
 */
export function Logo({ className }: { className?: string }) {
  return (
    <img
      src="/logo.png"
      alt="Bizgw"
      width={512}
      height={512}
      className={cn('size-8 select-none', className)}
      draggable={false}
    />
  )
}
