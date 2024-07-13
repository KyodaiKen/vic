using LargeCollections;
using System.Numerics;
using libvic.FileFormat.Headers;

namespace libvic.FileFormat
{
    public class Index
    {
        public struct Tile
        {
            public BigInteger       Offset;
            public TileHeader       Header;
        }
        public struct Layer
        {
            public BigInteger       Offset;
            public LayerHeader      Header;
            public LargeList<Tile>  Tiles;
        }
        public struct ImageObject
        {
            public BigInteger       Offset;
            public ImgObjHeader     Header;
            public LargeList<Layer> Layers;
        }

        public LargeList<ImageObject> ImageObjects { get; set; }

        Index()
        {
            ImageObjects = [];
        }
    }
}
