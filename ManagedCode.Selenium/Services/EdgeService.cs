using System.Diagnostics;
using System.Runtime.InteropServices;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;

namespace ManagedCode.Selenium.Services;

public class EdgeService : BaseBrowserService, IBrowserService
{
    public EdgeService(OSPlatform osPlatform, BrowserArchitecture browserArchitecture) : base(osPlatform, browserArchitecture)
    {
        _browserRootFolder = Path.Combine(Environment.CurrentDirectory, "edge", browserArchitecture.ToString());
        driverFileName = _osPlatform == OSPlatform.Windows ? "msedgedriver.exe" : "msedgedriver";
    }

    public async Task<IWebDriver> GetWebDriverAsync()
    {
        var browserPath = Path.Combine(_browserRootFolder, driverFileName);
        await DownloadBrowser();

        var options = new EdgeOptions();

        //options.BinaryLocation = browserPath;
        options.AcceptInsecureCertificates = true;
        options.PageLoadStrategy = PageLoadStrategy.Normal;
        options.AddArgument("--headless");
        //options.AddArgument("--start-maximized"); // open Browser in maximized mode
        //options.AddArgument("--disable-infobars"); // disabling infobars
        //options.AddArgument("--disable-extensions"); // disabling extensions
        //options.AddArgument("--disable-gpu"); // applicable to windows os only
        //options.AddArgument("--disable-dev-shm-usage"); // overcome limited resource problems
        options.AddArgument("--no-sandbox"); // Bypass OS security model
        //options.AddArgument("--remote-debugging-port=9222");
        //options.AddArgument("disable-logging");
        //options.AddArgument("--disable-setuid-sandbox");
        options.AddArgument("--log-level=WARNING");

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ||
            RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            var chmod = Process.Start("chmod", $"+x {browserPath}");
            await chmod.WaitForExitAsync();
        }

        return new EdgeDriver(_browserRootFolder, options);
    }

    public async Task DownloadBrowser()
    {
        var url = string.Empty;
        var version = await GetBrowserVersion("Edge"); //"99.0.1150.46";
        var baseUrl = $"https://msedgewebdriverstorage.blob.core.windows.net/edgewebdriver/{version}";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            url = _browserArchitecture == BrowserArchitecture.M1
                ? $"{baseUrl}/edgedriver_arm64.zip"
                : $"{baseUrl}/edgedriver_mac64.zip";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            url = _browserArchitecture == BrowserArchitecture.x32
                ? $"{baseUrl}/edgedriver_win32.zip"
                : $"{baseUrl}/edgedriver_win64.zip";
        }

        var file = await DownloadFile(url);

        if (File.Exists(driverFileName))
        {
            File.Delete(driverFileName);
        }

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