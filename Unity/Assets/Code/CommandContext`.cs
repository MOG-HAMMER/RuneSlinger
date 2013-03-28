using System.Collections.Generic;
using RuneSlinger.Base.Abstract;

namespace Assets.Code
{
    public class CommandContext<TResponse> : CommandContext where TResponse : ICommandResponse
    {
        public TResponse Response { get; private set; }

        public CommandContext(TResponse response, IDictionary<string, IEnumerable<string>> propertyErrors, IEnumerable<string> operationErrors)
            : base(propertyErrors, operationErrors)
        {
            Response = response;
        }

        
    }
}