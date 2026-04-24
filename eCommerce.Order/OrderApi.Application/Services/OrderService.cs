using OrderApi.Application.DTOs;
using OrderApi.Application.DTOs.Conversions;
using OrderApi.Application.Interfaces;
using Polly;
using Polly.Registry;
using System.Net.Http.Json;

namespace OrderApi.Application.Services
{
        public class OrderService(IOrder orderInterface, HttpClient httpClient, ResiliencePipelineProvider<string> resiliencePipeline) : IOrderService
    {
        //Get product
        public async Task<ProductDTO> GetProduct(int productId)
        {
            var getProduct = await httpClient.GetAsync($"http://localhost:5001/api/products/{productId}");
            // return null on failure
            if (!getProduct.IsSuccessStatusCode)
                return null!;

            var product = await getProduct.Content.ReadFromJsonAsync<ProductDTO>();
            return product!;
        }

        //GET user
        public async Task<AppUserDTO> GetUser(int userId)
        {
            //call product api using http client
            //redirect this call to the api gateway since product api is not responding to outsiders
            var getUser = await httpClient.GetAsync($"http://localhost:5000/api/Authentication/{userId}");
            // return null on failure
            if (!getUser.IsSuccessStatusCode)
                return null!;

            var user = await getUser.Content.ReadFromJsonAsync<AppUserDTO>();
            return user!;
        }

        //GET ORDER DETAILS BY ID
        public async Task<OrderDetailsDTO> GetOrderDetails(int orderId)
        {
            // Prepare Order
            var order = await orderInterface.FindByIdAsync(orderId);
            if (order is null || order.Id <= 0)
                return null!;

            // Get Retry Pipeline
            var retryPipeline = resiliencePipeline.GetPipeline("my-retry-pipeline");

            // Prepare Product & Client (Safe execution)
            var productDTO = await retryPipeline.ExecuteAsync(async token => await GetProduct(order.ProductId));
            var appUserDTO = await retryPipeline.ExecuteAsync(async token => await GetUser(order.ClientId));

            // Populate Order Details with NULL CHECKS
            return new OrderDetailsDTO(
                order.Id,                                          // Id
                order.Id,                                          // OrderId
                productDTO?.Id ?? 0,                               // ProductId (Safe)
                appUserDTO?.Id ?? 0,                               // Client (Safe)
                appUserDTO?.Name ?? "User not found",              // Name
                appUserDTO?.Email ?? "N/A",                        // Email
                appUserDTO?.Address ?? "N/A",                      // Address
                appUserDTO?.TelephoneNumber ?? "N/A",              // TelephoneNumber
                productDTO?.Name ?? "Product not found",           // ProductName
                order.PurchaseQuantity,                            // PurchaseQuantity
                productDTO?.Price ?? 0,                            // UnitPrice
                (productDTO?.Price ?? 0) * order.PurchaseQuantity, // TotalPrice
                order.OrderedDate                                  // OrderedDate
            );
        }

        //GET ORDERS BY CLIENT ID
        public async Task<IEnumerable<OrderDTO>> GetOrdersByClientId(int clientId)
        {
            //Get All clients orders
            var orders =  await orderInterface.GetOrdersAsync(o => o.ClientId == clientId);
            if(!orders.Any()) return null!;

            //Convert from entity to DTO
            var (_, _orders) = OrderConversion.FromEntity(null, orders);

            return _orders!;
        }
    }
}
