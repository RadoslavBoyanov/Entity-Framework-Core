﻿namespace VaporStore.Data.Models
{
    public class Tag
    {
        public Tag()
        {
            this.GameTags = new HashSet<GameTag>();
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public ICollection<GameTag> GameTags { get; set; }
    }
}
