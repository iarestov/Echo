using System;
using System.IO.IsolatedStorage;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Echo.Domain.Tests
{
    [TestClass]
    public class ClientUnitTest
    {
        [TestMethod]
        [ExpectedException(typeof (ArgumentNullException))]
        public void CtorInvalidIdThrowArgumentNull()
        {
            var mock = new Mock<IRoom>();
            var client = new Client(null, new object(), mock.Object);
        }

        [TestMethod]
        [ExpectedException(typeof (ArgumentNullException))]
        public void CtorInvalidNetworkIdThrowArgumentNull()
        {
            var mock = new Mock<IRoom>();
            var client = new Client("client", null, mock.Object);
        }

        [TestMethod]
        [ExpectedException(typeof (ArgumentNullException))]
        public void CtorInvalidRoomThrowArgumentNull()
        {
            var client = new Client("client", new object(), null);
        }

        [TestMethod]
        public void CtorValidNoThrowExceptions()
        {
            var mock = new Mock<IRoom>();
            var networkId = new object();
            var room = mock.Object;
            var client = new Client("client", networkId, room);
            Assert.AreEqual("client", client.Id);
            Assert.AreSame(networkId,client.NetworkId);
            Assert.AreSame(room, client.Room);
        }

        [TestMethod]
        public void SayTestMethod()
        {
            var mock = new Mock<IRoom>();

            var client = new Client("client", new object(), mock.Object);
            client.Say("hello");
            mock.Verify(x => x.Say(It.Is<Client>(d => d == client), It.Is<string>(m => m == "hello")), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof (ArgumentNullException))]
        public void SayNullThrowArgumentNull()
        {
            var mock = new Mock<IRoom>();

            var client = new Client("client", new object(), mock.Object);
            client.Say(null);
        }

        [TestMethod]
        public void SayEmptyTestMethod()
        {
            var mock = new Mock<IRoom>();

            var client = new Client("client", new object(), mock.Object);
            client.Say("");
            mock.Verify(x => x.Say(It.Is<Client>(d => d == client), It.Is<string>(m => m == "")), Times.Once);
        }
    }
}
