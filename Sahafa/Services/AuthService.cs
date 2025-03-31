using Sahafa;
using System.Data;
using System.Data.SqlClient;
using Sahafa.Models;

namespace Sahafa.Services
{
    public class AuthService
    {
        private readonly string _connectionString;

        public AuthService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<(object? user, string? role)> Authenticate(string username, string password)
        {
            string normalizedUsername = username.ToLower();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                // 🔹 Kiểm tra đăng nhập là Employee
                string queryEmp = @"
                    SELECT ae.EmployeeID, e.EmployeeType 
                    FROM AccountEmployee ae
                    JOIN Employees e ON ae.EmployeeID = e.EmployeeID
                    WHERE LOWER(ae.Username) = @username AND ae.Password = @password";

                using (SqlCommand cmd = new SqlCommand(queryEmp, conn))
                {
                    cmd.Parameters.AddWithValue("@username", normalizedUsername);
                    cmd.Parameters.AddWithValue("@password", password);

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (reader.Read())
                        {
                            var employee = new Employees
                            {
                                EmployeeID = Convert.ToInt32(reader["EmployeeID"]),
                                EmployeeType = Convert.ToInt32(reader["EmployeeType"])
                            };

                            string role = employee.EmployeeType == 1 ? "Manager" : "Staff";
                            return (employee, role);
                        }
                    }
                }

                // 🔹 Kiểm tra đăng nhập là Customer
                string queryCus = @"
                    SELECT ac.CustomerID, c.FullName 
                    FROM AccountCustomer ac
                    JOIN Customers c ON ac.CustomerID = c.CustomerID
                    WHERE LOWER(ac.Username) = @username AND ac.Password = @password";

                using (SqlCommand cmd = new SqlCommand(queryCus, conn))
                {
                    cmd.Parameters.AddWithValue("@username", normalizedUsername);
                    cmd.Parameters.AddWithValue("@password", password);

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (reader.Read())
                        {
                            var customer = new Customers
                            {
                                CustomerID = Convert.ToInt32(reader["CustomerID"]),
                                FullName = reader["FullName"].ToString()
                            };

                            return (customer, "Customer");
                        }
                    }
                }
            }

            return (null, ""); // Đăng nhập thất bại
        }
    }
}
