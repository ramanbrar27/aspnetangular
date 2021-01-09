using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly DataContext _context;
        private readonly ITokenService _itokenservice;

        public AccountController(DataContext context,ITokenService tokenservice)
        {
            _context = context;
            _itokenservice = tokenservice;
        }

        [HttpPost("register")]
        //public async Task<ActionResult<AppUser>> Register(string username,string password){
             public async Task<ActionResult<UserDto>> Register(RegisterDTO registerDTO){
                 if( await UserExists(registerDTO.Username.ToLower())) return BadRequest("UserName is taken");
                using var hmac=new HMACSHA512();
                var user=new AppUser{
                    // UserName=username,
                     UserName=registerDTO.Username,
                    // PasswordHash=hmac.ComputeHash(Encoding.UTF8.GetBytes(password)),
                     PasswordHash=hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDTO.Password)),
                    PasswordSalt=hmac.Key
                };
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                return new UserDto{
                    Username=user.UserName,
                    Token=_itokenservice.CreateToken(user)
                };
        }
        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDTO loginDTO){
            var user=await _context.Users
            .SingleOrDefaultAsync(x=>x.UserName==loginDTO.Username);
            if(user==null) return Unauthorized("Invalid username");
            using var hmac=new HMACSHA512(user.PasswordSalt);
            var computedHash=hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDTO.Password));
            for(int i=0;i<computedHash.Length;i++){
                if(computedHash[i]!=user.PasswordHash[i]) return Unauthorized("Invalid password");
            }
           return new UserDto{
                    Username=user.UserName,
                    Token=_itokenservice.CreateToken(user)
                };
        }
        

        public async Task<bool> UserExists(string username){
          return  await _context.Users.AnyAsync(x=>x.UserName.ToLower()==username);
        }
    }
}