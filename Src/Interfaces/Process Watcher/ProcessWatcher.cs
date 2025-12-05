using System;
using System.Diagnostics;
using System.Timers;

namespace Nihon.Watcher;

public class ProcessWatcher : IDisposable
{
    public delegate void CreatedDelegate(Process Proc);
    public event CreatedDelegate Created;

    private Timer Timer { get; } = new Timer();
    private string ProcessName;
    private Process[] Processes;
    private Process Process;

    private bool IsDisposed = false;

    public double Interval { get; set; }

    public ProcessWatcher(string ProcessName) => this.ProcessName = ProcessName;

    public void Init()
    {
        this.Timer.Interval = this.Interval;
        this.Timer.Elapsed += this.Elapsed;
        this.Timer.Start();
    }

    protected virtual void Elapsed(object sender, ElapsedEventArgs e)
    {
        this.Processes = Process.GetProcessesByName(this.ProcessName);
        if (this.Processes.Length < 1) return;

        this.OnProcessCreated(this.Processes[0]);
        Created?.Invoke(this.Processes[0]);
    }

    protected virtual void OnProcessCreated(Process Process)
    {
        this.Timer.Stop();
        this.Process = Process;
        Process.EnableRaisingEvents = true;

        Process.Exited += (object Sender, EventArgs Handler) => this.Timer.Start();
    }

    public void Dispose()
    {
        if (this.IsDisposed) return;

        this.Timer.Dispose();
        this.IsDisposed = true;
        GC.SuppressFinalize(this);
    }
}