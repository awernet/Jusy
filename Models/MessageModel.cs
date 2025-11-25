using System;

namespace Jusy.Models
{
    public class MessageModel
    {
        public int Id { get; set; }
        public int ItemId { get; set; }
        public string UserName { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
