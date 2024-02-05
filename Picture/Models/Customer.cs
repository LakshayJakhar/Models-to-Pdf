using System.ComponentModel.DataAnnotations;

namespace Picture.Models
{
    public class Customer
    {
        public int Id { get; set; }

        [Display(Name="Full Name")]
        [Required(ErrorMessage ="Can't leave null")]
        [StringLength(50,ErrorMessage ="Maximum 50 words only")]
        public string Name { get; set; }

        public List<CustomerPhoto> CustomerImages { get; set; }
    }
}
