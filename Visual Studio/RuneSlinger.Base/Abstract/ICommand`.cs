namespace RuneSlinger.Base.Abstract
{
    public interface ICommand<TResponse> : ICommand where TResponse : ICommandResponse
    {
         
    }

}