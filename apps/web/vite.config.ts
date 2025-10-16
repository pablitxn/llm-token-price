import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react-swc'
import path from 'path'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],

  // Path aliases for cleaner imports
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src'),
      '@components': path.resolve(__dirname, './src/components'),
      '@api': path.resolve(__dirname, './src/api'),
      '@store': path.resolve(__dirname, './src/store'),
    },
  },

  // Build optimization settings
  build: {
    target: 'es2020',
    minify: 'esbuild',
  },

  // Development server configuration
  server: {
    port: 5173,
    cors: true,
    // Proxy API requests to backend
    proxy: {
      '/api': {
        target: 'http://localhost:5000',
        changeOrigin: true,
        secure: false,
      },
    },
  },

  // Environment variables
  define: {
    '__APP_VERSION__': JSON.stringify(process.env.npm_package_version),
  },
})
