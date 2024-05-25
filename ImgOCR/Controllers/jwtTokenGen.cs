using ImgOCR.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ImgOCR.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class jwtTokenGen : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly string? _secretKey;
        public jwtTokenGen(IConfiguration configuration)
        {
            _configuration = configuration;
            _secretKey = _configuration["Jwt:Key"];
            if (_secretKey == null)
            {
                throw new ArgumentNullException(nameof(_secretKey), "JWT secret key is not configured.");
            }
        }

        [HttpPost("getToken")]
        public IActionResult GetToken([FromBody] TokenRequest request)
        {
            if (string.IsNullOrEmpty(request.partner_id) || string.IsNullOrEmpty(request.txn_id) || string.IsNullOrEmpty(request.time_stamp))
            {
                return BadRequest(new { message = "Invalid partner_id or txn_id or time_stamp" });
            }

            var token = GenerateJwtToken(request.partner_id, request.txn_id, request.time_stamp, _secretKey);

            return Ok(new { token });
        }

        public string GenerateJwtToken(string partnerId, string txnId, string timeStamp, string secretKey)
        {
            var claims = new[]
             {
                new Claim("partner_id", partnerId),
                new Claim("txn_id", txnId),
                new Claim("time_stamp", timeStamp)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(20),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    internal interface IOcrService
    {
    }
}
