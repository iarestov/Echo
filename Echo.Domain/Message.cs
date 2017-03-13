using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Echo.Domain
{
    public class Message
    {
        public IClient Author { get; }
        public IClient Client { get; }
        public string Data { get; }
        public MessageType MessageType { get; }

        public Message(IClient author, IClient client, string data, MessageType messageType)
        {
            if (author == null) throw new ArgumentNullException(nameof(author));
            if (client == null) throw new ArgumentNullException(nameof(client));
            if (data == null) throw new ArgumentNullException(nameof(data));

            Author = author;
            Client = client;
            Data = data;
            MessageType = messageType;
        }
    }
}
