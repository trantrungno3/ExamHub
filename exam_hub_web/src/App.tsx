import { App as AntdApp, ConfigProvider } from 'antd'
import { AppRouter } from './routes'
import { AuthProvider } from './AuthProvider'
import { BRAND } from './constants/theme'

const theme = {
  token: {
    colorPrimary: BRAND.primary,
    colorSuccess: BRAND.success,
    colorError: BRAND.danger,
    colorWarning: BRAND.warning,
    colorLink: BRAND.primary,
    colorTextHeading: BRAND.ink,
    fontFamily: "'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif",
    borderRadius: 8,
  },
  components: {
    // #f0f1f4 không có trong @theme — giữ nguyên hex tại chỗ.
    Table: { headerBg: BRAND.surface, headerColor: BRAND.muted, borderColor: '#f0f1f4' },
    Button: { controlHeight: 38 },
  },
}

export default function App() {
  return (
    <ConfigProvider theme={theme}>
      <AntdApp>
        <AuthProvider>
          <AppRouter />
        </AuthProvider>
      </AntdApp>
    </ConfigProvider>
  )
}
