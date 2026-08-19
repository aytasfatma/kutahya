namespace Application.Blogs;

public class BlogCategoryTranslationInput
{
    public int LanguageId { get; init; }
    public string? Name { get; init; }
}

public class BlogCategoryRequestBase
{
    public int DisplayOrder { get; init; }
    public IReadOnlyList<BlogCategoryTranslationInput> Translations { get; init; } = [];
}

public class CreateBlogCategoryRequest : BlogCategoryRequestBase
{
}

public class UpdateBlogCategoryRequest : BlogCategoryRequestBase
{
}
