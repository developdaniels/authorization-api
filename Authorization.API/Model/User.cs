using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Authorization.API.Model
{
    public class Telephone
    {
        [JsonIgnore]
        public int Id { get; set; }

        public String Number { get; set; }
    }

    public class User
    {
        [Key]
        public Guid Id { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime LastUpdatedOn { get; set; }

        [NotMapped]
        public String Token { get; set; }

        [JsonIgnore]
        public byte[] TokenHashed { get; set; }
        public DateTime LastLoginOn { get; set; }

        [JsonIgnore]
        public String Name { get; set; }

        [JsonIgnore]
        public String Email { get; set; }

        [JsonIgnore]
        public byte[] Password { get; set; }

        [JsonIgnore]
        public List<Telephone> Telephones { get; set; }

        [JsonIgnore]
        public int TelephoneId { get; set; }

        public User(String Name, String Email, byte[] Password, List<Telephone> Telephones, String Token = null)
        {
            this.Name = Name;
            this.Email = Email;
            this.Password = Password;
            this.Telephones = Telephones;
            this.Token = Token;

            Id = Guid.NewGuid();
            CreatedOn = DateTime.Now;
            LastUpdatedOn = DateTime.Now;
            LastLoginOn = DateTime.Now;
        }

        public User()
        {
            Id = new Guid();
        }
    }
    
}
