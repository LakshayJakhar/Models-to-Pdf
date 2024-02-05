using Microsoft.EntityFrameworkCore;
using Picture.Models;

namespace Picture.Data
{
    public class PhotosContext :DbContext
    {
        public PhotosContext(DbContextOptions<PhotosContext> options):base(options)
        {

        }

        public DbSet<Customer> Customers { get; set; }

        public DbSet<CustomerPhoto> CustomerPhotos { get; set; }

    }
}
