using System;

namespace Authorization.API.Model
{
    public class ErrorMessage : IEquatable<ErrorMessage>
    {
        public static readonly String Unauthorized = "Unauthorized";
        public static readonly String InvalidSession = "Invalid Session";
        public static readonly String InvalidUser = "Invalid user and / or password";
        public static readonly String Nonexistent = "Nonexistent User";
        public static readonly String Duplicated = "E-mail already exists";

        public String Message { get; set; }

        //Unitary Tests Purpouse
        public bool Equals(ErrorMessage other)
        {
            return (Message.CompareTo(other.Message) == 0);
        }
    }
}
