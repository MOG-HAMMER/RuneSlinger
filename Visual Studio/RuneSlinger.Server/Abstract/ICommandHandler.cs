using RuneSlinger.Base.Abstract;

namespace RuneSlinger.Server.Abstract
{
    public interface ICommandHandler<TCommand> where TCommand : ICommand
    {
        void Handle(CommandContext context, TCommand command);

    }
}