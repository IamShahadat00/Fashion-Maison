using System;
using System.Collections.Generic;
using Maison.Domain.Enums;

namespace Maison.BLL.DTOs
{
    public record OrderItemDto(Guid ProductId, int Quantity);
    public record CreateOrderDto(string ShippingAddress, List<OrderItemDto> Items);
    public record UpdateOrderStatusDto(OrderStatus Status, string? TrackingNumber);
    public record RefundSubmissionDto(Guid OrderId, decimal Amount, string Reason);
}