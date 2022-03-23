using System.Diagnostics;
using System.Runtime.InteropServices;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace ManagedCode.Selenium.Services;

public class ChromeService : BaseBrowserService, IBrowserService
{
    public ChromeService(OSPlatform osPlatform, BrowserArchitecture browserArchitecture) : base(osPlatform, browserArchitecture)
    {
        _browserRootFolder = Path.Combine(Environment.CurrentDirectory, "chrome", browserArchitecture.ToString());
        driverFileName = _osPlatform == OSPlatform.Windows ? "chromedriver.exe" : "chromedriver";
    }

    public async Task<IWebDriver> GetWebDriverAsync()
    {
        var browserPath = Path.Combine(_browserRootFolder, driverFileName);
        await DownloadBrowser();

        var options = new ChromeOptions();

        options.BinaryLocation = browserPath;
        options.AcceptInsecureCertificates = true;
        options.PageLoadStrategy = PageLoadStrategy.Normal;
        //options.AddArgument("--headless");
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

        return new ChromeDriver(_browserRootFolder, options);
    }

    public async Task DownloadBrowser()
    {
        var url = string.Empty;
        var version = await GetBrowserVersion("Chrome"); //"100.0.4896.20";

        var baseUrl = $"https://chromedriver.storage.googleapis.com/{version}";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            url = _browserArchitecture == BrowserArchitecture.M1
                ? $"{baseUrl}/chromedriver_mac64_m1.zip"
                : $"{baseUrl}/chromedriver_mac64.zip";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            url = $"{baseUrl}/chromedriver_linux64.zip";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            url = $"{baseUrl}/chromedriver_win32.zip";
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