using System;

namespace LegacyApp
{
    public class UserService
    {
        private readonly ClientRepository _clientRepository;
        private readonly UserCreditService _userCreditService;
        private static int _creditlimit;
        public UserService()
        {
            _clientRepository = new ClientRepository();
            _userCreditService = new UserCreditService();
            _creditlimit = 500;
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

            if (user.HasCreditLimit && user.CreditLimit < _creditlimit)
            {
                return false;
            }
            
            UserDataAccess.AddUser(user);
            
            CalculateCreditLimit(user, client);
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

        private void CalculateCreditLimit(User user, Client client)
        {
            var creditLimit = _userCreditService.GetCreditLimit(client.Name, user.DateOfBirth);
            if (client.Type == "ImportantClient")
            {
                creditLimit *= 2;
            }
            if (client.Type == "VeryImportantClient")
            {
                user.HasCreditLimit = false;
            }
            user.CreditLimit = creditLimit;
        }
    }
}
