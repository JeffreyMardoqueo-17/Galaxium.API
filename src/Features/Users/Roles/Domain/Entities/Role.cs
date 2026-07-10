
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Galaxium.API.Entities
{
    public class Role
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = null!;

        public ICollection<User> Users { get; set; } = new List<User>();
    }
}
