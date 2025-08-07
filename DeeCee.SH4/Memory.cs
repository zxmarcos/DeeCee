namespace DeeCee.SH4;

public class Memory : IMemory
{
    private const int PageShift = 12;
    private const int PageSize = 1 << PageShift;
    private const int PageMask = PageSize - 1;
    private const int MaxHandler = 32;

    private delegate byte MemoryRead8Handler(UInt32 address);

    private delegate UInt16 MemoryRead16Handler(UInt32 address);

    private delegate UInt32 MemoryRead32Handler(UInt32 address);

    private delegate UInt64 MemoryRead64Handler(UInt32 address);

    private delegate void MemoryWrite8Handler(UInt32 address, byte value);

    private delegate void MemoryWrite16Handler(UInt32 address, UInt16 value);

    private delegate void MemoryWrite32Handler(UInt32 address, UInt32 value);

    private delegate void MemoryWrite64Handler(UInt32 address, UInt64 value);

    private const int ReadMap8 = 0;
    private const int ReadMap16 = 1;
    private const int ReadMap32 = 2;
    private const int ReadMap64 = 3;
    private const int WriteMap8 = 4;
    private const int WriteMap16 = 5;
    private const int WriteMap32 = 6;
    private const int WriteMap64 = 7;

    private readonly UIntPtr[][] _memoryMap;
    private readonly MemoryRead8Handler?[] _read8;
    private readonly MemoryRead16Handler?[] _read16;
    private readonly MemoryRead32Handler?[] _read32;
    private readonly MemoryRead64Handler?[] _read64;

    private readonly MemoryWrite8Handler?[] _write8;
    private readonly MemoryWrite16Handler?[] _write16;
    private readonly MemoryWrite32Handler?[] _write32;
    private readonly MemoryWrite64Handler?[] _write64;

    public Memory()
    {
        _memoryMap = new UIntPtr[8][];
        for (int i = 0; i < 8; i++)
        {
            _memoryMap[i] = new UIntPtr[1 << PageShift];
        }

        _read8 = new MemoryRead8Handler[MaxHandler];
        _read16 = new MemoryRead16Handler[MaxHandler];
        _read32 = new MemoryRead32Handler[MaxHandler];
        _read64 = new MemoryRead64Handler[MaxHandler];
        _write8 = new MemoryWrite8Handler[MaxHandler];
        _write16 = new MemoryWrite16Handler[MaxHandler];
        _write32 = new MemoryWrite32Handler[MaxHandler];
        _write64 = new MemoryWrite64Handler[MaxHandler];
    }

    public unsafe byte Read8(UInt32 address)
    {
        var map = _memoryMap[ReadMap8];
        var idx = map[address >> PageShift].ToUInt32();
        if (idx < MaxHandler)
        {
            if (_read8[idx] != null)
                return _read8[idx]!(address);
            return 0;
        }

        var ptr = map[address >> PageShift].ToPointer();
        return *((byte*)ptr + (address & PageMask));
    }

    public unsafe UInt16 Read16(UInt32 address)
    {
        var map = _memoryMap[ReadMap16];
        var idx = map[address >> PageShift].ToUInt32();
        if (idx < MaxHandler)
        {
            if (_read16[idx] != null)
                return _read16[idx]!(address);
            return 0;
        }

        var ptr = map[address >> PageShift].ToPointer();
        return *((UInt16*)ptr + (address & PageMask));
    }

    public unsafe UInt32 Read32(UInt32 address)
    {
        var map = _memoryMap[ReadMap32];
        var idx = map[address >> PageShift].ToUInt32();
        if (idx < MaxHandler)
        {
            if (_read32[idx] != null)
                return _read32[idx]!(address);
            return 0;
        }

        var ptr = map[address >> PageShift].ToPointer();
        return *((UInt32*)ptr + (address & PageMask));
    }

    public unsafe UInt64 Read64(UInt32 address)
    {
        var map = _memoryMap[ReadMap64];
        var idx = map[address >> PageShift].ToUInt32();
        if (idx < MaxHandler)
        {
            if (_read64[idx] != null)
                return _read64[idx]!(address);
            return 0;
        }

        var ptr = map[address >> PageShift].ToPointer();
        return *((UInt64*)ptr + (address & PageMask));
    }

    public unsafe void Write8(UInt32 address, byte value)
    {
        var map = _memoryMap[WriteMap8];
        var idx = map[address >> PageShift].ToUInt32();
        if (idx < MaxHandler)
        {
            if (_write8[idx] != null)
                _write8[idx]!(address, value);
            return;
        }

        var ptr = map[address >> PageShift].ToPointer();
        *((byte*)ptr + (address & PageMask)) = value;
    }

    public unsafe void Write16(UInt32 address, UInt16 value)
    {
        var map = _memoryMap[WriteMap16];
        var idx = map[address >> PageShift].ToUInt32();
        if (idx < MaxHandler)
        {
            if (_write16[idx] != null)
                _write16[idx]!(address, value);
            return;
        }

        var ptr = map[address >> PageShift].ToPointer();
        *((UInt16*)ptr + (address & PageMask)) = value;
    }

    public unsafe void Write32(UInt32 address, UInt32 value)
    {
        var map = _memoryMap[WriteMap32];
        var idx = map[address >> PageShift].ToUInt32();
        if (idx < MaxHandler)
        {
            if (_write32[idx] != null)
                _write32[idx]!(address, value);
            return;
        }

        var ptr = map[address >> PageShift].ToPointer();
        *((UInt32*)ptr + (address & PageMask)) = value;
    }

    public unsafe void Write64(UInt32 address, UInt64 value)
    {
        var map = _memoryMap[WriteMap64];
        var idx = map[address >> PageShift].ToUInt32();
        if (idx < MaxHandler)
        {
            if (_write64[idx] != null)
                _write64[idx]!(address, value);
            return;
        }

        var ptr = map[address >> PageShift].ToPointer();
        *((UInt64*)ptr + (address & PageMask)) = value;
    }
}