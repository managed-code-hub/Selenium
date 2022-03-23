using OpenQA.Selenium;

namespace ManagedCode.Selenium.Services;

public interface IBrowserService
{
    Task<IWebDriver> GetWebDriverAsync();
    Task DownloadBrowser();
}