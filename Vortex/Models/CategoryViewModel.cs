using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Vortex.Models
{
    public class CategoryViewModel
    {
        public int Id { get; set; }
        
        public string Name { get; set; }

        public int? ParentId { get; set; }

    }
}
