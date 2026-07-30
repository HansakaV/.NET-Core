using Microsoft.AspNetCore.Mvc;

namespace StudentManagement.API.util
{
    public static class ControllerExtensions
    {
        public static void SetRefreshTokenCookie(
            this ControllerBase controller,
            string refreshToken,
            DateTime expiresAt,
            bool IsDevelopment
        )
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = !IsDevelopment,
                SameSite = SameSiteMode.Strict,
                Expires = expiresAt,
                Path = "/api/login"
            };
            controller.Response.Cookies.Append("RefreshToken",
            refreshToken , cookieOptions);
        }
    }
}