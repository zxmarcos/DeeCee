using System.Runtime.InteropServices;

namespace DeeCee.Core;

public class MemoryBlock : IDisposable
{
    private readonly byte[] _ptr;
    private GCHandle _handle;
    
    public MemoryBlock(int size)
    {
        _ptr = new byte[size];
        _handle = GCHandle.Alloc(_ptr, GCHandleType.Pinned);
    }

    public void LoadFrom(string path)
    {
        FileUtils.ReadFileToBuffer(path, _ptr);
    }
    
    public unsafe byte* Ptr => (byte*)_handle.AddrOfPinnedObject();
    
    public void Dispose()
    {
        _handle.Free();
    }
}