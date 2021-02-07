using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using AutoMapper;
namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly DataContext _context;
        private readonly ITokenService _itokenservice;
        private readonly IMapper _mapper;

        public AccountController(DataContext context,ITokenService tokenservice,IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
            _itokenservice = tokenservice;
        }

        [HttpPost("register")]
        //public async Task<ActionResult<AppUser>> Register(string username,string password){
             public async Task<ActionResult<UserDto>> Register(RegisterDTO registerDTO){
                 if( await UserExists(registerDTO.Username.ToLower())) return BadRequest("UserName is taken");

                var user=_mapper.Map<AppUser>(registerDTO);

                using var hmac=new HMACSHA512();
                // var user=new AppUser{
                //     // UserName=username,
                //      UserName=registerDTO.Username,
                //     // PasswordHash=hmac.ComputeHash(Encoding.UTF8.GetBytes(password)),
                //      PasswordHash=hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDTO.Password)),
                //     PasswordSalt=hmac.Key
                // };

                
                    
                     user.UserName=registerDTO.Username;
                     user.PasswordHash=hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDTO.Password));
                    user.PasswordSalt=hmac.Key;
                
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                return new UserDto{
                    Username=user.UserName,
                    Token=_itokenservice.CreateToken(user),
                    KnownAs=user.KnownAs
                };
        }
        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDTO loginDTO){
            var user=await _context.Users
            .Include(p=>p.Photos)
            .SingleOrDefaultAsync(x=>x.UserName==loginDTO.Username);
            if(user==null) return Unauthorized("Invalid username");
            using var hmac=new HMACSHA512(user.PasswordSalt);
            var computedHash=hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDTO.Password));
            for(int i=0;i<computedHash.Length;i++){
                if(computedHash[i]!=user.PasswordHash[i]) return Unauthorized("Invalid password");
            }
           return new UserDto{
                    Username=user.UserName,
                    Token=_itokenservice.CreateToken(user),
                    PhotoUrl=user.Photos.FirstOrDefault(x=>x.IsMain)?.Url,
                    KnownAs=user.KnownAs
                };
        }
        

        public async Task<bool> UserExists(string username){
          return  await _context.Users.AnyAsync(x=>x.UserName.ToLower()==username);
        }
    }
}