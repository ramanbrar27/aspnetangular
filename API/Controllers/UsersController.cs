using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace API.Controllers
{
   [Authorize]
    public class UsersController : BaseApiController
    {
        //private readonly DataContext _context;
        private readonly IUserRepository _iuserrepository;
        private readonly IMapper _imapper;

        // public UsersController(DataContext context)
        public UsersController(IUserRepository iuserrepository,IMapper  imapper )
        {
            _iuserrepository = iuserrepository;
            _imapper = imapper;
            // _context = context;

        }
        // [HttpGet]
        // public ActionResult<IEnumerable<AppUser>> GetUsers()
        // {
        //     return _context.Users.ToList();
        // }

         [HttpGet]
         //[AllowAnonymous]
        // public async Task<ActionResult<IEnumerable<AppUser>>> GetUsers()
         public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers()
        {
          //  return await _context.Users.ToListAsync();
          
        //   var users=await _iuserrepository.GetUsersAsync();
        //   var usersToReturn= _imapper.Map<IEnumerable<MemberDto>>(users);
        //   return Ok(usersToReturn);
            var users=await _iuserrepository.GetMembersAsync();
            return Ok(users);

        }
       
        // [HttpGet("{id}")]
        // //[Authorize]
        // public async Task<ActionResult<AppUser>> GetUser(int id)
        // {
        //    // return await _context.Users.FindAsync(id);
        //    return await _iuserrepository.GetUserByIdAsync(id);
        // }

        [HttpGet("{username}")]
        // public async Task<ActionResult<AppUser>> GetUser(string username)
        public async Task<ActionResult<MemberDto>> GetUser(string username)
        {
            // var user=await _iuserrepository.GetUserByUsernameAsync(username);
            // return _imapper.Map<MemberDto>(user);
            return await _iuserrepository.GetMemberByUsernameAsync(username);
            
        }
        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto){
            var username=User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user=await _iuserrepository.GetUserByUsernameAsync(username);

            _imapper.Map(memberUpdateDto,user);
            _iuserrepository.Update(user);

            if(await _iuserrepository.SaveAllAsync()) return NoContent();

            return BadRequest("Failed to update user");
        }

    }
}