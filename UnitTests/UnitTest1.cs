using System;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Woof.SystemEx;

namespace UnitTests {

    [TestClass]
    public class UnitTest1 {

        [TestMethod]
        public void StringReplaceTest() {
            var patterns = new[] {
                "Pchnąć w tę ŁÓDŹ jeża lub ośm skrzyń fig. Pchnąć w tę ŁÓDŹ jeża lub ośm skrzyń fig.",
                "ŻółwŻółwŻółw"
            };
            var searches = new[] {
                "łódź",
                "żółw"
            };
            var replacements = new[] {
                "DZICZ",
                "Zając"
            };
            var expected = new[] {
                "Pchnąć w tę DZICZ jeża lub ośm skrzyń fig. Pchnąć w tę DZICZ jeża lub ośm skrzyń fig.",
                "ZającZającZając"
            };
            var results = new string[2];
            for (int x = 0; x < 5000000; x++)
            for (int i = 0, n = patterns.Length; i < n; i++) {
                results[i] = patterns[i].Replace(searches[i], replacements[i], StringComparison.OrdinalIgnoreCase);
                Assert.AreEqual(expected[i], results[i], false);
            }
        }

    }

}