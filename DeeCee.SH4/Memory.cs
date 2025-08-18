namespace DeeCee.SH4;

public class Memory : IMemory
{
    private const int PageShift = 12;
    private const int PageSize = 1 << PageShift;
    private const int PageMask = PageSize - 1;
    private const int MaxHandler = 32;
    
    // Constantes para tipos de mapeamento
    [Flags]
    public enum MapType
    {
        None = 0,
        Read = 1,
        Write = 2,
        ReadWrite = Read | Write
    }

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
        const uint MaxPages = (0xFFFF_FFFFu >> PageShift) + 1;
        for (int i = 0; i < 8; i++)
        {
            _memoryMap[i] = new UIntPtr[MaxPages];
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
    
    // Método para mapear memória diretamente de um ponteiro
    public unsafe int MapMemory(void* pMemory, UInt32 startAddress, UInt32 endAddress, MapType mapType)
    {
        // Console.WriteLine($"MapMemory {(UIntPtr)pMemory:X} {startAddress:X} {endAddress:X} {mapType:X}");
        if (pMemory == null)
            return -1;
        
        if (startAddress > endAddress)
            return -1;

        // Calcula a página inicial e final
        var startPage = startAddress >> PageShift;
        var endPage = endAddress >> PageShift;
        var maxPages = (endPage - startPage) + 1;

        // Ponteiro base da memória
        var basePtr = new UIntPtr(pMemory);
 
        // Mapeia as páginas
        for (UInt32 i = 0; i < maxPages; i++)
        {
            var currentPage = startPage + i;
            var pageOffset = PageSize * i;
            var pagePtr = new UIntPtr((byte*)pMemory + pageOffset);
            
            // Mapeia para leitura se solicitado
            if ((mapType & MapType.Read) != 0)
            {
                _memoryMap[ReadMap8][currentPage] = pagePtr;
                _memoryMap[ReadMap16][currentPage] = pagePtr;
                _memoryMap[ReadMap32][currentPage] = pagePtr;
                _memoryMap[ReadMap64][currentPage] = pagePtr;
            }

            // Mapeia para escrita se solicitado
            if ((mapType & MapType.Write) != 0)
            {
                _memoryMap[WriteMap8][currentPage] = pagePtr;
                _memoryMap[WriteMap16][currentPage] = pagePtr;
                _memoryMap[WriteMap32][currentPage] = pagePtr;
                _memoryMap[WriteMap64][currentPage] = pagePtr;
            }
        }

        return 0; // Sucesso
    }

    // Método sobrecarregado para aceitar IntPtr (mais comum em C#)
    public int MapMemory(IntPtr pMemory, UInt32 startAddress, UInt32 endAddress, MapType mapType)
    {
        unsafe
        {
            return MapMemory(pMemory.ToPointer(), startAddress, endAddress, mapType);
        }
    }

    // Método para desmapear uma região de memória
    public int UnmapMemory(UInt32 startAddress, UInt32 endAddress, MapType mapType)
    {
        if (startAddress > endAddress)
            return -1;

        var startPage = startAddress >> PageShift;
        var endPage = endAddress >> PageShift;
        var maxPages = (endPage - startPage) + 1;

        for (UInt32 i = 0; i < maxPages; i++)
        {
            var currentPage = startPage + i;

            // Desmapeia leitura se solicitado
            if ((mapType & MapType.Read) != 0)
            {
                _memoryMap[ReadMap8][currentPage] = UIntPtr.Zero;
                _memoryMap[ReadMap16][currentPage] = UIntPtr.Zero;
                _memoryMap[ReadMap32][currentPage] = UIntPtr.Zero;
                _memoryMap[ReadMap64][currentPage] = UIntPtr.Zero;
            }

            // Desmapeia escrita se solicitado
            if ((mapType & MapType.Write) != 0)
            {
                _memoryMap[WriteMap8][currentPage] = UIntPtr.Zero;
                _memoryMap[WriteMap16][currentPage] = UIntPtr.Zero;
                _memoryMap[WriteMap32][currentPage] = UIntPtr.Zero;
                _memoryMap[WriteMap64][currentPage] = UIntPtr.Zero;
            }
        }

        return 0;
    }
    

    public unsafe byte Read8(UInt32 address)
    {
        var map = _memoryMap[ReadMap8];
        var idx = map[address >> PageShift].ToUInt32();
        if (idx < MaxHandler)
        {
            if (_read8[idx] != null)
                return _read8[idx]!(address);
            Console.WriteLine($"Read8 Invalid {address:X8}");
            return 0;
        }

        var ptr = map[address >> PageShift].ToPointer();
        return *((byte*)ptr + (address & PageMask));
    }

    public unsafe UInt16 Read16(UInt32 address)
    {
        var map = _memoryMap[ReadMap16];
        var idx = map[address >> PageShift].ToUInt64();
        if (idx < MaxHandler)
        {
            if (_read16[idx] != null)
                return _read16[idx]!(address);
            Console.WriteLine($"Read16 Invalid {address:X8}");
            return 0;
        }

        var ptr = map[address >> PageShift].ToPointer();
        return *((UInt16*)((byte*)ptr + (address & PageMask)));
    }

    public unsafe UInt32 Read32(UInt32 address)
    {
        var map = _memoryMap[ReadMap32];
        var idx = map[address >> PageShift].ToUInt64();
        if (idx < MaxHandler)
        {
            if (_read32[idx] != null)
                return _read32[idx]!(address);
            Console.WriteLine($"Read32 Invalid {address:X8}");
            return 0;
        }

        var ptr = map[address >> PageShift].ToPointer();
        return *((UInt32*)((byte*)ptr + (address & PageMask)));
    }

    public unsafe UInt64 Read64(UInt32 address)
    {
        var map = _memoryMap[ReadMap64];
        var idx = map[address >> PageShift].ToUInt64();
        if (idx < MaxHandler)
        {
            if (_read64[idx] != null)
                return _read64[idx]!(address);
            Console.WriteLine($"Read64 Invalid {address:X8}");
            return 0;
        }

        var ptr = map[address >> PageShift].ToPointer();
        return *((UInt64*)((byte*)ptr + (address & PageMask)));
    }

    public unsafe void Write8(UInt32 address, byte value)
    {
        var map = _memoryMap[WriteMap8];
        var idx = map[address >> PageShift].ToUInt64();
        if (idx < MaxHandler)
        {
            if (_write8[idx] != null)
                _write8[idx]!(address, value);
            Console.WriteLine($"Write8 Invalid {address:X8} {value:X}");
            return;
        }

        var ptr = map[address >> PageShift].ToPointer();
        *((byte*)ptr + (address & PageMask)) = value;
    }

    public unsafe void Write16(UInt32 address, UInt16 value)
    {
        var map = _memoryMap[WriteMap16];
        var idx = map[address >> PageShift].ToUInt64();
        if (idx < MaxHandler)
        {
            if (_write16[idx] != null)
                _write16[idx]!(address, value);
            Console.WriteLine($"Write16 Invalid {address:X8} {value:X}");
            return;
        }

        var ptr = map[address >> PageShift].ToPointer();
        *((UInt16*)((byte*)ptr + (address & PageMask))) = value;
    }

    public unsafe void Write32(UInt32 address, UInt32 value)
    {
        var map = _memoryMap[WriteMap32];
        var idx = map[address >> PageShift].ToUInt64();
        if (idx < MaxHandler)
        {
            if (_write32[idx] != null)
                _write32[idx]!(address, value);
            Console.WriteLine($"Write32 Invalid {address:X8} {value:X}");
            return;
        }

        var ptr = map[address >> PageShift].ToPointer();
        *((UInt32*)((byte*)ptr + (address & PageMask))) = value;
    }

    public unsafe void Write64(UInt32 address, UInt64 value)
    {
        var map = _memoryMap[WriteMap64];
        var idx = map[address >> PageShift].ToUInt64();
        if (idx < MaxHandler)
        {
            if (_write64[idx] != null)
                _write64[idx]!(address, value);
            Console.WriteLine($"Write64 Invalid {address:X8} {value:X}");
            return;
        }

        var ptr = map[address >> PageShift].ToPointer();
        *((UInt64*)((byte*)ptr + (address & PageMask))) = value;
    }
}