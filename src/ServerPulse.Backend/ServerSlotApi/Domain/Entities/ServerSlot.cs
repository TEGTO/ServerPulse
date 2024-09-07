﻿using System.ComponentModel.DataAnnotations;
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
        public string SlotKey { get; private init; }
        public DateTime CreationDate { get; private init; }

        public ServerSlot()
        {
            SlotKey = Guid.NewGuid().ToString();
            CreationDate = DateTime.UtcNow;
        }

        public void Copy(ServerSlot other)
        {
            this.Name = other.Name;
        }
    }
}