import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'
import path from 'node:path'

// Vite 8 (Rolldown is the native bundler in v8).
export default defineConfig({
  plugins: [react(), tailwindcss()],
  resolve: {
    alias: {
      '@': path.resolve(import.meta.dirname, './src'),
    },
  },
  server: {
    // dev: proxy API to the running ControlPlane (see its launchSettings, 5160)
    proxy: {
      '/api': { target: 'http://localhost:5160', changeOrigin: true },
    },
  },
  build: {
    // built assets land here; the ControlPlane Dockerfile copies dist -> wwwroot
    outDir: 'dist',
  },
})
