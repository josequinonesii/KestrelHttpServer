// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Threading;

namespace Microsoft.AspNet.Server.Kestrel.Networking
{
    public abstract class UvHandle : UvMemory
    {
        static Libuv.uv_close_cb _destroyMemory = DestroyMemory;
        Action<Action<IntPtr>, IntPtr> _queueCloseHandle;

        unsafe protected void CreateHandle(
            Libuv uv,
            int threadId,
            int size,
            Action<Action<IntPtr>, IntPtr> queueCloseHandle)
        {
            Debug.Assert(queueCloseHandle != null);
            _queueCloseHandle = queueCloseHandle;
            CreateMemory(uv, threadId, size);
        }

        protected override bool ReleaseHandle()
        {
            var memory = handle;
            if (memory != IntPtr.Zero)
            {
                handle = IntPtr.Zero;

                if (Thread.CurrentThread.ManagedThreadId == ThreadId)
                {
                    _uv.close(memory, _destroyMemory);
                }
                else if (_queueCloseHandle != null)
                {
                    _queueCloseHandle(memory2 => _uv.close(memory2, _destroyMemory), memory);
                }
                else
                {
                    var x = 5;
                }
            }
            return true;
        }

        public void Reference()
        {
            _uv.@ref(this);
        }

        public void Unreference()
        {
            _uv.unref(this);
        }
    }
}
