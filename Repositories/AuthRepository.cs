using ToDoWithAuth.Data;
using ToDoWithAuth.Models;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using ToDoWithAuth.Models.DTOs;

namespace ToDoWithAuth.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AuthRepository> _logger;
        private readonly IConfiguration _config;

        private User user;
        public string _accessToken { get; set; }
        public Roles role { get; set; }

        public AuthRepository(ILogger<AuthRepository> logger, ApplicationDbContext context, IConfiguration config)
        {
            _context = context;
            _logger = logger;
            _config = config;
        }

        public async Task<Tokens> Login(string username, string password)
        {
            if (!SetUserByUsername(username)) {
                _logger.LogInformation("User not found in the database!");
                return null;
            }
            
            if (user.Password != HashPassword(password)) {
                _logger.LogInformation("Password for the user found is incorrect!");
                return null;
            }
            string tokenId = Guid.NewGuid().ToString();

            var refreshToken = CreateRefreshToken(tokenId);
            var accessToken = GenerateAccessToken(tokenId);
            _logger.LogInformation("{x}", tokenId);
            user.Tokens.Add(refreshToken);
            
            if (user.Tokens.Count > 5) {
                _logger.LogWarning("Repository: More than 5 tokens found, logging out from the oldest session!");
                user.Tokens.Remove(user.Tokens.ElementAt(0));
            }

            await _context.SaveChangesAsync();
            return new Tokens {
                RefreshToken = refreshToken,
                AccessToken = accessToken
            };
        }
        public Tokens RenewAccessToken(string token) {
            if(!validateRefreshToken(token)){
                return null;
            }
            var refreshToken = new JwtSecurityTokenHandler().ReadJwtToken(token);
            var tId = refreshToken.Claims.First(claim => claim.Type == "tId").Value;

            var accessToken = GenerateAccessToken(tId);

            return new Tokens {
                AccessToken = accessToken,
                RefreshToken = token
            };

        }

        public async Task<bool> Logout(string token) {
            var claims = ReadClaims(token);
            var refId = claims.First(claim => claim.Type == "RefId").Value;
            var username = claims.First(claim => claim.Type == ClaimTypes.NameIdentifier).Value;
            if (!SetUserByUsername(username)) return false;
            string refreshToken = null;
            foreach (var t in user.Tokens) {
                string tId = new JwtSecurityTokenHandler().ReadJwtToken(t).Claims.First(claim => claim.Type == "tId").Value;
                if (tId == refId) {
                    refreshToken = t;
                }
            }
            if (refreshToken == null) {
                _logger.LogInformation("Repository: Invalid token received!");
                return false;
            }

            user.Tokens.Remove(token);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Logged out for current token!");
            return true;
        }  
        public async Task<bool> LogoutAll(string token) {
            var claims = ReadClaims(token);
            var refId = claims.First(claim => claim.Type == "RefId").Value;
            var username = claims.First(claim => claim.Type == ClaimTypes.NameIdentifier).Value;

            if (!SetUserByUsername(username)) return false;
            string refreshToken = null;
            foreach (var t in user.Tokens) {
                string tId = new JwtSecurityTokenHandler().ReadJwtToken(t).Claims.First(claim => claim.Type == "tId").Value;
                if (tId == refId) {
                    refreshToken = t;
                }
            }
            if (refreshToken == null) {
                _logger.LogInformation("Repository: Invalid token received!");
                return false;
            }

            user.Tokens = new List<string>();
            await _context.SaveChangesAsync();
            _logger.LogInformation("Logged out with all tokens");
            return true;
        }

        public string GenerateAccessToken(string tokenId) {

            // Making a security key from the jwt password string; Used to create credentials
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:AccessKey"]));
            // Used to sign the token
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            
            // create a list of claims that uniquely identify a user;
            List<Claim> claims = new List<Claim> {
                new Claim("RefId", tokenId),
                new Claim(ClaimTypes.NameIdentifier, user.Username),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim(ClaimTypes.Expiration, DateTime.Now.AddHours(1).ToString())
            };

            var token = new JwtSecurityToken(
                _config["Jwt:Issuer"],
                _config["Jwt:Audience"],
                claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        public string CreateRefreshToken(string tokenId) {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:RefreshKey"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            List<Claim> claims = new() {
                new Claim("tId", tokenId),
                new Claim(ClaimTypes.NameIdentifier, user.Username),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim(ClaimTypes.Expiration, DateTime.Now.AddDays(1).ToString())
            };

            var token = new JwtSecurityToken(
                _config["Jwt:Issuer"],
                _config["Jwt:Audience"],
                claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        
        public List<Claim> ReadClaims(string token) {  
            JwtSecurityToken jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(token);
            List<Claim> claims = jwtToken.Claims.ToList();
            // _logger.LogInformation("{x}", username);
            return claims;
        }
        public string HashPassword(string b64password) {
            string password = Encoding.UTF8.GetString(Convert.FromBase64String(b64password));
            _logger.LogInformation("{x}", password);
            if (isPasswordValid(password)) {
                string saltedPassword = password + _config["Encryption:Salt"];
                var sha256 = SHA256.Create();
                byte[] hashedPassword = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));
                return BitConverter.ToString(hashedPassword).Replace("-", "").ToLower();
            }
            return null;
        }
        public bool isPasswordValid(string password) {
            if(password.Length < 8) {
                _logger.LogInformation("Length less than 8 characters");
                return false;
            }
            if(!password.Any(char.IsUpper)) {
                _logger.LogInformation("Uppercase character not found");
                return false;
            }
            if(!password.Any(char.IsLower)) {
                _logger.LogInformation("Lowercase character not found");
                return false;
            }
            if(!password.Any(char.IsDigit)) {
                _logger.LogInformation("Number in the password not found");
                return false;
            }
            if(!password.Any(ch => !char.IsLetterOrDigit(ch))) {
                _logger.LogInformation("Symbol character not found");
                return false;
            }

            return true;
        }


        public bool validateAccessToken(string token) {
            var claims = ReadClaims(token);
            var refId = claims.First(claim => claim.Type == "RefId").Value;
            var username = claims.First(claim => claim.Type == ClaimTypes.NameIdentifier).Value;
            var expiry = DateTime.Parse(claims.First(claim => claim.Type == ClaimTypes.Expiration).Value);
            
            _logger.LogInformation("{x}", expiry);
            
            if (expiry < DateTime.Now) {
                _logger.LogInformation("Token expired");
                return false;
            }

            if (!SetUserByUsername(username)) {
                return false;
            }
            for(int i = 0; i < user.Tokens.Count; i++) {
                var jwt = new JwtSecurityTokenHandler().ReadJwtToken(user.Tokens[i]);
                var tId = jwt.Claims.First(claim => claim.Type == "tId").Value;
                if (tId == refId) {
                    return true;
                }
            }
            return false;
        }
        public bool validateRefreshToken(string token) {
            var claims = ReadClaims(token);
            var expiry = DateTime.Parse(claims.First(claim => claim.Type == ClaimTypes.Expiration).Value);
            
            _logger.LogInformation("{x}", expiry);
            
            if (expiry < DateTime.Now) {
                _logger.LogInformation("Refresh token expired");
                return false;
            }

            var username = claims.First(claim => claim.Type == ClaimTypes.NameIdentifier).Value;
            if (!SetUserByUsername(username)) {
                return false;
            }

            foreach (var t in user.Tokens) {
                if (t == token) {
                    return true;
                }
            }
            return false;
        }
        
        public bool SetUserByUsername(string username) {
            user = _context.Users.FirstOrDefault(u => u.Username == username);
            return user != null;
        }
        public bool SetRoleByToken(string token) {
            var _role = ReadClaims(token).First(claim => claim.Type == ClaimTypes.Role).Value;
            _logger.LogInformation("{x}", _role);
            Enum.TryParse(_role, out Roles role);
            this.role = role;
            _logger.LogInformation("{x}", role);

            return true;
        }

        public Guid? FindUserId(string username) {
            if (!SetUserByUsername(username)) return null;
            return user.Id;
        }
        public Guid? FindTaskId(string titleId) {
            var task = _context.TaskItems.FirstOrDefault(t => t.TitleId == titleId);
            return task.Id;
        }
    }
}