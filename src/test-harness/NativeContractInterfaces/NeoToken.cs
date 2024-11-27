namespace EpicChainTestHarness.NativeContractInterfaces
{
    public interface EpicChainToken : Xep17Token
    {
        EpicChain.VM.Types.Array getAccountState(EpicChain.UInt160 account);
        EpicChain.VM.Types.Array getCandidates();
        EpicChain.VM.Types.Array getCommittee();
        System.Numerics.BigInteger getGasPerBlock();
        EpicChain.VM.Types.Array getNextBlockValidators();
        System.Numerics.BigInteger getRegisterPrice();
        bool registerCandidate(EpicChain.Cryptography.ECC.ECPoint pubkey);
        void setGasPerBlock(System.Numerics.BigInteger gasPerBlock);
        void setRegisterPrice(System.Numerics.BigInteger registerPrice);
        System.Numerics.BigInteger unclaimedGas(EpicChain.UInt160 account, System.Numerics.BigInteger end);
        bool unregisterCandidate(EpicChain.Cryptography.ECC.ECPoint pubkey);
        bool vote(EpicChain.UInt160 account, EpicChain.Cryptography.ECC.ECPoint voteTo);
    }
}