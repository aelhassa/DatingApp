using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class MessagesController: BaseApiController
    {
        private readonly IMapper _mapper;
        private readonly IMessageRepository _messageRepository;

        private readonly IUserRepository _userRepository;

        public MessagesController(IUserRepository userRepository,IMapper mapper, IMessageRepository messageRepository)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _messageRepository = messageRepository;
        }

        [HttpPost]
        public async Task<ActionResult<IEnumerable<MessageDto>>> CreateMessage(CreateMessageDto
            createMessageDto)
        {
            var username = User.GetUserName();
            if(username==createMessageDto.RecipientUsername.ToLower()){
                return BadRequest("You cannot send messages to yourself");
            }
            var sender=await _userRepository.GetUserByUserNameAsync(username);
            var recipient=await _userRepository.GetUserByUserNameAsync(createMessageDto.RecipientUsername);

            if(recipient==null) return NotFound();

            var message=new Message(){
                Sender=sender,
                Recipient=recipient,
                SenderUsername=sender.UserName,
                RecipientUsername=recipient.UserName,
                Content=createMessageDto.Content
            };
            _messageRepository.AddMessage(message);

            if(await _messageRepository.SaveAllAsync()) return Ok(_mapper.Map<MessageDto>(message));
            return BadRequest("failed to add message");
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessagesForUser([FromQuery]
            MessageParams messageParams)
        {
            messageParams.Username = User.GetUserName();

            var messages = await _messageRepository.GetMessagesForUser(messageParams);

            Response.AddPaginationHeader(messages.currentPage, messages.PageSize,
                messages.TotalCount, messages.TotalPages);

            return messages;
        }
        [HttpGet("thread/{username}")]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessagesThread(string username
            )
        {
            var currentUsername = User.GetUserName();

            var messages = await _messageRepository.GetMessageThread(currentUsername,username);

            
            return Ok(messages);
        }
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMessage(int id)
        {
            var username = User.GetUserName();

            var message = await _messageRepository.GetMessage(id);

            if (message.Sender.UserName != username && message.Recipient.UserName != username)
                return Unauthorized();

            if (message.Sender.UserName == username) message.SenderDeleted = true;

            if (message.Recipient.UserName == username) message.RecipientDeleted = true;

            if (message.SenderDeleted && message.RecipientDeleted)
                _messageRepository.DeleteMessage(message);

            if (await _messageRepository.SaveAllAsync()) return Ok();

            return BadRequest("Problem deleting the message");
        }
    }
}