import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { Button, Checkbox, Form, Input, Typography } from 'antd'
import {
  LockOutlined,
  MailOutlined,
  PhoneOutlined,
  UserOutlined,
} from '@ant-design/icons'

const { Link } = Typography

const FEATURES = [
  'Miễn phí — không mất phí đăng ký',
  'Quản lý đề thi chuyên nghiệp',
  'Hỗ trợ đầy đủ môn học lớp 1–12',
]

function getStrengthLevel(pw: string): number {
  if (!pw) return 0
  let score = 0
  if (pw.length >= 8) score++
  if (/[A-Z]/.test(pw)) score++
  if (/[0-9]/.test(pw)) score++
  if (/[^A-Za-z0-9]/.test(pw)) score++
  return score
}

const STRENGTH_BARS = [
  'bg-red-400',
  'bg-yellow-400',
  'bg-blue-400',
  'bg-green-500',
]

export default function RegisterPage() {
  const [loading, setLoading] = useState(false)
  const [password, setPassword] = useState('')
  const navigate = useNavigate()

  const strength = getStrengthLevel(password)

  const onFinish = async (values: Record<string, unknown>) => {
    setLoading(true)
    try {
      console.log('Register:', values)
      await new Promise((r) => setTimeout(r, 1000))
      navigate('/login')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="auth-layout">
      {/* ── Left: brand panel ── */}
      <div className="brand-panel">
        <div className="brand-logo">
          <div className="brand-logo-icon">EH</div>
          <span className="brand-logo-name">ExamHub</span>
        </div>

        <h2 className="text-[26px] font-bold text-white leading-tight mb-3">
          Tạo tài khoản mới<br />để bắt đầu sử dụng hệ thống
        </h2>
        <p className="brand-subtitle">
          Dành cho giáo viên và học sinh<br />
          tại các trường phổ thông Việt Nam.
        </p>

        <ul className="brand-features list-none m-0 p-0">
          {FEATURES.map((f) => (
            <li key={f} className="brand-feature-item">
              <span className="brand-feature-dot" />
              <span className="brand-feature-text">{f}</span>
            </li>
          ))}
        </ul>

        <p className="mt-auto pt-16 text-white/60 text-[13px]">
          Đã có tài khoản?{' '}
          <Link href="/login" className="!text-white !font-medium">Đăng nhập →</Link>
        </p>
      </div>

      {/* ── Right: form panel ── */}
      <div className="form-panel">
        <div className="login-card !max-w-[480px]">
          <h1 className="login-title">Tạo tài khoản</h1>
          <p className="login-desc">Điền đầy đủ thông tin để hoàn tất đăng ký.</p>

          <Form layout="vertical" onFinish={onFinish} requiredMark={false} size="large">
            <Form.Item
              label="Họ và tên *"
              name="fullName"
              rules={[{ required: true, message: 'Vui lòng nhập họ và tên' }]}
            >
              <Input
                prefix={<UserOutlined className="text-gray-300" />}
                placeholder="VD: Nguyễn Văn An"
              />
            </Form.Item>

            <Form.Item
              label="Email *"
              name="email"
              rules={[{ required: true, type: 'email', message: 'Email không hợp lệ' }]}
            >
              <Input
                prefix={<MailOutlined className="text-gray-300" />}
                placeholder="VD: nguyenvanan@truong.edu.vn"
              />
            </Form.Item>

            <Form.Item label="Số điện thoại" name="phone">
              <Input
                prefix={<PhoneOutlined className="text-gray-300" />}
                placeholder="VD: 0901234567"
              />
            </Form.Item>

            <Form.Item
              label="Mật khẩu *"
              name="password"
              rules={[{ required: true, min: 8, message: 'Tối thiểu 8 ký tự' }]}
            >
              <Input.Password
                prefix={<LockOutlined className="text-gray-300" />}
                placeholder="Tối thiểu 8 ký tự"
                onChange={(e) => setPassword(e.target.value)}
              />
            </Form.Item>

            {password && (
              <div className="-mt-3 mb-5">
                <p className="text-[11px] text-gray-400 mb-1.5">Độ mạnh mật khẩu</p>
                <div className="flex gap-1">
                  {STRENGTH_BARS.map((color, i) => (
                    <div
                      key={i}
                      className={`h-1.5 flex-1 rounded-full transition-colors ${
                        strength > i ? color : 'bg-gray-200'
                      }`}
                    />
                  ))}
                </div>
              </div>
            )}

            <Form.Item
              label="Xác nhận mật khẩu *"
              name="confirmPassword"
              dependencies={['password']}
              rules={[
                { required: true, message: 'Vui lòng xác nhận mật khẩu' },
                ({ getFieldValue }) => ({
                  validator(_, value) {
                    if (!value || getFieldValue('password') === value)
                      return Promise.resolve()
                    return Promise.reject(new Error('Mật khẩu không khớp'))
                  },
                }),
              ]}
            >
              <Input.Password
                prefix={<LockOutlined className="text-gray-300" />}
                placeholder="Nhập lại mật khẩu"
              />
            </Form.Item>

            <Form.Item
              name="terms"
              valuePropName="checked"
              rules={[
                {
                  validator: (_, v) =>
                    v ? Promise.resolve() : Promise.reject('Vui lòng đồng ý điều khoản'),
                },
              ]}
            >
              <Checkbox>
                <span className="text-[13px] text-gray-600">
                  Tôi đồng ý với{' '}
                  <Link href="#" className="!text-[13px]">Điều khoản sử dụng</Link>
                  {' '}và{' '}
                  <Link href="#" className="!text-[13px]">Chính sách bảo mật</Link>
                </span>
              </Checkbox>
            </Form.Item>

            <Form.Item>
              <Button
                type="primary"
                htmlType="submit"
                loading={loading}
                block
                className="!h-11 !font-semibold !text-[15px]"
              >
                Tạo tài khoản
              </Button>
            </Form.Item>

            <p className="auth-footer">
              Đã có tài khoản?{' '}
              <Link href="/login" className="!font-medium">Đăng nhập tại đây →</Link>
            </p>
          </Form>
        </div>
      </div>
    </div>
  )
}
