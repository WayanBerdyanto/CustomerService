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
        Users? ValidateUser(string username, string password);

        Task UpdateBalancekAsync(string username, decimal balance);

    }
}