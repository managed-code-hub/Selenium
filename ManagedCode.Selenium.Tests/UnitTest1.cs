using System.Threading.Tasks;
using Xunit;

namespace ManagedCode.Selenium.Tests;

public class UnitTest1
{
    [Fact]
    public async Task FirefoxTest()
    {
        var driver = await WebDriverFactory.GetWebDriver(Browser.Firefox);
        driver.Navigate().GoToUrl("https://google.com");
    }

    [Fact]
    public async Task ChromeTest()
    {
        var driver = await WebDriverFactory.GetWebDriver(Browser.Chrome);
        driver.Navigate().GoToUrl("https://google.com");
    }

    [Fact]
    public async Task EdgeTest()
    {
        var driver = await WebDriverFactory.GetWebDriver(Browser.Edge);
        driver.Navigate().GoToUrl("https://google.com");
    }
}