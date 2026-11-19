using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Maison.BLL.DTOs;
using Maison.Domain.Entities;

namespace Maison.BLL.Interfaces
{
    public interface IOrderService
    {
        Task<Order> PlaceOrderAsync(Guid customerId, CreateOrderDto dto);
        Task<IEnumerable<Order>> GetOrdersForRoleAsync(Guid userId, string role);
        Task<Order> UpdateOrderStatusAsync(Guid orderId, UpdateOrderStatusDto dto, Guid actorId);
        Task<RefundRequest> RequestRefundAsync(RefundSubmissionDto dto, Guid requesterId, string role);
        Task<RefundRequest> AuditVerifyRefundAsync(Guid refundId, Guid auditorId, WorkflowGateDecisionDto decision);
        Task<RefundRequest> AdminDecideRefundAsync(Guid refundId, Guid adminId, WorkflowGateDecisionDto decision);
    }
}