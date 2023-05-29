using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web.Mvc;
using BHelp.Controllers;

namespace BHelpTests
{
    [TestClass]
    public class DeliveriesControllerTests
    {
        [TestMethod]
        public void HelperReport()
        {
            //Arrange
            var controller = new DeliveriesController();
            //Act
            var result = controller.HelperReport() as ViewResult;
            //Assert
            Assert.IsNotNull(result);
        }
    }
}
