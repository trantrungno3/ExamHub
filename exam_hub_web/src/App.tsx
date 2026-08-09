import { ConfigProvider } from 'antd'
import { RouterProvider } from 'react-router-dom'
import { router } from './routes'
import { AuthProvider } from './AuthProvider'

const theme = {
  token: {
    colorPrimary: '#3a74f5',
    colorSuccess: '#1ea375',
    colorError: '#e74242',
    colorWarning: '#d98a00',
    colorLink: '#3a74f5',
    colorTextHeading: '#191d27',
    fontFamily: "'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif",
    borderRadius: 8,
  },
  components: {
    Table: { headerBg: '#f5f5f6', headerColor: '#6f7788', borderColor: '#f0f1f4' },
    Button: { controlHeight: 38 },
  },
}

export default function App() {
  return (
    <ConfigProvider theme={theme}>
      <AuthProvider>
        <RouterProvider router={router} />
      </AuthProvider>
    </ConfigProvider>
  )
}
