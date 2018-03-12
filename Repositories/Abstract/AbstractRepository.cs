using System;
using System.Collections.Generic;
using System.Text;
using System.Data.Common;

namespace Repositories.Abstract
{
    public class AbstractRepository
    {
        protected readonly DbProviderFactory _providerFactory;
        protected readonly string _connectionString;

        public AbstractRepository(string connectionString, string provider)
        {
            _providerFactory = DbProviderFactories.GetFactory(provider);
            _connectionString = connectionString;
        }

        protected DbConnection GetConnection()
        {
            var connection = _providerFactory.CreateConnection();

            if (connection == null)
                throw new Exception("Connection can't be null");

            connection.ConnectionString = _connectionString;
            connection.Open();
            return connection;
        }
    }
}
