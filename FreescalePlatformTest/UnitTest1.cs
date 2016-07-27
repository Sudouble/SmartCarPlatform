using System;
using Freescale_debug;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.RegularExpressions;


namespace FreescalePlatformTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            Regex re = new Regex(@"\d+", RegexOptions.Compiled);
            string str = "aa";
        }
    }
}
