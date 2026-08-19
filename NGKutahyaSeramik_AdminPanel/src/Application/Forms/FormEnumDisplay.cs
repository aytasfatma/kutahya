using Domain.Enums;

namespace Application.Forms;

public static class FormEnumDisplay
{
    public static string GetFormTypeLabel(FormType formType) => formType switch
    {
        FormType.Contact => "İletişim Formu",
        FormType.RequestInformation => "Bilgi Talep Formu",
        FormType.SampleRequest => "Numune Talep Formu",
        _ => formType.ToString()
    };
}
