﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Platform.Xml.Serialization.Tests
{
    [TestFixture]
    public class InnerTextTest
    {
        [Test]
        public void TestInnerText()
        {
            ShouldCreateInnerText test = new ShouldCreateInnerText(){ThisShouldBeInner = "lorem ipsum"};
            var result = XmlSerializer<ShouldCreateInnerText>.New().SerializeToString(test);
            Assert.IsTrue(result.Contains("<ShouldCreateInnerText>lorem ipsum</ShouldCreateInnerText>"), "The element should contain the innertext");
        }

        [Test]
        public void ShouldOnlyContainOneTextAttribute()
        {
            var obj = new ShouldThrowError();
            bool errorThrown = false;
            try
            {
                var result = XmlSerializer<ShouldThrowError>.New().SerializeToString(obj);
            }
            catch (Exception ex)
            {
                errorThrown = true;
            }
            Assert.IsTrue(errorThrown, "There should be an error when there are multiple TextAttributes");
        }

        [Test]
        public void CanDeserialize()
        {
            ShouldCreateInnerText test = new ShouldCreateInnerText() { ThisShouldBeInner = "lorem ipsum" };
            var result = XmlSerializer<ShouldCreateInnerText>.New().SerializeToString(test);
            var obj = XmlSerializer<ShouldCreateInnerText>.New().Deserialize(result);
            Assert.IsTrue(!string.IsNullOrEmpty(obj.ThisShouldBeInner));
        }

        [Test]
        public void MixedTest()
        {
            var obj = new AMixedTest() {MyProperty = "myproperty", ThisShouldBeInner = "inner"};
            var result = XmlSerializer<AMixedTest>.New().SerializeToString(obj).Replace("\n","").Replace("\r","");
            Assert.IsTrue(result.Equals("<?xml version=\"1.0\" encoding=\"utf-16\"?><AMixedTest>  <MyProperty>myproperty</MyProperty>inner</AMixedTest>"), "Xml is not correct!");
            var deserialized = XmlSerializer<AMixedTest>.New().Deserialize(result);
            Assert.IsTrue(deserialized.MyProperty == obj.MyProperty && deserialized.ThisShouldBeInner == obj.ThisShouldBeInner, "Objects are not the same");
        }

        [XmlElement]
        private class ShouldCreateInnerText
        {
            [XmlTextAttribute]
            public string ThisShouldBeInner { get; set; }
        }

        [XmlElement]
        private class ShouldThrowError
        {
            [XmlTextAttribute]
            public string ThisShouldBeInner { get; set; }

            [XmlTextAttribute]
            public string ThisShouldBeInnerToo { get; set; }
        }

        [XmlElement]
        private class AMixedTest
        {
            [XmlTextAttribute]
            public string ThisShouldBeInner { get; set; }

            [XmlElement]
            public string MyProperty { get; set; }
        }
    }
}