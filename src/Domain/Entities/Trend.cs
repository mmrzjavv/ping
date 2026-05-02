using System;
using TwitterClone.Domain.Common;

namespace TwitterClone.Domain.Entities
{
    public class Trend : AuditableEntity
    {
        public int Id { get; set; }
        public string Tag { get; set; }
        public int PostCount { get; set; }
        public int LikeCount { get; set; }
        public DateTime RecordedAt { get; set; }
    }
}