using RuneSlinger.Base.Abstract;

namespace RuneSlinger.Base.Commands
{
    public class RegisterCommand : ICommand
    {
        public string Email { get; private set; }
        public string Username { get; private set; }
        public string Password { get; private set; }

        public RegisterCommand(string email, string username, string password)
        {
            Email = email;
            Username = username;
            Password = password;
        }
    }

    public class RegisterResponse : ICommandResponse
    {
        public uint Id { get; private set; }

        public RegisterResponse(uint id)
        {
            Id = id;
        }
    }
}