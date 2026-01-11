using BearingApi.Data;
using BearingApi.Models.DTOs;
using BearingApi.Models.Entities;

namespace BearingApi.Services
{
    public class BearingValidationService : IBearingValidationService
    {
        private readonly IBearingRepository _repository;
        private readonly ILogger<BearingValidationService> _logger;

        public BearingValidationService(IBearingRepository repository, ILogger<BearingValidationService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ValidationResult> ValidateBearingNumberAsync(string bearingNumber)
        {
            var result = new ValidationResult();

            if (string.IsNullOrWhiteSpace(bearingNumber))
            {
                result.Errors.Add("轴承型号不能为空");
                return result;
            }

            // 基本格式验证
            if (bearingNumber.Length < 2 || bearingNumber.Length > 50)
            {
                result.Errors.Add("轴承型号长度应在2-50个字符之间");
            }

            // 检查是否已存在
            var existing = await _repository.GetByBearingNumberAsync(bearingNumber);
            if (existing != null)
            {
                result.Errors.Add($"轴承型号 '{bearingNumber}' 已存在");
            }

            // 标准格式验证（简单实现）
            if (!IsValidBearingNumberFormat(bearingNumber))
            {
                result.Warnings.Add("轴承型号格式可能不符合标准");
            }

            result.IsValid = !result.Errors.Any();
            return result;
        }

        public async Task<ValidationResult> ValidateBearingParametersAsync(BearingParameters parameters)
        {
            var result = new ValidationResult();

            // 尺寸逻辑验证
            if (parameters.InnerDiameter.HasValue && parameters.OuterDiameter.HasValue)
            {
                if (parameters.InnerDiameter >= parameters.OuterDiameter)
                {
                    result.Errors.Add("内径应小于外径");
                }
            }

            if (parameters.OuterDiameter.HasValue && parameters.Width.HasValue)
            {
                if (parameters.Width > parameters.OuterDiameter * 2)
                {
                    result.Warnings.Add("宽度异常，请确认参数是否正确");
                }
            }

            // 载荷验证
            if (parameters.DynamicLoadRating.HasValue && parameters.StaticLoadRating.HasValue)
            {
                if (parameters.StaticLoadRating > parameters.DynamicLoadRating)
                {
                    result.Warnings.Add("静载荷通常应小于动载荷");
                }
            }

            result.IsValid = !result.Errors.Any();
            return result;
        }

        public async Task<List<string>> SuggestBearingNumbersAsync(string partialNumber)
        {
            if (string.IsNullOrWhiteSpace(partialNumber) || partialNumber.Length < 2)
                return new List<string>();

            // 这里应该实现更智能的推荐逻辑
            // 简化实现：返回包含部分型号的轴承
            var suggestions = new List<string>
        {
            $"{partialNumber}ZZ",  // 双面密封
            $"{partialNumber}RS",  // 单面密封
            $"{partialNumber}C3",  // C3游隙
            $"{partialNumber}P6"   // P6精度
        };

            return suggestions.Where(s => s.Length <= 50).Take(10).ToList();
        }

        public async Task<List<string>> GetStandardBearingNumbersAsync()
        {
            // 返回常见标准轴承型号
            return new List<string>
        {
            "6000", "6001", "6002", "6003", "6004", "6005", "6006", "6007", "6008", "6009",
            "6200", "6201", "6202", "6203", "6204", "6205", "6206", "6207", "6208", "6209",
            "6300", "6301", "6302", "6303", "6304", "6305", "6306", "6307", "6308", "6309"
        };
        }

        private bool IsValidBearingNumberFormat(string bearingNumber)
        {
            // 简单的轴承型号格式验证
            // 实际应根据轴承型号标准实现
            if (string.IsNullOrWhiteSpace(bearingNumber))
                return false;

            // 基本规则：以字母或数字开头，可包含数字、字母、横线、空格
            return System.Text.RegularExpressions.Regex.IsMatch(bearingNumber,
                @"^[a-zA-Z0-9][a-zA-Z0-9\-\s]*$");
        }
    }
}
