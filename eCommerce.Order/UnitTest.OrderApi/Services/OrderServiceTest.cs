using FakeItEasy;
using FluentAssertions;
using OrderApi.Application.DTOs;
using OrderApi.Application.Interfaces;
using OrderApi.Application.Services;
using OrderApi.Domain.Entities;
using System.Linq.Expressions;
using System.Net.Http.Json;

namespace UnitTest.OrderApi.Services
{
    public class OrderServiceTest
    {
        private readonly IOrderService orderServiceInterface;
        private readonly IOrder orderInterface;

        public OrderServiceTest()
        {
            orderInterface = A.Fake<IOrder>();
            orderServiceInterface = A.Fake<IOrderService>();
        }

        //CREATE FAKE HTTP MESSAGE HANDLER
        public class FakeHttpMessageHandler(HttpResponseMessage response) : HttpMessageHandler
        {
            private readonly HttpResponseMessage _response = response;

            protected override Task<HttpResponseMessage> SendAsync
                                (HttpRequestMessage request, CancellationToken cancellationToken)

                => Task.FromResult(_response);
        }

        //CREATE FAKE HTTP CLIENT USING FAKE HTTP MESSAGE HANDLER
        private static HttpClient CreateFakeHttpClient(object o)
        {
            var httpResponseMessage = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = JsonContent.Create(o)
            };
            var fakeHttpMessageHandler = new FakeHttpMessageHandler(httpResponseMessage);
            var _httptClient = new HttpClient(fakeHttpMessageHandler)
            {
                BaseAddress = new Uri("http://localhost")
            };
            return _httptClient;
        }

        //GET PRODUCT
        [Fact]
        public async Task GetProduct_ValidProductId_ReturnProduct()
        {
            //Arrange
            int productId = 1;
            var productDTO = new ProductDTO(1, "Product 1", 13, 56.78m);
            var _httpClient = CreateFakeHttpClient(productDTO);

            // SUT - SYSTEM UNDER TEST
            //USE ONLY THE HTTPCLIENT TO MAKE CALLS
            //SPECIFY ONLY HTTPCLIENT AND NULL TO THE REST
            var _orderService = new OrderService(null!,_httpClient, null!);

            //Act
            var result = await _orderService.GetProduct(productId);

            //Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(productId);
            result.Name.Should().Be("Product 1");
        }

        [Fact]
        public async Task GetProduct_InvalidProductId_ReturnNull()
        {
            int productId = 1;
            var _httpclient = CreateFakeHttpClient(null!);
            var _orderService = new OrderService(null!, _httpclient, null!);

            //Act
            var result = await _orderService.GetProduct(productId);

            //Assert
            result.Should().BeNull();
        }

        //GET CLIENT ORDERS BY ID
        [Fact]
        public async Task GetOrdersByClientId_OrdersExist_ReturnOrderDetails()
        {
            //Arrange
            int clientId = 1;
            var orders = new List<Order>
            {
                new() { Id = 1, ProductId = 1, ClientId = clientId, PurchaseQuantity = 2, OrderedDate = DateTime.Now },
                new() { Id = 2, ProductId = 2, ClientId = clientId, PurchaseQuantity = 1, OrderedDate = DateTime.Now }
            };

            //MOCK THE GETORDERBY METHOD
            A.CallTo(() => orderInterface.GetOrdersAsync
            (A<Expression<Func<Order, bool>>>.Ignored)).Returns(orders);
            var _orderService = new OrderService(orderInterface, null!, null!);

            //Act
            var result = await _orderService.GetOrdersByClientId(clientId);

            //Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(orders.Count);
            result.Should().HaveCountGreaterThanOrEqualTo(2);
        }
    }
}
