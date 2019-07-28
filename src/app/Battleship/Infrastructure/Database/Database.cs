using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;

namespace Battleship.Infrastructure.Database
{
    public interface IDatabase
    {
        Task<IEnumerable<T>> QueryList<T>(string sql, object parameters = null, CommandType? commandType = null, int? commandTimeout = null);
        Task<T> Query<T>(string sql, object parameters = null, CommandType? commandType = null, int? commandTimeout = null);
        int Execute(string sql, object parameters = null, CommandType? commandType = null, int? commandTimeout = null);
        Task<int> ExecuteAsync(string sql, object parameters = null, CommandType? commandType = null, int? commandTimeout = null);
        Task<IEnumerable<TReturn>> Query<TFirst, TSecond, TReturn>(string sql, Func<TFirst, TSecond, TReturn> map, string splitOn, object parameters = null, CommandType? commandType = null, int? commandTimeout = null);
    }

    public class Database : IDatabase
    {
        private readonly IDatabaseConfiguration _configuration;

        public Database(IDatabaseConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<IEnumerable<T>> QueryList<T>(string sql, object parameters = null, CommandType? commandType = null, int? commandTimeout = null)
        {
            using (var connection = NewConnection())
            {
                return commandType != null
                    ? await connection.QueryAsync<T>(sql, parameters, commandType: commandType, commandTimeout: commandTimeout)
                    : await connection.QueryAsync<T>(sql, parameters, commandTimeout: commandTimeout);
            }
        }

        public async Task<T> Query<T>(string sql, object parameters = null, CommandType? commandType = null, int? commandTimeout = null)
        {
            var resultList = await QueryList<T>(sql, parameters, commandType, commandTimeout);
            return resultList.FirstOrDefault();
        }

        public async Task<IEnumerable<TReturn>> Query<TFirst, TSecond, TReturn>(string sql, Func<TFirst, TSecond, TReturn> map,  string splitOn, object parameters = null, CommandType? commandType = null, int? commandTimeout = null)
        {
            using (var connection = NewConnection())
            {
                return commandType != null
                    ? await connection.QueryAsync<TFirst, TSecond, TReturn>(sql, map, parameters, splitOn: splitOn, commandType: commandType, commandTimeout: commandTimeout)
                    : await connection.QueryAsync<TFirst, TSecond, TReturn>(sql, map, parameters, splitOn: splitOn, commandTimeout: commandTimeout);
            }
        }

        public int Execute(string sql, object parameters = null, CommandType? commandType = null, int? commandTimeout = null)
        {
            using (var connection = NewConnection())
            {
                return connection.Execute(sql, parameters, commandTimeout: commandTimeout);
            }
        }

        public async Task<int> ExecuteAsync(string sql, object parameters = null, CommandType? commandType = null, int? commandTimeout = null)
        {
            using (var connection = NewConnection())
            {
                return await connection.ExecuteAsync(sql, parameters, commandType: commandType, commandTimeout: commandTimeout);
            }
        }

        protected virtual DbConnection NewConnection()
        {
            var connection = new SqlConnection(_configuration.BattleshipConnectionString);
            connection.Open();
            return connection;
        }
    }
}
