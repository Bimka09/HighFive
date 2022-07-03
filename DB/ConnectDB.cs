using Dapper;
using HighFive.Data;
using Npgsql;
using System;
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

                _dbConnection.Execute(query, new
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
        public PlaceInfo CheckPlace(PlaceInfo data)
        {
            var placeExists = new PlaceInfo();

            var query = @"select id from places
                        where adress = @adress and name = @name";

            placeExists = _dbConnection.Query<PlaceInfo>(query, new
            { adress = data.adress, name  = data.name})
                .FirstOrDefault();

            if (placeExists == null)
            {
                query = @"insert into places (adress, name)
                            values (@adress, @name)";

                _dbConnection.Execute(query, new
                {
                    adress = data.adress,
                    name= data.name
                });

                return CheckPlace(data);
            }
            else
            {
                data.id = placeExists.id;
                return data;
            }
        }
        public void RecordRate(ClientInput data)
        {
            var rateExists = new Ratings();

            var query = @"select id from ratings
                        where client_id = @id and organization_id = @organization_id";

            rateExists = _dbConnection.Query<Ratings>(query, new
            { 
                client_id = data.id,
                organization_id = data.organization_id
            }).FirstOrDefault();

            if (rateExists == null)
            {
                query = @"insert into ratings (client_id, organization_id, rate, rewiew)
                            values (@client_id, @organization_id, @rate, @rewiew)";

                _dbConnection.Execute(query, new
                {
                    client_id = data.id,
                    organization_id = data.organization_id,
                    rate = data.rate,
                    rewiew = data.rewiew
                }) ;
            }
            else
            {
                query = @"update ratings set rate = @rate, rewiew = @rewiew where id = @id";
                _dbConnection.Execute(query, new
                {
                    rate = data.rate,
                    rewiew = data.rewiew,
                    id = rateExists.id
                });
            }
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
                    _dbConnection.Close();
                }

                disposed = true;
            }
        }
    }
}
