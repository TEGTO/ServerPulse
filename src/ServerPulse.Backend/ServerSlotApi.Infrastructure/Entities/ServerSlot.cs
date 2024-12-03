using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServerSlotApi.Infrastructure.Entities
{
    public class ServerSlot
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; } = default!;
        [Required]
        [MaxLength(256)]
        public string UserEmail { get; set; } = default!;
        [Required]
        [MaxLength(256)]
        public string Name { get; set; } = default!;
        public string SlotKey { get; }
        public DateTime CreationDate { get; }

        public ServerSlot()
        {
            SlotKey = Guid.NewGuid().ToString();
            CreationDate = DateTime.UtcNow;
        }

        public void Copy(ServerSlot other)
        {
            Name = other.Name;
        }
    }
}