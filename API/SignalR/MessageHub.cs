
using System;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR
{
    public class MessageHub : Hub
    {
        private readonly IMessageRepository _messageRepository;
        private readonly IMapper _mapper;
        private readonly IUserRepository _userRepository;
        private readonly IHubContext<PresenceHub> _presenceHub;
        private readonly PresenceTracker _tracker;

        public MessageHub(IMessageRepository messageRepository,
        IMapper mapper,IUserRepository userRepository,IHubContext<PresenceHub> presenceHub,
        PresenceTracker tracker)
        {
            _messageRepository = messageRepository;
            _mapper = mapper;
            _userRepository = userRepository;
            _presenceHub = presenceHub;
            _tracker = tracker;
        }
        public override async Task OnConnectedAsync()
        {
            var httpContext=Context.GetHttpContext();
            var otherUser=httpContext.Request.Query["user"].ToString();

            var groupName=GetGroupName(Context.User.GetUsername(),otherUser);
            await Groups.AddToGroupAsync(Context.ConnectionId,groupName);

            await AddToGroup(Context,groupName);

            var messages=await _messageRepository.
            GetMessageThread(Context.User.GetUsername(),otherUser);

            await Clients.Group(groupName).
            SendAsync("ReceiveMessageThread",messages);
        }

        public override async Task OnDisconnectedAsync(Exception exception){
            
            await RemoveFromMessageGroup(Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(CreateMessageDto createMessageDto){
              var username=Context.User.GetUsername();
                if(username==createMessageDto.RecipientUsername.ToLower())
                   throw new HubException("You cannot send messages to yourself");

                var sender=await _userRepository.GetUserByUsernameAsync(username);
                var recipient=await _userRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername);
                if(recipient==null) throw new HubException("Not found user");

                var message=new Message{
                    Sender=sender,
                    Recipient=recipient,
                    SenderUsername=sender.UserName,
                    RecipientUsername=recipient.UserName,
                    Content=createMessageDto.Content
                };

                var groupName=GetGroupName(sender.UserName,recipient.UserName);

                var group=await _messageRepository.GetMessageGroup(groupName);

                if(group.Connections.Any(x=>x.Username==recipient.UserName))
                {
                    message.DateRead=DateTime.UtcNow;
                }
                else
                {
                    var connections=await _tracker.GetConnectionsForUser(recipient.UserName);
                    if(connections!=null)
                    {
                        await _presenceHub.Clients.Clients(connections).SendAsync("NewMessageReceived",
                        new {username=sender.UserName,knownAs=sender.KnownAs });
                    }
                }

                _messageRepository.AddMessage(message);
                if(await _messageRepository.SaveAllAsync())
                {                
                // var group=GetGroupName(sender.UserName,recipient.UserName);
              await Clients.Group(groupName).SendAsync("NewMessage",_mapper.Map<MessageDto>(message));
                } 
                
             
        }

        private async Task<bool> AddToGroup(HubCallerContext context,string groupName)
        {
            var group=await _messageRepository.GetMessageGroup(groupName);
            var connection=new Connection(Context.ConnectionId,Context.User.GetUsername());

            if(group==null)
            {
                group=new Group(groupName);
                _messageRepository.AddGroup(group);
            }

            group.Connections.Add(connection);

            return await _messageRepository.SaveAllAsync();
        }

        private async Task RemoveFromMessageGroup(string ConnectionId)
        {
            var connection=await _messageRepository.GetConnection(ConnectionId);
            _messageRepository.RemoveConnection(connection);

            await _messageRepository.SaveAllAsync();
        }
        private string GetGroupName(string caller,string other)
        {
            var stringCompare=string.CompareOrdinal(caller,other)<0;
            return stringCompare?$"{caller}-{other}":$"{other}-{caller}";
        }
    }

}