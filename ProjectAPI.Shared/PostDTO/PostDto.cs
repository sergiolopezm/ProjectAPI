﻿namespace ProjectAPI.Shared.PostDTO
{
    public class PostDto
    {
        public int PostId { get; set; }
        public string? Title { get; set; }
        public string? Body { get; set; }
        public int Type { get; set; }
        public string? Category { get; set; }
        public int CustomerId { get; set; }
        public string? CustomerName { get; set; }
    }
}
