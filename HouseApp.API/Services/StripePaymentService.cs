using HouseApp.API.Data;
using HouseApp.API.DTOs;
using HouseApp.API.Models;
using Microsoft.EntityFrameworkCore;
using Stripe;

namespace HouseApp.API.Services;

public class StripePaymentService : IPaymentService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    public StripePaymentService(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
        StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];
    }

    public async Task<string> CreatePaymentIntentAsync(CreatePaymentIntentDto dto)
    {
        var options = new PaymentIntentCreateOptions
        {
            Amount = (long)(dto.Amount * 100),
            Currency = "usd",
            AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
            {
                Enabled = true,
            },
            Metadata = new Dictionary<string, string>
            {
                { "student_id", dto.StudentId.ToString() },
                { "house_id", dto.HouseId.ToString() },
                { "payment_type", dto.PaymentType.ToString() }
            }
        };

        var service = new PaymentIntentService();
        var paymentIntent = await service.CreateAsync(options);

        var payment = new Models.Payment
        {
            StudentId = dto.StudentId,
            HouseId = dto.HouseId,
            Amount = dto.Amount,
            PaymentType = dto.PaymentType,
            Status = PaymentStatus.Pending,
            StripePaymentIntentId = paymentIntent.Id,
            DueDate = dto.DueDate,
            CreatedDate = DateTime.UtcNow
        };

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();

        return paymentIntent.ClientSecret;
    }

    public async Task<bool> ConfirmPaymentAsync(int paymentId, string paymentIntentId)
    {
        var payment = await _context.Payments.FindAsync(paymentId);
        if (payment == null || payment.StripePaymentIntentId != paymentIntentId)
        {
            return false;
        }

        var service = new PaymentIntentService();
        var paymentIntent = await service.GetAsync(paymentIntentId);

        if (paymentIntent.Status == "succeeded")
        {
            payment.Status = PaymentStatus.Completed;
            payment.PaidDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        payment.Status = PaymentStatus.Failed;
        await _context.SaveChangesAsync();
        return false;
    }
}
