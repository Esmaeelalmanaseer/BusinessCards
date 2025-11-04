using Application.DTOs;
using Domain.Entities;
using Domain.Helper;
using FluentValidation;
using FluentValidation.Results;
using Infrastructure.IRepository;
using Mapster;
using System;
using System.Text;

namespace Application.Service;

public class BusinessCardService : IBusinessCardService
{
    private readonly IBusinessCardRepository _repository;
    private readonly IValidator<CreateBusinessCardRequest> _createValidator;
    public BusinessCardService(IBusinessCardRepository repository, IValidator<CreateBusinessCardRequest> createValidator)
    {
        _repository = repository;
        _createValidator = createValidator;
    }

    public async Task<ApiResponse<object>> Delete(Guid ID) => await _repository.Delete(ID);


    public async Task<ApiResponse<Pagination<BusinessCardDto>>> GetAll(BusinessCardParams businessCardParams)
    {
        var LstBusinessCardDto = await _repository.GetAll(businessCardParams);
        var response = LstBusinessCardDto.Adapt<ApiResponse<Pagination<BusinessCardDto>>>();
        return response;
    }

    public async Task<ApiResponse<BusinessCardDto?>> GetById(Guid id)
    {
        var GetBusinessCardDto = await _repository.GetById(id);
        var reponse = GetBusinessCardDto.Adapt<ApiResponse<BusinessCardDto?>>();
        return reponse;
    }

    public async Task<ApiResponse<BusinessCardDto>> Save(CreateBusinessCardRequest createBusinessCardRequest)
    {
        ValidationResult v = await _createValidator.ValidateAsync(createBusinessCardRequest);
        if (!v.IsValid)
        {
            var errors = v.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray()
                );
            StringBuilder returnErr = new();
            foreach (var err in errors)
            {
                foreach (var errvalue in err.Value)
                    returnErr.Append($"{err.Key} : {errvalue} \n ");
            }
            return ApiResponse<BusinessCardDto>.FailureResponse(returnErr.ToString());
        }
        var MapTo = createBusinessCardRequest.Adapt<BusinessCard>();
        var result = await _repository.Save(MapTo);
        var response = result.Adapt<ApiResponse<BusinessCardDto>>();
        return response;
    }

    public async Task<ApiResponse<List<BusinessCardDto>>> SaveRange(List<CreateBusinessCardRequest> createBusinessCardRequest)
    {
        StringBuilder returnErr = new();
        bool IsValid = true;
        foreach (var item in createBusinessCardRequest)
        {
            ValidationResult v = await _createValidator.ValidateAsync(item);
            if (!v.IsValid)
            {
                IsValid = false;
                var errors = v.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray()
                    );
                foreach (var err in errors)
                {
                    foreach (var errvalue in err.Value)
                        returnErr.Append($"{err.Key} : {errvalue} \n ");
                }
            }
        }
        if (!IsValid)
            return ApiResponse<List<BusinessCardDto>>.FailureResponse(returnErr.ToString());
        var Card = createBusinessCardRequest.Select(s => new BusinessCard
        {
            Id = Guid.NewGuid(),
            Address = s.Address,
            DateOfBirth = s.DateOfBirth,
            Email = s.Email,
            Gender = s.Gender,
            Name = s.Name,
            Phone = s.Phone,
            PhotoBase64 = s.PhotoBase64,
            PhotoSizeBytes = s.PhotoSizeBytes
        }).ToList();
        var result = await _repository.SaveRange(Card);
        var response = result.Adapt<ApiResponse<List<BusinessCardDto>>>();
        return response;
    }
}
