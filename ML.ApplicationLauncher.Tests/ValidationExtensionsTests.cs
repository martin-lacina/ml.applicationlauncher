// Copyright © Martin Lacina

using System;
using ML.ApplicationLauncher.Core.Validation;
using NUnit.Framework;

namespace ML.ApplicationLauncher.Tests
{
    public class ValidationExtensionsTests
    {
        // ---------------------------------------------------------------------
        // ShouldNotBeNull (reference type)
        // ---------------------------------------------------------------------

        [Test]
        public void ShouldNotBeNull_ValidString_ReturnsValue()
        {
            string value = "hello";
            var result = value.ShouldNotBeNull();
            Assert.AreEqual("hello", result);
        }

        [Test]
        public void ShouldNotBeNull_NullString_ThrowsArgumentNullException()
        {
            string? value = null;
            Assert.Throws<ArgumentNullException>(() => value.ShouldNotBeNull());
        }

        [Test]
        public void ShouldNotBeNull_NullString_ThrowsWithCorrectParameterName()
        {
            string? value = null;
            var ex = Assert.Throws<ArgumentNullException>(() => value.ShouldNotBeNull());
            StringAssert.AreEqualIgnoringCase("value", ex.ParamName);
        }

        [Test]
        public void ShouldNotBeNull_EmptyString_ReturnsValue()
        {
            string value = string.Empty;
            var result = value.ShouldNotBeNull();
            Assert.AreEqual(string.Empty, result);
        }

        // ---------------------------------------------------------------------
        // ShouldHaveValue (nullable value type)
        // ---------------------------------------------------------------------

        [Test]
        public void ShouldHaveValue_ValidInt_ReturnsUnwrappedValue()
        {
            int? value = 42;
            var result = value.ShouldHaveValue();
            Assert.AreEqual(42, result);
        }

        [Test]
        public void ShouldHaveValue_NullInt_ThrowsArgumentNullException()
        {
            int? value = null;
            Assert.Throws<ArgumentNullException>(() => value.ShouldHaveValue());
        }

        [Test]
        public void ShouldHaveValue_NullInt_ThrowsWithCorrectParameterName()
        {
            int? value = null;
            var ex = Assert.Throws<ArgumentNullException>(() => value.ShouldHaveValue());
            StringAssert.AreEqualIgnoringCase("value", ex.ParamName);
        }

        [Test]
        public void ShouldHaveValue_ValidGuid_ReturnsUnwrappedValue()
        {
            Guid expected = Guid.NewGuid();
            Guid? value = expected;
            var result = value.ShouldHaveValue();
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void ShouldHaveValue_NullGuid_ThrowsArgumentNullException()
        {
            Guid? value = null;
            Assert.Throws<ArgumentNullException>(() => value.ShouldHaveValue());
        }

        // ---------------------------------------------------------------------
        // ShouldNotBeNullOrEmpty (array)
        // ---------------------------------------------------------------------

        [Test]
        public void ShouldNotBeNullOrEmpty_ValidArray_ReturnsArray()
        {
            int[] value = { 1, 2, 3 };
            var result = value.ShouldNotBeNullOrEmpty();
            Assert.AreEqual(new[] { 1, 2, 3 }, result);
        }

        [Test]
        public void ShouldNotBeNullOrEmpty_NullArray_ThrowsArgumentNullException()
        {
            int[]? value = null;
            Assert.Throws<ArgumentNullException>(() => value.ShouldNotBeNullOrEmpty());
        }

        [Test]
        public void ShouldNotBeNullOrEmpty_NullArray_ThrowsWithCorrectParameterName()
        {
            int[]? value = null;
            var ex = Assert.Throws<ArgumentNullException>(() => value.ShouldNotBeNullOrEmpty());
            StringAssert.AreEqualIgnoringCase("value", ex.ParamName);
        }

        [Test]
        public void ShouldNotBeNullOrEmpty_EmptyArray_ThrowsArgumentException()
        {
            int[] value = Array.Empty<int>();
            Assert.Throws<ArgumentException>(() => value.ShouldNotBeNullOrEmpty());
        }

        [Test]
        public void ShouldNotBeNullOrEmpty_EmptyArray_ThrowsWithCorrectParameterName()
        {
            int[] value = Array.Empty<int>();
            var ex = Assert.Throws<ArgumentException>(() => value.ShouldNotBeNullOrEmpty());
            StringAssert.AreEqualIgnoringCase("value", ex.ParamName);
        }

        [Test]
        public void ShouldNotBeNullOrEmpty_SingleElementArray_ReturnsArray()
        {
            string[] value = { "only" };
            var result = value.ShouldNotBeNullOrEmpty();
            Assert.AreEqual(new[] { "only" }, result);
        }
    }
}
