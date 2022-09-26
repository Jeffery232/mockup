using System;

namespace mockup.Model
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public byte[] PasswordHash { get; set; }= new byte[32];

        public byte[] PasswordSalt { get; set; } = new byte[32];

        public string VerificationToken { get; set; }

        public DateTime VerifiedAt { get; set; }
        public string PasswordRestToken { get; set; }

        public DateTime ResetTokenExpires { get; set; }

    }
}
