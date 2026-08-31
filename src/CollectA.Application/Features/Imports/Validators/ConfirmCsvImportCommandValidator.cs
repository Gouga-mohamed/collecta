using FluentValidation;

namespace CollectA.Application.Features.Imports;

public class ConfirmCsvImportCommandValidator : AbstractValidator<ConfirmCsvImportCommand>
{
    public ConfirmCsvImportCommandValidator()
    {
        RuleFor(x => x.ImportId).NotEmpty();
        RuleFor(x => x.ColumnMapping).NotNull();
    }
}
