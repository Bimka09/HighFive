using Dapper;
using HighFive.Data;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace HighFive.DB
{
    public class ConnectDB  : IDisposable
    {
        public IDbConnection _dbConnection = new NpgsqlConnection();
        private bool disposed = false;
        public ConnectDB(string connectionString)
        {
            _dbConnection = new NpgsqlConnection(connectionString);
            _dbConnection.Open();
        }
        public ClientInput CheckUser(ClientInput data)
        {
            var userExists = new ClientInput();

            var query = @"select id from clients
                        where first_name = @first_name and @middle_name = @middle_name and @last_name = @last_name";

            userExists = _dbConnection.Query<ClientInput>(query, new 
            { first_name = data.first_name, middle_name = data.middle_name , last_name = data.last_name })
                .FirstOrDefault();

            if(userExists == null)
            {
                query = @"insert into clients (first_name, middle_name, last_name)
                            values (@first_name, @middle_name, @last_name)";

                var result = _dbConnection.Execute(query, new
                {
                    first_name = data.first_name,
                    middle_name = data.middle_name,
                    last_name = data.last_name
                });

                return CheckUser(data);
            }
            else
            {
                data.id = userExists.id;
                return data;
            }
        }
        public void CheckPlace()
        {

        }
        public static void RecordRate(ClientInput data)
        {

        }
        public void Dispose()
        {
            Dispose(true);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    _dbConnection.Close();// Освобождаем управляемые ресурсы
                }
                // освобождаем неуправляемые объекты
                disposed = true;
            }
        }
    }
}
