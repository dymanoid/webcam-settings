using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace WebcamSettings;

internal sealed class CleanupContainer : IDisposable
{
    private List<object>? _resources = [];

    public void Add(object resource)
    {
        ArgumentNullException.ThrowIfNull(resource, nameof(resource));
        ObjectDisposedException.ThrowIf(_resources == null, this);

        if (resource is IDisposable || resource is IntPtr || resource.GetType().IsCOMObject)
        {
            _resources.Add(resource);
        }
    }

    void IDisposable.Dispose()
    {
        if (_resources == null)
        {
            return;
        }

        foreach (object resource in _resources)
        {
            EnsureCleanup(resource);
        }

        _resources = null;
    }

    private static void EnsureCleanup(object resource)
    {
        switch (resource)
        {
            case IDisposable disposable:
                disposable.Dispose();
                break;
            default:
                if (OperatingSystem.IsWindows())
                {
                    if (resource is nint ptr)
                    {
                        Marshal.Release(ptr);
                    }
                    else if (resource.GetType().IsCOMObject)
                    {
                        while (Marshal.ReleaseComObject(resource) > 0) { }
                    }
                }

                break;
        }
    }
}
