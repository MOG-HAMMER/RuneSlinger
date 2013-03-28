using RuneSlinger.Base.Abstract;

namespace RuneSlinger.Base.Commands
{
    public class LoginCommand : ICommand<LoginResponse>
    {
        public string Email { get; private set; }
        public string Password { get; private set; }

        public LoginCommand(string email, string password)
        {
            Email = email;
            Password = password;
        }
    }

    public class LoginResponse : ICommandResponse
    {
        public uint Id { get; private set; }
        public string Email { get; private set; }
        public string Username { get; private set; }

        public LoginResponse(uint id, string email, string username)
        {
            Id = id;
            Email = email;
            Username = username;
        }
    }
}