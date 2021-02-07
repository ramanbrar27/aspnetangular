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
using Microsoft.AspNetCore.Http;
using API.Extensions;
using API.Helpers;

namespace API.Controllers
{
   [Authorize]
    public class UsersController : BaseApiController
    {
        //private readonly DataContext _context;
        private readonly IUserRepository _iuserrepository;
        private readonly IMapper _imapper;
        private readonly IPhotoService _photoService;

        // public UsersController(DataContext context)
        public UsersController(IUserRepository iuserrepository,IMapper  imapper,
        IPhotoService photoService )
        {
            _iuserrepository = iuserrepository;
            _imapper = imapper;
            _photoService = photoService;
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
         public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers([FromQuery]UserParams userparams)
        {
          //  return await _context.Users.ToListAsync();
          
        //   var users=await _iuserrepository.GetUsersAsync();
        //   var usersToReturn= _imapper.Map<IEnumerable<MemberDto>>(users);
        //   return Ok(usersToReturn);
            var user=await _iuserrepository.GetUserByUsernameAsync(User.GetUsername());

            userparams.CurrentUsername=user.UserName;
            if(string.IsNullOrEmpty(userparams.Gender))
                userparams.Gender=user.Gender=="male"?"female":"male";
            var users=await _iuserrepository.GetMembersAsync(userparams);

            Response.AddPaginationHeader(users.CurrentPage,users.PageSize,users.TotalCount,users.TotalPages);
            return Ok(users);

        }
       
        // [HttpGet("{id}")]
        // //[Authorize]
        // public async Task<ActionResult<AppUser>> GetUser(int id)
        // {
        //    // return await _context.Users.FindAsync(id);
        //    return await _iuserrepository.GetUserByIdAsync(id);
        // }

        [HttpGet("{username}",Name="GetUser")]
        // public async Task<ActionResult<AppUser>> GetUser(string username)
        public async Task<ActionResult<MemberDto>> GetUser(string username)
        {
            // var user=await _iuserrepository.GetUserByUsernameAsync(username);
            // return _imapper.Map<MemberDto>(user);
            return await _iuserrepository.GetMemberByUsernameAsync(username);
            
        }
        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto){
            // var username=User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            // var user=await _iuserrepository.GetUserByUsernameAsync(username);

           // var username=User.GetUsername();
            var user=await _iuserrepository.GetUserByUsernameAsync(User.GetUsername());

            _imapper.Map(memberUpdateDto,user);
            _iuserrepository.Update(user);

            if(await _iuserrepository.SaveAllAsync()) return NoContent();

            return BadRequest("Failed to update user");
        }
        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file){
            var user = await _iuserrepository.GetUserByUsernameAsync(User.GetUsername());
            var result= await _photoService.AddPhotoAsync(file);
            if(result.Error!=null) return BadRequest(result.Error.Message);

            var photo=new Photo{
                Url=result.SecureUrl.AbsoluteUri,
                PublicId=result.PublicId
            };

            if(user.Photos.Count==0){
                photo.IsMain=true;
            }

            user.Photos.Add(photo);
            if(await _iuserrepository.SaveAllAsync()){
                    // return _imapper.Map<PhotoDto>(photo);
                    return CreatedAtRoute("GetUser",new{username=user.UserName}, _imapper.Map<PhotoDto>(photo));
            }
            

            return BadRequest("Problem adding photo");

        }

        [HttpPut("set-main-photo/{photoId}")]
        public async Task<ActionResult> SetMainPhoto(int photoId){
            var user=await _iuserrepository.GetUserByUsernameAsync(User.GetUsername());

            var photo=user.Photos.FirstOrDefault(x=>x.Id==photoId);

            if(photo.IsMain)return BadRequest("This is already your main photo");

            var currentMain=user.Photos.FirstOrDefault(x=>x.IsMain);
            if(currentMain!=null) currentMain.IsMain=false;
            photo.IsMain=true;
            
            if(await _iuserrepository.SaveAllAsync()) return NoContent();
            return BadRequest("Failed to set main photo");
        }
        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> DeletePhoto(int photoId){
            var user=await _iuserrepository.GetUserByUsernameAsync(User.GetUsername());

            var photo=user.Photos.FirstOrDefault(p=>p.Id==photoId);

            if(photo==null) return NotFound();

            if(photo.IsMain) return BadRequest("You cannot delete your main photo");

            if(photo.PublicId!=null){
                var result=await _photoService.DeletePhotoAsync(photo.PublicId);
                if(result.Error!=null) return BadRequest(result.Error.Message);
            }

            user.Photos.Remove(photo);

            if(await _iuserrepository.SaveAllAsync()) return Ok();

            return BadRequest("Failed to delete the photo");
        }
    }
}