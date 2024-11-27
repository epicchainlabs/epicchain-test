using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SimpleJSON;

namespace EpicChain.BuildTasks
{
    // Parse Manifest ABI JSON manually using SimpleJSON to avoid taking dependency on EpicChain.dll or a JSON parsing package
    public class EpicChainManifest
    {
        public class Method
        {
            public string Name { get; set; } = "";
            public string ReturnType { get; set; } = "";
            public IReadOnlyList<(string Name, string Type)> Parameters { get; set; } = Array.Empty<(string Name, string Type)>();
        }

        public class Event
        {
            public string Name { get; set; } = "";
            public IReadOnlyList<(string Name, string Type)> Parameters { get; set; } = Array.Empty<(string Name, string Type)>();
        }

        public string Name { get; set; } = "";
        public IReadOnlyList<Method> Methods { get; set; } = Array.Empty<Method>();
        public IReadOnlyList<Event> Events { get; set; } = Array.Empty<Event>();

        public static EpicChainManifest Load(string manifestPath)
        {
            var text = File.ReadAllText(manifestPath) ?? throw new FileNotFoundException("", manifestPath);
            var json = SimpleJSON.JSON.Parse(text) ?? throw new InvalidOperationException();
            return EpicChainManifest.FromManifestJson(json);
        }

        public static EpicChainManifest FromManifestJson(SimpleJSON.JSONNode json)
        {
            var contractName = json["name"].Value;
            var abi = json["abi"];
            var methods = abi["methods"].Linq.Select(kvp => MethodFromJson(kvp.Value));
            var events = abi["events"].Linq.Select(kvp => EventFromJson(kvp.Value));

            return new EpicChainManifest
            {
                Name = contractName,
                Methods = methods.ToList(),
                Events = events.ToList()
            };
        }

        static (string Name, string Type) ParamFromJson(JSONNode json)
        {
            var name = json["name"].Value;
            var type = json["type"].Value;
            return (name, type);
        }

        static Method MethodFromJson(JSONNode json)
        {
            var name = json["name"].Value;
            var returnType = json["returntype"].Value;
            var @params = json["parameters"].Linq.Select(kvp => ParamFromJson(kvp.Value));
            return new Method
            {
                Name = name,
                ReturnType = returnType,
                Parameters = @params.ToList()
            };
        }

        static Event EventFromJson(JSONNode json)
        {
            var name = json["name"].Value;
            var @params = json["parameters"].Linq.Select(kvp => ParamFromJson(kvp.Value));
            return new Event
            {
                Name = name,
                Parameters = @params.ToList()
            };
        }
    }
}
