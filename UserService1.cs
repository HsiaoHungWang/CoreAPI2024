using BCrypt.Net;

namespace CoreAPI2024
{
    public class UserService1
    {
        public string HashPassword(string password)
        {
            // 使用 BCrypt 進行加密和加鹽
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            // 驗證密碼
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
    }
}
