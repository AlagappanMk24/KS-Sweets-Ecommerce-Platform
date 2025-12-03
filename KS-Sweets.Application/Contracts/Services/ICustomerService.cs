using KS_Sweets.Application.Contracts.DTOs;
using KS_Sweets.Domain.Entities;

namespace KS_Sweets.Application.Contracts.Services
{
    public interface ICustomerService
    {
        HomePageDto LoadHomePage(string? userId);
        Category GetCategory(string slug);
    }
}