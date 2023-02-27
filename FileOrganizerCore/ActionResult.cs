using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileOrganizerCore
{
    public enum ErrorType
    {
        SQL,
        SymLinkCreate,
        SymLinkDelete,
        Path,
        Access,
        Empty
    }

    /// <summary>
    ///     Encapsulates errors into a return result. Is 
    ///     used for setting error messages for display.
    /// </summary>
    public class ActionResult
    {
        protected List<ErrorType> type;
        protected List<string> message;
        protected Dictionary<ErrorType, string> defaultMessage;
        public int Count { get { return type.Count; } }

        public static void AppendErrors<T1, T2>(ActionResult<T1> r1, ActionResult<T2> r2)
        {
            for (int i = 0; i < r2.Count; i++) {
                r1.AddError(r2.GetError(i), r2.GetErrorMessage(i));
            }
        }
    }

    public class ActionResult<T> : ActionResult
    {
        
        T result;
        public T Result { get { return result; } }

        public ActionResult()
        {
            type = new List<ErrorType>();
            message = new List<string>();
            defaultMessage = new Dictionary<ErrorType, string>()
            {
                { ErrorType.SQL, "An database access error was encountered" },
                { ErrorType.SymLinkCreate, "Error creating a symlink" },
                { ErrorType.SymLinkDelete, "Error raised when removing a symlink" },
                { ErrorType.Path, "Path error was raised due to a badly formed path" },
                { ErrorType.Access, "Error accessing a resource" },
                { ErrorType.Empty, "No error" }
            };
            result = default(T);
        }

        public ErrorType GetError(int idx)
        {
            if (idx >= Count) {
                return ErrorType.Empty;
            } else {
                return type[idx];
            }
        }

        public string GetErrorMessage(int idx)
        {
            if (idx >= Count) {
                return "";
            } else {
                return message[idx];
            }
        }

        public bool HasError(ErrorType err)
        {
            return type.Contains(err);
        }

        public bool HasError()
        {
            return type.Count > 0;
        }

        /// <summary>
        ///     Adds an error to the result object. 
        /// </summary>
        /// <param name="err"></param>
        /// <param name="msg"></param>
        public void AddError(ErrorType err, string msg = null)
        {
            type.Add(err);
            if (msg is null) {
                message.Add(defaultMessage[err]);
            } else {
                message.Add(msg);
            }
        }

        public void SetResult(T result)
        {
            this.result = result;
        }

        
    }
}
