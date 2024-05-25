using ImgOCR.Models;
using ImgOCR.Service;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace ImgOCR.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OcrController : ControllerBase
    {
        private readonly OcrService _ocrService;
        private readonly IConfiguration _configuration;
        private readonly string? _secretKey;
        public OcrController(OcrService ocrService, IConfiguration configuration)
        {
            _ocrService = ocrService;
            _configuration = configuration;
            _secretKey = _configuration["Jwt:Key"];
            if (_secretKey == null)
            {
                throw new ArgumentNullException(nameof(_secretKey), "JWT secret key is not configured.");
            }
        }

        [HttpPost("IMGtoOCR")]
        public IActionResult ExtractTextFromImage([FromBody] OcrRequest request)
        {
            var headers = Request.Headers;
            if (!headers.ContainsKey("txn_id") || !headers.ContainsKey("partner_id") || !headers.ContainsKey("time_stamp") || !headers.ContainsKey("token"))
            {
                return BadRequest(new OcrResponse
                {
                    ResponseCode = 0,
                    Response = new ResponseData { Data = "" },
                    ErrorMsg = "Missing headers"
                });
            }

            var txnId = headers["txn_id"].ToString();
            var partnerId = headers["partner_id"].ToString();
            var timeStamp = headers["time_stamp"].ToString();
            var token = headers["token"].ToString();
            var secretKey = _secretKey;

            if (!ValidateToken(token, secretKey, out var validatedPartnerId, out var validatedTxnId, out var validatedTimeStamp))
            {
                return Unauthorized(new OcrResponse
                {
                    ResponseCode = 0,
                    Response = new ResponseData { Data = "" },
                    ErrorMsg = "Invalid token"
                });
            }

            if (validatedPartnerId != partnerId || validatedTxnId != txnId || validatedTimeStamp != timeStamp)
            {
                return Unauthorized(new OcrResponse
                {
                    ResponseCode = 0,
                    Response = new ResponseData { Data = "" },
                    ErrorMsg = "Token data mismatch"
                });
            }


            if (string.IsNullOrEmpty(request.ImageBase64))
            {
                return BadRequest(new OcrResponse
                {
                    ResponseCode = 0,
                    Response = new ResponseData { Data = "" },
                    ErrorMsg = "Invalid image data"
                });
            }

            try
            {
                var imageBytes = Convert.FromBase64String(request.ImageBase64);
                var extractedText = _ocrService.ExtractTextFromImage(imageBytes);
                var extractedTextBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(extractedText));

                return Ok(new OcrResponse
                {
                    ResponseCode = 1,
                    Response = new ResponseData { Data = extractedTextBase64 },
                    ErrorMsg = ""
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new OcrResponse
                {
                    ResponseCode = 0,
                    Response = new ResponseData { Data = "" },
                    ErrorMsg = ex.Message
                });
            }
        }


        private bool ValidateToken(string token, string secretKey, out string partnerId, out string txnId, out string timeStamp)
        {
            partnerId = null;
            txnId = null;
            timeStamp = null;

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(secretKey);

            try
            {
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _configuration["Jwt:Audience"],
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateLifetime = true
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
                var jwtToken = (JwtSecurityToken)validatedToken;

                partnerId = jwtToken.Claims.First(x => x.Type == "partner_id").Value;
                txnId = jwtToken.Claims.First(x => x.Type == "txn_id").Value;
                timeStamp = jwtToken.Claims.First(x => x.Type == "time_stamp").Value;

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
