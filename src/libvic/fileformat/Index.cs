using LargeCollections;
using System.Numerics;

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
        public struct Frame
        {
            public BigInteger       Offset;
            public FrameHeader      Header;
            public LargeList<Layer> Layers;
        }

        public LargeList<Frame>     Frames { get; set; }

        Index()
        {
            Frames = [];
        }
    }
}
