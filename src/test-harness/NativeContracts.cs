using System;
using EpicChain;

namespace EpicChainTestHarness
{
    public static class NativeContracts
    {
        static Lazy<UInt160> epicchainToken = new Lazy<UInt160>(() => UInt160.Parse("0xef4073a0f2b305a38ec4050e4d3d28bc40ea63f5"));
        public static UInt160 EpicChainToken => epicchainToken.Value;

        static Lazy<UInt160> epicpulseToken = new Lazy<UInt160>(() => UInt160.Parse("0xd2a4cff31913016155e38e474a2c06d08be276cf"));
        public static UInt160 EpicPulseToken => epicpulseToken.Value;
    }
}