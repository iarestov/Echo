using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Echo.Domain
{
    public interface IClient
    {
        string Id { get; }
        string RoomName { get; }
        object NetworkId { get; set; }
        void Say(string messageText);
    }

    internal class Client : IClient
    {
        private object _networkId;

        public string Id { get; }
        public string RoomName => Room.Name;
        public IRoom Room { get; }

        public object NetworkId
        {
            get { return _networkId; }
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(value));
                _networkId = value;
            }
        }

        public Client(string id, object networkId, IRoom room)
        {
            if (String.IsNullOrEmpty(id)) throw new ArgumentNullException(nameof(id));
            if (room == null) throw new ArgumentNullException(nameof(room));

            Id = id;
            NetworkId = networkId;
            Room = room;
        }

        public void Say(string messageText)
        {
            if (messageText == null) throw new ArgumentNullException(nameof(messageText));
            Room.Say(this, messageText);
        }
    }
}
