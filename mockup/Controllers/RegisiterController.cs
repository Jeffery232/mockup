using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using mockup.Model;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace mockup.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegisiterController : ControllerBase
    {
        private readonly UserDbContext _userDbContext;
        private readonly IConfiguration _configuration;

        public RegisiterController(UserDbContext userDbContext,IConfiguration configuration)
        {
            _userDbContext = userDbContext;
            _configuration=configuration;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register(UserRegister userRegister)
        {
            if (_userDbContext.Users.Any(x => x.Email == userRegister.Email))
            {
                return BadRequest("Email already Exist");
            }
            CreatePasswordHash(userRegister.Password, out byte[] passwordHash, out byte[] passwordSalt);

            var user = new User
            {
                Email = userRegister.Email,
                PasswordSalt = passwordSalt,
                PasswordHash = passwordHash,
                VerificationToken = CreateRandomToken()
            };

            _userDbContext.Users.Add(user); 
            await _userDbContext.SaveChangesAsync();    
            return Ok(user);
        }

        [HttpPost("Login")]

        public async Task<ActionResult<string>> Login([FromBody] UserModel userModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            var user = _userDbContext.Users.FirstOrDefault(u => u.Email == userModel.Email);
            if (user == null)
            {
                return BadRequest("User not found");
            }
            if (!VerifyPasswordHash(userModel.Password, user.PasswordHash, user.PasswordSalt))
            {
                return BadRequest("password incorrect");

            }

                var authClaims = new List<Claim>
           {
               new Claim(ClaimTypes.Email, userModel.Email),
               new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()),
           };
                var authSigninKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_configuration["JWT:JWTSecret"]));
                var token = new JwtSecurityToken(

                    issuer: _configuration["JWT:ValidIssuer"],
                    audience: _configuration["JWT:ValidAudience"],
                    expires: DateTime.Now.AddDays(1),
                    claims: authClaims,
                    signingCredentials: new SigningCredentials(authSigninKey, SecurityAlgorithms.HmacSha512Signature)
                    );

                return new JwtSecurityTokenHandler().WriteToken(token);
            
        
            
        }

        private string CreateToken(User user)
        {
            List<Claim> claims = new List<Claim>
            {
                  new Claim(ClaimTypes.Name, user.Email)
            };
            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_configuration.GetSection("Appsettings:Token").Value));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.Now.AddHours(1),
            signingCredentials: creds);

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

                
             return jwt;

        }

        
          


        

        private string CreateRandomToken()
        {
            return Convert.ToString(RandomNumberGenerator.GetInt32(20));
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        { 
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }
        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computedHash =hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash);


            }
        }



        // private string CreateRandomToken() => Convert.ToHexString(RandomNumberGenerator.GetBytes());


    }
}
