namespace EpicChainTestHarness.NativeContractInterfaces
{
    public interface Xep17Token
    {
        System.Numerics.BigInteger balanceOf(EpicChain.UInt160 account);
        System.Numerics.BigInteger decimals();
        string symbol();
        System.Numerics.BigInteger totalSupply();
        bool transfer(EpicChain.UInt160 @from, EpicChain.UInt160 to, System.Numerics.BigInteger amount, object data);

        interface Events
        {
            void Transfer(EpicChain.UInt160 @from, EpicChain.UInt160 to, System.Numerics.BigInteger amount);
        }
    }
}