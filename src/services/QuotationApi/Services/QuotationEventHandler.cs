using Dapr.Client;

namespace QuotationApi.Services
{
    public class QuotationEventHandler
    {
        private readonly IQuotationService _quotationService;
        private readonly ILogger<QuotationEventHandler> _logger;
        private readonly DaprClient _daprClient;

        public QuotationEventHandler(
            IQuotationService quotationService,
            ILogger<QuotationEventHandler> logger,
            DaprClient daprClient)
        {
            _quotationService = quotationService;
            _logger = logger;
            _daprClient = daprClient;
        }

        // 处理需求创建事件（从 DemandApi）
        public async Task HandleDemandCreated(dynamic eventData)
        {
            try
            {
                _logger.LogInformation($"收到需求创建事件: {eventData.DemandId}");

                // 这里可以触发自动报价匹配逻辑
                // 例如：查找匹配的供应商并发送报价请求

                await _daprClient.PublishEventAsync("pubsub", "quotation-request-created", new
                {
                    DemandId = eventData.DemandId,
                    BearingNumber = eventData.BearingNumber,
                    Quantity = eventData.Quantity,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理需求创建事件失败");
            }
        }

        // 处理供应商响应事件（从 SupplierApi）
        public async Task HandleSupplierResponse(dynamic eventData)
        {
            try
            {
                _logger.LogInformation($"收到供应商响应事件: {eventData.SupplierId}");

                // 更新供应商信息和报价状态
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理供应商响应事件失败");
            }
        }
    }
}
