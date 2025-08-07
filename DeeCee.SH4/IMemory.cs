namespace DeeCee.SH4;

public interface IMemory
{
    unsafe byte Read8(UInt32 address) { throw new NotImplementedException(); }
    unsafe UInt16 Read16(UInt32 address) { throw new NotImplementedException(); }
    unsafe UInt32 Read32(UInt32 address) { throw new NotImplementedException(); }
    unsafe UInt64 Read64(UInt32 address) { throw new NotImplementedException(); }
    unsafe void Write8(UInt32 address, byte value) { throw new NotImplementedException(); }
    unsafe void Write16(UInt32 address, UInt16 value) { throw new NotImplementedException(); }
    unsafe void Write32(UInt32 address, UInt32 value) { throw new NotImplementedException(); }
    unsafe void Write64(UInt32 address, UInt64 value) { throw new NotImplementedException();}
}