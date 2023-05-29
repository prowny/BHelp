using BHelp.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web.Mvc;

namespace BHelpTests
{
    [TestClass]
    public class UsersControllerTests
    {
        [TestMethod]
        public void VolunteerRolesAndStartEndDates()
        {
            //Arrange
            var controller = new UsersController();
            //Act
            var result = controller.VolunteerDatesReport() as ViewResult;
            //Assert
            Assert.IsNotNull(result);
        }
    }
}
