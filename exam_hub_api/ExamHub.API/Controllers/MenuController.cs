using ExamHub.API.Controllers.Menu;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TVT.Core;

namespace ExamHub.API.Controllers;

/// <summary>Trả về danh sách menu điều hướng đã được lọc theo quyền của người dùng đang đăng nhập</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MenuController : AuthorizeControllerBase
{
    /// <summary>Lấy menu theo phân quyền của người dùng hiện tại</summary>
    [HttpGet]
    public ActionResult<RequestResponse<IReadOnlyList<MenuItemResponse>>> GetMenu()
    {
        var roles = CurrentUser.Roles ?? [];
        var items = MenuRegistry.GetForRoles(roles);
        return Ok(RequestResponse<IReadOnlyList<MenuItemResponse>>.Success(
            "Lấy menu thành công!", items, items.Count));
    }
}
