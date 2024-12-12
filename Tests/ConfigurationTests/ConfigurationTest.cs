using CDS;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace ConfigurationTests
{
    [TestClass]
    public class ConfigurationTest
    {
        [TestMethod]
        public void SetModo_Ok()
        {
            //Arange
            string insert = MODO.TEST.ToString();
            string expected = insert;
            //Act
            Data data = new Data()
            {
                Modo = insert
            };
            string actual = data.Modo;
            //Assert
            Assert.AreEqual(expected, actual);
        }
        [TestMethod]
        public void SetMode_NotOk()
        {
            //Arange
            string insert = "OtroModo";
            string expected = MODO.NORMAL.ToString();
            //Act
            Data data = new Data()
            {
                Modo = insert
            };
            string actual = data.Modo;
            //Assert
            Assert.AreEqual(expected, actual);
        }
    }
}
