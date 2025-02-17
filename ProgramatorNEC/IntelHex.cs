using System.Collections.Generic;
using System.IO;
using System.Net;

class IntelHex
{
    public List<IntelHex.Entry> Entries = new List<Entry>();

    public static IntelHex LoadIntelHex(string filePath)
    {
        return new IntelHex(File.ReadAllLines(filePath));
    }

    public IntelHex(string[] hexStrings)
    {
        foreach (string hexString in hexStrings)
        {
            IntelHex.Entry entry = IntelHex.Entry.ParseHexEntry(hexString);
            if (entry != null)
            {
                this.Entries.Add(entry);
            }
        }
    }

    public bool calcCRC()
    {
        foreach (IntelHex.Entry entry in this.Entries)
        {
            if (!entry.calcCRC()) return false;
        }
        return true;
    }

    public int getHexSize()
    {
        int size = 0;
        foreach (IntelHex.Entry entry in this.Entries)
        {
            int end_address = entry.address + entry.data_size;
            if (end_address > size)
            {
                size = end_address;
            }
        }
        return size;
    }

    public class Entry
    {

        public byte data_size;
        public ushort address;
        public byte type;
        public byte crc;
        public byte[] data;

        public Entry(byte data_size, ushort address, byte type, byte crc, byte[] data) {
            this.data_size = data_size;
            this.address = address;
            this.type = type;
            this.crc = crc;
            this.data = data;
        }

    public static Entry ParseHexEntry(string hexEntryString)
    {
        if (hexEntryString.Length == 0 || hexEntryString[0] != ':') return null;
        byte data_size = byte.Parse(hexEntryString.Substring(1, 2), System.Globalization.NumberStyles.HexNumber);
        byte[] data = new byte[data_size];
        for (int i = 0; i < data_size; i++)
        {
            data[i] = byte.Parse(hexEntryString.Substring(i * 2 + 9, 2), System.Globalization.NumberStyles.HexNumber);
        }
        return new Entry(
            data_size,
            ushort.Parse(hexEntryString.Substring(3, 4), System.Globalization.NumberStyles.HexNumber),
            byte.Parse(hexEntryString.Substring(7, 2), System.Globalization.NumberStyles.HexNumber),
            byte.Parse(hexEntryString.Substring(data_size * 2 + 9, 2), System.Globalization.NumberStyles.HexNumber),
            data
        );
    }

    public bool calcCRC()
    {
        byte sum = (byte)(data_size + (address & 0xFF) + ((address >> 8) & 0xFF) + type + crc);
        foreach (byte b in data)
        {
            sum += b;
        }
        return sum == 0;
    }
}
}