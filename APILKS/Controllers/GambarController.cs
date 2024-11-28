using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;

namespace APILKS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GambarController : ControllerBase
    {
        private readonly string _connectionString = "Data Source=HIDAYATULLAH;Initial Catalog=Latihan_API_LKS;Integrated Security=True;";

        [HttpPost]
        public async Task<IActionResult> UploadImage([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            byte[] imageData;
            using (var ms = new MemoryStream())
            {
                await file.CopyToAsync(ms);
                imageData = ms.ToArray();
            }

            using (var connection = new SqlConnection(_connectionString))
            {
                string query = "INSERT INTO tb_gambar (gambar) VALUES (@gambar)";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@gambar", imageData);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }

            return Ok("Image uploaded successfully.");
        }

        [HttpGet]
        public IActionResult GetAllImages()
        {
            var images = new List<byte[]>();

            using (var connection = new SqlConnection(_connectionString))
            {
                string query = "SELECT gambar FROM tb_gambar";
                using (var command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        images.Add((byte[])reader["gambar"]);
                    }
                }
            }

            var imageList = new List<string>();
            foreach (var image in images)
            {
                var base64Image = Convert.ToBase64String(image);
                var imageUrl = $"data:image/jpeg;base64,{base64Image}";
                imageList.Add(imageUrl);
            }

            return Ok(imageList);
        }
    }
}
