import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'

export default defineConfig({
  plugins: [react(), tailwindcss()],
  build: {
    rollupOptions: {
      output: {
        // Đo bằng sourcemap của chunk entry: antd + nội bộ của nó (@rc-component/*, rc-*,
        // @ant-design/icons) chiếm ~2.2 MB / 4.36 MB source. Tách đúng một dependency nặng này
        // ra khỏi entry để code ứng dụng không bị invalidate cache mỗi lần build.
        manualChunks: (id) =>
          /node_modules[\\/](?:\.pnpm[\\/])?.*?(?:antd|@ant-design|@rc-component|rc-[a-z-]+)@?/.test(id)
            ? 'antd'
            : undefined,
      },
    },
  },
})
