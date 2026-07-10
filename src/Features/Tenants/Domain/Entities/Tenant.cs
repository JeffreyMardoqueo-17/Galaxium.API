using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Galaxium.API.Entities
{
    public class Tenant
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = null!;

        public DateTime CreatedAt { get; set; }

        public ICollection<User> Users { get; set; } = new List<User>();
    }
}