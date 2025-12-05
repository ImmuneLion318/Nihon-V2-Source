using System;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace Nihon.Static;

public static class Data
{
    public class ModuleConfig
    {
        [JsonProperty("finjVersion")]
        public int InjectorVersion;

        [JsonProperty("qdRFzx_exe")]
        public string Injector;

        [JsonProperty("injDep")]
        public string InjectDependency;

        [JsonProperty("exploit-module")]
        public InfoObject ModuleInfo;

        public class InfoObject
        {
            [JsonProperty("version")]
            public int Version;

            [JsonProperty("patched")]
            public bool IsPatched;

            [JsonProperty("download")]
            public string DownloadLink;
        }
    }

    public class ExceptionInfo
    {
        [JsonProperty("Title")]
        public string Title;

        [JsonProperty("Version")]
        public string Version;

        [JsonProperty("Time")]
        public string Time;

        [JsonProperty("Exception")]
        public Exception Exception;
    }

    public class InterfaceSettings
    {
        [JsonProperty("Left")]
        public double Left;

        [JsonProperty("Top")]
        public double Top;

        [JsonProperty("Width")]
        public double Width;

        [JsonProperty("Height")]
        public double Height;

        [JsonProperty("Tabs")]
        public List<TabData> Tabs;

        public class TabData
        {
            [JsonProperty("Name")]
            public string Name;

            [JsonProperty("Text")]
            public string Text;
        }
    }

    public class InterfaceInfo
    {
        [JsonProperty("Module")]
        public ModuleObject Module;

        public class ModuleObject
        {
            [JsonProperty("x64")]
            public X64Object X64;

            public class X64Object
            {
                [JsonProperty("IsPatched")]
                public bool IsPatched;

                [JsonProperty("Version")]
                public string Version;

                [JsonProperty("Hash")]
                public string Hash;

                [JsonProperty("Link")]
                public string Link;
            }

            [JsonProperty("x86")]
            public X86Object X86;

            public class X86Object
            {
                [JsonProperty("IsPatched")]
                public bool IsPatched;

                [JsonProperty("Version")]
                public string Version;

                [JsonProperty("Hash")]
                public string Hash;

                [JsonProperty("Link")]
                public string Link;
            }
        }

        [JsonProperty("Interface")]
        public InterfaceObject Interface;

        public class InterfaceObject
        {
            [JsonProperty("Version")]
            public string Version;

            [JsonProperty("IsEnabled")]
            public bool IsEnabled;

            [JsonProperty("Hash")]
            public string Hash;
        }

        [JsonProperty("Invite")]
        public string Invite;

        [JsonProperty("Time")]
        public string Time;
    }

    public class EditorOptions
    {
        [JsonProperty("MiniMap")]
        public bool MiniMap;

        [JsonProperty("Links")]
        public bool Links;

        [JsonProperty("ReadOnly")]
        public bool ReadOnly;

        [JsonProperty("Theme")]
        public string Theme;

        [JsonProperty("FontSize")]
        public int FontSize;
    }

    public class InterfaceOptions
    {
        [JsonProperty("AutoAttach")]
        public bool AutoAttach;

        [JsonProperty("AutoExecute")]
        public bool AutoExecute;

        [JsonProperty("TopMost")]
        public bool TopMost;

        [JsonProperty("StealthMode")]
        public bool StealthMode;
    }

    public class ScriptObject
    {
        [JsonProperty("id")]
        public string Id;

        [JsonProperty("staffscript")]
        public bool IsVerified;

        [JsonProperty("games")]
        public List<long> Games;

        [JsonProperty("script")]
        public string Script;

        [JsonProperty("category")]
        public string Category;

        [JsonProperty("title")]
        public string Title;

        [JsonProperty("author")]
        public string Author;

        [JsonProperty("description")]
        public string Description;
    }

    public class VerificationObject
    {
        [JsonProperty("hwid")]
        public string Hardware;

        [JsonProperty("key")]
        public string Key;
    }

    public class LanguageObject
    {
        [JsonProperty("Id")]
        public string Id;

        [JsonProperty("Content")]
        public string Content;
    }

    public class InteractObject
    {
        [JsonProperty("Id")]
        public string Message;

        [JsonProperty("Type")]
        public string Type;
    }

    public class WebSocketEntry
    {
        [JsonProperty("Origin")]
        public string Origin;

        [JsonProperty("IsSecure")]
        public bool IsSecure;

        [JsonProperty("IsLocal")]
        public bool IsLocal;
    }

    public class WebSocketHolder
    {
        [JsonProperty("Entries")]
        public List<WebSocketEntry> Entries;
    }

    public class ScriptObjectHolder
    {
        [JsonProperty("scripts")]
        public List<ScriptObject> ScriptEntries;
    }

    public class LanguageObjectHolder
    {
        [JsonProperty("Id")]
        public string Id;

        [JsonProperty("Data")]
        public List<LanguageObject> Data;
    }
}
