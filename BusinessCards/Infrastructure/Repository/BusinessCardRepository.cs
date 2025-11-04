using BusinessCards.Infrastructure.Data;
using Domain.Entities;
using Domain.Helper;
using Infrastructure.IRepository;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repository;

public class BusinessCardRepository : IBusinessCardRepository
{
    private readonly AppDbContext _context;

    public BusinessCardRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<object>> Delete(Guid ID)
    {
        var FindBusinessCard = await _context.BusinessCards.FindAsync(ID);
        if (FindBusinessCard is null)
            return ApiResponse<object>.FailureResponse("Not Found");
        _context.BusinessCards.Remove(FindBusinessCard);
        if (await _context.SaveChangesAsync() > 0)
            return ApiResponse<object>.SuccessResponse(null, "Delete Duccessfully");
        return ApiResponse<object>.FailureResponse("Delete failed");
    }

    public async Task<ApiResponse<Pagination<BusinessCard>>> GetAll(BusinessCardParams p)
    {
        var q = _context.BusinessCards.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(p.name))
            q = q.Where(x => EF.Functions.Like(x.Name, $"%{p.name}%"));

        if (!string.IsNullOrWhiteSpace(p.email))
            q = q.Where(x => EF.Functions.Like(x.Email, $"%{p.email}%"));

        if (!string.IsNullOrWhiteSpace(p.phone))
            q = q.Where(x => EF.Functions.Like(x.Phone, $"%{p.phone}%"));

        if (p.gender.HasValue)
            q = q.Where(x => x.Gender == p.gender);

        if (p.dob.HasValue)
            q = q.Where(x => x.DateOfBirth.HasValue && x.DateOfBirth.Value.Date == p.dob.Value.Date);

        if (p.dobFrom.HasValue)
            q = q.Where(x => x.DateOfBirth.HasValue && x.DateOfBirth.Value.Date >= p.dobFrom.Value.Date);

        if (p.dobTo.HasValue)
            q = q.Where(x => x.DateOfBirth.HasValue && x.DateOfBirth.Value.Date <= p.dobTo.Value.Date);

        var sortBy = (p.sortBy ?? "name").ToLowerInvariant();
        var desc = string.Equals(p.sortDir, "desc", StringComparison.OrdinalIgnoreCase);

        q = sortBy switch
        {
            "email" => desc ? q.OrderByDescending(x => x.Email) : q.OrderBy(x => x.Email),
            "phone" => desc ? q.OrderByDescending(x => x.Phone) : q.OrderBy(x => x.Phone),
            "dob" => desc ? q.OrderByDescending(x => x.DateOfBirth) : q.OrderBy(x => x.DateOfBirth),
            "gender" => desc ? q.OrderByDescending(x => x.Gender) : q.OrderBy(x => x.Gender),
            _ => desc ? q.OrderByDescending(x => x.Name) : q.OrderBy(x => x.Name),
        };

        var count = await q.CountAsync();

        var page = p.PageNumber <= 0 ? 1 : p.PageNumber;
        var size = p.pageSize;
        var items = await q.Skip((page - 1) * size).Take(size).ToListAsync();

        var pagination = new Pagination<BusinessCard>(page, size, count, items);
        return ApiResponse<Pagination<BusinessCard>>.SuccessResponse(pagination);
    }

    public async Task<ApiResponse<BusinessCard>> GetById(Guid ID)
    {
        var FindBusinessCard = await _context.BusinessCards.AsNoTracking().FirstOrDefaultAsync(x => x.Id == ID);
        if (FindBusinessCard is null)
            return ApiResponse<BusinessCard>.FailureResponse("Not Found");
        return ApiResponse<BusinessCard>.SuccessResponse(FindBusinessCard);
    }

    public async Task<ApiResponse<BusinessCard>> Save(BusinessCard businessCardObj)
    {
        businessCardObj.Id = Guid.NewGuid();
        await _context.BusinessCards.AddAsync(businessCardObj);
        if (await _context.SaveChangesAsync() > 0)
            return ApiResponse<BusinessCard>.SuccessResponse(businessCardObj);
        return ApiResponse<BusinessCard>.FailureResponse("Save Failed");
    }

    public async Task<ApiResponse<List<BusinessCard>>> SaveRange(List<BusinessCard> businessCardObj)
    {
        await _context.BusinessCards.AddRangeAsync(businessCardObj);
        if (await _context.SaveChangesAsync() > 0)
            return ApiResponse<List<BusinessCard>>.SuccessResponse(businessCardObj);
        return ApiResponse<List<BusinessCard>>.FailureResponse("Save Failed");
    }
}
