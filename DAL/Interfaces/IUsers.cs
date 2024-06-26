using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CustomerService.Models;

namespace CustomerService.DAL.Interfaces
{
    public interface IUsers : ICrud<Users>
    {
        void DeleteUser(string username);
        IEnumerable<Users> GetByNameAsync(string username);

        Users? ValidateUser(string username, string password);

        Task UpdateBalanceAsync(string username, decimal balance);
        Task UpdateBalanceShippingAsync(string username, decimal balance);
        Task UpdateBackBalanceAsync(string username, decimal balance);
        Task TopUpBalanceAsync(string username, decimal balance);

    }
}