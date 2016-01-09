using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Globalization;

namespace EtcdNet
{
    using DTO;
    /// <summary>
    /// Represents the generic exception from etcd
    /// https://github.com/coreos/etcd/blob/master/Documentation/errorcode.md
    /// EtcdGenericException
    ///  ├── EtcdCommonException
    ///  |    ├─ KeyNotFound
    ///  |    ├─ TestFailed
    ///  |    ├─ NotFile
    ///  |    ├─ NotDir
    ///  |    ├─ NodeExist
    ///  |    ├─ RootReadOnly
    ///  |    └─ DirNotEmpty
    ///  ├── EtcdPostFormException
    ///  |    ├─ PrevValueRequired
    ///  |    ├─ TTLNaN
    ///  |    ├─ IndexNaN
    ///  |    ├─ InvalidField
    ///  |    └─ InvalidForm
    ///  ├── EtcdRaftException
    ///  |    ├─ RaftInternal
    ///  |    └─ LeaderElect
    ///  └── EtcdException
    ///       ├─ WatcherCleared
    ///       └─ EventIndexCleared
    /// </summary>
    public class EtcdGenericException : Exception
    {
        public ErrorCode Code { get; private set; }
        public string Cause { get; private set; }

        protected EtcdGenericException(string message) : base(message) { }

        public static EtcdGenericException Create(HttpRequestMessage request, ErrorResponse errorResponse)
        {
            int code = errorResponse.ErrorCode;

            string message = string.Format( CultureInfo.InvariantCulture
                , "{0} {1} failed. {2}; Code={3}; Cause=`{4}`; "
                , request.Method.ToString().ToUpperInvariant()
                , request.RequestUri
                , errorResponse.Message
                , errorResponse.ErrorCode
                , errorResponse.Cause
                );

            EtcdGenericException exception;
            if (code >= 100 && code <= 199)
                exception = EtcdCommonException.Create(code, message);
            else if (code >= 200 && code <= 299)
                exception = EtcdPostFormException.Create(code, message);
            else if (code >= 300 && code <= 399)
                exception = EtcdRaftException.Create(code, message);
            else if (code >= 400 && code <= 499)
                exception = EtcdException.Create(code, message);
            else
                exception = new EtcdGenericException(message);

            exception.Code = (ErrorCode)code;
            exception.Cause = errorResponse.Cause;
            return exception;
        }
    }

    public class EtcdCommonException : EtcdGenericException
    {
        internal static EtcdCommonException Create(int code, string message)
        {
            ErrorCode errorCode = (ErrorCode)code;
            switch(errorCode)
            {
                case ErrorCode.KeyNotFound:
                    return new KeyNotFound(message);

                case ErrorCode.TestFailed:
                    return new TestFailed(message);

                case ErrorCode.NotFile:
                    return new NotFile(message);

                case ErrorCode.NotDir:
                    return new NotDir(message);

                case ErrorCode.NodeExist:
                    return new NodeExist(message);

                case ErrorCode.RootReadOnly:
                    return new RootReadOnly(message);

                case ErrorCode.DirNotEmpty:
                    return new DirNotEmpty(message);

                default:
                    return new EtcdCommonException(message);
            }
        }

        internal EtcdCommonException(string message) : base(message) { }

        public class KeyNotFound : EtcdCommonException
        {
            internal KeyNotFound(string message) : base(message) { }
        }

        public class TestFailed : EtcdCommonException
        {
            internal TestFailed(string message) : base(message) { }
        }

        public class NotFile : EtcdCommonException
        {
            internal NotFile(string message) : base(message) { }
        }

        public class NotDir : EtcdCommonException
        {
            internal NotDir(string message) : base(message) { }
        }

        public class NodeExist : EtcdCommonException
        {
            internal NodeExist(string message) : base(message) { }
        }

        public class RootReadOnly : EtcdCommonException
        {
            internal RootReadOnly(string message) : base(message) { }
        }

        public class DirNotEmpty : EtcdCommonException
        {
            internal DirNotEmpty(string message) : base(message) { }
        }
    }

    

    public class EtcdPostFormException : EtcdGenericException
    {
        internal static EtcdPostFormException Create(int code, string message)
        {
            ErrorCode errorCode = (ErrorCode)code;
            switch (errorCode)
            {
                case ErrorCode.PrevValueRequired:
                    return new PrevValueRequired(message);

                case ErrorCode.TTLNaN:
                    return new TTLNaN(message);

                case ErrorCode.IndexNaN:
                    return new IndexNaN(message);

                case ErrorCode.InvalidField:
                    return new InvalidField(message);

                case ErrorCode.InvalidForm:
                    return new InvalidForm(message);

                default:
                    return new EtcdPostFormException(message);
            }
        }

        internal EtcdPostFormException(string message) : base(message) { }

        public class PrevValueRequired : EtcdPostFormException
        {
            internal PrevValueRequired(string message) : base(message) { }
        }

        public class TTLNaN : EtcdPostFormException
        {
            internal TTLNaN(string message) : base(message) { }
        }

        public class IndexNaN : EtcdPostFormException
        {
            internal IndexNaN(string message) : base(message) { }
        }

        public class InvalidField : EtcdPostFormException
        {
            internal InvalidField(string message) : base(message) { }
        }

        public class InvalidForm : EtcdPostFormException
        {
            internal InvalidForm(string message) : base(message) { }
        }
    }

    

    public class EtcdRaftException : EtcdGenericException
    {
        internal static EtcdRaftException Create(int code, string message)
        {
            ErrorCode errorCode = (ErrorCode)code;
            switch (errorCode)
            {
                case ErrorCode.RaftInternal:
                    return new Internal(message);

                case ErrorCode.LeaderElect:
                    return new LeaderElect(message);

                default:
                    return new EtcdRaftException(message);
            }
        }

        internal EtcdRaftException(string message) : base(message) { }

        public class Internal : EtcdRaftException
        {
            internal Internal(string message) : base(message) { }
        }

        public class LeaderElect : EtcdRaftException
        {
            internal LeaderElect(string message) : base(message) { }
        }
    }

    

    public class EtcdException : EtcdGenericException
    {
        internal static EtcdException Create(int code, string message)
        {
            ErrorCode errorCode = (ErrorCode)code;
            switch (errorCode)
            {
                case ErrorCode.WatcherCleared:
                    return new WatcherCleared(message);

                case ErrorCode.LeaderElect:
                    return new EventIndexCleared(message);

                default:
                    return new EtcdException(message);
            }
        }


        internal EtcdException(string message) : base(message) { }

        public class WatcherCleared : EtcdException
        {
            internal WatcherCleared(string message) : base(message) { }
        }

        public class EventIndexCleared : EtcdException
        {
            internal EventIndexCleared(string message) : base(message) { }
        }
    }

    
}
