using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Kinect.Recognition.Extensions;
using System.Xml;

namespace Kinect.Recognition.UnitTests
{
    /// <summary>
    /// Unit tests for the extension methods containing class
    /// </summary>
    [TestClass]
    public class ExtensionTests
    {
        /// <summary>
        /// Positive test of the Max method
        /// </summary>
        [TestMethod]
        public void TestMax()
        {
            // arrange
            var arrNormal = new[] { 1, 2, 5, 4 };
            // act
            int maxElement = ((IEnumerable<int>)arrNormal).Max( x => x, (x1, x2) => x1 - x2);
            // assert
            Assert.AreEqual(5, maxElement, "max element found");
        }

        /// <summary>
        /// Testing for invalid argument
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestMaxInvalidSelector()
        {
            // arrange
            var arrNormal = new[] { 1, 2, 5, 4 };
            // act
            int maxElement = ((IEnumerable<int>)arrNormal).Max<int, int>(null, (x1, x2) => x1 - x2);
            // assert
            Assert.Fail();
        }

        /// <summary>
        /// Testing for invalid argument
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestMaxInvalidComparator()
        {
            // arrange
            var arrNormal = new[] { 1, 2, 5, 4 };
            // act
            int maxElement = ((IEnumerable<int>)arrNormal).Max<int, int>(x => x, null);
            // assert
            Assert.Fail();
        }

        /// <summary>
        /// Positive test for the ProcessXmlNodes method
        /// </summary>
        [TestMethod]
        public void TestProcessXmlNodes()
        {
            // arrange
            string xPath = "//document/element";
            string result = string.Empty;

            // act
            this.GetXmlDocument().ProcessXmlNodes(xPath, node => result += node.InnerText);

            // assert
            Assert.AreEqual("123", result);
        }

        /// <summary>
        /// Negative test for the ProcessXmlNodes method
        /// </summary>
        [TestMethod]
        public void TestProcessXmlNodesEmpty()
        {
            // arrange
            string xPath = "//document/element/missingElement";
            string result = string.Empty;

            // act
            this.GetXmlDocument().ProcessXmlNodes(xPath, node => result += node.InnerText);

            // assert
            Assert.AreEqual(string.Empty, result);
        }

        /// <summary>
        /// Testing for invalid argument
        /// </summary>
        [TestMethod]
        [ExpectedExceptionAttribute(typeof(ArgumentException))]
        public void TestProcessXmlNodesEmptyPath()
        {
            // act
            this.GetXmlDocument().ProcessXmlNodes(string.Empty, x => { });
            // assert
            Assert.Fail();
        }

        /// <summary>
        /// Testing for invalid argument
        /// </summary>
        [TestMethod]
        [ExpectedExceptionAttribute(typeof(ArgumentException))]
        public void TestProcessXmlNodesMissingPath()
        {
            // act
            this.GetXmlDocument().ProcessXmlNodes(null, x => { });
            // assert
            Assert.Fail();
        }

        /// <summary>
        /// Testing for invalid argument
        /// </summary>
        [TestMethod]
        [ExpectedExceptionAttribute(typeof(ArgumentNullException))]
        public void TestProcessXmlNodesMissingAction()
        {
            // act
            this.GetXmlDocument().ProcessXmlNodes("//smth/smth", null);
            // assert
            Assert.Fail();
        }

        /// <summary>
        /// Helper method to create xml document
        /// </summary>
        /// <returns>xml document with some sample xml</returns>
        private XmlDocument GetXmlDocument()
        {
            XmlDocument res = new XmlDocument();
            res.LoadXml("<document><element>1</element><element>2</element><element>3</element></document>");
            return res;
        }
    }
}
