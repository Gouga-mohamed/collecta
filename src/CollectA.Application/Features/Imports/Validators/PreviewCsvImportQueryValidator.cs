using FluentValidation;

namespace CollectA.Application.Features.Imports;

public class PreviewCsvImportQueryValidator : AbstractValidator<PreviewCsvImportQuery>
{
    public PreviewCsvImportQueryValidator()
    {
        RuleFor(x => x.ImportId).NotEmpty();
        RuleFor(x => x.ColumnMapping).NotNull();
    }
}
