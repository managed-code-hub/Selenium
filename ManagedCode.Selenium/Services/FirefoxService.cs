using System.Diagnostics;
using System.Runtime.InteropServices;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;

namespace ManagedCode.Selenium.Services;

public class FirefoxService : BaseBrowserService, IBrowserService
{
    public FirefoxService(OSPlatform osPlatform, BrowserArchitecture browserArchitecture) : base(osPlatform, browserArchitecture)
    {
        _browserRootFolder = Path.Combine(Environment.CurrentDirectory, "firefox", browserArchitecture.ToString());
        _browserRootFolder = Environment.CurrentDirectory;
        driverFileName = _osPlatform == OSPlatform.Windows ? "geckodriver.exe" : "geckodriver";
    }

    public async Task<IWebDriver> GetWebDriverAsync()
    {
        var browserPath = Path.Combine(_browserRootFolder, driverFileName);
        await DownloadBrowser();

        var options = new FirefoxOptions();
        options.BrowserExecutableLocation = browserPath;
        options.AcceptInsecureCertificates = true;
        options.PageLoadStrategy = PageLoadStrategy.Normal;
        options.AddArgument("--headless");

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ||
            RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            var chmod = Process.Start("chmod", $"+x {browserPath}");
            await chmod.WaitForExitAsync();
        }

        Environment.SetEnvironmentVariable("webdriver.firefox.bin", browserPath, EnvironmentVariableTarget.Process);
        return new FirefoxDriver(_browserRootFolder, options);
    }

    public async Task DownloadBrowser()
    {
        var url = string.Empty;
        var version = "v0.30.0";

        var baseUrl = $"https://github.com/mozilla/geckodriver/releases/download/{version}/geckodriver-{version}";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            url = _browserArchitecture == BrowserArchitecture.M1
                ? $"{baseUrl}-macos-aarch64.tar.gz"
                : $"{baseUrl}-macos.tar.gz";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            url = _browserArchitecture == BrowserArchitecture.x32
                ? $"{baseUrl}-linux32.tar.gz"
                : $"{baseUrl}-linux64.tar.gz";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            url = _browserArchitecture == BrowserArchitecture.x32
                ? $"{baseUrl}-win32.zip"
                : $"{baseUrl}-win64.zip";
        }

        var file = await DownloadFile(url);
        try
        {
            UnZipArchive(file, _browserRootFolder);
        }
        finally
        {
            File.Delete(file);
        }
    }
}