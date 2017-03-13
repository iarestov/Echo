using System;
using System.IO.IsolatedStorage;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Echo.Domain.Tests
{
    [TestClass]
    public class EchoRoomServerUnitTest
    {
        [TestMethod]
        [ExpectedException(typeof (ArgumentOutOfRangeException))]
        public void CtorZeroTtlThrowArgumentOutOfRange()
        {
            var server = new EchoRoomServer(TimeSpan.Zero);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void CtorNegativeTtlThrowArgumentOutOfRange()
        {
            var server = new EchoRoomServer(TimeSpan.FromMilliseconds(-1));
        }


        [TestMethod]
        
        public void CtorValidNoThrowExceptions()
        {
            var server = new EchoRoomServer(TimeSpan.FromMilliseconds(1));
            Assert.AreEqual(0, server.RoomsCount);
            Assert.AreEqual(0, server.GetMessagesToSendCount());
            Assert.IsNull(server.GetMessageToSend());
        }

        [TestMethod]
        public void EnterTestMethod()
        {
            var server = new EchoRoomServer(TimeSpan.FromMilliseconds(1000));
            var networkId = new object();

            var client = server.EnterInRoom("client", networkId, "room");
            Assert.IsNotNull(client);
            Assert.AreEqual("client", client.Id);
            Assert.AreSame(networkId, client.NetworkId);
            Assert.AreEqual(1, server.GetMessagesToSendCount());
        }

        [TestMethod]
        public void GetMessageToSendTestMethod()
        {
            var server = new EchoRoomServer(TimeSpan.FromMilliseconds(1000));
            var networkId = new object();

            var client = server.EnterInRoom("client", networkId, "room");
            Message message = server.GetMessageToSend();
            Assert.IsNotNull(message);
            Assert.AreEqual(MessageType.System, message.MessageType);
            Assert.AreEqual("client", message.Client.Id);
            Assert.AreEqual("client", message.Author.Id);
        }

        [TestMethod]
        public void EnterToMultipleRoomsTestMethod()
        {
            var server = new EchoRoomServer(TimeSpan.FromMilliseconds(1000));
            var networkId1 = new object();
            var networkId2 = new object();

            var client1 = server.EnterInRoom("client1", networkId1, "room1");
            var client2 = server.EnterInRoom("client2", networkId2, "room2");
            Assert.AreEqual(2, server.RoomsCount);
            Assert.AreEqual(2, server.GetMessagesToSendCount());
        }

        [TestMethod]
        public void SameClientIdEnterToMultipleRoomsLeavesPrevRoomTestMethod()
        {
            var server = new EchoRoomServer(TimeSpan.FromMilliseconds(1000));
            var networkId1 = new object();
            var networkId2 = new object();

            var client1 = server.EnterInRoom("client", networkId1, "room1");
            var client2 = server.EnterInRoom("client", networkId2, "room2");
            Assert.AreEqual(3, server.GetMessagesToSendCount());
        }

        [TestMethod]
        public void EnterToExpiredRoomTestMethod()
        {
            var server = new EchoRoomServer(TimeSpan.FromMilliseconds(50));
            var networkId1 = new object();
            var networkId2 = new object();

            var client1 = server.EnterInRoom("client1", networkId1, "room");
            Thread.Sleep(100);
            var client2 = server.EnterInRoom("client2", networkId2, "room");
            Assert.AreEqual(2, server.GetMessagesToSendCount());
        }

        [TestMethod]
        public void CleanupExpiredRoomTestMethod()
        {
            var server = new EchoRoomServer(TimeSpan.FromMilliseconds(50));
            var networkId1 = new object();

            var client1 = server.EnterInRoom("client1", networkId1, "room");
            server.DropSilentRooms();
            Assert.AreEqual(1, server.RoomsCount);
            Thread.Sleep(100);
            server.DropSilentRooms();
            Assert.AreEqual(0, server.RoomsCount);
        }

    }
}
