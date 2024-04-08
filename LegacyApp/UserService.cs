using System;

namespace LegacyApp
{
    public class UserService
    {
        private readonly ClientRepository _clientRepository;
        private readonly UserCreditService _userCreditService;
        public UserService()
        {
            _clientRepository = new ClientRepository();
            _userCreditService = new UserCreditService();
        }

        public bool AddUser(string firstName, string lastName, string email, DateTime dateOfBirth, int clientId)
        {
            var age = CalculateAge(dateOfBirth);

            if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName) || !CheckEmail(email) || age < 21)
            {
                return false;
            }

            var client = _clientRepository.GetById(clientId);

            var user = CreateUser(firstName, lastName, email, dateOfBirth, client);

            CalculateCreditLimit(user, client.Type);

            if (user.HasCreditLimit && user.CreditLimit < 500)
            {
                return false;
            }

            UserDataAccess.AddUser(user);
            return true;
        }

        private bool CheckEmail(string email)
        {
            return email.Contains("@") && email.Contains(".");
        }

        private int CalculateAge(DateTime dateOfBirth)
        {
            var now = DateTime.Now;
            var age = now.Year - dateOfBirth.Year;
            if (now.Month < dateOfBirth.Month || (now.Month == dateOfBirth.Month && now.Day < dateOfBirth.Day))
            {
                age--;
            }
            return age;
        }

        private User CreateUser(string firstName, string lastName, string email, DateTime dateOfBirth, Client client)
        {
            return new User
            {
                Client = client,
                DateOfBirth = dateOfBirth,
                EmailAddress = email,
                FirstName = firstName,
                LastName = lastName
            };
        }

        private void CalculateCreditLimit(User user, string type)
        {
            var creditLimit = _userCreditService.GetCreditLimit(user.LastName, user.DateOfBirth);
            if (type == "ImportantClient")
            {
                creditLimit *= 2;
            }
            if (type == "VeryImportantClient")
            {
                user.HasCreditLimit = false;
            }
            user.CreditLimit = creditLimit;
        }
    }
}
