using Contacts.API.Entities;
using Contacts.API.ResourceParameters;
using Contacts.API.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Contacts.API.Services
{
    public interface IContactRepo
    {
        Contact GetContact(int id);
        PagedList<Contact> GetContacts(ContactsResourceParameters contactsResourceParameters);
        Contact AddContact(Contact contact);
        Contact UpdateContact(Contact contact);
        Contact DeleteContact(Contact contact);
    }
}
