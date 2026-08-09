import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'

export default defineConfig({
  plugins: [react(), tailwindcss()],
  css: {
    preprocessorOptions: {
      // `@import "tailwindcss"` là directive của Tailwind, không phải Sass import —
      // tắt cảnh báo deprecation `@import` của Dart Sass cho riêng nó.
      scss: {
        silenceDeprecations: ['import'],
      },
    },
  },
})
