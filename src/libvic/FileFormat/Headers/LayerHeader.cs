using LargeCollections;
using libvic;
using static libvic.FileFormat.Enums;

namespace libvic.FileFormat.Headers
{
    public class LayerHeader
    {
        //Fields
        uint LayerWidth { get; set; }
        uint LayerHeight { get; set; }
        uint LayerOffsetX { get; set; }
        uint LayerOffsetY { get; set; }
        LayerBlendMode LayerBlendMode { get; set; }
        double LayerOpacity { get; set; }
        public uint NumMetadataFields { get; set; }
        public ushort[] MetadataFieldLengths { get; set; }
        public LargeList<LargeArray<byte>> MetaData { get; set; }

        LayerHeader()
        {
            MetaData = [];
            MetadataFieldLengths = [];
        }

        public MemoryStream ToMemoryStream()
        {
            MemoryStream tmpMs = new();
            tmpMs.Write(BitConverter.GetBytes(LayerWidth));
            tmpMs.Write(BitConverter.GetBytes(LayerHeight));
            tmpMs.Write(BitConverter.GetBytes(LayerOffsetX));
            tmpMs.Write(BitConverter.GetBytes(LayerOffsetY));
            tmpMs.WriteByte((byte)LayerBlendMode);
            tmpMs.Write(BitConverter.GetBytes(LayerOpacity));
            tmpMs.Write(BitConverter.GetBytes(NumMetadataFields));
            for (uint i = 0; i < NumMetadataFields; i++)
                tmpMs.Write(BitConverter.GetBytes(MetadataFieldLengths[i]));
            for (int i = 0; i < NumMetadataFields; i++) tmpMs.Write(MetaData[i]);
            return tmpMs;
        }
        public void WriteToStream(Stream stream)
        {
            stream.Write(ToMemoryStream().ToArray());
        }

        public void ReadFromStream(Stream stream)
        {
            LayerWidth = BitConverter.ToUInt32(stream.Read(4));
            LayerHeight = BitConverter.ToUInt32(stream.Read(4));
            LayerOffsetX = BitConverter.ToUInt32(stream.Read(4));
            LayerOffsetY = BitConverter.ToUInt32(stream.Read(4));
            LayerBlendMode = (LayerBlendMode)stream.ReadByte();
            LayerOpacity = BitConverter.ToUInt32(stream.Read(8));
            for (uint i = 0; i < NumMetadataFields; i++)
                MetadataFieldLengths[i] = BitConverter.ToUInt16(stream.Read(4));
            MetaData = [];
            for (long i = 0; i < NumMetadataFields; i++)
                stream.Read(MetaData[i], 0, MetadataFieldLengths[i]);
        }

        public bool TryReadingFromStream(Stream stream)
        {
            try
            {
                ReadFromStream(stream);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}