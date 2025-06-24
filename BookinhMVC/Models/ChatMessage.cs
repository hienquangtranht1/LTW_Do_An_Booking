using System;
using System.ComponentModel.DataAnnotations;

namespace BookinhMVC.Models
{
    public class ChatMessage
    {
        [Key]
        public int Id { get; set; }
        public int SenderId { get; set; }
        public int ReceiverId { get; set; }
        [Required]
        public string Message { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; } = false;
    }
}