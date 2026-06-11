using Microsoft.AspNetCore.Razor.TagHelpers;

[HtmlTargetElement("script")]
[HtmlTargetElement("style")]
public class NonceTagHelper : TagHelper
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public NonceTagHelper(IHttpContextAccessor accessor)
    {
        _httpContextAccessor = accessor;
    }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var nonce = _httpContextAccessor.HttpContext?.Items["CSPNonce"]?.ToString();
        if (!string.IsNullOrEmpty(nonce))
            output.Attributes.SetAttribute("nonce", nonce);
    }
}