using GeoNorgeAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kartverket.MetadataEditor.Models
{
    public class MetadataViewModel
    {
        public string Uuid { get; set; }
        public string HierarchyLevel { get; set; }
        public string Title { get; set; }
        public string Purpose { get; set; }
        public string Abstract { get; set; }

        public Contact ContactPublisher { get; set; }
        public Contact ContactPointOfContact { get; set; }
        public Contact ContactAuthor { get; set; }
        public List<Contact> ContactOthers { get; set; }


        /* dataset only */
        public string SupplementalDescription { get; set; }
        public string SpecificUsage { get; set; }  // bruksområde
        public string ResourceIdentifierName { get; set; }  // teknisk navn
        

    }

    public class Contact
    {
        public string Name { get; set; }
        public string Organization { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }

        public Contact() { }
        public Contact(SimpleContact incoming, string defaultRole) 
        {
            if (incoming != null)
            {
                Name = incoming.Name;
                Organization = incoming.Organization;
                Email = incoming.Email;
                Role = incoming.Role;
            }
            else
            {
                Role = defaultRole;
            }
        }

        internal SimpleContact ToSimpleContact()
        {
            return new SimpleContact
            {
                Name = Name,
                Organization = Organization,
                Email = Email,
                Role = Role
            };
        }
    }

}