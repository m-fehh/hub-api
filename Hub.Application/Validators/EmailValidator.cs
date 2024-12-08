using Hub.Infrastructure.Extensions;

namespace Hub.Application.Validators
{
    public class EmailValidator
    {
        /// <summary>
        /// Método responsável por verificar a validade de um Email
        /// </summary>
        /// <param name="email"> e-mail </param>
        /// <returns> true: válido; false: inválido </returns>
        public bool IsValid(string email)
        {
            if (email == null || email == "" || !email.ValidateEmail())
                return false;

            return true;
        }
    }
}
