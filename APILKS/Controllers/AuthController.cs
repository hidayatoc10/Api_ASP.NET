using APILKS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace APILKS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("register")]
        public ActionResult Register(DataDto request)
        {
            if (IsUsernameTaken(request.Username))
            {
                return BadRequest(new { message = "Opss, Username sudah terdaftar coba pakai Username yang lain" });
            }

            if (IsEmailTaken(request.Email))
            {
                return BadRequest(new { message = "Opss, Email sudah terdaftar coba pakai Email yang lain" });
            }

            string encryptedPassword = Encrypt(request.Password);

            using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("SiswaAppCon")))
            {
                conn.Open();
                string query = "INSERT INTO tb_user (nama, username, email, nomor_telepon, password) VALUES (@Nama, @Username, @Email, @NomorTelepon, @Password)";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Nama", request.Nama);
                    cmd.Parameters.AddWithValue("@Username", request.Username);
                    cmd.Parameters.AddWithValue("@Email", request.Email);
                    cmd.Parameters.AddWithValue("@NomorTelepon", request.Phone);
                    cmd.Parameters.AddWithValue("@Password", encryptedPassword);
                    cmd.ExecuteNonQuery();
                }
            }

            return Ok(new { message = "Registrasi Berhasil" });
        }

        private bool IsUsernameTaken(string username)
        {
            using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("SiswaAppCon")))
            {
                conn.Open();
                string query = "SELECT COUNT(*) FROM tb_user WHERE username = @Username";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Username", username);
                    int count = (int)cmd.ExecuteScalar();

                    return count > 0;
                }
            }
        }

        private bool IsEmailTaken(string Email)
        {
            using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("SiswaAppCon")))
            {
                conn.Open();
                string query = "SELECT COUNT(*) FROM tb_user WHERE email = @Email";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Email", Email);
                    int count = (int)cmd.ExecuteScalar();

                    return count > 0;
                }
            }
        }

        private static string Encrypt(string plainText)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Encoding.UTF8.GetBytes("1234567890123456");
                aesAlg.Mode = CipherMode.CBC;

                aesAlg.GenerateIV();
                byte[] iv = aesAlg.IV;

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, iv);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    msEncrypt.Write(iv, 0, iv.Length);

                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                    }
                    return Convert.ToBase64String(msEncrypt.ToArray());
                }
            }
        }

        private static string Decrypt(string encryptedText)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Encoding.UTF8.GetBytes("1234567890123456");
                aesAlg.Mode = CipherMode.CBC;
                byte[] iv = new byte[aesAlg.BlockSize / 8];
                byte[] encryptedBytes = Convert.FromBase64String(encryptedText);
                Array.Copy(encryptedBytes, iv, iv.Length);

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, iv);

                using (MemoryStream msDecrypt = new MemoryStream(encryptedBytes, iv.Length, encryptedBytes.Length - iv.Length))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            return srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
        }

        [HttpPost("login")]
        public ActionResult Login(LoginDto request)
        {
            User user = null;

            using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("SiswaAppCon")))
            {
                conn.Open();
                string query = "SELECT * FROM tb_user WHERE username = @Username";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Username", request.Username);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string storedPassword = reader["password"].ToString();
                            string decryptedPassword = Decrypt(storedPassword);
                            if (decryptedPassword == request.Password)
                            {
                                user = new User
                                {
                                    Nama = reader["nama"].ToString().Trim(),
                                    Username = reader["username"].ToString(),
                                    Email = reader["email"].ToString().Trim(),
                                    Phone = Convert.ToInt64(reader["nomor_telepon"]),
                                    Id = Convert.ToInt64(reader["id"]),
                                };
                            }
                        }
                    }
                }
            }

            if (user == null)
            {
                return BadRequest(new { message = "Login Gagal, Coba Lagi" });
            }

            string token = CreateToken(user);

            return Ok(new { message = "Login Berhasil", Token = token, Email = user.Email, Nama = user.Nama, Phone = user.Phone, Id = user.Id });
        }


        private string CreateToken(User user)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("AppSettings:Token").Value));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds
            );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }

        [Authorize]
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            return Ok(new { message = "Logout Berhasil." });
        }
    }
}
