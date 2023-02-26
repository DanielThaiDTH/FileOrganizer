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
        Empty
    }

    public class ActionResult
    {
        List<ErrorType> type;
        List<string> message;
        Dictionary<ErrorType, string> defaultMessage;
        Type resultType;
        object result;
        public int Count { get { return type.Count; } }
        public Type ResultType { get { return resultType; } }
        public object Result { get { return result; } }


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
                { ErrorType.Empty, "No error" }
            };
            resultType = null;
            result = null;
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

        public void SetResult(Type resultType, object result)
        {
            this.resultType = resultType;
            this.result = result;
        }
    }
}
