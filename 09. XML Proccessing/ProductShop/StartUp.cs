using ProductShop.Data;
using ProductShop.Dtos.Export;
using ProductShop.Dtos.Import;
using ProductShop.Models;
using ProductShop.XMLHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace ProductShop
{
    public class StartUp
    {
        public static void Main(string[] args)
        {
            var context = new ProductShopContext();
            var productsXml = File.ReadAllText("../../../Datasets/products.xml");

            var result = ImportProducts(context, productsXml);
            System.Console.WriteLine(result);
        }

        //01. Import Users
        public static string ImportUsers(ProductShopContext context, string inputXml)
        {
            const string rootName = "Users";

            var usersResult = XMLConverter.Deserializer<ImportUserDto>(inputXml, rootName);

            List<User> users = new List<User>();

            foreach (var importUserDto in usersResult)
            {
                var user = new User
                {
                    FirstName = importUserDto.FirstName,
                    LastName = importUserDto.LastName,
                    Age = importUserDto.Age
                };

                users.Add(user);
            }

            context.Users.AddRange(users);
            context.SaveChanges();

            return $"Successfully imported {users.Count}";
        }

        //02. Import Products
        public static string ImportProducts(ProductShopContext context, string inputXml)
        {
            const string rootName = "Products";

            var productsResult = XMLConverter.Deserializer<ImportProductDto>(inputXml, rootName);

            var products = productsResult
                .Select(p => new Product
                {
                    Name = p.Name,
                    Price = p.Price,
                    BuyerId = p.BuyerId,
                    SellerId = p.SellerId
                })
                .ToArray();

            context.Products.AddRange(products);
            context.SaveChanges();

            return $"Successfully imported {products.Length}";
        }

        //03. Import Categories
        public static string ImportCategories(ProductShopContext context, string inputXml)
        {
            const string root = "Categories";

            var categoriesDto = XMLConverter.Deserializer<ImportCategoryDto>(inputXml, root);

            var categories = categoriesDto
                .Where(x => x.Name != null)
                .Select(c => new Category
                {
                    Name = c.Name
                })
                .ToArray();

            context.Categories.AddRange(categories);
            context.SaveChanges();

            return $"Successfully imported {categories.Length}";
        }

        //04. Import Categories and Products
        public static string ImportCategoryProducts(ProductShopContext context, string inputXml)
        {
            const string root = "CategoryProducts";

            var categoryProductResult = XMLConverter.Deserializer<ImportCategoryProductDto>(inputXml, root);

            var categoriesProducts = categoryProductResult
                .Where(x => context.Categories.Any(i => i.Id == x.CategoryId) && context.Products.Any(i => i.Id == x.ProductId))
                .Select(cp => new CategoryProduct
                {
                    CategoryId = cp.CategoryId,
                    ProductId = cp.ProductId,
                }).ToArray();

            context.CategoryProducts.AddRange(categoriesProducts);
            context.SaveChanges();

            return $"Successfully imported {categoriesProducts.Length}";
        }

        //05. Export Products In Range
        public static string GetProductsInRange(ProductShopContext context)
        {
            const string rootElement = "Products";

            var products = context
                .Products
                .Where(p => p.Price >= 500 && p.Price <= 1000)
                .Select(p => new ExportProductDto
                {
                    Name = p.Name,
                    Price = p.Price,
                    BuyerFullName = p.Buyer.FirstName + " " + p.Buyer.LastName
                })
                .OrderBy(p => p.Price)
                .Take(10)
                .ToArray();

            var resultXml = XMLConverter.Serialize(products, rootElement);

            return resultXml;
        }

        //06. Export Sold Products
        public static string GetSoldProducts(ProductShopContext context)
        {
            const string rootElement = "Users";

            var users = context
                .Users
                .Where(u => u.ProductsSold.Count > 0)
                .Select(u => new ExportSoldProductsDto
                {
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    soldProducts = u.ProductsSold
                        .Select(ps => new ProductDto
                        {
                            Name = ps.Name,
                            Price = ps.Price
                        }).ToList()
                })
                .OrderBy(x => x.LastName)
                .ThenBy(x => x.FirstName)
                .Take(5)
                .ToArray();

            var resultXml = XMLConverter.Serialize(users, rootElement);

            return resultXml;
        }

        //07. Export Categories By Products Count
        public static string GetCategoriesByProductsCount(ProductShopContext context)
        {
            const string rootName = "Categories";

            var resultCategories = context
                .Categories
                .Select(c => new ExportCategoriesDto
                {
                    Name = c.Name,
                    Count = c.CategoryProducts.Count,
                    AveragePrice = c.CategoryProducts.Average(cp => cp.Product.Price),
                    TotalRevenue = c.CategoryProducts.Sum(cp => cp.Product.Price)
                })
                .OrderByDescending(c => c.Count)
                .ThenBy(c => c.TotalRevenue)
                .ToArray();

            var resultXml = XMLConverter.Serialize(resultCategories, rootName);

            return resultXml;
        }

        //08. Export Users and Products
        public static string GetUsersWithProducts(ProductShopContext context)
        {
            const string rootName = "Users";

            var targetUsers = context
                .Users
                .AsEnumerable()
                .Where(u => u.ProductsSold.Count > 0)
                .Select(u => new UserInfo
                {
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Age = u.Age,
                    SoldProducts = new SoldProductCount
                    {
                        Count = u.ProductsSold.Count,
                        Products = u.ProductsSold
                            .Select(ps => new ProductsInfo
                            {
                                Name = ps.Name,
                                Price = ps.Price
                            })
                            .OrderByDescending(x => x.Price)
                            .ToList()
                    }
                })
                .OrderByDescending(x => x.SoldProducts.Count)
                .Take(10)
                .ToList();

            var finalObj = new ExportUsersWithProductsDto
            {
                Count = context.Users.Count(x => x.ProductsSold.Any()),
                Users = targetUsers
            };

            var resultXml = XMLConverter.Serialize(finalObj, rootName);

            return resultXml;
        }
    }
}