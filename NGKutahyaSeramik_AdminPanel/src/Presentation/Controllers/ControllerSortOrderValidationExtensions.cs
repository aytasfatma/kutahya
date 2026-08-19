using Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers;

internal static class ControllerSortOrderValidationExtensions
{
    public static void AddOperationError(this Controller controller, string? errorMessage)
    {
        if (string.IsNullOrWhiteSpace(errorMessage))
        {
            return;
        }

        var key = SortOrderValidationMessages.IsSortOrderMessage(errorMessage)
            ? "DisplayOrder"
            : string.Empty;

        controller.ModelState.AddModelError(key, errorMessage);
    }
}
