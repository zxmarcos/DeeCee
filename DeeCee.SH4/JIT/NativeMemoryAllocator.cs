using System;
using System.Runtime.InteropServices;

namespace DeeCee.SH4.JIT;

public class NativeMemoryAllocator
{
    private const int PROT_READ = 0x1;
    private const int PROT_WRITE = 0x2;
    private const int PROT_EXEC = 0x4;
    private const int MAP_PRIVATE = 0x02;

    private const uint MEM_COMMIT = 0x1000;
    private const uint MEM_RESERVE = 0x2000;
    private const uint PAGE_EXECUTE_READWRITE = 0x40;
    private const uint MEM_RELEASE = 0x8000;

    [DllImport("libc", SetLastError = true)]
    private static extern IntPtr mmap(IntPtr addr, UIntPtr length, int prot, int flags, int fd, IntPtr offset);

    [DllImport("libc", SetLastError = true)]
    private static extern int munmap(IntPtr addr, UIntPtr length);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr VirtualAlloc(IntPtr lpAddress, UIntPtr dwSize, uint flAllocationType, uint flProtect);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool VirtualFree(IntPtr lpAddress, UIntPtr dwSize, uint dwFreeType);

    public static IntPtr Allocate(int size)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return VirtualAlloc(IntPtr.Zero, (UIntPtr)size, MEM_COMMIT | MEM_RESERVE, PAGE_EXECUTE_READWRITE);
        }
        else
        {
             int mapAnon = RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? 0x1000 : 0x20;

             var ptr = mmap(IntPtr.Zero, (UIntPtr)size, PROT_READ | PROT_WRITE | PROT_EXEC, MAP_PRIVATE | mapAnon, -1, IntPtr.Zero);
             if (ptr == new IntPtr(-1))
             {
                 throw new OutOfMemoryException($"mmap failed with error {Marshal.GetLastPInvokeError()}");
             }
             return ptr;
        }
    }

    public static void Free(IntPtr ptr, int size)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            VirtualFree(ptr, UIntPtr.Zero, MEM_RELEASE);
        }
        else
        {
            munmap(ptr, (UIntPtr)size);
        }
    }
}
