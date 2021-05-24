using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace WebcamSettings
{
    internal sealed class CleanupContainer : IDisposable
    {
        private List<object> _resources = new();

        public void Add(object resource)
        {
            if (resource is null)
            {
                throw new ArgumentNullException(nameof(resource));
            }

            if (_resources == null)
            {
                throw new ObjectDisposedException(typeof(CleanupContainer).FullName);
            }

            if (resource is IDisposable || resource is IntPtr || resource.GetType().IsCOMObject)
            {
                _resources.Add(resource);
            }
        }

        void IDisposable.Dispose()
        {
            foreach (object resource in _resources)
            {
                EnsureCleanup(resource);
            }

            _resources = null;
        }

        private static void EnsureCleanup(object resource)
        {
            if (resource is IDisposable disposable)
            {
                disposable.Dispose();
            }
            else if (OperatingSystem.IsWindows())
            {
                if (resource is IntPtr ptr)
                {
                    Marshal.Release(ptr);
                }
                else if (resource.GetType().IsCOMObject)
                {
                    while (Marshal.ReleaseComObject(resource) > 0) { }
                }
            }
        }
    }
}
