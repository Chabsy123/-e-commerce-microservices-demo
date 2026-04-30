using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ProductApi.Domain.Entities;
using ProductApi.Infrastruct.Data;
using ProductApi.Infrastruct.Repositories;
using System.Linq.Expressions;

namespace UnitTest.ProductApi.Repositories
{
    public class ProductRepositoryTest
    {
        private readonly ProductDbContext productDbContext;
        private readonly ProductRepository productRepository;

        public ProductRepositoryTest()
        {
            var options = new DbContextOptionsBuilder<ProductDbContext>()
                .UseInMemoryDatabase(databaseName: "ProductDb")
                .Options;

            productDbContext = new ProductDbContext(options);
            productRepository = new ProductRepository(productDbContext);
        }

        //CREATE PRODUCT 
        [Fact]
        public async Task CreateAsync_WhenProductAlreadyExist_ReturnErrorResponse()
        {
            //arrange 
            var existingProduct = new Product { Name = "ExistingProduct" };
            productDbContext.Products.Add(existingProduct);
            await productDbContext.SaveChangesAsync();

            //Act
            var result = await productRepository.CreateAsync(existingProduct);

            //Assert
            result.Should().NotBeNull();
            result.Flag.Should().BeFalse();
            result.Message.Should().Be("ExistingProduct already added");
        }

        [Fact]
        public async Task CreateAsync_WhenProductDoesNotExist_AddProductAndReturnsSuccessResponse()
        {
            //Arrange 
            var product = new Product() { Name = "NewProduct" };

            //Act
            var result = await productRepository.CreateAsync(product);

            //Assert
            result.Should().NotBeNull();
            result.Flag.Should().BeTrue();
            result.Message.Should().Be("NewProduct added to database successfully");
        }

        //DELETE PRODUCT
        [Fact]
        public async Task DeleteAsync_WhenProductIsFound_ReturnsSuccessResponse()
        {
            //Arrange
            var product = new Product() { Id = 1, Name = "Existing Product", Price = 76.87m, Quantity = 100 };
            productDbContext.Products.Add(product);

            //Act
            var result = await productRepository.DeleteAsync(product);

            //Assert
            result.Should().NotBeNull();
            result.Flag.Should().BeTrue();
            result.Message.Should().Be("Existing Product deleted successfully");
        } 

        [Fact]
        public async Task DeleteAsync_WhenProductIsNotFound_ReturnsNotFoundResponse()
        {
            //Arrange
            var product = new Product() { Id = 2, Name = "Non Existing Product", Price = 78.67m, Quantity = 50 };

            //Act
            var result = await productRepository.DeleteAsync(product);

            //Assert
            result.Should().NotBeNull();
            result.Flag.Should().BeFalse();
            result.Message.Should().Be("Non Existing Product not found");
        }

        //GET PRODUCT BY ID
        [Fact]
        public async Task GetAsync_WhenProductIsFound_ReturnsProduct()
        {
            //Arrange
            var product = new Product() { Id = 1, Name = "ExistingProduct", Price = 76.87m, Quantity = 5 };
            productDbContext.Products.Add(product);
            await productDbContext.SaveChangesAsync();

            //Act
            var result = await productRepository.FindByIdAsync(product.Id);

            //Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(1);
            result.Name.Should().Be("ExistingProduct");
        }

        [Fact]
        public async Task FindByIdAsync_WhenProductIsNotFound_ReturnNull()
        {
            //Arrange
            //var productId = 2;

            //Act
            var result = await productRepository.FindByIdAsync(99);

            //Assert
            result.Should().BeNull();
        }

        //GET ALL PRODUCTS
        [Fact]
        public async Task GetAllAsync_WhenProductsAreFound_ReturnProducts()
        {
            //Arrange
            var products = new List<Product>

            {
                new(){Id = 1, Name = "Product 1"},
                new(){Id = 2, Name = "Product 2"},
            };
            productDbContext.Products.AddRange(products);
            await productDbContext.SaveChangesAsync();

            //Act
            var result = await productRepository.GetAllAsync();

            //Assert
            result.Should().NotBeNull();
            result.Count().Should().Be(2);
            result.Should().Contain(p => p.Name == "Product 1");
            result.Should().Contain(p => p.Name == "Product 2");
        }

        [Fact]
        public async Task GetAllAsync_WhenProductsAreNotFound_ReturnNull()
        {
            //Act
            var result = await productRepository.GetAllAsync();

            //Assert
            result.Should().NotBeNull();
        }

        //GET BY ANY TYPE (INT, STRING , BOOL E.T.C)
        [Fact]

        public async Task GetByAsync_WhenProductIsFound_ReturnProduct()
        {
            //Arrange
            var product = new Product() { Id = 1, Name = "Product 1"};
            productDbContext.Products.Add(product);
            await productDbContext.SaveChangesAsync();
            Expression<Func<Product, bool>> predicate = p => p.Name == "Product 1";

            //Act
            var result = await productRepository.GetByAsync(predicate);

            //Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("Product 1");
        }

        [Fact]

        public async Task GetByAsync_WhenProductIsNotFound_ReturnNull()
        {
            //Arrange
            Expression<Func<Product, bool>> predicate = p => p.Name == "Product 2";

            //Act
            var result = await productRepository.GetByAsync(predicate);

            //Assert
            result.Should().BeNull();
        }

        //UPDATE PRODUCT
        [Fact]
        public async Task UpdateAsync_WhenProductIsUpdatedSuccessfully_ReturnsSuccessResponse()
        {
            //Arrange
            var product = new Product() { Id = 1, Name = "Product 1" };
            productDbContext.Products.Add(product);
            await productDbContext.SaveChangesAsync();

            //Act
            var result = await productRepository.UpdateAsync(product);

            //Assert
            result.Should().NotBeNull();
            result.Flag.Should().BeTrue();
            result.Message.Should().Be("Product 1 is updated successfully");
        }

        [Fact]
        public async Task UpdateAsync_WhenProductIsNotFound_ReturnErrorResponse()
        {
            //Arrange
            var updateProduct = new Product() { Id = 2, Name = "Product 22" };

            //Act
            var result = await productRepository.UpdateAsync(updateProduct);

            //Assert
            result.Should().NotBeNull();
            result.Flag.Should().BeFalse();
            result.Message.Should().Be("Product 2 not found");
        }
    }
}
