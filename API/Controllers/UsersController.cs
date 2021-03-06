using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Authorize]
    public class UsersController :BaseApiController
    {
        
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IPhotoService _photoService;

        public UsersController(IUserRepository userRepository,
        IMapper mapper ,IPhotoService photoService  )
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _photoService = photoService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers([FromQuery] UserParams userParams)
        {  
            userParams.CurrentUserName=User.GetUserName();
            var user= await _userRepository.GetUserByUserNameAsync(User.GetUserName());
            if(string.IsNullOrEmpty(userParams.Gender)){
                userParams.Gender=user.Gender=="male" ?"female":"male";
            } 
            var users=await _userRepository.GetMembersAsync(userParams); 
            Response.AddPaginationHeader(users.currentPage,users.PageSize,
            users.TotalCount,users.TotalPages);

            return Ok(users);     
        }
        [HttpGet("{username}",Name ="GetUser")]
        public async Task<MemberDto> GetUser(string username){
            var user= await _userRepository.GetUserByUserNameAsync(username);
            return _mapper.Map<MemberDto>(user);
        }
        
        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto){
            var username=User.GetUserName();
            var user = await _userRepository.GetUserByUserNameAsync(username);
            _mapper.Map(memberUpdateDto,user);
            _userRepository.Update(user);
            if(await _userRepository.SaveAllAsync()) return NoContent();
            return BadRequest("Failed to update user");
        }
        [HttpPost("add-photo")]
        public async Task< ActionResult<PhotoDto>> AddPhoto(IFormFile file){
            var user = await _userRepository.GetUserByUserNameAsync(User.GetUserName());
            
            var result=await _photoService.AddPhotoAsync(file);
            if(result.Error!=null) return BadRequest(result.Error.Message);
            
            var photo=new Photo{
                Url=result.SecureUrl.AbsoluteUri,
                PublicId=result.PublicId
            };

            if(user.Photos.Count==0){
                photo.IsMain=true;
            }
            
            user.Photos.Add(photo);
            if(await _userRepository.SaveAllAsync()){
                //return _mapper.Map<PhotoDto>(photo);
                return CreatedAtRoute("GetUser", new {username=user.UserName}, _mapper.Map<PhotoDto>(photo));

            }
            return BadRequest("Failed to add photo");
        }
        
        [HttpPut("set-main-photo/{photoId}")]
        public async Task<ActionResult> SetMainPhoto(int photoId){
            var username=User.GetUserName();
            var user = await _userRepository.GetUserByUserNameAsync(username);
            var photo=user.Photos.FirstOrDefault(x=>x.Id==photoId);
            if(photo.IsMain){
                return BadRequest("this is already your main photo");
            }
            var currentMain= user.Photos.FirstOrDefault(x=>x.IsMain);
            if(currentMain!=null) currentMain.IsMain=false;
            photo.IsMain=true;

            if(await _userRepository.SaveAllAsync()) return NoContent();
            return BadRequest("Failed to set main photo");
        }
        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            var username=User.GetUserName();
            var user = await _userRepository.GetUserByUserNameAsync(username);
            var photo=user.Photos.FirstOrDefault(x=>x.Id==photoId);
            if(photo.IsMain){
                return BadRequest("You cannot delete your main photo");
            }
            if(photo== null){
                return NotFound();
            }
            if(photo.PublicId!=null){
                var result=await _photoService.DeletePhotoAsync(photo.PublicId);
                if(result.Error!=null) return BadRequest(result.Error.Message); 
            }
            user.Photos.Remove(photo);
            
         
            if(await _userRepository.SaveAllAsync()) return NoContent();
            return BadRequest("Failed to delete photo");
        }
    }
}