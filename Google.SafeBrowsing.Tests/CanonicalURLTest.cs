using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Google.SafeBrowsing.Tests
{
    [TestClass]
    public class CanonicalURL
    {
        [TestMethod]
        public void t01()
        {
            var url = Google.SafeBrowsing.CanonicalURL.Get("http://host/%25%32%35");

            Assert.AreEqual("http://host/%25", url.ToString());
        }

        [TestMethod]
        public void t02()
        {
            var url = Google.SafeBrowsing.CanonicalURL.Get("http://host/%25%32%35%25%32%35");

            Assert.AreEqual("http://host/%25%25", url.ToString());
        }

        [TestMethod]
        public void t03()
        {
            var url = Google.SafeBrowsing.CanonicalURL.Get("http://host/%2525252525252525");

            Assert.AreEqual("http://host/%25", url.ToString());
        }

        [TestMethod]
        public void t04()
        {
            var url = Google.SafeBrowsing.CanonicalURL.Get("http://host/asdf%25%32%35asd");

            Assert.AreEqual("http://host/asdf%25asd", url.ToString());
        }


        [TestMethod]
        public void t05()
        {
            var url = Google.SafeBrowsing.CanonicalURL.Get("http://host/%25%32%35");

            Assert.AreEqual("http://host/%25", url.ToString());
        }

        [TestMethod]
        public void t06()
        {
            var url = Google.SafeBrowsing.CanonicalURL.Get("http://host/%25%32%35%25%32%35");

            Assert.AreEqual("http://host/%25%25", url.ToString());
        }

        [TestMethod]
        public void t07()
        {
            var url = Google.SafeBrowsing.CanonicalURL.Get("http://host/%2525252525252525");

            Assert.AreEqual("http://host/%25", url.ToString());
        }

        [TestMethod]
        public void t08()
        {
            var url = Google.SafeBrowsing.CanonicalURL.Get("http://host/asdf%25%32%35asd");

            Assert.AreEqual("http://host/asdf%25asd", url.ToString());
        }

        [TestMethod]
        public void t09()
        {
            var url = Google.SafeBrowsing.CanonicalURL.Get("http://host/%%%25%32%35asd%%");

            Assert.AreEqual("http://host/%25%25%25asd%25%25", url.ToString());
        }

        [TestMethod]
        public void t10()
        {
            var url = Google.SafeBrowsing.CanonicalURL.Get("http://www.google.com/");

            Assert.AreEqual("http://www.google.com/", url.ToString());
        }

        [TestMethod]
        public void t11()
        {
            var url = Google.SafeBrowsing.CanonicalURL.Get("http://%31%36%38%2e%31%38%38%2e%39%39%2e%32%36/%2E%73%65%63%75%72%65/%77%77%77%2E%65%62%61%79%2E%63%6F%6D/");

            Assert.AreEqual("http://168.188.99.26/.secure/www.ebay.com/", url.ToString());
        }

        [TestMethod]
        public void t12()
        {
            var url = Google.SafeBrowsing.CanonicalURL.Get("http://195.127.0.11/uploads/%20%20%20%20/.verify/.eBaysecure=updateuserdataxplimnbqmn-xplmvalidateinfoswqpcmlx=hgplmcx/");

            Assert.AreEqual("http://195.127.0.11/uploads/%20%20%20%20/.verify/.eBaysecure=updateuserdataxplimnbqmn-xplmvalidateinfoswqpcmlx=hgplmcx/", url.ToString());
        }

        [TestMethod]
        public void t13()
        {
            var url = Google.SafeBrowsing.CanonicalURL.Get("http://host%23.com/%257Ea%2521b%2540c%2523d%2524e%25f%255E00%252611%252A22%252833%252944_55%252B");

            Assert.AreEqual("http://host%23.com/~a!b@c%23d$e%25f^00&11*22(33)44_55+", url.ToString());
        }

        [TestMethod]
        public void t14()
        {
            var url = Google.SafeBrowsing.CanonicalURL.Get("http://3279880203/blah");

            Assert.AreEqual("http://195.127.0.11/blah", url.ToString());
        }

        [TestMethod]
        public void t15()
        {
            var url = Google.SafeBrowsing.CanonicalURL.Get("http://www.google.com/blah/..");

            Assert.AreEqual("http://www.google.com/", url.ToString());
        }

        [TestMethod]
        public void t16()
        {
            var url = Google.SafeBrowsing.CanonicalURL.Get("www.google.com/");

            Assert.AreEqual("http://www.google.com/", url.ToString());
        }

        [TestMethod]
        public void t17()
        {
            var url = Google.SafeBrowsing.CanonicalURL.Get("www.google.com");

            Assert.AreEqual("http://www.google.com/", url.ToString());
        }

        [TestMethod]
        public void t18()
        {
            var url = Google.SafeBrowsing.CanonicalURL.Get("http://www.evil.com/blah#frag");

            Assert.AreEqual("http://www.evil.com/blah", url.ToString());
        }

        [TestMethod]
        public void t19()
        {
            var url = Google.SafeBrowsing.CanonicalURL.Get("http://www.GOOgle.com/");

            Assert.AreEqual("http://www.google.com/", url.ToString());
        }

        [TestMethod]
        public void t20()
        {
            var url = Google.SafeBrowsing.CanonicalURL.Get("http://www.google.com.../");

            Assert.AreEqual("http://www.google.com/", url.ToString());
        }

        [TestMethod]
        public void t21()
        {
            var url = Google.SafeBrowsing.CanonicalURL.Get("http://www.google.com/foo\tbar\rbaz\n2");

            Assert.AreEqual("http://www.google.com/foobarbaz2", url.ToString());
        }

        [TestMethod]
        public void t22()
        {
            var url = Google.SafeBrowsing.CanonicalURL.Get("http://www.google.com/q?");

            Assert.AreEqual("http://www.google.com/q?", url.ToString());
        }

        [TestMethod]
        public void t23()
        {
            var url = Google.SafeBrowsing.CanonicalURL.Get("http://www.google.com/q?r?");

            Assert.AreEqual("http://www.google.com/q?r?", url.ToString());
        }

        [TestMethod]
        public void t24()
        {
            var url = Google.SafeBrowsing.CanonicalURL.Get("http://www.google.com/q?r?s");

            Assert.AreEqual("http://www.google.com/q?r?s", url.ToString());
        }

        [TestMethod]
        public void t25()
        {
            var url = Google.SafeBrowsing.CanonicalURL.Get("http://evil.com/foo#bar#baz");

            Assert.AreEqual("http://evil.com/foo", url.ToString());
        }

        [TestMethod]
        public void t26()
        {
            var url = Google.SafeBrowsing.CanonicalURL.Get("http://evil.com/foo;");

            Assert.AreEqual("http://evil.com/foo;", url.ToString());
        }

        [TestMethod]
        public void t27()
        {
            var url = Google.SafeBrowsing.CanonicalURL.Get("http://evil.com/foo?bar;");

            Assert.AreEqual("http://evil.com/foo?bar;", url.ToString());
        }

        [TestMethod]
        public void t28()
        {
            var url = Google.SafeBrowsing.CanonicalURL.Get("http://\x01\x80.com/");

            Assert.AreEqual("http://%01%80.com/", url.ToString());
        }

        [TestMethod]
        public void t29()
        {
            var url = Google.SafeBrowsing.CanonicalURL.Get("http://notrailingslash.com");

            Assert.AreEqual("http://notrailingslash.com/", url.ToString());
        }

        [TestMethod]
        public void t30()
        {
            var url = Google.SafeBrowsing.CanonicalURL.Get("http://www.gotaport.com:1234/");

            Assert.AreEqual("http://www.gotaport.com:1234/", url.ToString());
        }

        [TestMethod]
        public void t31()
        {
            var url = Google.SafeBrowsing.CanonicalURL.Get("  http://www.google.com/  ");

            Assert.AreEqual("http://www.google.com/", url.ToString());
        }

        [TestMethod]
        public void t32()
        {
            var url = Google.SafeBrowsing.CanonicalURL.Get("http:// leadingspace.com/");

            Assert.AreEqual("http://%20leadingspace.com/", url.ToString());
        }

        [TestMethod]
        public void t33()
        {
            var url = Google.SafeBrowsing.CanonicalURL.Get("http://%20leadingspace.com/");

            Assert.AreEqual("http://%20leadingspace.com/", url.ToString());
        }

        [TestMethod]
        public void t34()
        {
            var url = Google.SafeBrowsing.CanonicalURL.Get("%20leadingspace.com/");

            Assert.AreEqual("http://%20leadingspace.com/", url.ToString());
        }

        [TestMethod]
        public void t35()
        {
            var url = Google.SafeBrowsing.CanonicalURL.Get("https://www.securesite.com/");

            Assert.AreEqual("https://www.securesite.com/", url.ToString());
        }

        [TestMethod]
        public void t36()
        {
            var url = Google.SafeBrowsing.CanonicalURL.Get("http://host.com/ab%23cd");

            Assert.AreEqual("http://host.com/ab%23cd", url.ToString());
        }

        [TestMethod]
        public void t37()
        {
            var url = Google.SafeBrowsing.CanonicalURL.Get("http://host.com//twoslashes?more//slashes");

            Assert.AreEqual("http://host.com/twoslashes?more//slashes", url.ToString());
        }
    }
}
