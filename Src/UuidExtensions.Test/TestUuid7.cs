namespace UuidExtensions.Test
{
    [TestClass]
    public class TestUuid7
    {
        [TestMethod]
        public void TestCantUseUuidv4()
        {
            // Attempting to form a Id25 from a Uuid 4 fails
            var g = Guid.Empty; // UUID v4
            string s = Uuid7.Id25(g);
            Assert.IsFalse(string.IsNullOrWhiteSpace(s));
        }

        [TestMethod]
        public void TestFixedTimes()
        {
            long t1 = Uuid7.TimeNs();
            long t2 = t1 + 1;
            string s1 = Uuid7.Id25(t1); // e.g. 0q996kioxxyfds1stmjqajen6
            string s2 = Uuid7.Id25(t2); //      0q996kioxxyfj83w8bqp67d2j
            string s3 = Uuid7.Id25(t2); //      0q996kioxxyfj83z4pmujhrx4
            Assert.IsTrue(string.Compare(s1, s2) < 0);
            Assert.IsTrue(string.Compare(s2, s3) < 0);

            // Using same timestamp give different values due to sequence counter and randomness
            var g1 = Uuid7.Guid(t1);
            var g2 = Uuid7.Guid(t1);
            Assert.IsFalse(g1 == g2);
        }

        [TestMethod]
        public void TestId25()
        {
            string s1 = Uuid7.Id25(); // e.g. 0q994uri6sp53j3eu7bty4wsv
            string s2 = Uuid7.Id25(); //      0q994uri70qe0gjxrq8iv4iyu
            Assert.IsTrue(string.Compare(s1, s2) < 0);
        }

        [TestMethod]
        public void TestNoRandomness()
        {
            // Two Id25s from a Uuid7 Guid input add no further randomness
            long t = Uuid7.TimeNs();
            Guid g = Uuid7.Guid(t);
            string s1 = Uuid7.Id25(g);
            string s2 = Uuid7.Id25(g);
            Assert.IsTrue(s1 == s2);
        }

        [TestMethod]
        public void TestUuid7Guid()
        {
            Guid g1 = Uuid7.Guid();     // e.g. 06338364-8305-788f-8000-9ada942eb663
            string s2 = Uuid7.String(); //      06338364-8305-7b74-8000-de4963503139
            Assert.IsTrue(string.Compare(g1.ToString(), s2) < 0);
        }

        [TestMethod]
        public void TestZero()
        {
            var s1 = Uuid7.String(0);
            Assert.IsTrue(s1 == "00000000-0000-0000-0000-000000000000");
            var s2 = Uuid7.Id25(0);
            Assert.IsTrue(s2 == "0000000000000000000000000");
        }
    }
}