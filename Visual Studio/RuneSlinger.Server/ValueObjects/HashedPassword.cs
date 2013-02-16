using System;
using System.Security.Cryptography;
using RuneSlinger.Base.Extensions;

namespace RuneSlinger.Server.ValueObjects
{
    public class HashedPassword
    {
        public static HashedPassword FromPlaintext(string plaintextPassword)
        {
            var random = new RNGCryptoServiceProvider();
            var bytes = new byte[16];
            random.GetBytes(bytes);
            var salt = bytes.ToHexString();

            return new HashedPassword((plaintextPassword + salt).ToSha1(), salt);
        }

        public string Hash { get; private set; }
        public string Salt { get; private set; }

        public HashedPassword(string hash, string salt)
        {
            Hash = hash;
            Salt = salt;
        }

        /// <summary>
        /// For nHibernate
        /// </summary>
        private HashedPassword()
        {
        }

        public bool EqualsPlaintext(string plaintext)
        {
            return (plaintext + Salt).ToSha1() == Hash;
        }
    }
}