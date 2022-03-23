using System.Runtime.InteropServices;

namespace ManagedCode.Selenium.Extensions;

public static class BrowserArchitectureExtensions
{
    public static BrowserArchitecture ToSpecificArchitecture(this BrowserArchitecture architecture)
    {
        if (architecture == BrowserArchitecture.Auto)
        {
            return RuntimeInformation.ProcessArchitecture switch
            {
                Architecture.X86 => BrowserArchitecture.x32,
                Architecture.X64 => BrowserArchitecture.x64,
                Architecture.Arm64 => BrowserArchitecture.M1,
                _ => architecture
            };
        }

        return architecture;
    }
}