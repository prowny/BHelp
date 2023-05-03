using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web.Mvc;
using BHelp.Controllers;

namespace BHelpTests
{
    [TestClass]
    public class ControllerTests
    {
        [TestMethod]
        public void TestActiveVolunteerDetails()
        {
            //Arrange
            var controller = new ReportsController();
            //Act
            var result = controller.ActiveVolunteerDetails() as ViewResult;
            //Assert
            Assert.IsNotNull(result);
        }
        [TestMethod]
        public void WeeklyInfoReport()
        {
            //Arrange
            var controller = new ReportsController();
            //Act
            var result = controller.WeeklyInfoReport(null) as ViewResult;
            //Assert
            Assert.IsNotNull(result);
        }
        [TestMethod]
        public void DriverMasterList()
        {
            //Arrange
            var controller = new ReportsController();
            //Act
            var result = controller.DriverMasterList() as ViewResult;
            //Assert
            Assert.IsNotNull(result);
        }
        [TestMethod]
        public void ODMasterList()
        {
            //Arrange
            var controller = new ReportsController();
            //Act
            var result = controller.ODMasterList() as ViewResult;
            //Assert
            Assert.IsNotNull(result);
        }
    }
}
