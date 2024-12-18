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
    public class PumpControllerTest
    {
        [TestMethod]
        public void Initi_ReturnTrue()
        {
            // Arrange
            PumpController controller = PumpController.Instance;

            Data data = new Data
            {
                Controller = "CEM-44",
                Timer = "8"
            };

            ControlPanel controlPanel = new ControlPanel();

            // Act
            bool actual = controller.Init(data, controlPanel);

            // Assert
            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void Init_NullArgument()
        {
            // Arrange
            PumpController controller = PumpController.Instance;

            Data data = null;

            ControlPanel controlPanel = null;

            // Act
            bool actual = controller.Init(data, controlPanel);

            // Assert
            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void Init_NullReference()
        {
            // Arrange
            PumpController controller = PumpController.Instance;

            Data data = new Data { };

            // Act
            bool actual = controller.Init(data, new ControlPanel());

            // Assert
            Assert.IsFalse(actual);
        }
    }
}
