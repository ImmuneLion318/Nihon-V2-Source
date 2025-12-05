using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Security.Principal;
using System.Security.AccessControl;

using Nihon.Injection;
using Nihon.Static;

using static Nihon.Static.Extension;
using static Nihon.Static.Native;

namespace Nihon.Interfaces;

public class ModuleBase
{
    public bool IsPipeOpen =>
        this.LuaPipe != string.Empty ? true : false;

    public string ModulePath { get; set; }
        = "Nihon-Module.dll";

    /* Looking Back At It, This Was Dumb Cause This Would Do The Same Loop Every Single Time Execute Was Called. */
    public string LuaPipe
    {
        get
        {
            string Pipe = string.Empty;

            foreach (string Text in Directory.GetFiles("\\\\.\\pipe\\"))
            {
                if (Text.Contains("Nihon-Pipe"))
                    Pipe = Text.Substring(Text.IndexOf("Sessions"));
            }

            return Pipe != string.Empty ? Pipe : string.Empty;
        }
    }

    public bool IsLoaded { get; set; } =
        false;

    public delegate void InjectorHandler(object Source, InjectorOutput Output);

    public enum InjectorEvent : int
    {
        Success = 1,
        Failure,
        AlreadyInjected,
        ProcessNotFound,
        Idle
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct InjectorOutput
    {
        public InjectorEvent Output;
        public string Message;
    }
}

public class Module : ModuleBase
{
    public event InjectorHandler ModuleReady;

    public Module(Action OnConstruct) =>
        OnConstruct?.Invoke();

    public async Task<bool> RunScript(string Script)
    {
        if (this.IsPipeOpen == false || Script.Length == 0)
            return false;

        using (NamedPipeClientStream PipeStream = new NamedPipeClientStream(".", this.LuaPipe, PipeDirection.Out))
        {
            await PipeStream.ConnectAsync(Timeout.Infinite);

            using (StreamWriter StreamWriter = new StreamWriter(PipeStream))
                await StreamWriter.WriteAsync(Script);
        }

        return true;
    }

    public async Task<InjectorOutput> Inject(double Delay)
    {
        InjectorOutput InjectionOutput = new InjectorOutput { };

        Process Proc = Process.GetProcessesByName("Windows10Universal")
            .GetProcessByTitle(string.Empty);

        if (Proc == null)
        {
            InjectionOutput = new InjectorOutput
            {
                Output = InjectorEvent.ProcessNotFound,
                Message = "Roblox Process Not Found, Open Roblox And Try Again."
            };
            return InjectionOutput;
        }

        if (this.IsPipeOpen == true)
        {
            InjectionOutput = new InjectorOutput
            {
                Output = InjectorEvent.AlreadyInjected,
                Message = "Already Injected, In Remote Process."
            };
            return InjectionOutput;
        }

        await Task.Delay(TimeSpan.FromSeconds(Delay));

        #region Set-Permissions

        FileInfo FileInfo = new FileInfo(this.ModulePath);

        FileSecurity Security = FileInfo.GetAccessControl();
        SecurityIdentifier Identity = new SecurityIdentifier("S-1-15-2-1");

        Security.AddAccessRule(new FileSystemAccessRule(
            Identity,
            FileSystemRights.FullControl,
            InheritanceFlags.None,
            PropagationFlags.NoPropagateInherit,
            AccessControlType.Allow));

        FileInfo.SetAccessControl(Security);

        #endregion

        if (!Injector.CreateThread(Proc, Path.GetFullPath(this.ModulePath)))
        {
            InjectionOutput = new InjectorOutput
            {
                Output = InjectorEvent.Failure,
                Message = "Injection Failed."
            };
            return InjectionOutput;
        }

        while (IsPipeOpen != true)
            await Task.Delay(150); 

        ModuleReady?.Invoke(this, InjectionOutput);

        return InjectionOutput = new InjectorOutput
        {
            Output = InjectorEvent.Success,
            Message = $"Module Created In - '{Proc.Id}'."
        };
    }
}