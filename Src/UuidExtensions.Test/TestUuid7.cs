using UuidExtensions;

namespace UuidExtensions.Test
{
    [TestClass]
    public class TestUuid7
    {
        [TestMethod]
        public void TestUuid7Guid()
        {
            var uuid7 = new Uuid7();
            Guid g1 = uuid7.Guid();     // e.g. 06338364-8305-788f-8000-9ada942eb663
            string s2 = uuid7.String(); //      06338364-8305-7b74-8000-de4963503139
            Assert.IsTrue(String.Compare(g1.ToString(), s2) < 0);
        }

        [TestMethod]
        public void TestId25()
        {
            var uuid7 = new Uuid7();
            string s1 = uuid7.Id25(); // e.g. 0q994uri6sp53j3eu7bty4wsv
            string s2 = uuid7.Id25(); //      0q994uri70qe0gjxrq8iv4iyu
            Assert.IsTrue(String.Compare(s1, s2) < 0);
        }

        [TestMethod]
        public void TestFixedTimes()
        {
            var uuid7 = new Uuid7();
            long t1 = Uuid7.TimeNs();
            long t2 = t1 + 1;
            string s1 = uuid7.Id25(t1); // e.g. 0q996kioxxyfds1stmjqajen6
            string s2 = uuid7.Id25(t2); //      0q996kioxxyfj83w8bqp67d2j
            string s3 = uuid7.Id25(t2); //      0q996kioxxyfj83z4pmujhrx4
            Assert.IsTrue(String.Compare(s1, s2) < 0);
            Assert.IsTrue(String.Compare(s2, s3) < 0);
        }
    }
}