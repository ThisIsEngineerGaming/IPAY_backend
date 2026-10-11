using IPAY.Application.DTOs.Payments;
using IPAY.Application.Interfaces.Payments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IPAY.WebApi.Controllers
{
    [ApiController]
    [Route("api/payments")]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;


        public PaymentsController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [Authorize]
        [HttpPost("create")]
        public async Task<ActionResult<PaymentResponseDto>> Create([FromBody] CreatePaymentDto dto)
        {
            var result = await _paymentService.StartPaymentAsync(dto.OrderId);
            return Ok(result);
        }

        // «Callback» от платёжки (для mock просто GET)
        [HttpGet("mock/confirm")]
        public async Task<ActionResult<PaymentResponseDto>> ConfirmMock(
            [FromQuery] string transactionId,
            [FromQuery] int orderId)
        {
            var result = await _paymentService.ConfirmMockPaymentAsync(transactionId, orderId);
            return Ok(result);
        }
    }
}
