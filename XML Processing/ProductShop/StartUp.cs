using System.Security.Cryptography;
using System.Threading.Channels;
using ProductShop.Data;
using System.Xml.Serialization;
using ProductShop.DTOs.Import;
using ProductShop.Models;
using System.Text;
using ProductShop.DTOs.Export;

namespace ProductShop
{
    public class StartUp
    {
        public static void Main()
        {
            ProductShopContext dbContext = new ProductShopContext();
            string xml = File.ReadAllText("../../../Datasets/categories-products.xml");
            //dbContext.Database.EnsureDeleted();
            //dbContext.Database.EnsureCreated();

            //Console.WriteLine("Database created successfully!");

            //Import
            //1
            //Console.WriteLine(ImportUsers(dbContext, xml));
            //2
            //Console.WriteLine(ImportProducts(dbContext, xml));
            //3
            //Console.WriteLine(ImportCategories(dbContext, xml));
            //4
            //Console.WriteLine(ImportCategoryProducts(dbContext, xml));


            //Export
            //5
            //Console.WriteLine(GetProductsInRange(dbContext));
            //6
            //Console.WriteLine(GetSoldProducts(dbContext));
            //7
            //Console.WriteLine(GetCategoriesByProductsCount(dbContext));
            //8
            //Console.WriteLine(GetUsersWithProducts(dbContext));
        }
        //1
        public static string ImportUsers(ProductShopContext context, string inputXml)
        {
            string rootName = "Users";
            UsersImportDto[] usersImportDtos = Deserialize<UsersImportDto[]>(inputXml, rootName);
            User[] users = usersImportDtos.Select(x => new User()
            {
                FirstName = x.FirstName,
                LastName = x.LastName,
                Age = x.Age
            }).ToArray();

            context.Users.AddRange(users);
            context.SaveChanges();


            return $"Successfully imported {users.Length}";
        }
        //2
        public static string ImportProducts(ProductShopContext context, string inputXml)
        {
            string rootName = "Products";
            ProductsImportDto[] productsImportDtos = Deserialize<ProductsImportDto[]>(inputXml, rootName);

            Product[] products = productsImportDtos.Select(x=> new Product()
            {
                Name = x.Name,
                Price = x.Price,
                SellerId = x.SellerId,
                BuyerId = x.BuyerId
            }).ToArray();

            context.Products.AddRange(products);
            context.SaveChanges();

            return $"Successfully imported {products.Length}";
        }
        //3
        public static string ImportCategories(ProductShopContext context, string inputXml)
        {
            string rootName = "Categories";
            CategoriesImportDto[] categoriesImportDtos = Deserialize<CategoriesImportDto[]>(inputXml, rootName);

            Category[] categories = categoriesImportDtos.Select(x=> new Category()
            {
                Name = x.Name
            }).ToArray();
            context.Categories.AddRange(categories);
            context.SaveChanges();

            return $"Successfully imported {categories.Length}";
        }
        //4
        public static string ImportCategoryProducts(ProductShopContext context, string inputXml)
        {
            string rootName = "CategoryProducts";
            ImportCategoriesProductsDto[] categoriesProductsDtos =
                Deserialize<ImportCategoriesProductsDto[]>(inputXml, rootName);

            ICollection<CategoryProduct> categoryProducts = new List<CategoryProduct>();

            foreach (var cpDto in categoriesProductsDtos)
            {
                CategoryProduct currentCP = new CategoryProduct();
                if (cpDto.CategoryId == 0 || cpDto.ProductId == 0)
                {
                    continue;
                }

                currentCP.CategoryId = cpDto.CategoryId;
                currentCP.ProductId = cpDto.ProductId;

                categoryProducts.Add(currentCP);
            }

            categoryProducts = categoryProducts.Distinct().ToList();

            context.CategoryProducts.AddRange(categoryProducts);
            context.SaveChanges();

            return $"Successfully imported {categoryProducts.Count}";
        }
        //5
        public static string GetProductsInRange(ProductShopContext context)
        {
            ProductsInRangeExportDto[] products = context.Products
                .Where(x => x.Price >= 500 && x.Price <= 1000)
                .Select(p => new ProductsInRangeExportDto()
                {
                    Name = p.Name,
                    Price = p.Price,
                    Buyer = $"{p.Buyer.FirstName} {p.Buyer.LastName}"
                })
                .OrderBy(x => x.Price).Take(10).ToArray();

            return Serialize(products, "Products");
        }
        //6
        public static string GetSoldProducts(ProductShopContext context)
        {
            UserSoldProductsDto[] exportUsers = context.Users
                .Where(u => u.ProductsSold.Count > 0)
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .Take(5)
                .Select(u => new UserSoldProductsDto()
                {
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    SoldProducts = u.ProductsSold.Select(p => new  SoldProductsDto()
                    {
                        Name = p.Name,
                        Price = p.Price
                    }).ToArray()
                }).ToArray();

            return Serialize(exportUsers, "Users");
        }

        //7
        public static string GetCategoriesByProductsCount(ProductShopContext context)
        {
            CategoriesByProductsDto[] categories = context.Categories
                .Select(c => new CategoriesByProductsDto()
                {
                    Name = c.Name,
                    Count = c.CategoryProducts.Count,
                    AveragePrice = c.CategoryProducts.Average(c => c.Product.Price),
                    TotalRevenue = c.CategoryProducts.Sum(c => c.Product.Price)
                }).OrderByDescending(x => x.Count)
                .ThenBy(x => x.TotalRevenue).ToArray();

            return Serialize(categories, "Categories");
        }

        //8
        public static string GetUsersWithProducts(ProductShopContext context)
        {
            var usersAndProducts = context.Users
                .Where(u => u.ProductsSold.Any())
                .OrderByDescending(u => u.ProductsSold.Count)
                .Take(10)
                .Select(u => new UsersAndProductsDto()
                {
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Age = u.Age,
                    Products = new ProductsSoldDto()
                    {
                        Count = u.ProductsSold.Count,
                        Producst = u.ProductsSold.Select(ps => new SoldProductsDto()
                        {
                            Name = ps.Name,
                            Price = ps.Price,
                        }).OrderByDescending(ps => ps.Price).ToArray()
                    }
                })
                .ToArray();

            var result = new UsersCountDto()
            {
                Count = context.Users.Count(u => u.ProductsSold.Any()),
                Users = usersAndProducts
            };

            return Serialize(result, "Users");
        }

        //Helper method Import
        private static T Deserialize<T>(string inputXml, string rootName)
        {
            XmlRootAttribute xmlRoot = new XmlRootAttribute(rootName);
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T), xmlRoot);

            using StringReader reader = new StringReader(inputXml);

            T dtos = (T)xmlSerializer.Deserialize(reader);

            return dtos;
        }


        //Helper method Export
        private static string Serialize<T>(T dto, string rootName)
        {
            StringBuilder builder = new StringBuilder();
            XmlRootAttribute xmlRoot = new XmlRootAttribute(rootName);
            XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
            namespaces.Add(string.Empty, string.Empty);
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T), xmlRoot);

            using StringWriter writer = new StringWriter(builder);
            xmlSerializer.Serialize(writer, dto, namespaces);

            return builder.ToString().TrimEnd();
        }


    }
}