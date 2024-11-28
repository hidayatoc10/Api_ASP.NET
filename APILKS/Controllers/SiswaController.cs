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
    public class SiswaController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public SiswaController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<IActionResult> GetSiswa()
        {
            var query = @"SELECT 
                    RTRIM(id) AS id,
                    RTRIM(nama) AS nama,
                    RTRIM(kelas) AS kelas,
                    RTRIM(sekolah) AS sekolah,
                    RTRIM(keterangan) AS keterangan
                  FROM tb_siswa";

            using (var connection = new SqlConnection(_configuration.GetConnectionString("SiswaAppCon")))
            {
                var siswaList = await connection.QueryAsync<Siswa>(query);
                return Ok(siswaList);
            }
        }

        [HttpPost]
        public async Task<IActionResult> InsertSiswa([FromBody] Siswa siswa)
        {
            var query = "INSERT INTO tb_siswa (nama, kelas, sekolah, keterangan) VALUES (@nama, @kelas, @sekolah, @keterangan)";

            using (var connection = new SqlConnection(_configuration.GetConnectionString("SiswaAppCon")))
            {
                await connection.ExecuteAsync(query, siswa);
                return Ok(new { message = "Data Berhasil Ditambahkan" });
            }
        }

        [HttpGet("view/{id}")]
        public async Task<IActionResult> ViewSiswa(int id)
        {
            var query = @"SELECT 
                    RTRIM(LTRIM(id)) AS id,
                    RTRIM(LTRIM(nama)) AS nama,
                    RTRIM(LTRIM(kelas)) AS kelas,
                    RTRIM(LTRIM(sekolah)) AS sekolah,
                    RTRIM(LTRIM(keterangan)) AS keterangan
                  FROM tb_siswa WHERE id = @id";

            using (var connection = new SqlConnection(_configuration.GetConnectionString("SiswaAppCon")))
            {
                var siswa = await connection.ExecuteAsync(query, new { id });
                return Ok(siswa);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSiswa(int id)
        {
            var query = "DELETE FROM tb_siswa WHERE id = @id";

            using (var connection = new SqlConnection(_configuration.GetConnectionString("SiswaAppCon")))
            {
                await connection.ExecuteAsync(query, new { id });
                return Ok(new { message = "Data Berhasil Dihapus" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSiswa([FromBody] Siswa siswa, int id)
        {
            var query = "UPDATE tb_siswa SET nama = @nama, kelas = @kelas, sekolah = @sekolah, keterangan = @keterangan WHERE id = @id";
            using (var connection = new SqlConnection(_configuration.GetConnectionString("SiswaAppCon")))
            {
                await connection.ExecuteAsync(query, new { siswa.nama, siswa.kelas, siswa.sekolah, siswa.keterangan, id });
                return Ok(new { message = "Data Berhasil Di Ubah" });
            }
        }

        [HttpGet("search/{keyword}")]
        public async Task<IActionResult> SearchSiswa(string keyword)
        {
            var query = @"SELECT 
                            RTRIM(LTRIM(id)) AS id,
                            RTRIM(LTRIM(nama)) AS nama,
                            RTRIM(LTRIM(kelas)) AS kelas,
                            RTRIM(LTRIM(sekolah)) AS sekolah,
                            RTRIM(LTRIM(keterangan)) AS keterangan
                          FROM tb_siswa
                          WHERE 
                            id LIKE @keyword OR 
                            nama LIKE @keyword OR 
                            kelas LIKE @keyword OR 
                            sekolah LIKE @keyword OR 
                            keterangan LIKE @keyword";

            using (var connection = new SqlConnection(_configuration.GetConnectionString("SiswaAppCon")))
            {
                var list = await connection.QueryAsync<Siswa>(query, new { keyword = $"%{keyword}%" });
                return Ok(list);
            }
        }
    }
}
