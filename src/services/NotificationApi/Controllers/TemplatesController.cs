using Microsoft.AspNetCore.Mvc;
using NotificationApi.Data;
using NotificationApi.Models.DTOs;
using NotificationApi.Models.Entities;
using NotificationApi.Services;

namespace NotificationApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TemplatesController : ControllerBase
    {
        private readonly INotificationRepository _repository;
        private readonly INotificationService _notificationService;
        private readonly ILogger<TemplatesController> _logger;

        public TemplatesController(
            INotificationRepository repository,
            INotificationService notificationService,
            ILogger<TemplatesController> logger)
        {
            _repository = repository;
            _notificationService = notificationService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<TemplateResponse>>>> GetTemplates([FromQuery] bool? isActive = null)
        {
            try
            {
                var templates = await _repository.GetTemplatesAsync(isActive);
                var result = templates.Select(MapToTemplateResponse).ToList();
                return ApiResponse<List<TemplateResponse>>.Success(result, "获取模板列表成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取模板列表失败");
                return BadRequest(ApiResponse<List<TemplateResponse>>.Error($"获取模板列表失败: {ex.Message}"));
            }
        }

        [HttpGet("{code}")]
        public async Task<ActionResult<ApiResponse<TemplateResponse>>> GetTemplate(string code)
        {
            try
            {
                var template = await _notificationService.GetTemplateAsync(code);
                if (template == null)
                    return NotFound(ApiResponse<TemplateResponse>.Error("模板不存在"));

                return ApiResponse<TemplateResponse>.Success(template, "获取模板成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取模板失败: {TemplateCode}", code);
                return BadRequest(ApiResponse<TemplateResponse>.Error($"获取模板失败: {ex.Message}"));
            }
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<TemplateResponse>>> CreateTemplate([FromBody] CreateTemplateRequest request)
        {
            try
            {
                // 检查模板代码是否已存在
                var existingTemplate = await _repository.GetTemplateByCodeAsync(request.Code);
                if (existingTemplate != null)
                    return Conflict(ApiResponse<TemplateResponse>.Error("模板代码已存在"));

                var template = new NotificationTemplate
                {
                    Code = request.Code,
                    Name = request.Name,
                    Description = request.Description,
                    NotificationType = request.NotificationType,
                    Subject = request.Subject,
                    Content = request.Content,
                    Variables = request.Variables,
                    IsActive = request.IsActive,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var savedTemplate = await _repository.AddTemplateAsync(template);
                var result = MapToTemplateResponse(savedTemplate);

                return CreatedAtAction(nameof(GetTemplate), new { code = result.Code },
                    ApiResponse<TemplateResponse>.Success(result, "创建模板成功"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建模板失败");
                return BadRequest(ApiResponse<TemplateResponse>.Error($"创建模板失败: {ex.Message}"));
            }
        }

        [HttpPut("{code}")]
        public async Task<ActionResult<ApiResponse<TemplateResponse>>> UpdateTemplate(string code, [FromBody] UpdateTemplateRequest request)
        {
            try
            {
                var template = await _repository.GetTemplateByCodeAsync(code);
                if (template == null)
                    return NotFound(ApiResponse<TemplateResponse>.Error("模板不存在"));

                // 更新字段
                if (!string.IsNullOrEmpty(request.Name))
                    template.Name = request.Name;
                if (!string.IsNullOrEmpty(request.Description))
                    template.Description = request.Description;
                if (!string.IsNullOrEmpty(request.Subject))
                    template.Subject = request.Subject;
                if (!string.IsNullOrEmpty(request.Content))
                    template.Content = request.Content;
                if (!string.IsNullOrEmpty(request.Variables))
                    template.Variables = request.Variables;
                if (request.IsActive.HasValue)
                    template.IsActive = request.IsActive.Value;

                template.UpdatedAt = DateTime.UtcNow;

                var updatedTemplate = await _repository.UpdateTemplateAsync(template);
                var result = MapToTemplateResponse(updatedTemplate);

                return ApiResponse<TemplateResponse>.Success(result, "更新模板成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新模板失败: {TemplateCode}", code);
                return BadRequest(ApiResponse<TemplateResponse>.Error($"更新模板失败: {ex.Message}"));
            }
        }

        [HttpDelete("{code}")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteTemplate(string code)
        {
            try
            {
                var template = await _repository.GetTemplateByCodeAsync(code);
                if (template == null)
                    return NotFound(ApiResponse<object>.Error("模板不存在"));

                var success = await _repository.DeleteTemplateAsync(template.Id);
                if (!success)
                    return BadRequest(ApiResponse<object>.Error("删除模板失败"));

                return ApiResponse<object>.Success(null, "删除模板成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除模板失败: {TemplateCode}", code);
                return BadRequest(ApiResponse<object>.Error($"删除模板失败: {ex.Message}"));
            }
        }

        [HttpPost("{code}/render")]
        public async Task<ActionResult<ApiResponse<object>>> RenderTemplate(string code, [FromBody] Dictionary<string, object> data)
        {
            try
            {
                var template = await _repository.GetTemplateByCodeAsync(code);
                if (template == null || !template.IsActive)
                    return NotFound(ApiResponse<object>.Error("模板不存在或已停用"));

                var renderedSubject = await _notificationService.RenderTemplateAsync(template.Subject, data);
                var renderedContent = await _notificationService.RenderTemplateAsync(template.Content, data);

                return ApiResponse<object>.Success(new
                {
                    Subject = renderedSubject,
                    Content = renderedContent
                }, "模板渲染成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "渲染模板失败: {TemplateCode}", code);
                return BadRequest(ApiResponse<object>.Error($"渲染模板失败: {ex.Message}"));
            }
        }

        [HttpGet("{code}/preview")]
        public async Task<ActionResult<ApiResponse<object>>> PreviewTemplate(string code)
        {
            try
            {
                var template = await _repository.GetTemplateByCodeAsync(code);
                if (template == null || !template.IsActive)
                    return NotFound(ApiResponse<object>.Error("模板不存在或已停用"));

                var variables = ExtractTemplateVariables(template.Subject + " " + template.Content);

                return ApiResponse<object>.Success(new
                {
                    Code = template.Code,
                    Name = template.Name,
                    Type = template.NotificationType,
                    Subject = template.Subject,
                    Content = template.Content,
                    Description = template.Description,
                    Variables = variables
                }, "模板预览成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "预览模板失败: {TemplateCode}", code);
                return BadRequest(ApiResponse<object>.Error($"预览模板失败: {ex.Message}"));
            }
        }

        [HttpPost("{code}/test")]
        public async Task<ActionResult<ApiResponse<SendNotificationResult>>> TestTemplate(
            string code,
            [FromBody] TestTemplateRequest request)
        {
            try
            {
                var result = await _notificationService.SendNotificationByTemplateAsync(
                    code,
                    request.TemplateData,
                    request.TestRecipient);

                return ApiResponse<SendNotificationResult>.Success(result, "模板测试完成");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "测试模板失败: {TemplateCode}", code);
                return BadRequest(ApiResponse<SendNotificationResult>.Error($"测试模板失败: {ex.Message}"));
            }
        }

        // 私有方法
        private TemplateResponse MapToTemplateResponse(NotificationTemplate template)
        {
            return new TemplateResponse
            {
                Id = template.Id,
                Code = template.Code,
                Name = template.Name,
                Description = template.Description,
                NotificationType = template.NotificationType,
                Subject = template.Subject,
                Content = template.Content,
                Variables = template.Variables,
                IsActive = template.IsActive,
                Version = template.Version,
                CreatedAt = template.CreatedAt,
                UpdatedAt = template.UpdatedAt
            };
        }

        private List<string> ExtractTemplateVariables(string content)
        {
            var variables = new List<string>();
            var startIndex = 0;

            while (true)
            {
                var start = content.IndexOf("{{", startIndex);
                if (start == -1) break;

                var end = content.IndexOf("}}", start);
                if (end == -1) break;

                var variable = content.Substring(start + 2, end - start - 2).Trim();
                if (!string.IsNullOrWhiteSpace(variable) && !variables.Contains(variable))
                {
                    variables.Add(variable);
                }

                startIndex = end + 2;
            }

            return variables;
        }
    }
}
