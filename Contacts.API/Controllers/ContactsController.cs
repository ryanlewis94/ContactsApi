using AutoMapper;
using Contacts.API.Entities;
using Contacts.API.Helpers;
using Contacts.API.Models;
using Contacts.API.ResourceParameters;
using Contacts.API.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Contacts.API.Controllers
{
    [ApiController]
    [Route("api/contacts")]
    //[ResponseCache(CacheProfileName = "240SecondsCacheProfile")]
    public class ContactsController : ControllerBase
    {
        private IContactRepo _contactRepo;
        private IMapper _mapper;
        private IPropertyMappingService _propertyMappingService;
        private IPropertyCheckerService _propertyCheckerService;

        public ContactsController(IContactRepo contactRepo, IMapper mapper, 
            IPropertyMappingService propertyMappingService,
            IPropertyCheckerService propertyCheckerService)
        {
            _contactRepo = contactRepo ??
                throw new ArgumentNullException(nameof(contactRepo));
            _mapper = mapper ??
                throw new ArgumentNullException(nameof(mapper));
            _propertyMappingService = propertyMappingService ??
                throw new ArgumentNullException(nameof(propertyMappingService));
            _propertyCheckerService = propertyCheckerService ??
                throw new ArgumentNullException(nameof(propertyCheckerService));
        }
        //Get all contacts
        [HttpGet(Name = "GetContacts")]
        [HttpHead]
        //[ResponseCache(Duration = 0)]
        public IActionResult GetContactsList(
            [FromQuery] ContactsResourceParameters contactsResourceParameters)
        {
            if (!_propertyMappingService.ValidMappingExistsFor<ContactDto, Contact>
                (contactsResourceParameters.OrderBy))
            {
                return BadRequest();
            }

            if (!_propertyCheckerService.TypeHasProperties<Contact>
                (contactsResourceParameters.Fields))
            {
                return BadRequest();
            }

            var ContactsFromRepo = _contactRepo.GetContacts(contactsResourceParameters);

            var paginationMetaData = new
            {
                totalCount = ContactsFromRepo.TotalCount,
                pageSize = ContactsFromRepo.PageSize,
                currentPage = ContactsFromRepo.CurrentPage,
                totalPages = ContactsFromRepo.TotalPages
            };

            Response.Headers.Add("X-Pagination",
                JsonSerializer.Serialize(paginationMetaData));

            var links = CreateLinksForContacts(contactsResourceParameters,
                ContactsFromRepo.HasNext, ContactsFromRepo.HasPrevious);

            var shapedContacts = ContactsFromRepo.ShapeDatas(contactsResourceParameters.Fields);

            var shapedContactsWithLinks = shapedContacts.Select(contact =>
            {
                var contactAsDictionary = contact as IDictionary<string, object>;
                //var contactLinks = CreateLinksForContact((int)contactAsDictionary["Id"], null);
                //contactAsDictionary.Add("links", contactLinks);
                return contactAsDictionary;
            });

            var linkedCollectionResource = new
            {
                value = shapedContactsWithLinks,
                links
            };

            return Ok(shapedContacts);
        }

        //Get a contact by ID
        [HttpGet("id={contactId}", Name ="GetContact")]
        [ResponseCache(Duration = 120)]
        public IActionResult GetContact(int contactId, string fields,
            [FromHeader(Name = "Accept")] string mediaType)
        {
            if (!MediaTypeHeaderValue.TryParse(mediaType,
                out MediaTypeHeaderValue parsedMediaType))
            {
                return BadRequest();
            }

            if (!_propertyCheckerService.TypeHasProperties<Contact>
                (fields))
            {
                return BadRequest();
            }

            var ContactFromRepo = _contactRepo.GetContact(contactId);

            if (ContactFromRepo == null)
            {
                return NotFound();
            }

            return Ok(ContactFromRepo.ShapeData(fields));
        }

        //Add a contact
        [HttpPost(Name = "CreateContact")]
        public ActionResult<Contact> CreateContact(Contact contact)
        {
            _contactRepo.AddContact(contact);

            return NoContent();
        }

        //Update a contact
        [HttpPut(Name = "UpdateContact")]
        public ActionResult<Contact> UpdateContact(Contact contact)
        {
            var ContactFromRepo = _contactRepo.GetContact(contact.Id);
            if (ContactFromRepo == null)
            {
                return NotFound();
            }

            _contactRepo.UpdateContact(contact);

            return NoContent();
        }

        //Delete a contact
        [HttpDelete(Name = "DeleteContact")]
        public ActionResult<Contact> DeleteContact(Contact contact)
        {
            var ContactFromRepo = _contactRepo.GetContact(contact.Id);
            if (ContactFromRepo == null)
            {
                return NotFound();
            }

            _contactRepo.DeleteContact(contact);

            return NoContent();
        }

        //Get the options that the user has available
        [HttpOptions]
        public ActionResult GetContactsOptions()
        {
            Response.Headers.Add("Allow", "GET,POST,PUT,DELETE,OPTIONS");
            return Ok();
        }

        private string CreateContactsResourceUri(
            ContactsResourceParameters contactsResourceParameters, ResourceUriType type)
        {
            switch (type)
            {
                case ResourceUriType.PreviousPage:
                    return Url.Link("GetContacts",
                        new
                        {
                            fields = contactsResourceParameters.Fields,
                            orderBy = contactsResourceParameters.OrderBy,
                            pageNumber = contactsResourceParameters.PageNumber - 1,
                            pageSize = contactsResourceParameters.PageSize,
                            iD = contactsResourceParameters.id,
                            firstName = contactsResourceParameters.firstname,
                            Search = contactsResourceParameters.search
                        });
                case ResourceUriType.NextPage:
                    return Url.Link("GetContacts",
                        new
                        {
                            fields = contactsResourceParameters.Fields,
                            orderBy = contactsResourceParameters.OrderBy,
                            pageNumber = contactsResourceParameters.PageNumber + 1,
                            pageSize = contactsResourceParameters.PageSize,
                            iD = contactsResourceParameters.id,
                            firstName = contactsResourceParameters.firstname,
                            Search = contactsResourceParameters.search
                        });
                case ResourceUriType.Current:
                    return Url.Link("GetContacts",
                        new
                        {
                            fields = contactsResourceParameters.Fields,
                            orderBy = contactsResourceParameters.OrderBy,
                            pageNumber = contactsResourceParameters.PageNumber + 1,
                            pageSize = contactsResourceParameters.PageSize,
                            iD = contactsResourceParameters.id,
                            firstName = contactsResourceParameters.firstname,
                            Search = contactsResourceParameters.search
                        });
                default:
                    return Url.Link("GetContacts",
                        new
                        {
                            fields = contactsResourceParameters.Fields,
                            orderBy = contactsResourceParameters.OrderBy,
                            pageNumber = contactsResourceParameters.PageNumber,
                            pageSize = contactsResourceParameters.PageSize,
                            iD = contactsResourceParameters.id,
                            firstName = contactsResourceParameters.firstname,
                            Search = contactsResourceParameters.search
                        });
            }
        }

        private IEnumerable<LinkDto> CreateLinksForContact(int contactId, string fields)
        {
            var links = new List<LinkDto>();

            if (string.IsNullOrWhiteSpace(fields))
            {
                links.Add(
                new LinkDto(Url.Link("GetContact", new { contactId }),
                "self",
                "GET"));
            }
            else
            {
                links.Add(
                new LinkDto(Url.Link("GetContact", new { contactId, fields }),
                "self",
                "GET"));
            }            

            links.Add(
                    new LinkDto(Url.Link("GetContacts", new { contactId }),
                    "get_contacts",
                    "GET"));

            links.Add(
                    new LinkDto(Url.Link("DeleteContact", new { contactId }),
                    "delete_contact",
                    "DELETE"));

            links.Add(
                    new LinkDto(Url.Link("CreateContact", new { contactId }),
                    "create_contact",
                    "POST"));

            links.Add(
                    new LinkDto(Url.Link("UpdateContact", new { contactId }),
                    "update_contact",
                    "PUT"));

            return links;
        }

        private IEnumerable<LinkDto> CreateLinksForContacts(
            ContactsResourceParameters contactsResourceParameters, 
            bool hasNext, bool hasPrevious)
        {
            var links = new List<LinkDto>();

            //links.Add(
            //    new LinkDto(CreateContactsResourceUri(
            //        contactsResourceParameters, ResourceUriType.Current),
            //    "self",
            //    "GET"));

            if (hasNext)
            {
                links.Add(
                  new LinkDto(CreateContactsResourceUri(
                      contactsResourceParameters, ResourceUriType.NextPage),
                  "nextPage", 
                  "GET"));
            }

            if (hasPrevious)
            {
                links.Add(
                    new LinkDto(CreateContactsResourceUri(
                        contactsResourceParameters, ResourceUriType.PreviousPage),
                    "previousPage", 
                    "GET"));
            }

            return links;
        }
    }
}
