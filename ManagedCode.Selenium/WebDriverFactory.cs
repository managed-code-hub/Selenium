using System.Runtime.InteropServices;
using ManagedCode.Selenium.Extensions;
using ManagedCode.Selenium.Services;
using OpenQA.Selenium;

namespace ManagedCode.Selenium;

public static class WebDriverFactory
{
    public static Task<IWebDriver> GetWebDriver(Browser browser)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return GetWebDriver(browser, OSPlatform.Windows, BrowserArchitecture.Auto);
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return GetWebDriver(browser, OSPlatform.Linux, BrowserArchitecture.Auto);
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return GetWebDriver(browser, OSPlatform.OSX, BrowserArchitecture.Auto);
        }

        throw new PlatformNotSupportedException("Unsupported platform " + RuntimeInformation.OSDescription);
    }

    public static Task<IWebDriver> GetWebDriver(Browser browser, OSPlatform osPlatform, BrowserArchitecture browserArchitecture)
    {
        IBrowserService browserService = browser switch
        {
            Browser.Chrome => new ChromeService(osPlatform, browserArchitecture.ToSpecificArchitecture()),
            Browser.Edge => new EdgeService(osPlatform, browserArchitecture.ToSpecificArchitecture()),
            Browser.Firefox => new FirefoxService(osPlatform, browserArchitecture.ToSpecificArchitecture()),
            _ => throw new NotSupportedException($"Unsupported browser: {browser}")
        };

        return browserService.GetWebDriverAsync();
    }
}