﻿using GeographyModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


public partial class GeographyRepository 
{
    readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
    protected void WriteByLock(Action action)
    {
        _lock.EnterWriteLock();
        try
        {
            action?.Invoke();
        }
        finally { _lock.ExitWriteLock(); }
    }

    protected T WriteByLock<T>(Func<T> func)
    {
        _lock.EnterWriteLock();
        try
        {
            return func.Invoke();
        }
        finally { _lock.ExitWriteLock(); }
    }

    protected void ReadByLock(Action action)
    {
        _lock.EnterReadLock();
        try
        {
            action?.Invoke();
        }
        finally { _lock.ExitReadLock(); }
    }

    protected T ReadByLock<T>(Func<T> func)
    {
        _lock.EnterReadLock();
        try
        {
            return func.Invoke();
        }
        finally { _lock.ExitReadLock(); }
    }
}
