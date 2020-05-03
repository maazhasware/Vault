using System;
using System.Collections.Generic;
using System.Text;

namespace csharpcorner.Models
{
    public class User
    {
        public Guid UserID { get; set; }
        public string FirstName { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public byte[] Key { get; set; }
        public byte[] Salt { get; set; }
    }
}
