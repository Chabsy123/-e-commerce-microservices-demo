using Azure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OrderApi.Application.DTOs;
using OrderApi.Application.DTOs.Conversions;
using OrderApi.Application.Interfaces;
using OrderApi.Application.Services;
using Response = eCommerce.SharedLibrary.Responses.Response;

namespace OrderApi.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController(IOrder orderInterface, IOrderService orderService) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderDTO>>> GetOrders()
        {
            var orders = await orderInterface.GetAllAsync();
            if (!orders.Any())
                return NotFound("No orders found in the database.");

            var (_, list) = OrderConversion.FromEntity(null, orders);
            return !list!.Any() ? NotFound() : Ok(list);

        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<OrderDTO>> GetOrder(int id)
        {
            var order = await orderInterface.FindByIdAsync(id);
            if (order is null)
                return NotFound(null);

            var (_order, _) = OrderConversion.FromEntity(order, null);
            return Ok(_order);
        }

        [HttpGet("client/{clientId:int}")]

        public async Task<ActionResult<OrderDTO>> GetClientOrders(int clientId)
        {
            if (clientId <= 0)
                return BadRequest("Invalid data provided.");

            var orders = await orderService.GetOrdersByClientId(clientId);
            return !orders.Any()? NotFound(null) : Ok(orders);
        }

        [HttpGet("details/{orderId:int}")]
        public async Task<ActionResult<OrderDetailsDTO>> GetOrderDetails(int orderId)
        {
            if (orderId <= 0)
                return BadRequest("Invalid data provided.");

            var orderDetail = await orderService.GetOrderDetails(orderId);
            return orderDetail.OrderId > 0 ? Ok(orderDetail) : NotFound("Order details not found.");
        }

        [HttpPost]
        public async Task<ActionResult<Response>> CreateOrder(OrderDTO orderDTO)
        {
            //Check model state if all data annotations are passed.
            if (!ModelState.IsValid)
                return BadRequest("Incomplete data submitted.");

            //convert to entity
            var getEntity = OrderConversion.ToEntity(orderDTO);
            var response = await orderInterface.CreateAsync(getEntity);
            return response.Flag ? Ok(response) : BadRequest(response);
        }

        [HttpPut]
        public async Task<ActionResult<Response>> UpdateOrder(OrderDTO orderDTO)
        {
            // Use orderDTO.Id (Ensure your OrderDTO has the Id property!)
            var existingOrder = await orderInterface.FindByIdAsync(orderDTO.Id);
            if (existingOrder is null)
                return NotFound(new Response(false, "Order not found in database."));

            existingOrder.ProductId = orderDTO.ProductId;
            existingOrder.ClientId = orderDTO.ClientId;
            existingOrder.PurchaseQuantity = orderDTO.PurchaseQuantity;
            existingOrder.OrderedDate = orderDTO.OrderedDate;

            var response = await orderInterface.UpdateAsync(existingOrder);
            return response.Flag ? Ok(response) : BadRequest(response); 
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult<Response>> DeleteOrder(int id)
        {
            if (id <= 0)
                return BadRequest("Invalid data provided.");

            var order = await orderInterface.FindByIdAsync(id);
            if (order is null)
                return NotFound("Order not found.");

            var response = await orderInterface.DeleteAsync(order);
            return response.Flag ? Ok(response) : BadRequest(response);
        }

    }
}
