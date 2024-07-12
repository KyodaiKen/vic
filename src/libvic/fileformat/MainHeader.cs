using LargeCollections;
using static libvic.FileFormat.Enums;

namespace libvic.FileFormat
{
    public class MainHeader
    {
        //Constants
        const uint CMagicWordAnimation = 0x56494341; //VICA
        const uint CMagicWordImageColl = 0x56494343; //VICC

        //Fields
        private uint           MagicWord { get; set; } = CMagicWordImageColl;
        public Usage           Usage
        {
            get {
                if (MagicWord.Equals(CMagicWordImageColl))
                {
                    return Usage.ImageCollection;
                }
                else if (MagicWord.Equals(CMagicWordAnimation))
                {
                    return Usage.Animation;
                }
                else
                {
                    return Usage.ImageCollection;
                }
            }

            set {
                if (value.Equals(Usage.Animation))
                {
                    MagicWord = CMagicWordAnimation;
                }
                else if (value.Equals(Usage.ImageCollection))
                {
                    MagicWord = CMagicWordImageColl;
                }
                else
                {
                }
            }
        }
        public uint            NumMetadataFields { get; set; }
        public uint[]          MetadataFieldLengths { get; set; }
        public LargeList<LargeArray<byte>> MetaData { get; set; }

        MainHeader()
        {
            MetadataFieldLengths = [];
            MetaData = [];
        }

        public MemoryStream ToMemoryStream()
        {
            MemoryStream tmpMs = new();
            tmpMs.Write(BitConverter.GetBytes(MagicWord));
            tmpMs.Write(BitConverter.GetBytes(NumMetadataFields));
            for (long i = 0; i < NumMetadataFields; i++)
                tmpMs.Write(BitConverter.GetBytes(MetadataFieldLengths[i]));
            for (long i = 0; i < NumMetadataFields; i++)
                tmpMs.Write(MetaData[i]);
            return tmpMs;
        }

        public void WriteToStream(Stream stream)
        {
            stream.Write(ToMemoryStream().ToArray());
        }

        public void ReadFromStream(Stream stream)
        {
            uint tmp = BitConverter.ToUInt32(stream.Read(4));
            if (!tmp.Equals(CMagicWordImageColl) && !tmp.Equals(CMagicWordAnimation))
            {
                throw new Exception("Master Header does not start with the magic word!");
            }
            NumMetadataFields = BitConverter.ToUInt32(stream.Read(4));
            MetadataFieldLengths = new uint[NumMetadataFields];
            for (long i = 0; i < NumMetadataFields; i++)
                MetadataFieldLengths[i] = BitConverter.ToUInt32(stream.Read(4));
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
