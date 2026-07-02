# CLAUDE.md — web-ui

## 硬规则:shadcn/ui 组件不可手动修改

`src/components/ui/**` 下是 shadcn/ui 生成(vendored)的组件,**一律视为只读,禁止手动编辑** —— 不改样式、不改结构、不加逻辑、不重排导出。

需要变更时,按下列方式,**绝不直接改 `ui/` 里的文件**:

- **新增 / 更新组件**:只用 CLI —— `pnpm dlx shadcn@latest add <name>`(这是唯一允许写入 `src/components/ui/` 的途径)。
- **定制外观 / 行为**:在**自己的**组件层(`src/components/*`,非 `ui/`)里**包一层或组合**;或通过 `className`(配 `cn()`)、props、CSS 变量扩展。
- **主题 / 配色**:改 `src/index.css` 里的 CSS 变量(theme token),不动组件源码。

原因:保持这些组件可被 CLI 重新生成 / 升级、跨组件一致;手改会在下次 `shadcn add` 或升级时被覆盖或冲突。

例外(不算「手改」):shadcn CLI 自身写入 `ui/`、以及 `add` 时向 `src/index.css` 注入所需 CSS 变量。

## 约定:`src/pages/` 目录结构

- **一页一目录**,目录名 **大驼峰(PascalCase)**,页面组件为其中的 `index.tsx`(default export)。例:`pages/Routes/index.tsx`、`pages/Login/index.tsx`。
- **页面组**(含多个子页面的分组)用 **全小写**目录,页面仍是其下的大驼峰子目录。例:`pages/settings/Profile/index.tsx`、`pages/settings/Members/index.tsx`。
- 引用用 `@/pages/Xxx`(解析到 `index.tsx`)。
