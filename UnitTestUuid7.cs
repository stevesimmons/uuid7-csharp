using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using UuidExtensions;

namespace TestUuid7
{
    [TestClass]
    public class UnitTestUuid7
    {
        [TestMethod]
        public void TestGuid()
        {
            // Two Uuid7s from nearly the same time are time-ordered - as Guid 
            var uuid7 = new Uuid7();
            var g1 = uuid7.Guid();
            var g2 = uuid7.Guid();
            Console.WriteLine(g1);
            Console.WriteLine(g2);
            Assert.IsTrue(String.Compare(g1.ToString(), g2.ToString()) < 0);
        }

        [TestMethod]
        public void TestString()
        {
            // Two Uuid7s from nearly the same time are time-ordered - as string
            var uuid7 = new Uuid7();
            var s1 = uuid7.String();
            var s2 = uuid7.String();
            Console.WriteLine(s1);
            Console.WriteLine(s2);
            Assert.IsTrue(String.Compare(s1, s2) < 0);
        }
        
        [TestMethod]
        public void TestId25()
        {
            // Two Uuid7s from nearly the same time are time-ordered - as Id25
            var uuid7 = new Uuid7();
            var s1 = uuid7.Id25();
            var s2 = uuid7.Id25();
            Console.WriteLine(s1);
            Console.WriteLine(s2);
            Assert.IsTrue(String.Compare(s1, s2) < 0);
        }

        [TestMethod]
        public void TestTimingPrecision()
        {   
            // For now, just print this. Checks it runs without crashing.
            Console.WriteLine(Uuid7.CheckTimingPrecision());
        }
    }
}
