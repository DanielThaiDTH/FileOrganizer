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

        Dictionary<State, Dictionary<char, State>> stateTransition;

        FileSearchFilter filter;
        ILogger logger;
        public FileSearchFilter Filter { get { return filter; } }

        public SearchParser(ILogger logger) 
        {
            this.logger = logger;
            stateTransition = new Dictionary<State, Dictionary<char, State>>();
            
            var baseTransistion = new Dictionary<char, State>();
            baseTransistion.Add(' ', State.Base);
            baseTransistion.Add('\\', State.FilterType);
            baseTransistion.Add('(', State.Sub);
            baseTransistion.Add(')', State.Exit);
            baseTransistion.Add('+', State.And);
            baseTransistion.Add('"', State.Raw);
            baseTransistion.Add('=', State.Fail);
            baseTransistion.Add('~', State.Fail);
            stateTransition.Add(State.Base, baseTransistion);

            var plainTransition = new Dictionary<char, State>();
            plainTransition.Add(' ', State.Base);
            plainTransition.Add('+', State.And);
            plainTransition.Add('=', State.Fail);
            plainTransition.Add('~', State.Fail);
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
            andTransition.Add(' ', State.Base);
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
                logger.LogDebug($"Defaulting to Base state for token:{token} when in state {current}");
                return State.Base;
            }
        }

        public bool Parse(string query)
        {
            bool result = true;
            Stack<char> tokens = new Stack<char>(query.Reverse());
            Stack<char> lookahead = new Stack<char>();
            char current;
            State currentState = State.Base;
            Stack<State> parseState = new Stack<State>();
            
            while (tokens.Count > 0) {
                current = tokens.Pop();
                State next = GetNextState(current, currentState);

            }

            

            return result;
        }

        public void AddFilter(FileSearchFilter subFilter)
        {
            filter.AddSubfilter(subFilter);
        }
    }
}
