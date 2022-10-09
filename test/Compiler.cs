using System;
using NUnit.Framework;
using runtime.Utils;

namespace test
{
    public class Tests
    {
        [SetUp]
        public void Setup() {}

        [Test]
        public void UnsafeList() {
            UnsafeList<byte> v = new();
            int i1 = v.Write<byte>(3);
            int i2 = v.Write<long>(5345);
            int i3 = v.Write(33123);
            Assert.IsTrue(v.Read<byte>(i1) == 3, "Cannot Read i1");
            Assert.IsTrue(v.Read<long>(i2) == 5345, "Cannot Read i2");
            Assert.IsTrue(v.Read<int>(i3) == 33123, "Cannot Read i3");
        }
    }
}