using AutoMapper;
using QIMy.Application.Products.Commands.CreateProduct;
using QIMy.Application.Products.Commands.UpdateProduct;
using QIMy.Application.Products.DTOs;
using QIMy.Core.Entities;

namespace QIMy.Application.MappingProfiles;

public class ProductProfile : Profile
{
    public ProductProfile()
    {
        CreateMap<Product, ProductDto>();

        CreateMap<CreateProductCommand, Product>();

        CreateMap<UpdateProductCommand, Product>();
    }
}
