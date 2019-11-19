
using System.ComponentModel.DataAnnotations;

namespace Contacts.API.Entities
{
    public class Contact
    {
        public int Id { get; set; }
        [Required]
        public string FirstName { get; set; }
        public string LastName { get; set; }
        [Required]
        [MinLength(3)]
        [MaxLength(15)]
        //[Phone]
        public string Phone { get; set; }
        //[EmailAddress]
        public string Email { get; set; }
    }
}
