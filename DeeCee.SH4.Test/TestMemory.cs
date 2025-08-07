namespace DeeCee.SH4.Test;

using System;

public class TestMemory : IMemory
{
    private readonly byte[] _buffer;
    private readonly uint _size;

    public TestMemory(uint size)
    {
        _size = size;
        _buffer = new byte[size];
    }

    public uint Size => _size;

    // Método auxiliar para verificar se o endereço está dentro dos limites
    private void ValidateAddress(uint address, int dataSize)
    {
        if (address + dataSize > _size)
        {
            throw new ArgumentOutOfRangeException(nameof(address), 
                $"Address {address:X8} + {dataSize} bytes exceeds memory size {_size}");
        }
    }

    public unsafe byte Read8(uint address)
    {
        ValidateAddress(address, 1);
        return _buffer[address];
    }

    public unsafe ushort Read16(uint address)
    {
        ValidateAddress(address, 2);
        fixed (byte* ptr = &_buffer[address])
        {
            return *(ushort*)ptr;
        }
    }

    public unsafe uint Read32(uint address)
    {
        ValidateAddress(address, 4);
        fixed (byte* ptr = &_buffer[address])
        {
            return *(uint*)ptr;
        }
    }

    public unsafe ulong Read64(uint address)
    {
        ValidateAddress(address, 8);
        fixed (byte* ptr = &_buffer[address])
        {
            return *(ulong*)ptr;
        }
    }

    public unsafe void Write8(uint address, byte value)
    {
        ValidateAddress(address, 1);
        _buffer[address] = value;
    }

    public unsafe void Write16(uint address, ushort value)
    {
        ValidateAddress(address, 2);
        fixed (byte* ptr = &_buffer[address])
        {
            *(ushort*)ptr = value;
        }
    }

    public unsafe void Write32(uint address, uint value)
    {
        ValidateAddress(address, 4);
        fixed (byte* ptr = &_buffer[address])
        {
            *(uint*)ptr = value;
        }
    }

    public unsafe void Write64(uint address, ulong value)
    {
        ValidateAddress(address, 8);
        fixed (byte* ptr = &_buffer[address])
        {
            *(ulong*)ptr = value;
        }
    }

    // Métodos auxiliares para testes
    public void Clear()
    {
        Array.Clear(_buffer, 0, _buffer.Length);
    }

    public void Fill(byte value)
    {
        Array.Fill(_buffer, value);
    }

    public byte[] GetBuffer()
    {
        return (byte[])_buffer.Clone();
    }

    public void LoadBuffer(byte[] data)
    {
        if (data.Length > _size)
        {
            throw new ArgumentException($"Data size {data.Length} exceeds memory size {_size}");
        }
        
        Array.Copy(data, 0, _buffer, 0, data.Length);
    }

    // Método para dump da memória para debug
    public string HexDump(uint startAddress = 0, uint length = 0)
    {
        if (length == 0) length = _size;
        if (startAddress + length > _size) length = _size - startAddress;

        var result = new System.Text.StringBuilder();
        
        for (uint i = 0; i < length; i += 16)
        {
            result.AppendFormat("{0:X8}: ", startAddress + i);
            
            // Hex bytes
            for (uint j = 0; j < 16 && i + j < length; j++)
            {
                result.AppendFormat("{0:X2} ", _buffer[startAddress + i + j]);
            }
            
            // Padding
            for (uint j = length - i; j < 16; j++)
            {
                result.Append("   ");
            }
            
            result.Append(" ");
            
            // ASCII representation
            for (uint j = 0; j < 16 && i + j < length; j++)
            {
                byte b = _buffer[startAddress + i + j];
                result.Append(b >= 32 && b <= 126 ? (char)b : '.');
            }
            
            result.AppendLine();
        }
        
        return result.ToString();
    }
}