
using NUnit.Framework;

namespace SearchEngine.Tests.Controllers
{
    [Category("Integration")]
    [TestFixture]
    public class SearchControllerTests
    {
        [Test, Ignore("Controller integration requires ASP.NET host and DB storage; provide connection string and WebApplicationFactory to enable.")]
        public void Controller_Endpoints_Available()
        {
            Assert.Ignore("Integration test placeholder: configure WebApplicationFactory and MySQL to run.");
        }
    }
}
