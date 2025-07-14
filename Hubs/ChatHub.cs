using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace StudentFreelance.Hubs
{
    public class ChatHub : Hub
    {
      
        public async Task JoinConversation(string conversationId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, conversationId);
        }
   
        public async Task SendMessage(string conversationId, int senderId, string content)
        {
            var msg = new
            {
                conversationID = conversationId,
                senderId = senderId,
                content = content,
                sentAt = DateTime.UtcNow
            };

            await Clients.Group(conversationId).SendAsync("ReceiveMessage", msg);
        }
    }
}
