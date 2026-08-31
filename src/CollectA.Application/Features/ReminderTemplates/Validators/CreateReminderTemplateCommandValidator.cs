using FluentValidation;

namespace CollectA.Application.Features.ReminderTemplates;

public class CreateReminderTemplateCommandValidator : AbstractValidator<CreateReminderTemplateCommand>
{
    public CreateReminderTemplateCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Subject).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Body).NotEmpty();
        RuleFor(x => x.Channel).MaximumLength(50);
    }
}
