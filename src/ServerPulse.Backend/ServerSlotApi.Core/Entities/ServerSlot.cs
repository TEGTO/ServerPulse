using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServerSlotApi.Core.Entities
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
        [Required]
        public DateTime CreationDate { get; set; }

        public void Copy(ServerSlot other)
        {
            Name = other.Name;
        }
    }
}