using Microsoft.EntityFrameworkCore;
using NotificationApi.Models.Entities;

namespace NotificationApi.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(NotificationDbContext context)
        {
            if (!await context.NotificationTemplates.AnyAsync())
            {
                await SeedTemplatesAsync(context);
            }
        }

        private static async Task SeedTemplatesAsync(NotificationDbContext context)
        {
            var templates = new List<NotificationTemplate>
            {
                new()
                {
                    Code = "QUOTATION_CREATED",
                    Name = "报价创建通知",
                    Description = "报价创建时发送给供应商的通知",
                    NotificationType = NotificationType.Email,
                    Subject = "新报价请求 - 轴承编号: {{BearingNumber}}",
                    Content = @"<p>尊敬的供应商，</p>
<p>我们收到了一个新的报价请求，详情如下：</p>
<ul>
    <li>报价编号：{{QuotationId}}</li>
    <li>轴承编号：{{BearingNumber}}</li>
    <li>创建时间：{{CreatedAt}}</li>
</ul>
<p>请尽快在系统中处理此报价请求。</p>
<p>谢谢！</p>",
                    Variables = "QuotationId,BearingNumber,CreatedAt",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new()
                {
                    Code = "ORDER_CREATED",
                    Name = "订单创建通知",
                    Description = "订单创建时发送给客户的通知",
                    NotificationType = NotificationType.Email,
                    Subject = "订单创建成功 - 订单号: {{OrderNumber}}",
                    Content = @"<p>尊敬的客户，</p>
<p>您的订单已成功创建！</p>
<ul>
    <li>订单编号：{{OrderNumber}}</li>
    <li>订单金额：{{Amount}} 元</li>
    <li>创建时间：{{CreatedAt}}</li>
</ul>
<p>我们会在订单状态更新时通知您。</p>",
                    Variables = "OrderNumber,Amount,CreatedAt",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };

            await context.NotificationTemplates.AddRangeAsync(templates);
            await context.SaveChangesAsync();
        }
    }
}
