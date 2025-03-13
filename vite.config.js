import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
      '/api': {
        target: 'http://localhost:5000',  // eller vilken port din .NET-server kör på
        changeOrigin: true,
        secure: false,
      }
    }
  }
}) 