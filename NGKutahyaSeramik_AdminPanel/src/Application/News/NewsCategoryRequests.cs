namespace Application.News;

public class NewsCategoryTranslationInput
{
    public int LanguageId { get; init; }
    public string? Name { get; init; }
}

public class NewsCategoryRequestBase
{
    public int DisplayOrder { get; init; }
    public IReadOnlyList<NewsCategoryTranslationInput> Translations { get; init; } = [];
}

public class CreateNewsCategoryRequest : NewsCategoryRequestBase
{
}

public class UpdateNewsCategoryRequest : NewsCategoryRequestBase
{
}
