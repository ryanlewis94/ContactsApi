using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Contacts.API.ResourceParameters
{
    public class ContactsResourceParameters
    {
        const int maxPageSize = 2000;
        public int id { get; set; }
        public string firstname { get; set; }
        public string search { get; set; }
        public int PageNumber { get; set; } = 1;
        private int _pageSize { get; set; } = 100;
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = (value > maxPageSize) ? maxPageSize : value;
        }

        public string OrderBy { get; set; }
        public string Fields { get; set; }
    }
}
