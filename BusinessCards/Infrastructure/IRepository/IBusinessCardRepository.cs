using Domain.Entities;
using Domain.Helper;

namespace Infrastructure.IRepository;

public interface IBusinessCardRepository
{
    Task<ApiResponse<Pagination<BusinessCard>>> GetAll(BusinessCardParams businessCard);
    Task<ApiResponse<BusinessCard>> Save(BusinessCard businessCardObj);
    Task<ApiResponse<List<BusinessCard>>> SaveRange(List<BusinessCard> businessCardObj);
    Task<ApiResponse<BusinessCard>> GetById(Guid ID);
    Task<ApiResponse<object>> Delete(Guid ID);
}
