using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Text;

namespace Ring.io
{
    public class ExceptionUtility
    {
        public static bool IsFatal(Exception exception)
        {
            while (exception != null)
            {
                if (//exception as FatalException != null || 
                    exception as OutOfMemoryException != null && exception as InsufficientMemoryException == null || 
                    exception as ThreadAbortException != null || 
                    exception as AccessViolationException != null || 
                    //exception as AssertionFailedException != null || 
                    exception as SEHException != null)
                {
                    return true;
                }
                else
                {
                    if (exception as TypeInitializationException == null && exception as TargetInvocationException == null)
                    {
                        break;
                    }
                    exception = exception.InnerException;
                }
            }
            return false;
        }
    }
}
