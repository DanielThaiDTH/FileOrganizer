using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileOrganizerCore;
using FileDBManager.Entities;
using Microsoft.Extensions.Logging;

namespace FileOrganizerUI.CodeBehind
{
    public class SearchParser
    {
        enum State
        {
            Base,
            Plain,
            Raw,
            FilterType,
            Equal,
            Like,
            And,
            Sub,
            Exit,
            Fail
        }

        enum FilterType
        {
            Filename,
            Tag,
            Path,
            FileType,
            Altname
        }

        Dictionary<State, Dictionary<char, State>> stateTransition;
        static Dictionary<string, FilterType> typeMap = new Dictionary<string, FilterType>
        {
            { "file", FilterType.Filename },
            { "filename", FilterType.Filename },
            { "tag", FilterType.Tag },
            { "type", FilterType.FileType },
            { "path", FilterType.Path },
            { "altname", FilterType.Altname }
        };

        FileSearchFilter filter;
        ILogger logger;
        public FileSearchFilter Filter { get { return filter; } }

        public SearchParser(ILogger logger) 
        {
            this.logger = logger;
            stateTransition = new Dictionary<State, Dictionary<char, State>>();
            
            var baseTransition = new Dictionary<char, State>();
            baseTransition.Add(' ', State.Base);
            baseTransition.Add('\\', State.FilterType);
            baseTransition.Add('(', State.Sub);
            baseTransition.Add(')', State.Exit);
            baseTransition.Add('+', State.And);
            baseTransition.Add('"', State.Raw);
            baseTransition.Add('=', State.Fail);
            baseTransition.Add('~', State.Fail);
            stateTransition.Add(State.Base, baseTransition);

            var plainTransition = new Dictionary<char, State>();
            plainTransition.Add(' ', State.Base);
            plainTransition.Add('+', State.And);
            plainTransition.Add('=', State.Fail);
            plainTransition.Add('~', State.Fail);
            plainTransition.Add('(', State.Fail);
            plainTransition.Add(')', State.Exit);
            plainTransition.Add('"', State.Fail);
            stateTransition.Add(State.Plain, plainTransition);

            var rawTransition = new Dictionary<char, State>();
            rawTransition.Add('"', State.Base);
            stateTransition.Add(State.Raw, rawTransition);

            var filterTypeTransition = new Dictionary<char, State>();
            filterTypeTransition.Add(' ', State.Fail);
            filterTypeTransition.Add('\\', State.Fail);
            filterTypeTransition.Add('+', State.Fail);
            filterTypeTransition.Add('=', State.Equal);
            filterTypeTransition.Add('~', State.Like);
            filterTypeTransition.Add('(', State.Fail);
            filterTypeTransition.Add(')', State.Fail);
            filterTypeTransition.Add('"', State.Fail);
            stateTransition.Add(State.FilterType, filterTypeTransition);

            var equalTransition = new Dictionary<char, State>();
            equalTransition.Add(' ', State.Fail);
            equalTransition.Add('\\', State.Fail);
            equalTransition.Add('+', State.Fail);
            equalTransition.Add('=', State.Fail);
            equalTransition.Add('~', State.Fail);
            equalTransition.Add('(', State.Fail);
            equalTransition.Add(')', State.Fail);
            equalTransition.Add('"', State.Raw);
            stateTransition.Add(State.Equal, equalTransition);

            var likeTransition = new Dictionary<char, State>();
            likeTransition.Add(' ', State.Fail);
            likeTransition.Add('\\', State.Fail);
            likeTransition.Add('+', State.Fail);
            likeTransition.Add('=', State.Fail);
            likeTransition.Add('~', State.Fail);
            likeTransition.Add('(', State.Fail);
            likeTransition.Add(')', State.Fail);
            likeTransition.Add('"', State.Raw);
            stateTransition.Add(State.Like, likeTransition);

            var andTransition = new Dictionary<char, State>();
            andTransition.Add(' ', State.And);
            andTransition.Add('\\', State.Fail);
            andTransition.Add('+', State.Fail);
            andTransition.Add('=', State.Fail);
            andTransition.Add('~', State.Fail);
            andTransition.Add('(', State.Sub);
            andTransition.Add(')', State.Fail);
            andTransition.Add('"', State.Raw);
            stateTransition.Add(State.And, andTransition);

            var subTransition = new Dictionary<char, State>();
            subTransition.Add(' ', State.Base);
            subTransition.Add('\\', State.FilterType);
            subTransition.Add('+', State.Fail);
            subTransition.Add('=', State.Fail);
            subTransition.Add('~', State.Fail);
            subTransition.Add('(', State.Sub);
            subTransition.Add(')', State.Fail);
            subTransition.Add('"', State.Raw);
            stateTransition.Add(State.Sub, subTransition);

            filter = new FileSearchFilter();
        }

        public void Reset() { filter = new FileSearchFilter(); }

        State GetNextState(char token, State current)
        {
            if (current == State.Exit || current == State.Fail) {
                return current;
            }

            try {
                State next = stateTransition[current][token];
                return next;
            } catch {
                logger.LogDebug($"Defaulting to current state for token:{token} when in state {current}");
                return current;
            }
        }

        /// <summary>
        ///     Parses a search query. If an error is found while parsing the 
        ///     query, will return false and set an error message in the out 
        ///     parameter.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        public bool Parse(string query, out string errMsg)
        {
            bool result = true;
            var queryFilter = new FileSearchFilter();
            var subFilter = new FileSearchFilter().SetOr(true);
            Queue<char> tokens = new Queue<char>(query);
            string tempStore = "";
            char current;
            State currentState = State.Base;
            bool currentExact = false;
            FilterType currentType = FilterType.Filename;
            Stack<State> stateStack = new Stack<State>();
            Stack<FileSearchFilter> filterStack = new Stack<FileSearchFilter>();
            errMsg = "";
            int position = 0;
            
            while (tokens.Count > 0) {
                current = tokens.Dequeue();
                State next = GetNextState(current, currentState);
                
                if (next == State.Fail) {
                    result = false;
                    errMsg = $"Unexpected token '{current}' found at position {position}";
                    break;
                }

                if (next == currentState) {
                    switch (next) {
                        case State.FilterType:
                        case State.Raw:
                        case State.Plain:
                            tempStore += current;
                            break;
                        case State.Base:
                        default:
                            break;
                    }
                } else {
                    if (currentState == State.Plain || currentState == State.Raw) {
                        switch (currentType) {
                            case FilterType.Filename:
                                subFilter.SetFilenameFilter(tempStore, currentExact);
                                break;
                            case FilterType.Tag:
                                subFilter.SetTagFilter(new List<string> { tempStore });
                                break;
                            case FilterType.Path:
                                subFilter.SetPathFilter(tempStore, currentExact);
                                break;
                            case FilterType.FileType:
                                subFilter.SetFileTypeFilter(tempStore, currentExact);
                                break;
                            case FilterType.Altname:
                                subFilter.SetAltnameFilter(tempStore, currentExact);
                                break;
                        }
                        if (filterStack.Count > 0) {
                            filterStack.Peek().AddSubfilter(subFilter);
                        } else {
                            queryFilter.AddSubfilter(subFilter);
                        }
                        subFilter = new FileSearchFilter().SetOr(true);
                        currentExact = false;
                        currentType = FilterType.Filename;
                    } else if (currentState == State.FilterType) {
                        try {
                            currentType = typeMap[tempStore.ToLowerInvariant()];
                        } catch {
                            result = false;
                            errMsg = $"Unknown filter type {tempStore}";
                            break;
                        }
                    }

                    tempStore = "";
                    if (next == State.Sub) {
                        filterStack.Push(subFilter);
                        stateStack.Push(State.Base);
                        next = State.Base;
                        subFilter = new FileSearchFilter().SetOr(true);
                        currentExact = false;
                        currentType = FilterType.Filename;
                    } else if (next == State.Exit) {
                        if (filterStack.Count == 0) {
                            result = false;
                            errMsg = $"Missing opening bracket";
                            break;
                        }
                        filterStack.Pop();
                        next = stateStack.Pop();
                        subFilter = new FileSearchFilter().SetOr(true);
                        currentExact = false;
                        currentType = FilterType.Filename;
                    } else if (next == State.Plain) {
                        tempStore += current;
                    } else if (next == State.And) {
                        subFilter.SetOr(false);
                    } else if (next == State.Like) {
                        currentExact = false;
                    } else if (next == State.Equal) {
                        currentExact = true;
                    }
                }

                currentState = next;
                position++;
            }

            if (result && filterStack.Count == 0) {
                filter.AddSubfilter(queryFilter);
            } else if (result && filterStack.Count > 0) {
                result = false;
                errMsg = $"Missing {filterStack.Count} closing bracket pairs";
            }

            return result;
        }

        public void AddFilter(FileSearchFilter subFilter)
        {
            filter.AddSubfilter(subFilter);
        }
    }
}
