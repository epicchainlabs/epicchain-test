using System;
using System.Buffers;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using EpicChain;
using EpicChain.BlockchainToolkit;
using EpicChain.BlockchainToolkit.Utilities;
using EpicChain.Persistence;
using EpicChain.SmartContract;
using EpicChain.SmartContract.Native;
using EpicChain.VM;
using static EpicChain.Utility;

namespace EpicChainTestHarness
{
    using EpicChainStorage = IReadOnlyDictionary<ReadOnlyMemory<byte>, StorageItem>;

    public static class Extensions
    {
        public static VMState ExecuteScript<T>(this ApplicationEngine engine, params Expression<Action<T>>[] expressions)
            where T : class
        {
            engine.LoadScript<T>(expressions);
            return engine.Execute();
        }

        public static VMState ExecuteScript(this ApplicationEngine engine, Script script)
        {
            engine.LoadScript(script);
            return engine.Execute();
        }

        public static void LoadScript<T>(this ApplicationEngine engine, params Expression<Action<T>>[] expressions)
            where T : class
        {
            var script = engine.Snapshot.CreateScript<T>(expressions);
            engine.LoadScript(script);
        }

        public static Script CreateScript<T>(this ApplicationEngine engine, params Expression<Action<T>>[] expressions)
            where T : class
            => CreateScript<T>(engine.Snapshot, expressions);

        public static Script CreateScript<T>(this DataCache snapshot, params Expression<Action<T>>[] expressions)
            where T : class
        {
            var scriptHash = snapshot.GetContractScriptHash<T>();
            using var builder = new ScriptBuilder();
            for (int i = 0; i < expressions.Length; i++)
            {
                builder.EmitContractCall(scriptHash, expressions[i]);
            }
            return builder.ToArray();
        }

        public static void EmitContractCall<T>(this ScriptBuilder builder, ApplicationEngine engine, Expression<Action<T>> expression)
            where T : class
            => EmitContractCall<T>(builder, engine.Snapshot, expression);

        public static void EmitContractCall<T>(this ScriptBuilder builder, DataCache snapshot, Expression<Action<T>> expression)
            where T : class
        {
            var scriptHash = snapshot.GetContractScriptHash<T>();
            EmitContractCall<T>(builder, scriptHash, expression);
        }

        public static void EmitContractCall<T>(this ScriptBuilder builder, UInt160 scriptHash, Expression<Action<T>> expression)
        {
            var methodCall = (MethodCallExpression)expression.Body;
            var operation = methodCall.Method.Name;

            for (var x = methodCall.Arguments.Count - 1; x >= 0; x--)
            {
                var obj = Expression.Lambda(methodCall.Arguments[x]).Compile().DynamicInvoke();
                var param = ContractParameterParser.ConvertObject(obj);
                builder.EmitPush(param);
            }
            builder.EmitPush(methodCall.Arguments.Count);
            builder.Emit(OpCode.PACK);
            builder.EmitPush(CallFlags.All);
            builder.EmitPush(operation);
            builder.EmitPush(scriptHash);
            builder.EmitSysCall(ApplicationEngine.System_Contract_Call);
        }

        public static EpicChainStorage GetContractStorages<T>(this ApplicationEngine engine) where T : class
            => GetContractStorages<T>(engine.Snapshot);

        public static EpicChainStorage GetContractStorages<T>(this DataCache snapshot)
            where T : class
        {
            var contract = snapshot.GetContract<T>();
            var prefix = StorageKey.CreateSearchPrefix(contract.Id, default);

            return snapshot.Find(prefix)
                .ToDictionary(s => s.Key.Key, s => s.Value, MemorySequenceComparer.Default);
        }

        public static EpicChainStorage StorageMap(this EpicChainStorage storages, byte prefix)
        {
            byte[]? buffer = null;
            try
            {
                buffer = ArrayPool<byte>.Shared.Rent(1);
                buffer[0] = prefix;
                return storages.StorageMap(buffer.AsMemory(0, 1));
            }
            finally
            {
                if (buffer != null) ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        public static EpicChainStorage StorageMap(this EpicChainStorage storages, string prefix)
        {
            byte[]? buffer = null;
            try
            {
                var count = StrictUTF8.GetByteCount(prefix);
                buffer = ArrayPool<byte>.Shared.Rent(count);
                count = StrictUTF8.GetBytes(prefix, buffer);
                return storages.StorageMap(buffer.AsMemory(0, count));
            }
            finally
            {
                if (buffer != null) ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        public static EpicChainStorage StorageMap(this EpicChainStorage storages, ReadOnlyMemory<byte> prefix)
            => storages.Where(kvp => kvp.Key.Span.StartsWith(prefix.Span))
                .ToDictionary(kvp => kvp.Key.Slice(prefix.Length), kvp => kvp.Value, MemorySequenceComparer.Default);


        public static bool TryGetValue(this EpicChainStorage storages, byte key, [MaybeNullWhen(false)] out StorageItem item)
        {
            byte[]? buffer = null;
            try
            {
                buffer = ArrayPool<byte>.Shared.Rent(1);
                buffer[0] = key;
                return storages.TryGetValue(buffer.AsMemory(0, 1), out item);
            }
            finally
            {
                if (buffer != null) ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        public static bool TryGetValue(this EpicChainStorage storages, string key, [MaybeNullWhen(false)] out StorageItem item)
        {
            byte[]? buffer = null;
            try
            {
                var count = StrictUTF8.GetByteCount(key);
                buffer = ArrayPool<byte>.Shared.Rent(count);
                count = StrictUTF8.GetBytes(key, buffer);
                return storages.TryGetValue(buffer.AsMemory(0, count), out item);
            }
            finally
            {
                if (buffer != null) ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        public static bool TryGetValue(this EpicChainStorage storage, UInt160 key, [MaybeNullWhen(false)] out StorageItem item)
            => storage.TryGetValue(EpicChain.IO.Helper.ToArray(key), out item);

        public static bool TryGetValue(this EpicChainStorage storage, UInt256 key, [MaybeNullWhen(false)] out StorageItem item)
            => storage.TryGetValue(EpicChain.IO.Helper.ToArray(key), out item);

        public static UInt160 GetContractScriptHash<T>(this ApplicationEngine engine)
            where T : class
            => GetContractScriptHash<T>(engine.Snapshot);

        public static UInt160 GetContractScriptHash<T>(this DataCache snapshot)
            where T : class
            => snapshot.GetContract<T>().Hash;

        public static ContractState GetContract<T>(this ApplicationEngine engine)
            where T : class
            => GetContract<T>(engine.Snapshot);

        public static ContractState GetContract<T>(this DataCache snapshot)
            where T : class
        {
            var contractName = GetContractName(typeof(T));
            return snapshot.GetContract(contractName);

            static string GetContractName(Type type)
            {
                if (type.IsNested)
                {
                    return GetContractName(type.DeclaringType ?? throw new Exception("reflection IsNested DeclaringType returned null"));
                }

                var contractAttrib = Attribute.GetCustomAttribute(type, typeof(ContractAttribute)) as ContractAttribute;
                if (contractAttrib != null) return contractAttrib.Name;

                var descriptionAttrib = Attribute.GetCustomAttribute(type, typeof(DescriptionAttribute)) as DescriptionAttribute;
                if (descriptionAttrib != null) return descriptionAttrib.Description;

                throw new Exception("reflection - FullName returned null");
            }
        }

        public static ContractState GetContract(this ApplicationEngine engine, string contractName)
            => GetContract(engine.Snapshot, contractName);

        public static ContractState GetContract(this DataCache snapshot, string contractName)
        {
            foreach (var contractState in NativeContract.ContractManagement.ListContracts(snapshot))
            {
                var name = contractState.Id >= 0 ? contractState.Manifest.Name : "EpicChain.SmartContract.Native." + contractState.Manifest.Name;
                if (string.Equals(contractName, name))
                {
                    return contractState;
                }
            }

            throw new Exception($"couldn't find {contractName} contract");
        }

        public static SnapshotCache GetSnapshot(this CheckpointFixture fixture)
        {
            return new SnapshotCache(fixture.CheckpointStore);
        }
    }
}
