using Application.DTOs;
using Domain.Helper;

namespace Application.Service;

public interface IBusinessCardService
{
    Task<ApiResponse<Pagination<BusinessCardDto>>> GetAll(BusinessCardParams businessCardParams);
    Task<ApiResponse<BusinessCardDto>> Save(CreateBusinessCardRequest createBusinessCardRequest);
    Task<ApiResponse<List<BusinessCardDto>>> SaveRange(List<CreateBusinessCardRequest> createBusinessCardRequest);
    Task<ApiResponse<BusinessCardDto>> GetById(Guid id);
    Task<ApiResponse<object>> Delete(Guid ID);
}
