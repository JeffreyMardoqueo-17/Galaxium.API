using System.Collections.Generic;

namespace Galaxium.API.Entities
{
    public class Role
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;

        // 🔹 Navegación
        public ICollection<User> Users { get; set; } = new List<User>();
    }
}
