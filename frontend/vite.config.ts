import { defineConfig } from 'vite'
import react, { reactCompilerPreset } from '@vitejs/plugin-react'
import babel from '@rolldown/plugin-babel'

// https://vite.dev/config/
export default defineConfig({
  plugins: [
    react(),
    babel({ presets: [reactCompilerPreset()] })
  ],
  server: {
    // En desarrollo, redirige las llamadas /api al host .NET (evita CORS).
    proxy: {
      '/api': {
        target: 'http://localhost:5289',
        changeOrigin: true,
      },
    },
  },
})
