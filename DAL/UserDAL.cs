using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using CustomerService.Connection;
using CustomerService.DAL.Interfaces;
using CustomerService.Models;
using Dapper;
using Microsoft.IdentityModel.Tokens;

namespace CustomerService.DAL
{
    public class UserDAL : IUsers
    {
        private readonly IConfiguration _config;
        private readonly Connect _conn;

        public UserDAL(IConfiguration config)
        {
            _config = config;
            _conn = new Connect(_config);
        }

        public void DeleteUser(string username)
        {
            using (SqlConnection conn = _conn.GetConnectDb())
            {
                var strSql = @"DELETE FROM Users WHERE Username = @Username";
                try
                {
                    conn.Execute(strSql, new { Username = username });
                }
                catch (SqlException sqlEx)
                {
                    throw new ArgumentException($"Error: {sqlEx.Message} - {sqlEx.Number}");
                }
                catch (Exception ex)
                {
                    throw new ArgumentException($"Error: {ex.Message}");
                }
            }
        }
        public void Delete(int id)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Users> GetAll()
        {
            using (SqlConnection conn = _conn.GetConnectDb())
            {
                var strSql = @"SELECT * FROM Users ORDER BY UserName asc";
                var users = conn.Query<Users>(strSql);
                return users;
            }
        }
        public async Task<Users> GetByNameAsync(string name)
        {
            using (SqlConnection conn = _conn.GetConnectDb())
            {
                var strSql = @"SELECT * FROM Users  WHERE UserName = @UserName";
                var param = new { UserName = name };
                var user = conn.QuerySingleOrDefault<Users>(strSql, param);
                if (user == null)
                {
                    throw new ArgumentException("Data tidak ditemukan");
                }
                try
                {
                    int rowsAffected = await conn.ExecuteAsync(strSql, param);
                    if (rowsAffected == 0)
                    {
                        throw new InvalidOperationException("Tidak ada baris yang diupdate.");
                    }
                    return user;
                }
                catch (SqlException sqlEx)
                {
                    throw new ArgumentException($"Error: {sqlEx.Message} - {sqlEx.Number}");
                }
                catch (Exception ex)
                {
                    throw new ArgumentException($"Error: {ex.Message}");
                }
            }
        }
        public Users GetByName(string name)
        {
            using (SqlConnection conn = _conn.GetConnectDb())
            {
                var strSql = @"SELECT * FROM Users  WHERE UserName = @UserName";
                var param = new { UserName = name };
                var user = conn.QueryFirstOrDefault<Users>(strSql, param);
                if (user == null)
                {
                    throw new ArgumentException("Data tidak ditemukan");
                }
                return user;
            }
        }

        public void Insert(Users obj)
        {
            using (SqlConnection conn = _conn.GetConnectDb())
            {
                var strSql = @"INSERT INTO Users (UserName, Password, FullName, Balance) VALUES (@UserName, @Password, @FullName, @Balance); SELECT CAST(SCOPE_IDENTITY() as int);";
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(obj.Password);
                var param = new { UserName = obj.UserName, Password = hashedPassword, FullName = obj.FullName, Balance = obj.Balance };
                try
                {
                    conn.Execute(strSql, param);

                }
                catch (SqlException sqlEx)
                {
                    throw new ArgumentException($"Error: {sqlEx.Message} - {sqlEx.Number}");
                }
                catch (Exception ex)
                {
                    throw new ArgumentException($"Error: {ex.Message}");
                }
            }
        }

        public async Task UpdateBalanceAsync(string username, decimal balance)
        {
            using (SqlConnection conn = _conn.GetConnectDb())
            {
                var strSql = @"UPDATE Users SET Balance = Balance - @Price WHERE UserName = @UserName";
                var param = new
                {
                    UserName = username,
                    Price = balance
                };
                try
                {
                    int rowsAffected = await conn.ExecuteAsync(strSql, param);
                    if (rowsAffected == 0)
                    {
                        throw new InvalidOperationException("Tidak ada baris yang diupdate.");
                    }
                }
                catch (SqlException sqlEx)
                {
                    throw new ArgumentException($"Error: {sqlEx.Message} - {sqlEx.Number}");
                }
                catch (Exception ex)
                {
                    throw new ArgumentException($"Error: {ex.Message}");
                }
            }
        }

        public async Task UpdateBackBalanceAsync(string username, decimal balance)
        {
            using (SqlConnection conn = _conn.GetConnectDb())
            {
                var strSql = @"UPDATE Users SET Balance = Balance + @Price WHERE UserName = @UserName";
                var param = new
                {
                    UserName = username,
                    Price = balance
                };
                try
                {
                    int rowsAffected = await conn.ExecuteAsync(strSql, param);
                    if (rowsAffected == 0)
                    {
                        throw new InvalidOperationException("Tidak ada baris yang diupdate.");
                    }
                }
                catch (SqlException sqlEx)
                {
                    throw new ArgumentException($"Error: {sqlEx.Message} - {sqlEx.Number}");
                }
                catch (Exception ex)
                {
                    throw new ArgumentException($"Error: {ex.Message}");
                }
            }
        }

        public async Task TopUpBalanceAsync(string username, decimal balance)
        {
            using (SqlConnection conn = _conn.GetConnectDb())
            {
                var strSql = @"UPDATE Users SET Balance = Balance + @Amount WHERE UserName = @UserName";
                var param = new
                {
                    UserName = username,
                    Amount = balance
                };
                try
                {
                    int rowsAffected = await conn.ExecuteAsync(strSql, param);
                    if (rowsAffected == 0)
                    {
                        throw new InvalidOperationException("Tidak ada baris yang diupdate.");
                    }
                }
                catch (SqlException sqlEx)
                {
                    throw new ArgumentException($"Error: {sqlEx.Message} - {sqlEx.Number}");
                }
                catch (Exception ex)
                {
                    throw new ArgumentException($"Error: {ex.Message}");
                }
            }
        }

        public Users? ValidateUser(string username, string password)
        {
            using (SqlConnection conn = _conn.GetConnectDb())
            {
                var strSql = @"SELECT UserName, Password 
                               FROM Users 
                               WHERE UserName = @UserName";
                var user = conn.QuerySingleOrDefault<Users>(strSql, new { UserName = username });

                if (user != null && BCrypt.Net.BCrypt.Verify(password, user.Password))
                {
                    return user;
                }
                return null;
            }
        }

        public void Update(Users obj)
        {
            using (SqlConnection conn = _conn.GetConnectDb())
            {
                var strSql = @"UPDATE Users SET Password =  @Password, FullName = @FullName";
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(obj.Password);
                var param = new { UserName = obj.UserName, Password = hashedPassword, FullName = obj.FullName };
                try
                {
                    conn.Execute(strSql, param);
                }
                catch (SqlException sqlEx)
                {
                    throw new ArgumentException($"Error: {sqlEx.Message} - {sqlEx.Number}");
                }
                catch (Exception ex)
                {
                    throw new ArgumentException($"Error: {ex.Message}");
                }
            }
        }

        public string GenerateJwtToken(Users user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("your_secret_key_here"));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: "your_issuer_here",
                audience: "your_audience_here",
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}