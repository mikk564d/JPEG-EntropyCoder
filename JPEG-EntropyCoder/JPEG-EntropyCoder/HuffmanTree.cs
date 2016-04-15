using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace JPEG_EntropyCoder
{
    public class HuffmanTree : IHuffmanTree
    {

        public byte[] DHT { get; }
        private HuffmanNode Root { get; }

        public HuffmanTree(byte[] DHT)
        {
            this.DHT = DHT;
            Root = new HuffmanNode(new BitArray(0), DHT);
        }

        public byte Find(BitArray treePath)
        {
            return Root.SearchFor(treePath);
        }

        public List<string> PrintTree()
        {
            List<string> result = new List<string> { };
            Root.PrintAddresses(ref result);
            return result;
        }
    }
}
