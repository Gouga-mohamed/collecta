using FluentValidation;

namespace CollectA.Application.Features.Imports;

public class UploadCsvCommandValidator : AbstractValidator<UploadCsvCommand>
{
    public UploadCsvCommandValidator()
    {
        RuleFor(x => x.FileName).NotEmpty().MaximumLength(255);
        RuleFor(x => x.ContentBase64).NotEmpty();
    }
}
