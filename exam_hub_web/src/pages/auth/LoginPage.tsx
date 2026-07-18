import {useEffect, useState} from 'react'
import {useNavigate} from 'react-router-dom'
import {Button, Form, Input, message, Typography} from 'antd'
import {LockOutlined, UserOutlined} from '@ant-design/icons'
import {useAuth} from '../../AuthProvider'
import {useAuthStore} from '../../stores/authStore'
import {ROUTES} from '../../routes/paths'

const {Link} = Typography

interface LoginFormValues {
    username: string
    password: string
}

/** Học sinh (chỉ role Student) → trang đề thi; còn lại → dashboard admin/teacher. */
function homePathForRoles(roles?: string[]): string {
    const r = roles ?? []
    if (r.length === 0) return ROUTES.NO_ROLE
    const isStudentOnly = r.includes('Student') && !r.includes('Admin') && !r.includes('Teacher')
    return isStudentOnly ? ROUTES.STUDENT_EXAMS : ROUTES.DASHBOARD
}

const FEATURES = [
    'Ngân hàng câu hỏi theo đa dạng',
    'Sinh đề tự động theo tỉ lệ độ khó',
    'Xuất PDF / Word chỉ một thao tác',
    'Dễ dàng thao tác, giao diện trực quan',
]

export default function LoginPage() {
    const [loading, setLoading] = useState(false)
    const navigate = useNavigate()
    const {login, isAuthenticated, user} = useAuth();
    const [messageApi, contextHolder] = message.useMessage()

    useEffect(() => {
        if (isAuthenticated) navigate(homePathForRoles(user?.roles))
    }, [isAuthenticated, user, navigate]);

    const onFinish = async (values: LoginFormValues) => {
        setLoading(true)
        try {
            const error = await login(values.username, values.password);
            if (error) {
                messageApi.error(error)
            } else {
                navigate(homePathForRoles(useAuthStore.getState().user?.roles))
            }
        } finally {
            setLoading(false)
        }
    }

    return (
        <div className="auth-layout">
            {contextHolder}
            {/* ── Left: brand panel ── */}
            <div className="brand-panel">
                <div className="brand-logo">
                    <div className="brand-logo-icon">EH</div>
                    <span className="brand-logo-name">ExamHub</span>
                </div>

                <p className="brand-subtitle">Hệ thống Quản lý &amp; Tạo sinh Đề thi</p>

                <ul className="brand-features list-none m-0 p-0">
                    {FEATURES.map((f) => (
                        <li key={f} className="brand-feature-item">
                            <span className="brand-feature-dot"/>
                            <span className="brand-feature-text">{f}</span>
                        </li>
                    ))}
                </ul>
            </div>

            {/* ── Right: form panel ── */}
            <div className="form-panel">
                <div className="login-card">
                    <h1 className="login-title">Đăng nhập</h1>
                    <p className="login-desc">Nhập thông tin tài khoản để tiếp tục.</p>

                    <Form<LoginFormValues>
                        layout="vertical"
                        onFinish={onFinish}
                        requiredMark={false}
                        size="large"
                    >
                        <Form.Item
                            label="Tên đăng nhập"
                            name="username"
                            rules={[{required: true, message: 'Vui lòng nhập tên đăng nhập'}]}
                        >
                            <Input
                                prefix={<UserOutlined className="text-gray-300"/>}
                                placeholder="example@school.edu.vn"
                                autoComplete="username"
                            />
                        </Form.Item>

                        <Form.Item
                            label="Mật khẩu"
                            name="password"
                            rules={[{required: true, message: 'Vui lòng nhập mật khẩu'}]}
                        >
                            <Input.Password
                                prefix={<LockOutlined className="text-gray-300"/>}
                                placeholder="••••••••••"
                                autoComplete="current-password"
                            />
                        </Form.Item>

                        <div className="forgot-link-row">
                            <Link href="#" className="text-[13px]!">Quên mật khẩu?</Link>
                        </div>

                        <Form.Item>
                            <Button
                                type="primary"
                                htmlType="submit"
                                loading={loading}
                                block
                                className="h-11! font-semibold! text-[15px]!"
                            >
                                Đăng nhập
                            </Button>
                        </Form.Item>

                        <div className="role-badge">
                            Vai trò:&nbsp;
                            <span className="role-badge-name">Quản trị viên</span>
                            &nbsp;·&nbsp;
                            <span className="role-badge-name">Giáo viên</span>
                            &nbsp;·&nbsp;
                            <span className="role-badge-name">Học sinh</span>
                        </div>

                        <p className="auth-footer">
                            Chưa có tài khoản?&nbsp;
                            <Link href="/register" className="font-medium!">Đăng ký ngay →</Link>
                        </p>
                    </Form>
                </div>
            </div>
        </div>
    )
}
