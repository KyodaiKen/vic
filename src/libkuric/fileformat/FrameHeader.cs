using LargeCollections;
using System.Numerics;
using System.Text;

namespace libkuric.FileFormat
{
    internal class FrameHeader
    {
        //Constants
        const uint CMagicWord = 0x4B4652FF; //KFR + FF

        //Fields
        public uint         MagicWord                   { get; set; } = CMagicWord;
        public ulong        FrameSeqNbr                 { get; set; }
        public byte         FrameNameLen                { get; set; }
        public uint         FrameDescriptionLen         { get; set; }
        public uint         DisplayDuration             { get; set; }
        public uint         NumMetadateFields           { get; set; }
        public ushort[]     MetadataFieldLengths        { get; set; }
        public ulong        NumLayers                   { get; set; }
        public ulong[]      LayerDataLengths            { get; set; }
        public string       FrameName                   { get; set; }
        public string       FrameDescription            { get; set; }

        public LargeArray<byte> GetEmptyHeader()
        {
            return new LargeArray<byte>((long)(31u + FrameNameLen + FrameDescriptionLen + NumMetadateFields * 4 + NumLayers * 8));
        }

        public void WriteEmptyHeaderToStream(Stream stream)
        {
            stream.Write(GetEmptyHeader());
        }

        public MemoryStream ToMemoryStream()
        {
            MemoryStream tmpMs = new MemoryStream();
            tmpMs.Write(BitConverter.GetBytes(MagicWord));
            tmpMs.Write(BitConverter.GetBytes(FrameSeqNbr));
            tmpMs.WriteByte(FrameNameLen);
            tmpMs.Write(BitConverter.GetBytes(FrameDescriptionLen));
            tmpMs.Write(BitConverter.GetBytes(FrameDescriptionLen));
            tmpMs.Write(BitConverter.GetBytes(DisplayDuration));
            for (uint i = 0; i < NumMetadateFields; i++)
                tmpMs.Write(BitConverter.GetBytes(MetadataFieldLengths[i]));
            tmpMs.Write(BitConverter.GetBytes(NumLayers));
            for (ulong i = 0; i < NumLayers; i++)
                tmpMs.Write(BitConverter.GetBytes(LayerDataLengths[i]));
            if (FrameName.Length > byte.MaxValue) FrameName = FrameName.Substring(0, byte.MaxValue);
            if (FrameDescription.Length > ushort.MaxValue) FrameDescription = FrameDescription.Substring(0, ushort.MaxValue);
            tmpMs.Write(Encoding.UTF8.GetBytes(FrameName));
            tmpMs.Write(Encoding.UTF8.GetBytes(FrameDescription));

            return tmpMs;
        }
        public void WriteToStream(Stream stream)
        {
            stream.Write(ToMemoryStream().ToArray());
        }


    }
}
