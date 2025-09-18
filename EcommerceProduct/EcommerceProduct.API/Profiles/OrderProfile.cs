using AutoMapper;
using EcommerceProduct.API.Entities;
using EcommerceProduct.API.Models;

namespace EcommerceProduct.API.Profiles
{
    public class OrderProfile : Profile
    {
        public OrderProfile()
        {
            CreateMap<Order, OrderDto>()
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src =>
                    src.Customer != null && src.Customer.User != null ?
                    $"{src.Customer.User.FirstName} {src.Customer.User.LastName}" : string.Empty));

            CreateMap<OrderItem, OrderItemDto>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product!.Name))
                .ForMember(dest => dest.TotalPrice, opt => opt.MapFrom(src => src.Quantity * src.UnitPrice));
        }
    }
}