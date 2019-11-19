using Contacts.API.DB;
using Contacts.API.Entities;
using Contacts.API.Helpers;
using Contacts.API.Models;
using Contacts.API.ResourceParameters;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Contacts.API.Services
{
    public class ContactRepo : IContactRepo
    {
        private readonly ContactDb _context;
        private readonly IPropertyMappingService _propertyMappingService;

        public ContactRepo(ContactDb context,
            IPropertyMappingService propertyMappingService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _propertyMappingService = propertyMappingService ??
                throw new ArgumentNullException(nameof(propertyMappingService));
        }

        public Contact GetContact(int id)
        {
            return _context.Contacts.FirstOrDefault(c => c.Id == id);
        }

        public PagedList<Contact> GetContacts(ContactsResourceParameters contactsResourceParameters)
        {
            if (contactsResourceParameters == null)
            {
                throw new ArgumentNullException(nameof(contactsResourceParameters));
            }

            var collection = _context.Contacts as IQueryable<Contact>;

            if (contactsResourceParameters.id != 0)
            {
                collection = _context.Contacts.Where(c => c.Id == contactsResourceParameters.id);
            }

            if (!string.IsNullOrWhiteSpace(contactsResourceParameters.firstname))
            {
                var firstName = contactsResourceParameters.firstname.Trim();
                collection = _context.Contacts.Where(c => c.FirstName == firstName);
            }

            if (!string.IsNullOrWhiteSpace(contactsResourceParameters.search))
            {
                var searchQuery = contactsResourceParameters.search.Trim();
                collection = collection.Where(c => 
                    c.FirstName.Contains(searchQuery) ||
                    c.LastName.Contains(searchQuery) ||
                    c.Phone.Contains(searchQuery) ||
                    c.Email.Contains(searchQuery));
            }

            if (!string.IsNullOrWhiteSpace(contactsResourceParameters.OrderBy))
            {
                //get property mapping dictionary
                var contactPropertyMappingDictionary =
                    _propertyMappingService.GetPropertyMapping<ContactDto, Contact>();

                collection = collection.ApplySort(contactsResourceParameters.OrderBy, contactPropertyMappingDictionary);
            }
            else
            {
                collection = collection.OrderBy(c => c.Id);
            }

            return PagedList<Contact>.Create(collection,
                contactsResourceParameters.PageNumber,
                contactsResourceParameters.PageSize);
        }

        public Contact AddContact(Contact contact)
        {
            _context.Contacts.Add(contact);
            _context.SaveChanges();
            return contact;
        }

        public Contact UpdateContact(Contact contact)
        {
            var result = _context.Contacts.First(c => c.Id == contact.Id);
            if (result != null)
            {
                result.FirstName = contact.FirstName;
                result.LastName = contact.LastName;
                result.Phone = contact.Phone;
                result.Email = contact.Email;
                _context.SaveChanges();
            }
            
            return contact;
        }

        public Contact DeleteContact(Contact contact)
        {
            var contactToDelete = _context.Contacts.First(c => c.Id == contact.Id);
            if (contactToDelete != null)
            {
                _context.Contacts.Remove(contactToDelete);
            }

            _context.SaveChanges();
            return contact;
        }
    }
}
