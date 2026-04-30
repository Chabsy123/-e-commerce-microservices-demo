using eCommerce.SharedLibrary.Responses;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProductApi.Application.DTOs;
using ProductApi.Presentation.Controllers;
using ProductApi.Application.Interface;
using ProductModel = ProductApi.Domain.Entities.Product;

namespace UnitTest.ProductApi.Controllers
{
    public class ProductControllerTest
    {
        private readonly IProduct productInterface;
        private readonly ProductsController productsController;

        public ProductControllerTest()
        {
            //set up the dependencies 
            productInterface = A.Fake<IProduct>();

            //set up system under test - SUT
            productsController = new ProductsController(productInterface);
        }

        //Get All Products
        [Fact]
        public async Task GetProduct_WhenProductExists_ReturnOkResponseWithProducts()
        {
            //Arange
            var products = new List<ProductModel>()
            {
                new(){Id = 1, Name = "Product 1", Quantity = 10, Price = 100.70m},
                new(){Id = 2, Name = "Product 2", Quantity = 110, Price = 1004.70m},
             };

            //set up fake response for GetAllAsync Method
            A.CallTo(() => productInterface.GetAllAsync()).Returns(products);

            //Act
            var result = await productsController.GetProducts();

            //Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(StatusCodes.Status200OK);

            var returnedProducts = okResult.Value as IEnumerable<ProductDTO>;
            returnedProducts.Should().NotBeNull();
            returnedProducts.Should().HaveCount(2);
            returnedProducts!.First().Id.Should().Be(1);
            returnedProducts!.Last().Id.Should().Be(2);


        }

        [Fact]
        public async Task GetProducts_WhenNoProductsExist_ReturnNotFoundResponse()
        {
            //Arrange
            var products = new List<ProductModel>();
            //set up fake response for GetAllAsync Method
            A.CallTo(() => productInterface.GetAllAsync()).Returns(products);
            //Act
            var result = await productsController.GetProducts();
            //Assert
            var notFoundResult = result.Result as NotFoundObjectResult;
            notFoundResult.Should().NotBeNull();
            notFoundResult!.StatusCode.Should().Be(StatusCodes.Status404NotFound);

            var message = notFoundResult.Value as string;
            message.Should().Be("No product detected in the database");

        }

        //CREATE PRODUCT
        [Fact]
        public async Task CreateProduct_WhenModelStateIsInvalid_ReturnBadRequest()
        {
            //Arrange
            var productDTO = new ProductDTO(1, "Product 1", 34, 67.95m);
            productsController.ModelState.AddModelError("Name", "Required");

            //Act
            var result = await productsController.CreateProduct(productDTO);

            //Assert
            var badRequestResult = result.Result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult!.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

        }

        [Fact]

        public async Task CreateProduct_WhenCreateIsSuccessful_ReturnOkResponse()
        {

            //Arange 
            var productDTO = new ProductDTO(1, "Product 1", 34, 67.95m);
            var response = new Response(true, "Created");

            //Act 
            A.CallTo(() => productInterface.CreateAsync(A<ProductModel>.Ignored)).Returns(response);
            var result = await productsController.CreateProduct(productDTO);

            //Assert
            var okResult = result.Result as OkObjectResult;
            okResult!.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(StatusCodes.Status200OK);

            var responseResult = okResult.Value as Response;
            responseResult!.Message.Should().Be("Created");
            responseResult!.Flag.Should().BeTrue();
        }

        [Fact]
        public async Task CreateProduct_WhenCreateFails_ReturnBadRequestResponse()
        {
            //Arrange 
            var productDTO = new ProductDTO(1, "Product 1", 78, 45.36m);
            var response = new Response(false, "Failed");

            //Act
            A.CallTo(() => productInterface.CreateAsync(A<ProductModel>.Ignored)).Returns(response);
            var result = await productsController.CreateProduct(productDTO);

            //Assert
            var badRequestResult = result.Result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult!.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

            var responseResult = badRequestResult.Value as Response;
            responseResult.Should().NotBeNull();
            responseResult!.Message.Should().Be("Failed");
            responseResult!.Flag.Should().BeFalse();
        }

        [Fact]
        public async Task UpdateProduct_WhenUpdateIsSuccessful_ReturnOkResponse()
        {
            //Arrange
            var productDTO = new ProductDTO(1, "Product 1", 78, 45.36m);
            var response = new Response(true, "Updated");

            //Act
            A.CallTo(() => productInterface.UpdateAsync(A<ProductModel>.Ignored)).Returns(response);
            var result = await productsController.UpdateProduct(productDTO);

            //Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(StatusCodes.Status200OK);

            var responseResult = okResult.Value as Response;
            responseResult!.Message.Should().Be("Updated");
            responseResult!.Flag.Should().BeTrue();
        }

        [Fact]
        public async Task UpdateProduct_WhenUpdateFails_ReturnBadRequestResponse()
        {
            //Arrange
            var productDTO = new ProductDTO(1, "Product 1", 78, 45.36m);
            var response = new Response(false, "Update Failed");

            //Act
            A.CallTo(() => productInterface.UpdateAsync(A<ProductModel>.Ignored)).Returns(response);
            var result = await productsController.UpdateProduct(productDTO);

            //Assert
            var badRequestResult = result.Result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult!.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

            var responseResult = badRequestResult.Value as Response;
            responseResult.Should().NotBeNull();
            responseResult!.Message.Should().Be("Update Failed");
            responseResult!.Flag.Should().BeFalse();
        }

        [Fact]
        public async Task DeleteProduct_WhenDeleteIsSuccessful_ReturnOkResponse()
        {
            //Arrange
            var productDTO = new ProductDTO(1, "Product 1", 78, 45.36m);
            var response = new Response(true, "Deleted Successfully");

            //set up fake response for DeleteAsync
            A.CallTo(() => productInterface.DeleteAsync(A<ProductModel>.Ignored)).Returns(response);

            //Act
            var result = await productsController.DeleteProduct(productDTO);

            //Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(StatusCodes.Status200OK);

            var responseResult = okResult.Value as Response;
            responseResult!.Message.Should().Be("Deleted Successfully");
            responseResult!.Flag.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteProduct_WhenDeleteFails_ReturnBadRequestResponse()
        {
            //Arrange
            var productDTO = new ProductDTO(1, "Product 1", 78, 67.95m);
            var response = new Response(false, "Delete Failed");

            //set up fake response for DeleteAsync
            A.CallTo(() => productInterface.DeleteAsync(A<ProductModel>.Ignored)).Returns(response);

            //Act
            var result = await productsController.DeleteProduct(productDTO);

            //Assert
            var badRequestResult = result.Result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult!.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

            var responseResult = badRequestResult.Value as Response;
            responseResult.Should().NotBeNull();
            responseResult!.Message.Should().Be("Delete Failed");
            responseResult!.Flag.Should().BeFalse();
        }
    }
}
    