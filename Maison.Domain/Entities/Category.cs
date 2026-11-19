using System;
using System.Collections.Generic;

namespace Maison.Domain.Entities
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string GenderSector { get; set; } = "Unisex"; // Male, Female, Unisex
        public string Occasion { get; set; } = "Casual";     // Summer, Winter, Autumn, Eid, Puja, Formal, Casual, Office
        public string? Description { get; set; }
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}