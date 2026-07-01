import { Routes, Route } from 'react-router'

function Home() {
  return (
    <div className="flex min-h-svh flex-col items-center justify-center gap-3">
      <h1 className="text-2xl font-semibold tracking-tight">Bizgw 管理后台</h1>
      <p className="text-muted-foreground text-sm">
        前端脚手架就绪：React + Vite (Rolldown) + TypeScript + Tailwind + shadcn
      </p>
      <span className="bg-primary text-primary-foreground rounded-md px-3 py-1 text-xs">
        scaffold ready
      </span>
    </div>
  )
}

export default function App() {
  return (
    <Routes>
      <Route path="/" element={<Home />} />
    </Routes>
  )
}
