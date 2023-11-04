using FolderHierarchyViewer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Linq;

namespace FolderHierarchyViewer.Controllers
{
    public class CatalogController : Controller
    {
        private readonly CatalogDbContext _context;

        public CatalogController(CatalogDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("SelectRootCatalog");
            }

            var catalog = _context.Catalogs
                .Include(c => c.Children)
                .Include(c => c.Parent)
                .FirstOrDefault(c => c.Id == id);

            if (catalog == null)
            {
                return NotFound();
            }

            return View(catalog);
        }



        public IActionResult ChildCatalogs(int id)
        {
            var catalog = _context.Catalogs
                .Include(c => c.Children)
                .Include(c => c.Parent)
                .FirstOrDefault(c => c.Id == id);

            if (catalog == null)
            {
                return NotFound();
            }

            return View("Index", catalog);
        }

        public void LoadDirectoryStructure(string path)
        {
            var rootDirectory = new DirectoryInfo(path);
            var rootCatalog = new Catalog { Name = rootDirectory.Name };
            _context.Catalogs.Add(rootCatalog);
            _context.SaveChanges();

            LoadSubDirectories(rootDirectory, rootCatalog.Id);
        }

        private void LoadSubDirectories(DirectoryInfo directoryInfo, int parentId)
        {
            foreach (var subDirectory in directoryInfo.GetDirectories())
            {
                var catalog = new Catalog { Name = subDirectory.Name, ParentId = parentId };
                _context.Catalogs.Add(catalog);
                _context.SaveChanges();

                LoadSubDirectories(subDirectory, catalog.Id);
            }
        }

        public void ExportDirectoryStructure(string filePath)
        {
            var catalogs = _context.Catalogs.ToList();
            var dataTable = new DataTable("Catalogs");

            dataTable.Columns.Add("Id", typeof(int));
            dataTable.Columns.Add("Name", typeof(string));
            dataTable.Columns.Add("ParentId", typeof(int));

            foreach (var catalog in catalogs)
            {
                dataTable.Rows.Add(catalog.Id, catalog.Name, catalog.ParentId);
            }

            dataTable.WriteXml(filePath);
        }
        public IActionResult Import()
        {
            string path = "D:\\ConsoleApplication1";

            LoadDirectoryStructure(path);

            return RedirectToAction("Index");
        }

        public IActionResult Export()
        {
            string filePath = "D:\\new.txt";

            ExportDirectoryStructure(filePath);

            return RedirectToAction("Index");
        }

        public IActionResult SelectRootCatalog()
        {
            var rootCatalogs = _context.Catalogs.Where(c => c.ParentId == null).ToList();
            return View(rootCatalogs);
        }


    }
}
