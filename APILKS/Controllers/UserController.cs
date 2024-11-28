using APILKS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using Dapper;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace APILKS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        public UserController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<IActionResult> GetUser()
        {
            var query = @"SELECT 
                    RTRIM(LTRIM(id)) AS id,
                    RTRIM(LTRIM(nama)) AS nama,
                    RTRIM(LTRIM(username)) AS username,
                    RTRIM(LTRIM(tanggal_register)) AS tanggal_register
                  FROM tb_user";

            using (var connection = new SqlConnection(_configuration.GetConnectionString("UserAppCon")))
            {
                var userList = await connection.QueryAsync<User>(query);

                return Ok(new { message = "Data berhasil di tampilkan", Data = userList });
            }
        }


        [HttpGet("search/{keyword}")]
        public async Task<IActionResult> UserSiswa(string keyword)
        {
            var query = @"SELECT 
                    RTRIM(LTRIM(id)) AS id,
                    RTRIM(LTRIM(nama)) AS nama,
                    RTRIM(LTRIM(username)) AS username,
                    'Password123' AS password,
                    'example@example.com' AS email,
                    '0812345678' AS nomor_telepon,
                    RTRIM(LTRIM(tanggal_register)) AS tanggal_register
                  FROM tb_user
                  WHERE 
                    id LIKE @keyword OR 
                    nama LIKE @keyword OR 
                    username LIKE @keyword OR
                    tanggal_register LIKE @keyword";

            using (var connection = new SqlConnection(_configuration.GetConnectionString("UserAppCon")))
            {
                var list = await connection.QueryAsync<User>(query, new { keyword = $"%{keyword}%" });
                return Ok(new { Data = list });
            }
        }
    }
}
