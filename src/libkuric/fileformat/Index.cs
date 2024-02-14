using LargeCollections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace libkuric.FileFormat
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
    }
}
