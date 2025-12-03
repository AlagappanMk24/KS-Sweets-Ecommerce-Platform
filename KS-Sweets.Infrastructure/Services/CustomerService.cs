using KS_Sweets.Application.Contracts.DTOs;
using KS_Sweets.Application.Contracts.Persistence;
using KS_Sweets.Application.Contracts.Services;
using KS_Sweets.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace KS_Sweets.Infrastructure.Services
{
    public class CustomerService(IUnitOfWork unitOfWork, ICategoryService categoryService, ILogger<CustomerService> logger) : ICustomerService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ICategoryService _categoryService = categoryService;
        private readonly ILogger<CustomerService> _logger = logger;

        public HomePageDto LoadHomePage(string? userId)
        {
            try
            {
                var dto = new HomePageDto
                {
                    Categories = _categoryService.GetAllCategories().ToList(),
                };

                return dto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading homepage data");
                throw; // rethrow exception to controller
            }
        }

        public Category GetCategory(string slug)
        {
            try
            {
                return _categoryService.GetCategoryForCustomerView(slug);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching category: {slug}");
                throw;
            }
        }
    }
}
