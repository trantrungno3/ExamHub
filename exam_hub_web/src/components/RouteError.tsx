import {Button, Result} from 'antd'
import {useRouteError} from 'react-router-dom'

/** Màn hiển thị khi một route ném lỗi — thay cho màn trắng. */
export default function RouteError() {
    const error = useRouteError()
    console.error(error)

    return (
        <Result
            status="error"
            title="Đã xảy ra lỗi"
            subTitle="Không tải được nội dung trang. Thử tải lại giúp mình nhé."
            extra={<Button type="primary" onClick={() => window.location.reload()}>Tải lại trang</Button>}
        />
    )
}
