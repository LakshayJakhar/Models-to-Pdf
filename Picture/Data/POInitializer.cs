using Picture.Models;
using System.Diagnostics;
namespace Picture.Data
{
    public static class POInitializer
    {
        public static void Seed(IApplicationBuilder applicationBuilder)
        {
            PhotosContext context = applicationBuilder.ApplicationServices.CreateScope()
                .ServiceProvider.GetRequiredService<PhotosContext>();

            try
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                if (!context.Customers.Any())
                {
                    context.Customers.AddRange(
                        new Customer
                        {
                            Name = "Kamil"
                        }, new Customer
                        {
                            Name = "Bilal"
                        }, new Customer
                        {
                            Name = "Mannat"
                        }, new Customer
                        {
                            Name = "Rishi"
                        });
                    context.SaveChanges();
                }
            }catch (Exception ex)
            {
                Debug.WriteLine(ex.GetBaseException().Message);
            }
        }
    }
}
