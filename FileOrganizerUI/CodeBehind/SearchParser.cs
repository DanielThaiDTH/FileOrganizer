using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileOrganizerCore;
using FileDBManager.Entities;

namespace FileOrganizerUI.CodeBehind
{
    public class SearchParser
    {
        FileSearchFilter filter;
        public FileSearchFilter Filter { get { return filter; } }

        public SearchParser() { filter = new FileSearchFilter(); }

        public void Reset() { filter = new FileSearchFilter(); }

        public bool Parse(string query)
        {
            bool result = true;

            

            return result;
        }

        public void AddFilter(FileSearchFilter subFilter)
        {
            filter.AddSubfilter(subFilter);
        }
    }
}
