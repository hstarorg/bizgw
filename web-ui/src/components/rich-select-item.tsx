import { CheckIcon } from 'lucide-react'
import { Select as SelectPrimitive } from 'radix-ui'
import { cn } from '@/lib/utils'

/**
 * 两行式下拉项(label + 等宽 code + 灰色描述)。
 * 关键:只有 label 放进 ItemText —— Radix 会把 ItemText 镜像到触发器(SelectValue)与原生 option,
 * code/描述放在 ItemText 之外,只在下拉列表中展示。
 */
export function RichSelectItem({
  value,
  label,
  code,
  desc,
  className,
}: {
  value: string
  label: string
  code?: string
  desc?: string
  className?: string
}) {
  return (
    <SelectPrimitive.Item
      value={value}
      textValue={label}
      className={cn(
        'focus:bg-accent focus:text-accent-foreground relative flex w-full cursor-default items-start rounded-sm py-1.5 pr-8 pl-2 text-sm outline-hidden select-none data-[disabled]:pointer-events-none data-[disabled]:opacity-50',
        className,
      )}
    >
      <span className="absolute top-2 right-2 flex size-3.5 items-center justify-center">
        <SelectPrimitive.ItemIndicator>
          <CheckIcon className="size-4" />
        </SelectPrimitive.ItemIndicator>
      </span>
      <div className="flex flex-col items-start gap-0.5">
        <span>
          <SelectPrimitive.ItemText>{label}</SelectPrimitive.ItemText>
          {code && <span className="text-muted-foreground ml-2 font-mono text-xs">{code}</span>}
        </span>
        {desc && <span className="text-muted-foreground max-w-72 text-xs whitespace-normal">{desc}</span>}
      </div>
    </SelectPrimitive.Item>
  )
}
