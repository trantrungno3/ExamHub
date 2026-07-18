using ExamHub.API.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExamHub.API.Controllers;

/// <summary>Controller cơ sở cung cấp thông tin người dùng hiện tại (parse từ JWT claim) cho các controller kế thừa</summary>
[Authorize]
[ApiController]
public class AuthorizeControllerBase : ControllerBase
{
    /// <summary>Thông tin người dùng đang đăng nhập, được parse lazy từ JWT claim khi lần đầu truy cập</summary>
    protected CurrentUserInfo CurrentUser =>  new (User);
}