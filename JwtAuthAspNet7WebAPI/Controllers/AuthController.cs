using JwtAuthAspNet7WebAPI.Core.Dtos;
using JwtAuthAspNet7WebAPI.Core.OtherObjects;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace JwtAuthAspNet7WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;

        public AuthController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }


        //route For Seeding my roles to db
        [HttpPost]
        [Route("seed-roles")]
        public async Task<IActionResult> SeedRoles()
        {
            bool isUSERRoleExists = await _roleManager.RoleExistsAsync(StaticUserRoles.USER);
            bool isADMINRoleExists = await _roleManager.RoleExistsAsync(StaticUserRoles.ADMIN);
            bool isOwnerRoleExists = await _roleManager.RoleExistsAsync(StaticUserRoles.OWNER);

            if (isUSERRoleExists && isADMINRoleExists && isOwnerRoleExists)
                return Ok("Roles Seeding is already Done");

            await _roleManager.CreateAsync(new IdentityRole(StaticUserRoles.USER));
            await _roleManager.CreateAsync(new IdentityRole(StaticUserRoles.ADMIN));
            await _roleManager.CreateAsync(new IdentityRole(StaticUserRoles.OWNER));

            return Ok("Role Seeding Done Successfully");
        }


        //route for register
        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            var isExitstsUser = await _userManager.FindByNameAsync(registerDto.UserName);

            if (isExitstsUser != null)
                return BadRequest("UserName already exists");

            IdentityUser newUser = new IdentityUser()
            {
                UserName = registerDto.UserName,
                Email = registerDto.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
            };

            var CreateUserResult = await _userManager.CreateAsync(newUser, registerDto.Password);

            if (!CreateUserResult.Succeeded)
            {
                var errorString = "User creation failed because: ";
                foreach (var error in CreateUserResult.Errors)
                {
                    errorString += " # " + error.Description;
                }
                return BadRequest(errorString);
            }
            await _userManager.AddToRoleAsync(newUser, StaticUserRoles.USER);
            return Ok("User Created Successfully");
        }


        //route for login
        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var User = await _userManager.FindByNameAsync(loginDto.UserName);
            if (User is null)
                return Unauthorized("Invalid Credentials");

            var IsPasswordCorrect = await _userManager.CheckPasswordAsync(User, loginDto.Password);
            if (!IsPasswordCorrect)
                return Unauthorized("Invalid Credentials");

            var userRoles = await _userManager.GetRolesAsync(User);

            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, User.UserName),
                new Claim(ClaimTypes.NameIdentifier, User.Id),
                new Claim("JWTID", Guid.NewGuid().ToString()),
            };
            foreach (var userRole in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, userRole));
            }

            var token = GenerateNewJsonWebToken(authClaims);

            return Ok(token);

        }


        // route ==> make user have a role admin
        [HttpPost]
        [Route("make-admin")]
        public async Task<IActionResult> MakeAdmin([FromBody] UpdatePermissionDto updatePermissionDto)
        {
            var User = await _userManager.FindByNameAsync(updatePermissionDto.UserName);

            if (User is null)
                return Unauthorized("Invalid Credentials");

            await _userManager.AddToRoleAsync(User, StaticUserRoles.ADMIN);

            return Ok("User is now an ADMIN");
        }


        // route ==> make user have a role owner
        [HttpPost]
        [Route("make-owner")]
        public async Task<IActionResult> MakeOwner([FromBody] UpdatePermissionDto updatePermissionDto)
        {
            var User = await _userManager.FindByNameAsync(updatePermissionDto.UserName);

            if (User is null)
                return Unauthorized("Invalid Credentials");

            await _userManager.AddToRoleAsync(User, StaticUserRoles.OWNER);

            return Ok("User is now an OWNER");
        }


        //customize token json
        private string GenerateNewJsonWebToken(List<Claim> authClaims)
        {
            var authSecret = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

            var tokenObject = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                expires: DateTime.Now.AddHours(1),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSecret, SecurityAlgorithms.HmacSha256)
                );

            string token = new JwtSecurityTokenHandler().WriteToken(tokenObject);

            return token;
        }
    }
}
