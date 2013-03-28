using System;
using System.Linq;
using NHibernate;
using NHibernate.Linq;
using RuneSlinger.Base.Commands;
using RuneSlinger.Server.Abstract;
using RuneSlinger.Server.Entities;
using RuneSlinger.Server.ValueObjects;

namespace RuneSlinger.Server.CommandHandlers
{
    public class RegisterHandler : ICommandHandler<RegisterCommand>
    {
        private readonly ISession _database;

        public RegisterHandler(ISession database)
        {
            _database = database;
        }

        public void Handle(CommandContext context, RegisterCommand command)
        {
            if (string.IsNullOrWhiteSpace(command.Username) || string.IsNullOrWhiteSpace(command.Password) || string.IsNullOrWhiteSpace(command.Email))
            {
                context.RaiseOperationError("All fields are required.");
                return;
            }

            if (command.Username.Length > 128)
            {
                context.RaisePropertyError("Username", "Must be less than 128 characters long.");
                return;
            }

            if (command.Email.Length > 200)
            {
                context.RaisePropertyError("Email", "Must be less than 200 characters long.");
                return;
            }

            if (_database.Query<User>().Any(t => t.Username == command.Username || t.Email == command.Email))
            {
                context.RaiseOperationError("Username and email must be unique!");
                return;
            }

            var user = new User
            {
                Username = command.Username,
                Email = command.Email,
                CreatedAt = DateTime.UtcNow,
                Password = HashedPassword.FromPlaintext(command.Password)
            };

            _database.Save(user);
            context.SetResponse(new RegisterResponse(user.Id));
        }
    }
}