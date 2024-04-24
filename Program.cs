
namespace HFSLIFWriter
{
    internal struct LifVolumeHeader
    {
        // 42-byte record (See https://www.hp9845.net/9845/projects/hpdir)
        // 0-1:   Magic ID (0x8000 hex) [big endian]
        // 2-7:   Volume label
        // 8-11:  First directory entry 256-byte record location (offset in file in 256-byte blocks) [big endian]
        // 12-13: "LIF Identifier". Usage unknown. Set to 0x1000. [big endian]
        // 14-15: Unused. Set to 0.
        // 16-19: Directory size. Is this size in records or size in entries? Unknown. Set to 0x00000001. [big endian]
        // 20-21: "LIF version". Set to 0.
        // 22-23: Unused. Set to 0.
        // 24-27: Track count. Set to 1. [big endian]
        // 28-31: Head count. Set to 1. [big endian]
        // 32-35: Sector count. Set to ending 256-byte record location. [big endian]
        // 36-41: Date/Time field. Set to 0x1111, 0x1111, 0x1111.
        public UInt16 magicId;
        public string volumeLabel;
        public UInt32 directoryStartRecordPos;
        public UInt16 lifIdentifier;
        public UInt16 unused1;
        public UInt32 directorySize;
        public UInt16 lifVersion;
        public UInt16 unused2;
        public UInt32 trackCount;
        public UInt32 headCount;
        public UInt32 sectorCount;
        public byte year;
        public byte month;
        public byte day;
        public byte hour;
        public byte minute;
        public byte second;
    }

    internal struct LifDirectoryEntry
    {
        // 32-byte record (See https://www.hp9845.net/9845/projects/hpdir)
        // 0-9:   Filename (left aligned, space padded on right)
        // 10-11: File type (set to 0xC302 for inverse assemblers) [big endian]
        // 12-15: Starting 256-byte record location (offset in file in 256-byte blocks) [big endian]
        // 16-19: Length in 256-byte records [big endian]
        // 20-25: Date/Time field. Set to 0x0000, 0x0000, 0x0000.
        // 26-27: Volume record (set to 0x8001 for HFSLIF images)
        // 28-31: General purpose field (set to 0x00000080 for HFSLIF images)
        public string fileName;
        public UInt16 fileType;
        public UInt32 startRecordPos;
        public UInt32 recordLength;
        public byte year;
        public byte month;
        public byte day;
        public byte hour;
        public byte minute;
        public byte second;
        public UInt16 volumeRecord;
        public UInt32 generalPurposeField;
    }

    internal struct LifFileHeader
    {
        // 36-byte record
        // 0-3:  Size of the actual file content in bytes, not including this header or other record headers.
        // 4-35: Description string of this file to appear on directory lists
        public UInt32 fileDataSizeInBytes;
        public string fileDescription;
    }

    internal static class StreamExtensions
    {
        public static void WriteBigEndian(this FileStream stream, byte data)
        {
            stream.WriteByte(data);
        }
        public static void WriteBigEndian(this FileStream stream, Int16 data)
        {
            byte[] buffer = new byte[sizeof(Int16)];
            System.Buffers.Binary.BinaryPrimitives.WriteInt16BigEndian(buffer, data);
            stream.Write(buffer);
        }
        public static void WriteBigEndian(this FileStream stream, Int32 data)
        {
            byte[] buffer = new byte[sizeof(Int32)];
            System.Buffers.Binary.BinaryPrimitives.WriteInt32BigEndian(buffer, data);
            stream.Write(buffer);
        }
        public static void WriteBigEndian(this FileStream stream, UInt16 data)
        {
            byte[] buffer = new byte[sizeof(UInt16)];
            System.Buffers.Binary.BinaryPrimitives.WriteUInt16BigEndian(buffer, data);
            stream.Write(buffer);
        }
        public static void WriteBigEndian(this FileStream stream, UInt32 data)
        {
            byte[] buffer = new byte[sizeof(UInt32)];
            System.Buffers.Binary.BinaryPrimitives.WriteUInt32BigEndian(buffer, data);
            stream.Write(buffer);
        }
        public static void WriteFixedLengthASCII(this FileStream stream, string data, int stringLength, bool leftAligned = true, char paddingChar = ' ')
        {
            int stringLengthToWrite = Math.Min(stringLength, data.Length);
            int paddingBytesToWrite = stringLength - stringLengthToWrite;
            if (leftAligned)
            {
                stream.Write(data.Select((x) => (byte)x).ToArray(), 0, stringLengthToWrite);
            }
            while (paddingBytesToWrite > 0)
            {
                stream.WriteByte((byte)paddingChar);
                --paddingBytesToWrite;
            }
            if (!leftAligned)
            {
                stream.Write(data.Select((x) => (byte)x).ToArray(), 0, stringLengthToWrite);
            }
        }
    }

    internal class Program
    {
        static void WriteRecordToStream(FileStream stream, LifVolumeHeader header)
        {
            // Write the file header
            stream.WriteBigEndian(header.magicId);
            stream.WriteFixedLengthASCII(header.volumeLabel, 6);
            stream.WriteBigEndian(header.directoryStartRecordPos);
            stream.WriteBigEndian(header.lifIdentifier);
            stream.WriteBigEndian(header.unused1);
            stream.WriteBigEndian(header.directorySize);
            stream.WriteBigEndian(header.lifVersion);
            stream.WriteBigEndian(header.unused2);
            stream.WriteBigEndian(header.trackCount);
            stream.WriteBigEndian(header.headCount);
            stream.WriteBigEndian(header.sectorCount);
            stream.WriteBigEndian(header.year);
            stream.WriteBigEndian(header.month);
            stream.WriteBigEndian(header.day);
            stream.WriteBigEndian(header.hour);
            stream.WriteBigEndian(header.minute);
            stream.WriteBigEndian(header.second);

            // Write the padding bytes to fill the 256 byte record. There are some bytes in this area which are set to
            // 0x11, probably containing some kind of date/time stamp of unknown purpose.
            int paddingBytes = 214;
            for (int i = 0; i < paddingBytes; ++i)
            {
                if (i == 206 || i == 207 || i == 208 || i == 209 || i == 210 || i == 211)
                {
                    stream.WriteByte(0x11);
                }
                else
                {
                    stream.WriteByte(0x00);
                }
            }
        }

        static void WriteRecordToStream(FileStream stream, LifDirectoryEntry directoryEntry)
        {
            // Write the directory entry
            stream.WriteFixedLengthASCII(directoryEntry.fileName, 10, true, ' ');
            stream.WriteBigEndian(directoryEntry.fileType);
            stream.WriteBigEndian(directoryEntry.startRecordPos);
            stream.WriteBigEndian(directoryEntry.recordLength);
            stream.WriteBigEndian(directoryEntry.year);
            stream.WriteBigEndian(directoryEntry.month);
            stream.WriteBigEndian(directoryEntry.day);
            stream.WriteBigEndian(directoryEntry.hour);
            stream.WriteBigEndian(directoryEntry.minute);
            stream.WriteBigEndian(directoryEntry.second);
            stream.WriteBigEndian(directoryEntry.volumeRecord);
            stream.WriteBigEndian(directoryEntry.generalPurposeField);

            // Write the padding bytes to fill out the 256 byte record. We assume there's only one entry here and fill
            // the record accordingly, which is true for our "HFSLIF" files with a single "WS_FILE" entry. Note that
            // some bytes here need to be set to 0xFF. Presumably this is actually a "directory list terminator" flag
            // by setting the "fileType" to 0xFFFF for the following entry, but this is undocumented in our sources. We
            // don't need to worry for this function that only supports a single directory entry, but if you wanted to
            // write a method which takes in a list of directory entries and writes the appropriate set of records, this
            // would need to be properly determined.
            int paddingBytes = 224;
            for (int i = 0; i < paddingBytes; ++i)
            {
                if (i == 10 || i == 11)
                {
                    stream.WriteByte(0xFF);
                }
                else
                {
                    stream.WriteByte(0x00);
                }
            }
        }

        static void WriteHeaderToStream(FileStream stream, LifFileHeader fileHeader)
        {
            // Write the file header
            stream.WriteBigEndian(fileHeader.fileDataSizeInBytes);
            stream.WriteFixedLengthASCII(fileHeader.fileDescription, 32, true, ' ');
        }

        internal enum InvasmFieldOption
        {
            None,
            NoPopup,
            Popup2Choices,
            Popup8Choices,
        }

        static void Main(string[] args)
        {
            // Display usage information if required
            if (args.Length != 4)
            {
                Console.WriteLine(@"
Packs a relocatable HP Inverse Assembler into a HFSLIF file structure, suitable
for transferring to a HP Logic Analyzer via FTP. This program provides an
alternative to the HP provided IALDOWN.EXE file, which only supports uploading
via a serial or GPIB connection.

usage:
HFSLIFWriter.exe inputFilePath outputFilePath fileDescription invasmFieldOpt

inputFilePath    Path to the relocatable inverse assembler file on disk.
                 Usually a "".A"" file as output by ASM.EXE.
outputFilePath   Path to write the generated HFSLIF file to
fileDescription  A file description up to 32 characters to display on the logic
                 analyzer when listing this file on disk.
invasmFieldOpt   The control setting for the invasm field. Usage is the same as
                 in IALDOWN.EXE, a single character of A,B,C or D must be
                 specified as follows:
                    A = No ""Invasm"" Field
                    B = ""Invasm"" Field with no pop-up
                    C = ""Invasm"" Field with pop-up. 2 choices in pop-up.
                    D = ""Invasm"" Field with pop-up. 8 choices in pop-up.");
                return;
            }

            // Extract the command line arguments
            string inputFilePath = args[0];
            string outputFilePath = args[1];
            string fileDescription = args[2];
            string invasmFieldOptionText = args[3];

            // Process the invasm field option
            InvasmFieldOption invasmFieldOption = InvasmFieldOption.None;
            if (String.Equals(invasmFieldOptionText, @"A", StringComparison.OrdinalIgnoreCase))
            {
                invasmFieldOption = InvasmFieldOption.None;
            }
            else if (String.Equals(invasmFieldOptionText, @"B", StringComparison.OrdinalIgnoreCase))
            {
                invasmFieldOption = InvasmFieldOption.NoPopup;
            }
            else if (String.Equals(invasmFieldOptionText, @"C", StringComparison.OrdinalIgnoreCase))
            {
                invasmFieldOption = InvasmFieldOption.Popup2Choices;
            }
            else if (String.Equals(invasmFieldOptionText, @"D", StringComparison.OrdinalIgnoreCase))
            {
                invasmFieldOption = InvasmFieldOption.Popup8Choices;
            }
            else
            {
                Console.WriteLine("Invalid invasm field option \"{0}\" specified", invasmFieldOptionText);
                return;
            }

            // Read in the input file content
            var inputFileBytes = File.ReadAllBytes(inputFilePath);

            // Calculate our file properties
            const int recordLengthInBytes = 256;
            const int fileDataChunkLengthInBytes = recordLengthInBytes - sizeof(UInt16);
            const int fileHeaderLengthInBytes = sizeof(UInt32) + 33;
            const int fileFooterLengthInBytes = 3;
            int fileDataChunkCount = (fileHeaderLengthInBytes + inputFileBytes.Length + fileFooterLengthInBytes + (fileDataChunkLengthInBytes - 1)) / fileDataChunkLengthInBytes;

            // Build the volume header
            LifVolumeHeader volumeHeader = new LifVolumeHeader()
            {
                magicId = 0x8000,
                volumeLabel = "HFSLIF",
                directoryStartRecordPos = 1,
                lifIdentifier = 0x1000,
                unused1 = 0,
                directorySize = 1,
                lifVersion = 0,
                unused2 = 0,
                trackCount = 1,
                headCount = 1,
                sectorCount = (UInt16)(fileDataChunkCount + 2),
                year = 0x11,
                month = 0x11,
                day = 0x11,
                hour = 0x11,
                minute = 0x11,
                second = 0x11,
            };

            // Build the directory entry for our single WS_FILE entry
            LifDirectoryEntry directoryEntry = new LifDirectoryEntry()
            {
                fileName = "WS_FILE",
                fileType = 0xC302,
                startRecordPos = 2,
                recordLength = (UInt16)fileDataChunkCount,
                year = 0,
                month = 0,
                day = 0,
                hour = 0,
                minute = 0,
                second = 0,
                volumeRecord = 0x8001,
                generalPurposeField = 0x00000080,
            };

            // Create the target output file
            using var outputFileStream = File.Create(outputFilePath);

            // Write the volume and directory records
            WriteRecordToStream(outputFileStream, volumeHeader);
            WriteRecordToStream(outputFileStream, directoryEntry);

            // Write each file data record
            int currentInputBytesPos = 0;
            for (int fileRecordNo = 0; fileRecordNo < fileDataChunkCount; ++fileRecordNo)
            {
                bool firstFileRecord = fileRecordNo == 0;
                bool lastFileRecord = (fileRecordNo + 1) == fileDataChunkCount;
                int fileByteSpaceInThisRecord = (fileRecordNo == 0 ? fileDataChunkLengthInBytes - fileHeaderLengthInBytes: fileDataChunkLengthInBytes);
                int inputBytesToWriteInThisRecord = Math.Min(fileByteSpaceInThisRecord, inputFileBytes.Length - currentInputBytesPos);

                // Write the record byte count. This is a 16-bit big endian value that indicates the number of valid
                // bytes which follow this byte count in the 256 byte record.
                outputFileStream.WriteBigEndian((UInt16)(inputBytesToWriteInThisRecord + (fileRecordNo == 0 ? fileHeaderLengthInBytes : 0)));

                // If this is the first file record, write the LIF file header and invasm field option byte.
                if (firstFileRecord)
                {
                    // Write the standard LIF file header. Note that we need to add "1" to the file size to allow for
                    // the invasm field option byte we're about to write, which is part of the specific file structure,
                    // not part of the general header.
                    LifFileHeader fileHeader = new LifFileHeader()
                    {
                        fileDataSizeInBytes = (UInt32)(inputFileBytes.Length + 1),
                        fileDescription = fileDescription,
                    };
                    WriteHeaderToStream(outputFileStream, fileHeader);

                    // Write the invasm field option, which corresponds with the settings provided to IALDOWN.EXE.
                    // Description as follows:
                    //0xFF: A = No "Invasm" Field
                    //0x00: B = "Invasm" Field with no pop-up
                    //0x01: C = "Invasm" Field with pop-up. 2 choices in pop-up.
                    //0x02: D = "Invasm" Field with pop-up. 8 choices in pop-up.
                    switch (invasmFieldOption)
                    {
                        case InvasmFieldOption.None:
                            outputFileStream.WriteByte(0xFF);
                            break;
                        case InvasmFieldOption.NoPopup:
                            outputFileStream.WriteByte(0x00);
                            break;
                        case InvasmFieldOption.Popup2Choices:
                            outputFileStream.WriteByte(0x01);
                            break;
                        case InvasmFieldOption.Popup8Choices:
                            outputFileStream.WriteByte(0x02);
                            break;
                    }
                }

                // Write the file data
                var inputBytesForRecord = inputFileBytes.AsSpan(currentInputBytesPos, inputBytesToWriteInThisRecord);
                outputFileStream.Write(inputBytesForRecord);
                currentInputBytesPos += inputBytesToWriteInThisRecord;

                // Write the file footer if required. Immediately following the last byte of the file data, a three byte
                // marker value of 0x07FFFF is included. After this, in the original tools that generated these LIF
                // files, the remaining bytes in the 256 byte record were filled with the data that occupied those bytes
                // in the preceding 256 byte record. In our case, we zero these bytes.
                if (lastFileRecord)
                {
                    int paddingBytesInThisRecord = fileByteSpaceInThisRecord - inputBytesToWriteInThisRecord;
                    if (paddingBytesInThisRecord > 0)
                    {
                        outputFileStream.WriteByte(0x07);
                        --paddingBytesInThisRecord;
                    }
                    if (paddingBytesInThisRecord > 0)
                    {
                        outputFileStream.WriteByte(0xFF);
                        --paddingBytesInThisRecord;
                    }
                    if (paddingBytesInThisRecord > 0)
                    {
                        outputFileStream.WriteByte(0xFF);
                        --paddingBytesInThisRecord;
                    }
                    while (paddingBytesInThisRecord > 0)
                    {
                        outputFileStream.WriteByte(0x00);
                        --paddingBytesInThisRecord;
                    }
                }
            }
        }
    }
}
