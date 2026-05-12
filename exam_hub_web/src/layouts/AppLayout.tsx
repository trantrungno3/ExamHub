import { Outlet, useLocation, useNavigate } from 'react-router-dom'
import {
  AppstoreOutlined,
  UnorderedListOutlined,
  FileTextOutlined,
  ThunderboltOutlined,
  UserOutlined,
  TagsOutlined,
  LogoutOutlined,
} from '@ant-design/icons'

const NAV_ITEMS = [
  { path: '/app/dashboard',  label: 'Tổng quan',   icon: <AppstoreOutlined /> },
  { path: '/app/questions',  label: 'Câu hỏi',     icon: <UnorderedListOutlined /> },
  { path: '/app/exams',      label: 'Mẫu đề thi',  icon: <FileTextOutlined /> },
  { path: '/app/generate',   label: 'Sinh đề thi', icon: <ThunderboltOutlined /> },
  { path: '/app/users',      label: 'Người dùng',  icon: <UserOutlined /> },
  { path: '/app/category',   label: 'Danh mục',    icon: <TagsOutlined /> },
]

export default function AppLayout() {
  const location = useLocation()
  const navigate = useNavigate()

  return (
    <div className="app-layout">
      <aside className="sidebar">
        <div className="sidebar-logo">
          <div className="sidebar-logo-icon">EH</div>
          <span className="sidebar-logo-name">ExamHub</span>
        </div>

        <nav className="sidebar-nav">
          {NAV_ITEMS.map((item) => (
            <button
              key={item.path}
              onClick={() => navigate(item.path)}
              className={`sidebar-nav-item ${
                location.pathname.startsWith(item.path) ? 'sidebar-nav-item--active' : ''
              }`}
            >
              <span className="text-base">{item.icon}</span>
              <span>{item.label}</span>
            </button>
          ))}
        </nav>

        <div className="sidebar-footer">
          <button
            onClick={() => navigate('/login')}
            className="sidebar-nav-item text-red-400 hover:!text-red-300 hover:!bg-red-500/10"
          >
            <LogoutOutlined />
            <span>Đăng xuất</span>
          </button>
        </div>
      </aside>

      <div className="page-canvas">
        <Outlet />
      </div>
    </div>
  )
}
