using Contacts.API.Entities;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Contacts.API.DB
{
    public class ContactDb : DbContext
    {
        public ContactDb(DbContextOptions<ContactDb> options)
           : base(options)
        {
        }
        public DbSet<Contact> Contacts { get; set; }
    }
}
