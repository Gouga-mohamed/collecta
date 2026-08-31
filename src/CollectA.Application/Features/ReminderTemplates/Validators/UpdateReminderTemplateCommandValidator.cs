using FluentValidation;

namespace CollectA.Application.Features.ReminderTemplates;

public class UpdateReminderTemplateCommandValidator : AbstractValidator<UpdateReminderTemplateCommand>
{
    public UpdateReminderTemplateCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Subject).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Body).NotEmpty();
        RuleFor(x => x.Channel).MaximumLength(50);
    }
}
