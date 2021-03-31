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
using Microsoft.AspNetCore.Identity;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;

        //private readonly DataContext _context;
        private readonly ITokenService _itokenservice;
        private readonly IMapper _mapper;

        // public AccountController(DataContext context,ITokenService tokenservice,IMapper mapper)
        public AccountController(UserManager<AppUser> userManager,SignInManager<AppUser> signInManager,ITokenService tokenservice,IMapper mapper)
        {
            _mapper = mapper;
            _userManager = userManager;
            _signInManager = signInManager;
            //_context = context;
            _itokenservice = tokenservice;
        }

        [HttpPost("register")]
        //public async Task<ActionResult<AppUser>> Register(string username,string password){
             public async Task<ActionResult<UserDto>> Register(RegisterDTO registerDTO){
                 if( await UserExists(registerDTO.Username.ToLower())) return BadRequest("UserName is taken");

                var user=_mapper.Map<AppUser>(registerDTO);

                // using var hmac=new HMACSHA512();


                // var user=new AppUser{
                //     // UserName=username,
                //      UserName=registerDTO.Username,
                //     // PasswordHash=hmac.ComputeHash(Encoding.UTF8.GetBytes(password)),
                //      PasswordHash=hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDTO.Password)),
                //     PasswordSalt=hmac.Key
                // };

                
                    
                     user.UserName=registerDTO.Username;


                    //  user.PasswordHash=hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDTO.Password));
                    // user.PasswordSalt=hmac.Key;
                
                // _context.Users.Add(user);
                // await _context.SaveChangesAsync();

                var result=await _userManager.CreateAsync(user,registerDTO.Password);
                if(!result.Succeeded) return BadRequest(result.Errors);

                var roleResult=await _userManager.AddToRoleAsync(user,"Member");
                if(!roleResult.Succeeded) return BadRequest(roleResult.Errors);
                
                return new UserDto{
                    Username=user.UserName,
                    Token=await _itokenservice.CreateToken(user),
                    KnownAs=user.KnownAs,
                    Gender=user.Gender
                };
        }
        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDTO loginDTO){
            // var user=await _context.Users
             var user=await _userManager.Users
            .Include(p=>p.Photos)
            .SingleOrDefaultAsync(x=>x.UserName==loginDTO.Username.ToLower());
            if(user==null) return Unauthorized("Invalid username");

            var result= await _signInManager.CheckPasswordSignInAsync(user,loginDTO.Password,false);
            if(!result.Succeeded) return Unauthorized();
            // using var hmac=new HMACSHA512(user.PasswordSalt);
            // var computedHash=hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDTO.Password));
            // for(int i=0;i<computedHash.Length;i++){
            //     if(computedHash[i]!=user.PasswordHash[i]) return Unauthorized("Invalid password");
            // }
           return new UserDto{
                    Username=user.UserName,
                    Token=await _itokenservice.CreateToken(user),
                    PhotoUrl=user.Photos.FirstOrDefault(x=>x.IsMain)?.Url,
                    KnownAs=user.KnownAs,
                    Gender=user.Gender
                };
        }
        

        public async Task<bool> UserExists(string username){
        //   return  await _context.Users.AnyAsync(x=>x.UserName.ToLower()==username);
          return  await _userManager.Users.AnyAsync(x=>x.UserName.ToLower()==username);
        }
    }
}