using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace Nihon.Static;

public class Native
{
    public const nint InvalidHandle = -1;
    public const nint Success = 0;

    [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
    public static extern nint GetModuleHandleA(string lpModuleName);

    [DllImport("psapi.dll", SetLastError = true)]
    public static extern IntPtr GetModuleBaseName(
        IntPtr Window,
        IntPtr Handle,
        StringBuilder lpBaseName,
        uint nSize);

    [DllImport("user32.dll")]
    public static extern IntPtr FindWindowA(
        string lpClassName,
        string lpWindowName);

    [DllImport("kernel32.dll")]
    public static extern bool ReadProcessMemory(
        IntPtr hProcess,
        IntPtr lpBaseAddress,
        byte[] lpBuffer,
        int dwSize,
        out int lpNumberOfBytesRead);

    [DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true)]
    public static extern IntPtr VirtualAllocEx(
        IntPtr hProcess,
        IntPtr lpAddress,
        int dwSize,
        AllocationType flAllocationType,
        MemoryProtection flProtect);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool WriteProcessMemory(
        IntPtr hProcess,
        IntPtr lpBaseAddress,
        byte[] lpBuffer,
        int nSize,
        out IntPtr lpNumberOfBytesWritten);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern bool WaitNamedPipe(
        string Name,
        int Timeout);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern uint WaitForSingleObject(
        IntPtr hHandle,
        uint Milliseconds);

    [Flags]
    public enum AllocationType : uint
    {
        Commit = 0x00001000,
        Reserve = 0x00002000,
        Decommit = 0x00004000,
        Release = 0x00008000,
        Reset = 0x00080000,
        Physical = 0x00400000,
        TopDown = 0x00100000,
        WriteWatch = 0x00200000,
        LargePages = 0x20000000,
    }

    [Flags]
    public enum MemoryProtection : uint
    {
        Execute = 0x010,
        ReadExecute = 0x020,
        ReadWriteExecute = 0x040,
        WriteCopyExecute = 0x080,
        NoAccess = 0x001,
        ReadOnly = 0x002,
        ReadWrite = 0x004,
        WriteCopy = 0x008,
        Guard = 0x100,
        NoCache = 0x200,
        WriteCombine = 0x400,
    }

    [DllImport("kernel32.dll")]
    public static extern IntPtr OpenProcess(
        ProcessAccess dwDesiredAccess,
        [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle,
        int dwProcessId);

    [Flags]
    public enum ProcessAccess : uint
    {
        All = 0x001F0FFF,
        Terminate = 0x00000001,
        CreateThread = 0x00000002,
        VirtualMemoryOperation = 0x00000008,
        VirtualMemoryRead = 0x00000010,
        VirtualMemoryWrite = 0x00000020,
        DuplicateHandle = 0x00000040,
        CreateProcess = 0x000000080,
        SetQuota = 0x00000100,
        SetInformation = 0x00000200,
        QueryInformation = 0x00000400,
        QueryLimitedInformation = 0x00001000,
        Synchronize = 0x00100000,
    }

    [DllImport("kernel32.dll")]
    public static extern bool VirtualProtectEx(
        IntPtr hProcess,
        IntPtr lpAddress,
        int Size,
        MemoryProtection lpNewProtect,
        out MemoryProtection lpOldProtect);

    [DllImport("kernel32.dll")]
    public static extern bool CheckRemoteDebuggerPresent(
        IntPtr hProcess,
        ref bool IsDebuggerPresent);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern IntPtr GetModuleHandle(string ModuleName);

    [DllImport("kernel32", CharSet = CharSet.Ansi, SetLastError = true)]
    public static extern IntPtr LoadLibraryA(string lpFileName);

    [DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
    public static extern IntPtr GetProcAddress(
        IntPtr hModule,
        string ProcName);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool FreeLibrary(IntPtr hModule);

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
    public static extern bool CloseHandle(IntPtr hThread);

    [DllImport("kernel32.dll")]
    public static extern IntPtr CreateRemoteThread(
        IntPtr hProcess,
        IntPtr lpThreadAttributes,
        uint dwStackSize,
        IntPtr lpStartAddress,
        IntPtr lpParameter,
        CreationFlags dwCreationFlags,
        out IntPtr lpThreadId);

    [Flags]
    public enum CreationFlags
    {
        RunImmediately = 0,
        CreateSuspended = 4,
        StackSizeIsReserved = 65536
    }

    [DllImport("kernel32.dll")]
    public static extern bool GetExitCodeThread(
        IntPtr hThread,
        out uint lpExitCode);

    [DllImport("kernel32.dll")]
    public static extern bool VirtualFreeEx(
        IntPtr hProcess,
        IntPtr lpAddress,
        int Size,
        FreeType Type);

    [Flags]
    public enum FreeType : uint
    {
        Decommit = 0x4000,
        Release = 0x8000
    }

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
    public static extern IntPtr OpenThread(
        ThreadAccess dwDesiredAccess,
        bool bInheritHandle,
        int dwThreadId);

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
    public static extern IntPtr SuspendThread(IntPtr hThread);

    public static void SuspendProcess(Process Proc)
    {
        IntPtr Handle = OpenProcess(
            ProcessAccess.All,
            false,
            Proc.Id);

        foreach (ProcessThread Thread in Proc.Threads)
        {
            IntPtr hThread = OpenThread(
                ThreadAccess.SuspendResume,
                false,
                Thread.Id);

            SuspendThread(hThread);
        }
    }

    public static void ResumeProcess(Process Proc)
    {
        IntPtr Handle = OpenProcess(
            ProcessAccess.All,
            false,
            Proc.Id);

        foreach (ProcessThread Thread in Proc.Threads)
        {
            IntPtr hThread = OpenThread(
                ThreadAccess.SuspendResume,
                false,
                Thread.Id);

            ResumeThread(hThread);
        }
    }

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
    public static extern IntPtr ResumeThread(IntPtr hThread);

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
    public static extern bool GetThreadContext(
        IntPtr hThread,
        ref ThreadContext lpContext);

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
    public static extern bool SetThreadContext(
        IntPtr hThread,
        ref ThreadContext lpContext);

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
    public static extern int CreateToolhelp32Snapshot(
        SnapRules dwFlags,
        int th32ProcessID);

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
    public static extern bool Thread32First(
        IntPtr hSnapshot,
        ref ThreadEntry lpte);

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
    public static extern bool Thread32Next(
        IntPtr hSnapshot,
        ref ThreadEntry lpte);

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
    public static extern IntPtr GetThreadId(IntPtr Thread);

    [Flags]
    public enum ThreadAccess : uint
    {
        Terminate = 0x0001,
        SuspendResume = 0x0002,
        GetContext = 0x0008,
        SetContext = 0x0010,
        SetInfo = 0x0020,
        QueryInfo = 0x0040,
        SetToken = 0x0080,
        Impersonate = 0x0100,
        DirectImpersonate = 0x0200,
        All = Terminate | SuspendResume | GetContext | SetContext | SetInfo | QueryInfo | SetToken | Impersonate | DirectImpersonate,
    }

    [Flags]
    public enum ContextFlags : uint
    {
        I386 = 0x10000,
        Control = I386 | 0x01,
        Integer = I386 | 0x02,
        Segments = I386 | 0x04,
        FloatingPoint = I386 | 0x08,
        DebugRegisters = I386 | 0x10,
        ExtendedRegisers = I386 | 0x20,
        Full = Control | Integer | Segments,
        All = Control | Integer | Segments | FloatingPoint | DebugRegisters | ExtendedRegisers,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ThreadInfoBlock
    {
        public IntPtr ExceptionList;
        public IntPtr StackBase;
        public IntPtr StackLimit;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ThreadClientId
    {
        public int UniqueProcess;
        public int UniqueThread;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ThreadInfoBasic
    {
        public IntPtr ExitStatus;
        public IntPtr TebBaseAddress;
        public ThreadClientId ClientId;
        public IntPtr AffinityMask;
        public IntPtr Priority;
        public IntPtr BasePriority;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct XmmSave
    {
        public int ControlWord;
        public int StatusWord;
        public int TagWord;
        public int ErrorOffset;
        public int ErrorSelector;
        public int DataOffset;
        public int DataSelector;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 80)]
        public byte[] RegisterArea;
        public int Cr0NpxState;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ThreadContext
    {
        public ContextFlags Flags;
        public uint Dr0;
        public uint Dr1;
        public uint Dr2;
        public uint Dr3;
        public uint Dr6;
        public uint Dr7;
        public XmmSave Xmm;
        public uint SegGs;
        public uint SegFs;
        public uint SegEs;
        public uint SegDs;
        public uint Edi;
        public uint Esi;
        public uint Ebx;
        public uint Edx;
        public uint Ecx;
        public uint Eax;
        public uint Ebp;
        public uint Eip;
        public uint SegCs;
        public uint EFlags;
        public uint Esp;
        public uint SegSs;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 512)]
        public byte[] ExtendedRegisters;
    }

    [Flags]
    public enum SnapRules : uint
    {
        HeapList = 0x00000001,
        Process = 0x00000002,
        Thread = 0x00000004,
        Module = 0x00000008,
        Module32 = 0x00000010,
        Inherit = 0x80000000,
        SnapAll = HeapList | Process | Thread | Module,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ThreadEntry
    {
        public int Size;
        public uint UsageCount;
        public int ThreadId;
        public int ProcessId;
        public int KeBasePriority;
        public int DeltaPriority;
        public uint Flags;
    }

    public delegate IntPtr HookDelegate(int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    public static extern IntPtr CallNextHookEx(
        IntPtr Hook,
        int nCode,
        IntPtr wParam,
        IntPtr lParam);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
    public static extern IntPtr SetWindowsHookEx(
        HookType HookType,
        HookDelegate lPfn,
        IntPtr hMod,
        uint dwThreadId);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
    public static extern bool UnhookWindowsHookEx(IntPtr Hook);

    public enum HookType : uint
    {
        JournalRecord = 0,
        JournalPlayback = 1,
        Keyboard = 2,
        GetMessage = 3,
        CallWndProc = 4,
        CBT = 5,
        SysMsgFilter = 6,
        Mouse = 7,
        Hardware = 8,
        Debug = 9,
        Shell = 10,
        ForegroundIdle = 11,
        CallWndProcRet = 12,
        KeyboardLL = 13,
        MouseLL = 14
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ImageDosHeader
    {
        public ushort e_magic;
        public ushort e_cblp;
        public ushort e_cp;
        public ushort e_crlc;
        public ushort e_cparhdr;
        public ushort e_minalloc;
        public ushort e_maxalloc;
        public ushort e_ss;
        public ushort e_sp;
        public ushort e_csum;
        public ushort e_ip;
        public ushort e_cs;
        public ushort e_lfarlc;
        public ushort e_ovno;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public ushort[] e_res;

        public ushort e_oemid;
        public ushort e_oeminfo;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
        public ushort[] e_res2;

        public int e_lfanew;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ImageNtHeader
    {
        public uint Signature;
        public ImageFileHeader FileHeader;
        public ImageOptionalHeader OptionalHeader;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ImageSectionHeader
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public char[] Name;
        public uint VirtualSize;
        public uint VirtualAddress;
        public uint SizeOfRawData;
        public uint PointerToRawData;
        public uint PointerToRelocations;
        public uint PointerToLinenumbers;
        public ushort NumberOfRelocations;
        public ushort NumberOfLinenumbers;
        public DataSectionFlags Characteristics;

        public string Section
        {
            get { return new string(this.Name); }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ImageFileHeader
    {
        public MachineType Machine;
        public ushort NumberOfSections;
        public uint TimeDateStamp;
        public uint PointerToSymbolTable;
        public uint NumberOfSymbols;
        public ushort SizeOfOptionalHeader;
        public ushort Characteristics;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ImageOptionalHeader
    {
        public MagicType Magic;
        public byte MajorLinkerVersion;
        public byte MinorLinkerVersion;
        public uint SizeOfCode;
        public uint SizeOfInitializedData;
        public uint SizeOfUninitializedData;
        public uint AddressOfEntryPoint;
        public uint BaseOfCode;
        public uint BaseOfData;
        public uint ImageBase;
        public uint SectionAlignment;
        public uint FileAlignment;
        public ushort MajorOperatingSystemVersion;
        public ushort MinorOperatingSystemVersion;
        public ushort MajorImageVersion;
        public ushort MinorImageVersion;
        public ushort MajorSubsystemVersion;
        public ushort MinorSubsystemVersion;
        public uint Win32VersionValue;
        public uint SizeOfImage;
        public uint SizeOfHeaders;
        public uint CheckSum;
        public SubSystemType Subsystem;
        public CharacteristicsType DllCharacteristics;
        public uint SizeOfStackReserve;
        public uint SizeOfStackCommit;
        public uint SizeOfHeapReserve;
        public uint SizeOfHeapCommit;
        public uint LoaderFlags;
        public uint NumberOfRvaAndSizes;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public ImageDataDirectory[] DataDirectory;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ImageDataDirectory
    {
        public DataDirectoryType Type;
        public uint VirtualAddress;
        public uint Size;
    }

    public enum DataDirectoryType
    {
        ExportTable = 0,
        ImportTable,
        ResourceTable,
        ExceptionTable,

        CertificateTable,
        BaseRelocationTable,
        Debug,
        Architecture,

        GlobalPtr,
        TLSTable,
        LoadConfigTable,
        BoundImport,

        IAT,
        DelayImportDescriptor,
        CLRRuntimeHeader,
        Reserved,
    }

    public enum MagicType : ushort
    {
        Hdr32 = 0x10b,
        Hdr64 = 0x20b
    }

    public enum SubSystemType : ushort
    {
        Unknown = 0,
        Native = 1,
        WindowsGui = 2,
        WindowsCui = 3,
        PosixCui = 7,
        WindowsCeGui = 9,
        EfiApplication = 10,
        EfiBootServerDriver = 11,
        EfiRuntimeDriver = 12,
        Rom = 13,
        Xbox = 14
    }

    public enum CharacteristicsType : ushort
    {
        RES_0 = 0x0001,
        RES_1 = 0x0002,
        RES_2 = 0x0004,
        RES_3 = 0x0008,
        RES_4 = 0x1000,
        DynamicBase = 0x0040,
        ForceIntegrity = 0x0080,
        NxCompat = 0x0100,
        NoIsolation = 0x0200,
        NoSeh = 0x0400,
        NoBind = 0x0800,
        WdmDriver = 0x2000,
        TerminalServerAware = 0x8000
    }

    public enum MachineType : ushort
    {
        Unknown = 0x0000,
        I386 = 0x014c,
        R3000 = 0x0162,
        R4000 = 0x0166,
        R10000 = 0x0168,
        WCEMIPSV2 = 0x0169,
        Alpha = 0x0184,
        SH3 = 0x01a2,
        SH3DSP = 0x01a3,
        SH4 = 0x01a6,
        SH5 = 0x01a8,
        ARM = 0x01c0,
        Thumb = 0x01c2,
        ARMNT = 0x01c4,
        AM33 = 0x01d3,
        PowerPC = 0x01f0,
        PowerPCFP = 0x01f1,
        IA64 = 0x0200,
        MIPS16 = 0x0266,
        M68K = 0x0268,
        Alpha64 = 0x0284,
        MIPSFPU = 0x0366,
        MIPSFPU16 = 0x0466,
        EBC = 0x0ebc,
        RISCV32 = 0x5032,
        RISCV64 = 0x5064,
        RISCV128 = 0x5128,
        AMD64 = 0x8664,
        ARM64 = 0xaa64,
        LoongArch32 = 0x6232,
        LoongArch64 = 0x6264,
        M32R = 0x9041
    }

    [Flags]
    public enum DataSectionFlags : uint
    {
        TypeReg = 0x00000000,
        TypeDsect = 0x00000001,
        TypeNoLoad = 0x00000002,
        TypeGroup = 0x00000004,
        TypeNoPadded = 0x00000008,
        TypeCopy = 0x00000010,
        ContentCode = 0x00000020,
        ContentInitializedData = 0x00000040,
        ContentUninitializedData = 0x00000080,
        LinkOther = 0x00000100,
        LinkInfo = 0x00000200,
        TypeOver = 0x00000400,
        LinkRemove = 0x00000800,
        LinkComDat = 0x00001000,
        NoDeferSpecExceptions = 0x00004000,
        RelativeGP = 0x00008000,
        MemPurgeable = 0x00020000,
        Memory16Bit = 0x00020000,
        MemoryLocked = 0x00040000,
        MemoryPreload = 0x00080000,
        Align1Bytes = 0x00100000,
        Align2Bytes = 0x00200000,
        Align4Bytes = 0x00300000,
        Align8Bytes = 0x00400000,
        Align16Bytes = 0x00500000,
        Align32Bytes = 0x00600000,
        Align64Bytes = 0x00700000,
        Align128Bytes = 0x00800000,
        Align256Bytes = 0x00900000,
        Align512Bytes = 0x00A00000,
        Align1024Bytes = 0x00B00000,
        Align2048Bytes = 0x00C00000,
        Align4096Bytes = 0x00D00000,
        Align8192Bytes = 0x00E00000,
        LinkExtendedRelocationOverflow = 0x01000000,
        MemoryDiscardable = 0x02000000,
        MemoryNotCached = 0x04000000,
        MemoryNotPaged = 0x08000000,
        MemoryShared = 0x10000000,
        MemoryExecute = 0x20000000,
        MemoryRead = 0x40000000,
        MemoryWrite = 0x80000000
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct LdrData
    {
        public uint Length;
        public byte Initialized;
        public IntPtr SsHandle;
        public ListEntry InLoadOrderModuleList;
        public ListEntry InMemoryOrderModuleList;
        public ListEntry InInitializationOrderModuleList;
        public IntPtr EntryInProgress;
        public byte ShutdownInProgress;
        public IntPtr ShutdownThreadId;
        public ListEntry HashLinks;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ListEntry
    {
        public IntPtr Flink;
        public IntPtr Blink;
    }

    /* Unused Struct.
    [StructLayout(LayoutKind.Sequential)]
    public struct LdrDataTableEntry
    {
        public ListEntry InLoadOrderLinks;
        public ListEntry InMemoryOrderLinks;
        public ListEntry InInitializationOrderLinks;
        public IntPtr DllBase;
        public IntPtr EntryPoint;
        public uint SizeOfImage;
        public UnicodeString FullDllName;
        public UnicodeString BaseDllName;
        public uint Flags;
        public ushort LoadCount;
        public ushort TlsIndex;
        public ListEntry HashLinks;
        public uint TimeDateStamp;
    }
    */

    [StructLayout(LayoutKind.Sequential)]
    public struct UnicodeString
    {
        public ushort Length;
        public ushort MaximumLength;
        public IntPtr Buffer;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Peb
    {
        public byte InheritedAddressSpace;
        public byte ReadImageFileExecOptions;
        public byte BeingDebugged;
        public byte BitField;
        public IntPtr Mutant;
        public IntPtr ImageBaseAddress;
        public LdrData Ldr;
        public ProcessParameters ProcessParameters;
        public IntPtr SubSystemData;
        public IntPtr ProcessHeap;
        public IntPtr FastPebLock;
        public IntPtr FastPebLockRoutine;
        public IntPtr FastPebUnlockRoutine;
        public uint EnvironmentUpdateCount;
        public IntPtr KernelCallbackTable;
        public uint SystemReserved;
        public uint AtlThunkSListPtr32;
        public IntPtr FreeList;
        public uint TlsExpansionCounter;
        public IntPtr TlsBitmap;
        public uint TlsBitmapBits;
        public IntPtr ReadOnlySharedMemoryBase;
        public IntPtr ReadOnlySharedMemoryHeap;
        public IntPtr ReadOnlyStaticServerData;
        public IntPtr AnsiCodePageData;
        public IntPtr OemCodePageData;
        public IntPtr UnicodeCaseTableData;
        public uint NumberOfProcessors;
        public uint NtGlobalFlag;
        public long CriticalSectionTimeout;
        public IntPtr HeapSegmentReserve;
        public IntPtr HeapSegmentCommit;
        public IntPtr HeapDeCommitTotalFreeThreshold;
        public IntPtr HeapDeCommitFreeBlockThreshold;
        public uint NumberOfHeaps;
        public uint MaximumNumberOfHeaps;
        public IntPtr ProcessHeaps;
        public IntPtr GdiSharedHandleTable;
        public IntPtr ProcessStarterHelper;
        public uint GdiDCAttributeList;
        public IntPtr LoaderLock;
        public uint OSMajorVersion;
        public uint OSMinorVersion;
        public ushort OSBuildNumber;
        public ushort OSCSDVersion;
        public uint OSPlatformId;
        public uint ImageSubsystem;
        public uint ImageSubsystemMajorVersion;
        public IntPtr ImageSubsystemMinorVersion;
        public IntPtr ImageProcessAffinityMask;
        public IntPtr GdiHandleBuffer;
        public PostProcessInitRoutine PostProcessInitRoutine;
        public IntPtr TlsExpansionBitmap;
        public uint TlsExpansionBitmapBits;
        public uint SessionId;
        public long AppCompatFlags;
        public long AppCompatFlagsUser;
        public IntPtr pShimData;
        public IntPtr AppCompatInfo;
        public UnicodeString CSDVersion;
        public IntPtr ActivationContextData;
        public IntPtr ProcessAssemblyStorageMap;
        public IntPtr SystemDefaultActivationContextData;
        public IntPtr SystemAssemblyStorageMap;
        public IntPtr MinimumStackCommit;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct ProcessParameters
    {
        public uint MaximumLength;
        public uint Length;

        public uint Flags;
        public uint DebugFlags;

        public IntPtr ConsoleHandle;
        public uint ConsoleFlags;
        public IntPtr StandardInput;
        public IntPtr StandardOutput;
        public IntPtr StandardError;

        public UnicodeString CurrentDirectoryPath;
        public IntPtr CurrentDirectoryHandle;

        public UnicodeString DllPath;
        public UnicodeString ImagePathName;
        public UnicodeString CommandLine;
        public IntPtr Environment;

        public uint StartingX;
        public uint StartingY;
        public uint CountX;
        public uint CountY;
        public uint CountCharsX;
        public uint CountCharsY;
        public uint FillAttribute;

        public uint WindowFlags;
        public uint ShowWindowFlags;
        public UnicodeString WindowTitle;
        public UnicodeString DesktopInfo;
        public UnicodeString ShellInfo;
        public UnicodeString RuntimeData;
        public DriveLetterCurDir[] CurrentDirectories;
        public uint EnvironmentSize;
        public uint EnvironmentVersion;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct DriveLetterCurDir
    {
        public ushort Flags;
        public ushort Length;
        public uint TimeStamp;
        public UnicodeString DosPath;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PostProcessInitRoutine
    {
        public IntPtr Routine;
        public IntPtr Context;
    }

    [DllImport("psapi.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GetProcessInformation(
        IntPtr hProcess,
        int ProcessInformationClass,
        IntPtr ProcessInformation,
        int ProcessInformationLength,
        out int ReturnLength);

    [StructLayout(LayoutKind.Sequential)]
    public struct ProcessBasicInformation
    {
        public IntPtr Reserved1;
        public IntPtr PebBaseAddress;
        public IntPtr Reserved2_0;
        public IntPtr Reserved2_1;
        public IntPtr UniqueProcessId;
        public IntPtr InheritedFromUniqueProcessId;
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern IntPtr VirtualQueryEx(
        IntPtr Process,
        IntPtr Address,
        out MemoryBasicInformation Buffer,
        int Length);

    public enum MemoryState : uint
    {
        Commit = 0x1000,
        Free = 0x10000,
        Reserve = 0x2000,
        Reset = 0x80000,
        LargePages = 0x20000000,
        Physical = 0x400000,
        TopDown = 0x100000,
        WriteWatch = 0x200000
    }

    public enum MemoryType : uint
    {
        Image = 0x1000000,
        Mapped = 0x40000,
        Private = 0x20000
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MemoryBasicInformation
    {
        public IntPtr BaseAddress;
        public IntPtr AllocationBase;
        public AllocationType AllocationProtect;
        public IntPtr RegionSize;
        public MemoryState State;
        public MemoryProtection Protect;
        public MemoryType Type;
    }

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern bool CreateProcess(
        string lpApplicationName,
        string lpCommandLine,
        IntPtr lpProcessAttributes,
        IntPtr lpThreadAttributes,
        bool bInheritHandles,
        ProcessCreationFlags dwCreationFlags,
        IntPtr lpEnvironment,
        string lpCurrentDirectory,
        ref StartupInfo lpStartupInfo,
        out ProcessInformation lpProcessInformation);

    [Flags]
    public enum ProcessCreationFlags : uint
    {
        DebugProcess = 0x00000001,
        DebugOnlyThisProcess = 0x00000002,
        CreateSuspended = 0x00000004,
        DetachedProcess = 0x00000008,
        CreateNewConsole = 0x00000010,
        NormalPriorityClass = 0x00000020,
        IdlePriorityClass = 0x00000040,
        HighPriorityClass = 0x00000080,
        RealtimePriorityClass = 0x00000100,
        CreateNewProcessGroup = 0x00000200,
        CreateUnicodeEnvironment = 0x00000400,
        CreateSeparateWowVdm = 0x00000800,
        CreateSharedWowVdm = 0x00001000,
        CreateForcedOs = 0x00002000,
        BelowNormalPriorityClass = 0x00004000,
        AboveNormalPriorityClass = 0x00008000,
        InheritParentAffinity = 0x00010000,
        InheritCallerPriority = 0x00020000,
        CreateProtectedProcess = 0x00040000,
        ExtendedStartupInfoPresent = 0x00080000,
        ProcessModeBackgroundBegin = 0x00100000,
        ProcessModeBackgroundEnd = 0x00200000,
        CreateBreakawayFromJob = 0x01000000,
        CreatePreserveCodeAuthzLevel = 0x02000000,
        CreateDefaultErrorMode = 0x04000000,
        ProfileUser = 0x10000000,
        ProfileKernel = 0x20000000,
        ProfileServer = 0x40000000,
        CreateIgnoreSystemDefault = 0x80000000,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct StartupInfo
    {
        public int cb;
        public string lpReserved;
        public string lpDesktop;
        public string lpTitle;
        public int dwX;
        public int dwY;
        public int dwXSize;
        public int dwYSize;
        public int dwXCountChars;
        public int dwYCountChars;
        public int dwFillAttribute;
        public int dwFlags;
        public short wShowWindow;
        public short cbReserved2;
        public IntPtr lpReserved2;
        public IntPtr hStdInput;
        public IntPtr hStdOutput;
        public IntPtr hStdError;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ProcessInformation
    {
        public IntPtr hProcess;
        public IntPtr hThread;
        public int dwProcessId;
        public int dwThreadId;
    }

    [DllImport("ntdll.dll", SetLastError = true)]
    public static extern uint RtlGetVersion(out OsVersionInfo VersionInfo);

    [StructLayout(LayoutKind.Sequential)]
    public struct OsVersionInfo
    {
        public readonly uint OsVersionInfoSize;

        public readonly uint MajorVersion;
        public readonly uint MinorVersion;
        public readonly uint BuildNumber;
        public readonly uint PlatformId;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public readonly string CSDVersion;
    }

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern bool SetWindowDisplayAffinity(
        IntPtr Handle,
        DisplayAffinity Affinity);

    [Flags]
    public enum DisplayAffinity : uint
    {
        None = 0x00000000,
        Monitor = 0x00000001,
        Exclude = 0x00000011
    }

    [DllImport("dwmapi.dll", PreserveSig = true)]
    public static extern int DwmSetWindowAttribute(
        IntPtr Handle,
        int Preference,
        ref WindowAttribute Attribute,
        int Size);

    [Flags]
    public enum WindowAttribute : uint
    {
        Default = 0x00000000,
        None = 0x00000001,
        Round = 0x00000002,
        SmallRound = 0x00000003
    }

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern int SetWindowText(
        IntPtr Handle,
        string Text);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool AllocConsole();

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool FreeConsole();
}
