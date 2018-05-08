using Authorization.API.Model;
using System;
using System.Collections.Generic;

namespace Authorization.API.DTO
{
    public class UserDTO : IEquatable<UserDTO>
    {
        public UserDTO(User user)
        {
            Name = user.Name;
            Email = user.Email;
            Telephones = user.Telephones;
            Id = user.Id;
            CreatedOn = user.CreatedOn;
            LastUpdatedOn = user.LastUpdatedOn;
            LastLoginOn = user.LastLoginOn;
        }

        public String Name { get; set; }
        public String Email { get; set; }
        public List<Telephone> Telephones { get; set; }
        public Guid Id { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime LastUpdatedOn { get; set; }
        public DateTime LastLoginOn { get; set; }

        public bool Equals(UserDTO other)
        {
            return
            (
                Name == other.Name
                 && Email == other.Email
                 && Telephones == other.Telephones

                 && Id == other.Id
                 && CreatedOn == other.CreatedOn
                 && LastUpdatedOn == other.LastUpdatedOn
                 && LastLoginOn == other.LastLoginOn
            );
        }
    }
}
