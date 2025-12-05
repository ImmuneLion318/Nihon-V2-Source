using System;
using System.Diagnostics;
using System.Text;

using static Nihon.Injection.Utilities;
using static Nihon.Static.Native;

namespace Nihon.Injection;

public static class Utilities
{
    public static uint Relative(uint Address, uint Location) =>
        (Address - Location) - 5;
}

public static class Injector
{
    public static bool CreateThread(Process Proc, string Path)
    {
        byte[] Bytes = Encoding.Default.GetBytes(Path);

        IntPtr ProcAddress = GetProcAddress(
            LoadLibraryA("kernel32.dll"),
            "LoadLibraryA");

        FreeLibrary(LoadLibraryA("kernel32.dll"));

        IntPtr hProcess = OpenProcess(
            ProcessAccess.All,
            false,
            Proc.Id);

        if (ProcAddress == IntPtr.Zero || hProcess == IntPtr.Zero)
            return false;

        IntPtr AllocatedMemory = VirtualAllocEx(
            hProcess,
            IntPtr.Zero,
            Bytes.Length,
            AllocationType.Commit | AllocationType.Reserve,
            MemoryProtection.ReadWriteExecute);

        Console.WriteLine($"Memory - {AllocatedMemory:X8}.");

        return AllocatedMemory != IntPtr.Zero && WriteProcessMemory(
            hProcess,
            AllocatedMemory,
            Bytes,
            Bytes.Length,
            out _) && CreateRemoteThread(
                hProcess,
                IntPtr.Zero,
                0u,
                ProcAddress,
                AllocatedMemory,
                0u,
                out _) != IntPtr.Zero;
    }

    public static bool HijackThread(Process Proc, string Path)
    {
        IntPtr hProcess = OpenProcess(
            ProcessAccess.All,
            false,
            Proc.Id);

        ProcessThread Thread = Proc.Threads[0];

        IntPtr hThread = OpenThread(
            ThreadAccess.GetContext | ThreadAccess.SetContext | ThreadAccess.SuspendResume,
            false,
            Thread.Id);

        ThreadContext Context = new ThreadContext
        { Flags = ContextFlags.Full };

        #region Payload

        /* 
         0x60 | Pushad              -> Save Registers                
         0x68 | Push Module Path    -> Push Module Path To Stack     
         0xE8 | Call lpLoadLibraryA -> Load The Module               
         0x61 | Popad               -> Pop Registers                 
         0xE9 | Jmp                 -> Jmp - Context.Eip  
        */

        byte[] ShellCode = new byte[]
        {
            0x60,
            0x68, 0x00, 0x00, 0x00, 0x00,
            0xE8, 0x00, 0x00, 0x00, 0x00,
            0x61,
            0xE9, 0x00, 0x00, 0x00, 0x00
        };

        #endregion

        #region Create-Memory

        IntPtr ModuleSpace = VirtualAllocEx(
            hProcess,
            IntPtr.Zero,
            ShellCode.Length,
            AllocationType.Commit,
            MemoryProtection.ReadWriteExecute);

        IntPtr MemorySpace = VirtualAllocEx(
            hProcess,
            IntPtr.Zero,
            ShellCode.Length,
            AllocationType.Commit,
            MemoryProtection.ReadWriteExecute);

        if (ModuleSpace == IntPtr.Zero || MemorySpace == IntPtr.Zero)
        {
            Console.WriteLine($"Failed To Create Memory In {Proc.MainWindowTitle}.");
            return false;
        }

        Console.WriteLine($"Memory - {MemorySpace.ToString("x")}.");

        #endregion

        SuspendThread(hThread);

        if (GetThreadContext(hThread, ref Context))
            Console.WriteLine($"Context.Eip - {Context.Eip:X8}.");

        #region Write-Payload

        if (WriteProcessMemory(
            hProcess,
            ModuleSpace,
            Encoding.ASCII.GetBytes(Path),
            Path.Length,
            out _) != true)
            return false;

        BitConverter.GetBytes((uint)ModuleSpace).CopyTo(ShellCode, 2);

        uint LoadLibrary = (uint)GetProcAddress(
            LoadLibraryA("Kernel32.dll"),
            "LoadLibraryA"
        );

        FreeLibrary(LoadLibraryA("kernel32.dll"));

        BitConverter.GetBytes(Relative(
            LoadLibrary, (uint)MemorySpace + 6)
        ).CopyTo(ShellCode, 7);

        BitConverter.GetBytes(Relative(
            Context.Eip, (uint)MemorySpace + 12)
        ).CopyTo(ShellCode, 13);

        if (WriteProcessMemory(
            hProcess,
            MemorySpace,
            ShellCode,
            ShellCode.Length,
            out _) != true)
            return false;

        #endregion

        Context.Eip = (uint)MemorySpace;

        if (!SetThreadContext(hThread, ref Context))
            Console.WriteLine("Failed To Set Thread Context.");

        ResumeThread(hThread);

        Console.WriteLine("Thread-Hijacked - Information : \n" +
            $"  Id '{Thread.Id}'\n" +
            $"  Process - {Proc.MainWindowTitle}");

        WaitForSingleObject(hThread, 0xFFFFFFFF);

        #region Free-Memory

        VirtualFreeEx(
            hProcess,
            ModuleSpace,
            Path.Length,
            FreeType.Release | FreeType.Decommit);

        VirtualFreeEx(
            hProcess,
            MemorySpace,
            ShellCode.Length,
            FreeType.Release | FreeType.Decommit);

        #endregion

        #region Dispose-Objects

        CloseHandle(hThread);
        CloseHandle(hProcess);

        #endregion

        return true;
    }
}