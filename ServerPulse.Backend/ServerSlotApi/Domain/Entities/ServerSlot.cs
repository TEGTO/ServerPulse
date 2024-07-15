using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServerSlotApi.Domain.Entities
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
        [Required]
        public string SlotKey { get; set; } = default!;
        public DateTime CreationDate { get; set; } = DateTime.UtcNow;

        public ServerSlot()
        {
            SlotKey = Guid.NewGuid().ToString();
        }

        public void Copy(ServerSlot other)
        {
            this.Name = other.Name;
        }
    }
}