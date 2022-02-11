using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class LikesController:BaseApiController
    {
        private readonly IUserRepository _userRepository;
        private readonly ILikesRepository _likesRepository;


        public LikesController(IUserRepository userRepository,
        ILikesRepository likesRepository  )
        {
            _userRepository = userRepository;
            this._likesRepository = likesRepository;
        }
        [HttpPost("{username}")]
        public async Task<ActionResult> AddLike(string username)
        { 
            var sourceUserId=User.GetUserId();
            var likedUser=await _userRepository.GetUserByUserNameAsync(username);
            var sourceUser=await _likesRepository.GetUserWithLikesAsync(sourceUserId);

            if(likedUser==null) return NotFound();
            if(sourceUser.UserName==username) return BadRequest("You cannot like yoursel");

            var userLike=await _likesRepository.GetUserLikeAsync(sourceUserId,likedUser.Id);
            if(userLike!=null) return BadRequest("You already like this user");
            userLike=new UserLike(){
                SourceUserId=sourceUserId,
                LikedUserId=likedUser.Id
            };
            sourceUser.LikedUsers.Add(userLike);
            if(await _userRepository.SaveAllAsync()) return Ok();

            return BadRequest("failed to like user");
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LikeDto>>> GetUserLikes([FromQuery]LikesParams likesParams)
        { 
            likesParams.UserId=User.GetUserId();
            var users =await _likesRepository.GetUserLikesAsync(likesParams);
            Response.AddPaginationHeader(users.currentPage,users.PageSize,users.TotalCount,users.TotalPages);
            return Ok(users);
        }
    }
}