using CDS;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigurationTests
{
    [TestClass]
    public class LogTest
    {
        [TestMethod]
        public void GetLogLevel_NotNull()
        {
            //Arrange
            string logType = LogType.t_debug.ToString();
            LogType expected = LogType.t_debug;
            //Act
            Log.Instance.SetLogType(logType);
            LogType actual = Log.Instance.GetLogLevel();
            //Assert
            Assert.IsNotNull(actual);
            Assert.AreEqual(expected, actual);
        }
        [TestMethod]
        public void GetLogLevel_Default()
        {
            //Arrange
            string logType = "Algun error";
            LogType expected = LogType.t_info;
            //Act
            Log.Instance.SetLogType(logType);
            LogType actual = Log.Instance.GetLogLevel();
            //Assert
            Assert.IsNotNull(actual);
            Assert.AreEqual(expected, actual);
        }
    }
}
