using System.Diagnostics;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Text;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using Microsoft.Win32;
using OpenQA.Selenium;

namespace ManagedCode.Selenium.Services;

public abstract class BaseBrowserService
{
    protected BrowserArchitecture _browserArchitecture;
    protected string _browserRootFolder;
    protected OSPlatform _osPlatform;
    protected string driverFileName;

    public BaseBrowserService(OSPlatform osPlatform, BrowserArchitecture browserArchitecture)
    {
        _osPlatform = osPlatform;
        _browserArchitecture = browserArchitecture;
    }

    protected static async Task<string> DownloadFile(string url)
    {
        using var response = await new HttpClient().GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
        using var streamToReadFrom = await response.Content.ReadAsStreamAsync();
        var fileToWriteTo = Path.GetTempFileName();
        fileToWriteTo += Path.GetExtension(url);
        using Stream streamToWriteTo = File.Open(fileToWriteTo, FileMode.Create);
        await streamToReadFrom.CopyToAsync(streamToWriteTo);
        return fileToWriteTo;
    }

    protected void UnZipArchive(string path, string destination)
    {
        if (!Directory.Exists(destination))
        {
            Directory.CreateDirectory(destination);
        }

        foreach (var file in Directory.EnumerateFiles(destination))
        {
            File.Delete(file);
        }

        var fileInfo = new FileInfo(path);
        if (fileInfo.Extension == ".zip")
        {
            UnZip(path, destination);
        }
        else
        {
            UnZipTgz(path, destination);
        }
    }

    private void UnZip(string path, string destination)
    {
        using var zip = ZipFile.Open(path, ZipArchiveMode.Read);
        foreach (var entry in zip.Entries)
        {
            if (entry.Name == driverFileName)
            {
                entry.ExtractToFile(Path.Combine(destination, driverFileName), true);
            }
        }
    }

    private void UnZipTgz(string gzArchiveName, string destination)
    {
        using (var inStream = File.OpenRead(gzArchiveName))
        {
            using (var gzipStream = new GZipInputStream(inStream))
            {
                using (var tarArchive = TarArchive.CreateInputTarArchive(gzipStream, Encoding.Default))
                {
                    tarArchive.ExtractContents(destination);
                }
            }
        }
    }

    protected async Task<string> GetBrowserVersion(string browserName)
    {
        if (_osPlatform == OSPlatform.Linux)
        {
            return await GetBrowserVersionLinux(browserName);
        }

        if (_osPlatform == OSPlatform.Windows)
        {
            return GetBrowserVersionWindows(browserName);
        }

        if (_osPlatform == OSPlatform.OSX)
        {
            return await GetBrowserVersionMacOs(browserName);
        }

        return null;
    }

    private async Task<string> GetBrowserVersionLinux(string executableFileName)
    {
        try
        {
            var outputVersion = await GetVersionFromProcess(executableFileName, "--version");
            return outputVersion;
        }
        catch (Exception e)
        {
            throw new NotFoundException($"Browser {executableFileName} not found. {Environment.OSVersion.Platform}", e);
        }
    }

    private async Task<string> GetBrowserVersionMacOs(string executableFileName)
    {
        try
        {
            var executableFilePath = $"/Applications/{executableFileName}.app/Contents/MacOS/{executableFileName}";
            var outputVersion = await GetVersionFromProcess(executableFilePath, "-- version");
            return outputVersion.Replace($"{executableFileName} ", "");
        }
        catch (Exception e)
        {
            throw new NotFoundException($"Browser {executableFileName} not found. {Environment.OSVersion.Platform}", e);
        }
    }

    private string GetBrowserVersionWindows(string executableFileName)
    {
        var currentUser = $"HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\CurrentVersion\\App Paths\\{executableFileName}";
        var localMachine = $"HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\App Paths\\{executableFileName}";

        var currentUserPath = Registry.GetValue(currentUser, "", null);
        if (currentUserPath != null)
        {
            return FileVersionInfo.GetVersionInfo(currentUserPath.ToString()).FileVersion;
        }

        var localMachinePath = Registry.GetValue(localMachine, "", null);
        if (localMachinePath != null)
        {
            return FileVersionInfo.GetVersionInfo(localMachinePath.ToString()).FileVersion;
        }

        return null;
    }

    private async Task<string> GetVersionFromProcess(string executableFileName, string arguments)
    {
        var process = Process.Start(new ProcessStartInfo
        {
            FileName = executableFileName,
            Arguments = arguments,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        });

        if (process == null)
        {
            throw new Exception("The process did not start");
        }

        var output = await process.StandardOutput.ReadToEndAsync();
        var error = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();
        process.Kill();

        if (!string.IsNullOrEmpty(error))
        {
            throw new Exception(error);
        }

        return output;
    }
}