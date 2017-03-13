using System;
using System.IO.IsolatedStorage;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Echo.Domain.Tests
{
    [TestClass]
    public class RoomUnitTest
    {
        [TestMethod]
        [ExpectedException(typeof (ArgumentNullException))]
        public void CtorInvalidNameThrowArgumentNull()
        {
            var mock = new Mock<IMessagePoster>();
            var room = new Room(null, mock.Object);
        }

        [TestMethod]
        [ExpectedException(typeof (ArgumentNullException))]
        public void CtorInvalidPosterThrowArgumentNull()
        {
            var room = new Room("room", null);
        }

        [TestMethod]
        public void CtorValidNoThrowExceptions()
        {
            var mock = new Mock<IMessagePoster>();
            var poster = mock.Object;
            var room = new Room("room", poster);

            Assert.AreEqual("room", room.Name);
            Assert.AreEqual(0, room.ClientCount);
        }

        [TestMethod]
        public void EnterTestMethod()
        {
            var mock = new Mock<IMessagePoster>();
            var poster = mock.Object;
            var networkId = new object();

            var room = new Room("room", poster);
            var client = room.Enter("client", networkId);
            Assert.AreEqual(1, room.ClientCount);
            Assert.AreEqual("client", client.Id);
            Assert.AreSame(networkId, client.NetworkId);
            Assert.AreSame(room, client.Room);
            mock.Verify(x => x.PostMessage(It.Is<Message>(m =>
              m.Author == client &&
              m.Client == client &&
              m.MessageType == MessageType.System
              )), Times.Once);
        }

        [TestMethod]
        public void ReenterTestMethod()
        {
            var mock = new Mock<IMessagePoster>();
            var poster = mock.Object;
            var room = new Room("room", poster);

            var networkId = new object();
            var client = room.Enter("client", networkId);
            Assert.AreEqual(1, room.ClientCount);

            var networkId2 = new object();
            var client2 = room.Enter("client", networkId2);
            Assert.AreEqual(1, room.ClientCount);

            Assert.AreSame(client, client2);

            Assert.AreEqual("client", client2.Id);
            Assert.AreSame(networkId2, client2.NetworkId);
            Assert.AreSame(room, client2.Room);
        }

        [TestMethod]
        public void MultipleClientsEnterTestMethod()
        {
            var mock = new Mock<IMessagePoster>();
            var poster = mock.Object;
            var room = new Room("room", poster);

            var networkId1 = new object();
            var client1 = room.Enter("client1", networkId1);
            Assert.AreEqual(1, room.ClientCount);

            var networkId2 = new object();
            var client2 = room.Enter("client2", networkId2);
            Assert.AreEqual(2, room.ClientCount);

            Assert.AreNotSame(client1, client2);

            Assert.AreEqual("client1", client1.Id);
            Assert.AreSame(networkId1, client1.NetworkId);
            Assert.AreSame(room, client1.Room);

            Assert.AreEqual("client2", client2.Id);
            Assert.AreSame(networkId2, client2.NetworkId);
            Assert.AreSame(room, client2.Room);
        }


        [TestMethod]
        public void LeaveTestMethod()
        {
            var mock = new Mock<IMessagePoster>();
            var poster = mock.Object;
            var room = new Room("room", poster);

            var networkId1 = new object();
            var client1 = room.Enter("client1", networkId1);
            Assert.AreEqual(1, room.ClientCount);

            room.Leave("client1");

            Assert.AreEqual(0, room.ClientCount);

            mock.Verify(x => x.PostMessage(It.Is<Message>(m =>
               m.Author == client1 &&
               m.Client == client1 &&
               m.MessageType == MessageType.System
               )), Times.Exactly(2));
        }

        [TestMethod]
        public void LeaveNotExistentTestMethod()
        {
            var mock = new Mock<IMessagePoster>();
            var poster = mock.Object;
            var room = new Room("room", poster);

            room.Leave("client1");

            Assert.AreEqual(0, room.ClientCount);
        }

        [TestMethod]
        public void SatisfiedSilentTestMethod()
        {
            var mock = new Mock<IMessagePoster>();
            var poster = mock.Object;
            var room = new Room("room", poster);

            Thread.Sleep(100);

            Assert.IsTrue(room.IsSatisfiedSilentCriteria(TimeSpan.FromMilliseconds(50)));
        }

        [TestMethod]
        public void SayAloneTestMethod()
        {
            var mock = new Mock<IMessagePoster>();
            var poster = mock.Object;
            var room = new Room("room", poster);

            var networkId1 = new object();
            var client1 = room.Enter("client1", networkId1);
            room.Say(client1, "hello");

            mock.Verify(x => x.PostMessage(It.Is<Message>(m =>
                m.Author == client1 &&
                m.Client == client1 &&
                m.Data == "hello" &&
                m.MessageType == MessageType.User
                )), Times.Once);
        }

        [TestMethod]
        public void SayBroadcastTestMethod()
        {
            var mock = new Mock<IMessagePoster>();
            var poster = mock.Object;
            var room = new Room("room", poster);

            var networkId1 = new object();
            var client1 = room.Enter("client1", networkId1);

            var networkId2 = new object();
            var client2 = room.Enter("client2", networkId2);

            room.Say(client1, "hello");

            mock.Verify(x => x.PostMessage(It.Is<Message>(m =>
                m.Author == client1 &&
                m.Client == client1 &&
                m.Data == "hello" &&
                m.MessageType == MessageType.User
                )), Times.Once);

            mock.Verify(x => x.PostMessage(It.Is<Message>(m =>
                m.Author == client1 &&
                m.Client == client2 &&
                m.Data == "hello" &&
                m.MessageType == MessageType.User
                )), Times.Once);
        }

        [TestMethod]
        public void UpdateLastMessageDateTimeTestMethod()
        {
            var mock = new Mock<IMessagePoster>();
            var poster = mock.Object;
            var room = new Room("room", poster);

            Thread.Sleep(100);
            Assert.IsTrue(room.IsSatisfiedSilentCriteria(TimeSpan.FromMilliseconds(50)));

            var networkId1 = new object();
            var client1 = room.Enter("client1", networkId1);
            Assert.IsFalse(room.IsSatisfiedSilentCriteria(TimeSpan.FromMilliseconds(50)));

            Thread.Sleep(100);
            Assert.IsTrue(room.IsSatisfiedSilentCriteria(TimeSpan.FromMilliseconds(50)));

            room.Say(client1, "hello");
            Assert.IsFalse(room.IsSatisfiedSilentCriteria(TimeSpan.FromMilliseconds(50)));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SayNonExistentClientThrowInvalidOperation()
        {
            var mock = new Mock<IMessagePoster>();
            var poster = mock.Object;
            var room1 = new Room("room1", poster);
            var room2 = new Room("room2", poster);

            var networkId1 = new object();
            var client1 = room1.Enter("client1", networkId1);

            room2.Say(client1, "hello");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SayNullClientThrowArgumentNull()
        {
            var mock = new Mock<IMessagePoster>();
            var poster = mock.Object;
            var room = new Room("room", poster);

            room.Say(null, "hello");
        }
    }
}
