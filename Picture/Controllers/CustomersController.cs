using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using iTextSharp.text.pdf;
using iTextSharp.text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Picture.Data;
using Picture.Models;
using Picture.Utilities;

namespace Picture.Controllers
{
    public class CustomersController : Controller
    {
        private readonly PhotosContext _context;

        public CustomersController(PhotosContext context)
        {
            _context = context;
        }

        // GET: Customers
        public async Task<IActionResult> Index()
        {
            var customer = _context.Customers.Include( c => c.CustomerImages).AsNoTracking();
            
            return View(customer);
        }

        // GET: Customers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers
                .Include(c => c.CustomerImages)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);
            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }

        // GET: Customers/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Customers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name")] Customer customer, List<IFormFile> Pictures)
        {
            if (ModelState.IsValid)
            {
                await AddPictures(customer, Pictures);
                _context.Add(customer);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(customer);
        }

        // GET: Customers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var existingCustomer = await _context.Customers.Include(c => c.CustomerImages).FirstOrDefaultAsync(c => c.Id == id);
            if (existingCustomer == null)
            {
                return NotFound();
            }

            // Detach the existing customer before attaching the modified one
            _context.Entry(existingCustomer).State = EntityState.Detached;

            return View(existingCustomer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,CustomerImages")] Customer customer, string chkRemoveImage, List<IFormFile> pictures)
        {
            var customertoupdate = await _context.Customers.Include(c => c.CustomerImages).FirstOrDefaultAsync(c => c.Id == id);

            if (customertoupdate == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (chkRemoveImage != null)
                    {
                        // Remove the image if the checkbox is checked
                        customertoupdate.CustomerImages = null;
                    }

                    // Detach the existing customer before updating
                    _context.Entry(customertoupdate).State = EntityState.Detached;

                    // Attach the modified customer
                    _context.Update(customer);

                    // Add or update pictures
                    await AddPictures(customer, pictures);

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CustomerExists(customer.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(customer);
        }


        // GET: Customers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers
                .FirstOrDefaultAsync(m => m.Id == id);
            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }

        // POST: Customers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer != null)
            {
                _context.Customers.Remove(customer);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CustomerExists(int id)
        {
            return _context.Customers.Any(e => e.Id == id);
        }


        private async Task AddPictures(Customer customer, List<IFormFile> pictures)
        {
            // Get the pictures and save them with the Customer (1 size)
            if (pictures != null && pictures.Any())
            {
                foreach (var picture in pictures)
                {
                    string mimeType = picture.ContentType;
                    long fileLength = picture.Length;

                    if (!(mimeType == "" || fileLength == 0)) // Looks like we have a file!!!
                    {
                        if (mimeType.Contains("image"))
                        {
                            using (var memoryStream = new MemoryStream())
                            {
                                await picture.CopyToAsync(memoryStream);
                                var pictureArray = memoryStream.ToArray(); // Gives us the Byte[]

                                // Create a new CustomerPhoto instance for each image
                                var customerPhoto = new CustomerPhoto
                                {
                                    Content = pictureArray,
                                    MimeType = mimeType
                                };

                                // Check if the CustomerImages list is null
                                if (customer.CustomerImages == null)
                                {
                                    // If it's null, create a new list and add the first image
                                    customer.CustomerImages = new List<CustomerPhoto> { customerPhoto };
                                }
                                else
                                {
                                    // If it's not null, add the image to the existing list
                                    customer.CustomerImages.Add(customerPhoto);
                                }
                            }
                        }
                    }
                }
            }
        }

        public IActionResult DownloadPDF(int id)
        {
            var customer = _context.Customers
                .Include(c => c.CustomerImages)
                .FirstOrDefault(c => c.Id == id);

            if (customer == null)
            {
                return NotFound();
            }

            var document = new Document();
            using (var memoryStream = new MemoryStream())
            {
                PdfWriter.GetInstance(document, memoryStream);
                document.Open();

              
                document.Add(new Paragraph("Customer Details"));
                document.Add(new Paragraph($"Customer ID: {customer.Id}"));
                document.Add(new Paragraph($"Name: {customer.Name}"));

                
                if (customer.CustomerImages != null && customer.CustomerImages.Count > 0)
                {
                    foreach (var image in customer.CustomerImages)
                    {
                        var imageBytes = image.Content;
                        var img = iTextSharp.text.Image.GetInstance(imageBytes);
                        img.ScaleToFit(300f, 300f); 
                        document.Add(img);
                    }
                }

                document.Close();

               
                var pdfData = memoryStream.ToArray();
                return File(pdfData, "application/pdf", "CustomerDetails.pdf");
            }
        }
    }
}
