namespace APILKS.Models
{
    public class User
    {
        public long Id { get; set; }
        public string Nama { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = "Password123";
        public string Email { get; set; } = "example@example.com";
        public long Phone { get; set; } = +6212345678901;
        public string? tanggal_register { get; set; } = string.Empty;
    }
}