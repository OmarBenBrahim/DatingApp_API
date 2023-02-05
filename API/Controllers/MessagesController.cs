using API.DTOs;
using API.Entity;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class MessagesController : BaseApiController
    {
        private readonly IMapper mapper;
        private readonly IUnitOfWork uow;

        public MessagesController(IMapper mapper, IUnitOfWork uow)
        {
            this.mapper = mapper;
            this.uow = uow;
        }

        [HttpPost]
        public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto)
        {
            var username = User.GetUsername();
            if (username == createMessageDto.RecipientUsername.ToLower())
                return BadRequest("You cannot send Messages to yourself");

            var sender = await uow.UserRepository.GetUserByUsernameAsync(username);
            var recipient = await uow.UserRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername);

            if (recipient == null) return NotFound();

            var message = new Message
            {
                Sender = sender,
                Recipient = recipient,
                SenderUsername = sender.UserName,
                RecipientUsername = recipient.UserName,
                Content = createMessageDto.Content
            };

            uow.MessageRepository.AddMessage(message);
            if (await uow.Complete()) return Ok(mapper.Map<MessageDto>(message));
            return BadRequest("Faild To Send Messge");
        }

        [HttpGet]
        public async Task<ActionResult<PageList<MessageDto>>> GetMessagesForUser([FromQuery] MessageParams messageParams)
        {
            messageParams.Username = User.GetUsername();
            var messages = await uow.MessageRepository.GetMessageForUser(messageParams);

            Response.AddPaginationHeader(new PaginationHeader(messages.CurrentPage, messages.PageSize, messages.TotalCount, messages.TotalPages));
            return messages;

        }
        /*
        [HttpGet("thread/{username}")]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessagesThread(string username)
        {
            var currentUserName = User.GetUsername();
            return Ok(await uow.MessageRepository.GetMessageThread(currentUserName, username));
        }
        */
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMessage(int id)
        {
            var username = User.GetUsername();
            var message = await uow.MessageRepository.GetMessage(id);
            if(message.SenderUsername != username && message.RecipientUsername != username) 
                return Unauthorized();

            if(message.SenderUsername == username) message.SenderDeleted= true;
            if(message.RecipientUsername == username) message.RecipientDeleted= true;

            if(message.SenderDeleted && message.RecipientDeleted)
            {
                uow.MessageRepository.DeleteMessage(message);
            }

            if (await uow.Complete()) return Ok();

            return BadRequest();
        }
    }
}
