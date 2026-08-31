using FluentValidation;

namespace CollectA.Application.Features.ReminderTemplates;

public class DeleteReminderTemplateCommandValidator : AbstractValidator<DeleteReminderTemplateCommand>
{
    public DeleteReminderTemplateCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
