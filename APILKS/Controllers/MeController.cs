using APILKS.Models;
using Azure.Core;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;

namespace APILKS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MeController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        public MeController(IConfiguration configuration)
        {
            _configuration = configuration;
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
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMe([FromBody] User user, int id)
        {
            if (IsUsernameTaken(user.Username))
            {
                return BadRequest(new { message = "Opss, Username sudah terdaftar coba pakai Username yang lain" });
            }

            if (IsEmailTaken(user.Email))
            {
                return BadRequest(new { message = "Opss, Email sudah terdaftar coba pakai Email yang lain" });
            }
            string encryptedPassword = Encrypt(user.Password);
            var query = "UPDATE tb_user SET nama = @Nama, username = @Username, email = @Email, nomor_telepon = @Phone, password = @Password WHERE id = @id";
            using (var connection = new SqlConnection(_configuration.GetConnectionString("UpdateUser")))
            {
                await connection.ExecuteAsync(query, new { user.Nama, user.Username, user.Email, user.Phone, Password = encryptedPassword, id });
                return Ok(new { message = "Data berhasil di ubah" });
            }
        }
    }
}