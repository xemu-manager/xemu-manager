using System.Buffers.Binary;
using System.Text;

namespace XemuManager.Core.LibraryManager;

/// <summary>
/// Reads game metadata from an Xbox disc image (XISO or Redump) by walking
/// the XDVDFS file system to default.xbe and reading its certificate.
/// </summary>
public class XisoReader : IGameMetadataReader
{
    private const int SectorSize = 2048;
    private const long VolumeDescriptorOffset = 32 * SectorSize; // 0x10000
    private static readonly byte[] Magic = "MICROSOFT*XBOX*MEDIA"u8.ToArray();

    // Where the game partition starts inside each kind of image.
    private static readonly (ImageType Type, long Offset)[] PartitionOffsets =
    [
        (ImageType.Xiso, 0x0),
        (ImageType.RedumpXgd1, 0x18300000),
        (ImageType.RedumpXgd2, 0xFD90000),
        (ImageType.RedumpXgd3, 0x2080000),
    ];

    private const byte DirectoryAttribute = 0x10;

    static GameMetaData? IGameMetadataReader.Read(string filePath) => Read(filePath);

    public static XboxMetaData? Read(string isoPath)
    {
        using var stream = File.OpenRead(isoPath);

        // 1. Sector 32: find the signature and the image type
        if (FindPartition(stream) is not { } partition)
            return null;
        var (type, partitionOffset) = partition;

        // 2. Volume descriptor: where the root table is
        var descriptor = ReadBytes(stream, partitionOffset + VolumeDescriptorOffset, SectorSize);
        var rootSector = BinaryPrimitives.ReadUInt32LittleEndian(descriptor.AsSpan(0x14));
        var rootSize = BinaryPrimitives.ReadUInt32LittleEndian(descriptor.AsSpan(0x18));

        // 3. Root table: find the default.xbe card
        var rootTable = ReadBytes(stream, partitionOffset + (long)rootSector * SectorSize, (int)rootSize);
        if (FindEntry(rootTable, "default.xbe") is not { } xbe)
            return null;
        var (xbeSector, xbeSize) = xbe;

        // 4. default.xbe: read the certificate
        var xbeOffset = partitionOffset + (long)xbeSector * SectorSize;
        return ReadXbeCertificate(stream, xbeOffset, xbeSize, isoPath, type);
    }

    private static (ImageType, long)? FindPartition(Stream stream)
    {
        foreach (var (type, offset) in PartitionOffsets)
        {
            var descriptorStart = offset + VolumeDescriptorOffset;
            if (descriptorStart + SectorSize > stream.Length)
                continue;

            var descriptor = ReadBytes(stream, descriptorStart, SectorSize);
            // The signature is at the start and repeated at the end of the sector
            if (descriptor.AsSpan(0, Magic.Length).SequenceEqual(Magic) &&
                descriptor.AsSpan(0x7EC, Magic.Length).SequenceEqual(Magic))
                return (type, offset);
        }
        return null;
    }

    /// <summary>
    /// Binary search through a directory table. Each entry:
    /// left(u16) right(u16) sector(u32) size(u32) attributes(u8) nameLength(u8) name
    /// Left/right are offsets in 4-byte units from the start of the table; 0 = none.
    /// </summary>
    private static (uint Sector, uint Size)? FindEntry(byte[] table, string name)
    {
        var offset = 0;
        // Guard against loops in a corrupt image
        for (var steps = 0; steps < 1024; steps++)
        {
            if (offset + 14 > table.Length)
                return null;

            var entry = table.AsSpan(offset);
            var left = BinaryPrimitives.ReadUInt16LittleEndian(entry);
            var right = BinaryPrimitives.ReadUInt16LittleEndian(entry[2..]);
            var sector = BinaryPrimitives.ReadUInt32LittleEndian(entry[4..]);
            var size = BinaryPrimitives.ReadUInt32LittleEndian(entry[8..]);
            var attributes = entry[12];
            var nameLength = entry[13];
            if (offset + 14 + nameLength > table.Length)
                return null;
            var entryName = Encoding.ASCII.GetString(entry.Slice(14, nameLength));

            var compare = string.Compare(name, entryName, StringComparison.OrdinalIgnoreCase);
            if (compare == 0)
                return (attributes & DirectoryAttribute) == 0 ? (sector, size) : null;

            var next = compare < 0 ? left : right;
            if (next == 0)
                return null;
            offset = next * 4;
        }
        return null;
    }

    private static XboxMetaData? ReadXbeCertificate(
        Stream stream, long xbeOffset, uint xbeSize, string isoPath, ImageType type)
    {
        // XBE header: "XBEH", base address at 0x104, certificate address at 0x118
        var header = ReadBytes(stream, xbeOffset, 0x178);
        if (!header.AsSpan(0, 4).SequenceEqual("XBEH"u8))
            return null;

        var baseAddress = BinaryPrimitives.ReadUInt32LittleEndian(header.AsSpan(0x104));
        var certAddress = BinaryPrimitives.ReadUInt32LittleEndian(header.AsSpan(0x118));
        // Addresses are in memory; subtract the base to get the position in the file
        var certFileOffset = (long)certAddress - baseAddress;
        if (certFileOffset < 0 || certFileOffset + 0xB0 > xbeSize)
            return null;

        var cert = ReadBytes(stream, xbeOffset + certFileOffset, 0xB0);
        var titleId = BinaryPrimitives.ReadUInt32LittleEndian(cert.AsSpan(0x08));
        // Title name: 40 UTF-16 characters, padded with zeros
        var title = Encoding.Unicode.GetString(cert, 0x0C, 80).TrimEnd('\0');
        var region = (GameRegion)BinaryPrimitives.ReadUInt32LittleEndian(cert.AsSpan(0xA0));
        var discNumber = (int)BinaryPrimitives.ReadUInt32LittleEndian(cert.AsSpan(0xA8));
        var version = BinaryPrimitives.ReadUInt32LittleEndian(cert.AsSpan(0xAC));

        return new XboxMetaData(isoPath, type, titleId.ToString("X8"), title, region, version, discNumber);
    }

    private static byte[] ReadBytes(Stream stream, long offset, int count)
    {
        var buffer = new byte[count];
        stream.Seek(offset, SeekOrigin.Begin);
        stream.ReadExactly(buffer);
        return buffer;
    }
}
