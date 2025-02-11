import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,  // Ensure React runs on this port
    proxy: {
      "/api": {
        target: "http://localhost:5000",  // Your .NET backend
        changeOrigin: true,
        secure: false,
      },
    },
  },
});