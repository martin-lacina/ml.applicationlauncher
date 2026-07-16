using NUnit.Framework;
using System;
using ML.ApplicationLauncher.Shared.Services;

namespace ML.ApplicationLauncher.Tests
{
    public class MessageServiceTests
    {
        [Test]
        public void TrimMessageLength_NullInput_ReturnsNull()
        {
            var result = MessageService.TrimMessageLength(null);
            Assert.IsNull(result);
        }

        [Test]
        public void TrimMessageLength_EmptyString_ReturnsEmptyString()
        {
            var result = MessageService.TrimMessageLength(string.Empty);
            Assert.AreEqual(string.Empty, result);
        }

        [Test]
        public void TrimMessageLength_NormalLength_ReturnsUnchanged()
        {
            const string message = "Short message";
            var result = MessageService.TrimMessageLength(message);
            Assert.AreEqual(message, result);
        }

        [Test]
        public void TrimMessageLength_Exactly1000Chars_ReturnsUnchanged()
        {
            var message = CreateStringOfLength(1000);
            var result = MessageService.TrimMessageLength(message);
            Assert.AreEqual(1000, result.Length);
        }

        [Test]
        public void TrimMessageLength_Over1000Chars_ReturnsTruncated()
        {
            var message = CreateStringOfLength(2000);
            var result = MessageService.TrimMessageLength(message);
            Assert.AreEqual(1000, result.Length);
        }

        [Test]
        public void TrimMessageLength_JustOver1000Chars_ReturnsTruncated()
        {
            var message = CreateStringOfLength(1001);
            var result = MessageService.TrimMessageLength(message);
            Assert.AreEqual(1000, result.Length);
        }

        private static string CreateStringOfLength(int length) => new string('x', length);
    }
}
