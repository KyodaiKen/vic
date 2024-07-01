using LargeCollections;
using System.Text;
using static libvic.FileFormat.Enums;

namespace libvic.FileFormat
{
    public class LayerHeader
    {
        //Constants
        const uint CMagicWord = 0x564C52DB; //VLR + 0xDB

        //Fields
        readonly uint       MagicWord = CMagicWord;
        byte                LayerNameLength             { get; set; }
        ushort              LayerDescriptionLength      { get; set; }
        uint                LayerWidth                  { get; set; }
        uint                LayerHeight                 { get; set; }
        uint                LayerOffsetX                { get; set; }
        uint                LayerOffsetY                { get; set; }
        LayerBlendMode      LayerBlendMode              { get; set; }
        double              LayerOpacity                { get; set; }
        public uint         NumMetadataFields           { get; set; }
        public ushort[]     MetadataFieldLengths        { get; set; }
        string              LayerName                   { get; set; }
        string              LayerDescription            { get; set; }
        public LargeList<LargeArray<byte>> MetaData     { get; set; }

        LayerHeader()
        {
            LayerName = string.Empty;
            LayerDescription = string.Empty;
            MetaData = [];
            MetadataFieldLengths = [];
        }

        public MemoryStream ToMemoryStream()
        {
            MemoryStream tmpMs = new();
            tmpMs.Write(BitConverter.GetBytes(MagicWord));
            tmpMs.WriteByte(LayerNameLength);
            tmpMs.Write(BitConverter.GetBytes(LayerDescriptionLength));
            tmpMs.Write(BitConverter.GetBytes(LayerWidth));
            tmpMs.Write(BitConverter.GetBytes(LayerHeight));
            tmpMs.Write(BitConverter.GetBytes(LayerOffsetX));
            tmpMs.Write(BitConverter.GetBytes(LayerOffsetY));
            tmpMs.WriteByte((byte)LayerBlendMode);
            tmpMs.Write(BitConverter.GetBytes(LayerOpacity));
            tmpMs.Write(BitConverter.GetBytes(NumMetadataFields));
            for (uint i = 0; i < NumMetadataFields; i++)
                tmpMs.Write(BitConverter.GetBytes(MetadataFieldLengths[i]));
            if (LayerName.Length > byte.MaxValue) LayerName = LayerName[..byte.MaxValue];
            if (LayerDescription.Length > ushort.MaxValue) LayerDescription = LayerDescription[..ushort.MaxValue];
            tmpMs.Write(Encoding.UTF8.GetBytes(LayerName));
            tmpMs.Write(Encoding.UTF8.GetBytes(LayerDescription));
            for (int i = 0; (i < NumMetadataFields); i++) tmpMs.Write(MetaData[i]);
            return tmpMs;
        }
        public void WriteToStream(Stream stream)
        {
            stream.Write(ToMemoryStream().ToArray());
        }

        public void ReadFromStream(Stream stream)
        {
            uint tmp = BitConverter.ToUInt32(stream.Read(4));
            if (!tmp.Equals(CMagicWord))
            {
                throw new Exception("Layer Header does not start with the magic word!");
            }
            LayerNameLength = (byte)stream.ReadByte();
            LayerDescriptionLength = BitConverter.ToUInt16(stream.Read(2));
            LayerWidth = BitConverter.ToUInt32(stream.Read(4));
            LayerHeight = BitConverter.ToUInt32(stream.Read(4));
            LayerOffsetX = BitConverter.ToUInt32(stream.Read(4));
            LayerOffsetY = BitConverter.ToUInt32(stream.Read(4));
            LayerBlendMode = (LayerBlendMode)stream.ReadByte();
            LayerOpacity = BitConverter.ToUInt32(stream.Read(8));
            for (uint i = 0; i < NumMetadataFields; i++)
                MetadataFieldLengths[i] = BitConverter.ToUInt16(stream.Read(4));
            LayerName = Encoding.UTF8.GetString(stream.Read(LayerNameLength));
            LayerDescription = Encoding.UTF8.GetString(stream.Read((int)LayerDescriptionLength));
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