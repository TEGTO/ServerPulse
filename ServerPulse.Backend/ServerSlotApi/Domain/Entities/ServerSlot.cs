using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServerSlotApi.Domain.Entities
{
    public class ServerSlot
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; } = default!;
        public string UserEmail { get; set; } = default!;
        [MaxLength(256)]
        public string Name { get; set; } = default!;
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string SlotKey { get; set; } = default!;
        public DateTime CreationDate { get; set; } = DateTime.UtcNow;

        public void Copy(ServerSlot other)
        {
            this.UserEmail = other.UserEmail;
            this.Name = other.Name;
        }
    }
}