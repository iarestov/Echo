using System;
using System.Collections.Concurrent;
using System.Linq;

namespace Echo.Domain
{
    public interface IMessagePoster
    {
        void PostMessage(Message message);
    }

    public interface IMessageQueue
    {
        Message GetMessageToSend();
        int GetMessagesToSendCount();
    }

    public interface IEchoRoomServer
    {
        IClient EnterInRoom(string clientId, object networkId, string roomName);
        void DropSilentRooms();
    }

    public class EchoRoomServer : IMessagePoster, IMessageQueue, IEchoRoomServer
    {
        private readonly TimeSpan _roomTimeToLive;
        private readonly ConcurrentDictionary<string, Room> _rooms = new ConcurrentDictionary<string, Room>();
        private readonly ConcurrentQueue<Message> _outQueue = new ConcurrentQueue<Message>();

        public int RoomsCount => _rooms.Count;

        public EchoRoomServer(TimeSpan roomTimeToLive)
        {
            if (roomTimeToLive.TotalMilliseconds <= 0) throw new ArgumentOutOfRangeException(nameof(roomTimeToLive));
            _roomTimeToLive = roomTimeToLive;
        }

        private Room GetRoomByName(string name)
        {
            while (true)
            {
                var room = _rooms.GetOrAdd(name, (n) => new Room(name, this));
                if (!room.IsSatisfiedSilentCriteria(_roomTimeToLive)) return room;
                RemoveRoom(name);
            }
        }

        private void RemoveRoom(string name)
        {
            Room room;
            _rooms.TryRemove(name, out room);
        }

        public IClient EnterInRoom(string clientId, object networkId, string roomName)
        {
            LeaveAllOtherRooms(clientId, roomName);
            var room = GetRoomByName(roomName);
            return room.Enter(clientId, networkId);
        }

        private void LeaveAllOtherRooms(string clientId, string roomName)
        {
            foreach (var r in _rooms.Where(r => r.Value.Name != roomName))
            {
                r.Value.Leave(clientId);
            }
        }

        public void PostMessage(Message message)
        {
            _outQueue.Enqueue(message);
        }

        public Message GetMessageToSend()
        {
            Message message;
            if (!_outQueue.TryDequeue(out message))
            {
                return null;
            }
            return message;
        }

        public int GetMessagesToSendCount()
        {
            return _outQueue.Count;
        }

        public void DropSilentRooms()
        {
            var roomsToDrop = _rooms
                .Where(x => x.Value.IsSatisfiedSilentCriteria(_roomTimeToLive))
                .Select(x => x.Key)
                .ToList();
            foreach (var name in roomsToDrop)
            {
                RemoveRoom(name);
            }
        }
    }
}
