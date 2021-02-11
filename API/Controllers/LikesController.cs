using System.Collections.Generic;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using API.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class LikesController : BaseApiController
    {
        private readonly IUserRepository _userRepository;
        private readonly ILikeRepository _likeRepository;

        public LikesController(IUserRepository userRepository,ILikeRepository likeRepository)
        {
            _userRepository = userRepository;
            _likeRepository = likeRepository;
        }
        [HttpPost("{username}")]
        public async Task<ActionResult> AddLike(string username){
            var sourceUserId=User.GetUserId();
            var LikedUser= await _userRepository.GetUserByUsernameAsync(username);
            var SourceUser= await _likeRepository.GetUserWithLikes(sourceUserId);

            if(LikedUser==null)return NotFound();
            if(SourceUser.UserName==username)return BadRequest("You cannot like yourself");

            var userlike= await _likeRepository.GetUserLike(sourceUserId,LikedUser.ID);
            if(userlike!=null)return BadRequest("You already like this user");
            userlike=new UserLike{
                SourceUserId=sourceUserId,
                LikedUserId=LikedUser.ID
            };
            SourceUser.LikedUsers.Add(userlike);
            if(await _userRepository.SaveAllAsync()) return Ok();

            return BadRequest("Failed to like user");


        }
        [HttpGet]
        // public async Task<ActionResult<IEnumerable<LikeDTO>>> GetUserlikes(string predicate){
             public async Task<ActionResult<IEnumerable<LikeDTO>>> GetUserlikes([FromQuery]LikesParams likesParams){
                 likesParams.UserId=User.GetUserId();
            // var users=await _likeRepository.GetUserLikes(predicate,User.GetUserId());
             var users=await _likeRepository.GetUserLikes(likesParams);
             Response.AddPaginationHeader(users.CurrentPage,
             users.PageSize,users.TotalCount,users.TotalPages);
            return Ok(users);
        }

    }
}