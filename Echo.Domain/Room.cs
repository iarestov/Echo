using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Echo.Domain
{
    internal interface IRoom
    {
        string Name { get; set; }
        Client Enter(string clientId, object networkId);
        void Leave(string clientId);
        void Say(Client client, string messageText);
    }

    internal class Room : IRoom
    {
        private readonly IMessagePoster _messagePoster;
        public string Name { get; set; }
        public int ClientCount => _clients.Count;
        private DateTime LastMessageReceived { get; set; } = DateTime.UtcNow;
        private readonly ConcurrentDictionary<string, Client> _clients = new ConcurrentDictionary<string, Client>();

        public Room(string name, IMessagePoster messagePoster)
        {
            if (String.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));
            if (messagePoster == null) throw new ArgumentNullException(nameof(messagePoster));

            _messagePoster = messagePoster;
            Name = name;
        }

        public Client Enter(string clientId, object networkId)
        {
            var client = _clients.AddOrUpdate(clientId,
                (id) =>
                {
                    var newClient = new Client(clientId, networkId, this);
                    Broadcast(newClient, "Enter room", MessageType.System);
                    return newClient;
                },
                (id, existing) =>
                {
                    existing.NetworkId = networkId; // TODO: сделать способ сравнения?
                    Broadcast(existing, "Reenter room", MessageType.System);
                    return existing;
                });
            return client;
        }

        public void Leave(string clientId)
        {
            Client client;
            if (_clients.TryRemove(clientId, out client))
            {
                Broadcast(client, "Leaves room", MessageType.System);
            }
        }

        public bool IsSatisfiedSilentCriteria(TimeSpan timeToLive)
        {
            return LastMessageReceived.Add(timeToLive) < DateTime.UtcNow;
        }

        public void Say(Client client, string messageText)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));

            if (!_clients.TryGetValue(client.Id, out client))
                throw new InvalidOperationException("client not in room");

            Broadcast(client, messageText);
        }

        private void Broadcast(Client client, string messageText, MessageType messageType = MessageType.User)
        {
            // сначла ответим себе же
            EnqueueMessage(new Message(client, client, messageText, messageType)); 
            // потом остальным
            foreach (var other in _clients.Where(x => x.Key != client.Id).ToList())
            {
                EnqueueMessage(new Message(client, other.Value, messageText, messageType));
            }
        }

        private void EnqueueMessage(Message message)
        {
            UpdateLastReceived();
            _messagePoster.PostMessage(message);
        }

        private void UpdateLastReceived()
        {
            LastMessageReceived = DateTime.UtcNow;
        }
    }
}
