using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HouseApp.API.Data;
using HouseApp.API.DTOs;
using HouseApp.API.Services;

namespace HouseApp.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IPaymentService _paymentService;

    public PaymentsController(AppDbContext context, IPaymentService paymentService)
    {
        _context = context;
        _paymentService = paymentService;
    }

    [HttpGet("student/{studentId}")]
    public async Task<ActionResult<IEnumerable<PaymentDto>>> GetStudentPayments(int studentId)
    {
        var payments = await _context.Payments
            .Where(p => p.StudentId == studentId)
            .Select(p => new PaymentDto
            {
                Id = p.Id,
                StudentId = p.StudentId,
                HouseId = p.HouseId,
                Amount = p.Amount,
                PaymentType = p.PaymentType,
                Status = p.Status,
                StripePaymentIntentId = p.StripePaymentIntentId,
                DueDate = p.DueDate,
                PaidDate = p.PaidDate,
                CreatedDate = p.CreatedDate
            })
            .ToListAsync();

        return Ok(payments);
    }

    [HttpGet("house/{houseId}")]
    public async Task<ActionResult<IEnumerable<PaymentDto>>> GetHousePayments(int houseId)
    {
        var payments = await _context.Payments
            .Where(p => p.HouseId == houseId)
            .Select(p => new PaymentDto
            {
                Id = p.Id,
                StudentId = p.StudentId,
                HouseId = p.HouseId,
                Amount = p.Amount,
                PaymentType = p.PaymentType,
                Status = p.Status,
                StripePaymentIntentId = p.StripePaymentIntentId,
                DueDate = p.DueDate,
                PaidDate = p.PaidDate,
                CreatedDate = p.CreatedDate
            })
            .ToListAsync();

        return Ok(payments);
    }

    [HttpPost("create-payment-intent")]
    [Authorize(Roles = "Student,Landlord")]
    public async Task<ActionResult<object>> CreatePaymentIntent(CreatePaymentIntentDto dto)
    {
        try
        {
            var clientSecret = await _paymentService.CreatePaymentIntentAsync(dto);
            return Ok(new { clientSecret });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{paymentId}/confirm")]
    public async Task<ActionResult> ConfirmPayment(int paymentId, [FromBody] string paymentIntentId)
    {
        var success = await _paymentService.ConfirmPaymentAsync(paymentId, paymentIntentId);

        if (!success)
        {
            return BadRequest(new { message = "Payment confirmation failed" });
        }

        return Ok(new { message = "Payment confirmed successfully" });
    }
}
