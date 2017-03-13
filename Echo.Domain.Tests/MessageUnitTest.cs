using System;
using System.IO.IsolatedStorage;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Echo.Domain.Tests
{
    [TestClass]
    public class MessageUnitTest
    {
        [TestMethod]
        [ExpectedException(typeof (ArgumentNullException))]
        public void CtorNullAuthorThrowArgumentNull()
        {
            var mock = new Mock<IClient>();
            var message = new Message(null, mock.Object,"hello",MessageType.System);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CtorNullClientThrowArgumentNull()
        {
            var mock = new Mock<IClient>();
            var message = new Message(mock.Object, null, "hello", MessageType.System);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CtorNullMessageTextThrowArgumentNull()
        {
            var mock = new Mock<IClient>();
            var message = new Message(mock.Object, mock.Object, null, MessageType.System);
        }

        [TestMethod]
        public void CtorValidNotThrowExceptions()
        {
            var author = new Mock<IClient>().Object;
            var client = new Mock<IClient>().Object;
            var message = new Message(author, client, "hello", MessageType.User);
            Assert.AreSame(author, message.Author);
            Assert.AreSame(client, message.Client);
            Assert.AreEqual("hello", message.Data);
            Assert.AreEqual(MessageType.User, message.MessageType);
        }

    }
}
