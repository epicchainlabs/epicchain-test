using EpicChain.SmartContract;
using EpicChain.VM.Types;

namespace EpicChain.Assertions
{
    public static class EpicChainAssertionsExtensions
    {
        public static StackItemAssertions Should(this StackItem item) => new StackItemAssertions(item);

        public static NotifyEventArgsAssertions Should(this NotifyEventArgs args) => new NotifyEventArgsAssertions(args);

        public static StorageItemAssertions Should(this StorageItem item) => new StorageItemAssertions(item);
    }
}
