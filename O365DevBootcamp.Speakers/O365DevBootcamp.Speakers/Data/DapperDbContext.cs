using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Dapper.FastCrud;

namespace O365DevBootcamp.Speakers.Data
{
    public class O365DevContext : DapperDbContext
    {
        public O365DevContext(string connectionstring) : base(connectionstring)
        {
            if (String.IsNullOrEmpty(connectionstring)) throw new ArgumentNullException(nameof(connectionstring));
        }

        protected override IDbConnection OpenConnection()
        {
            var connection = CreateConnection();
            connection.Open();
            return connection;
        }
        protected override IDbConnection CreateConnection()
        {
            return new SqlConnection(Connectionstring);
        }
    }

    public interface IDbContext
    {
        Task<TEntity> ExecuteAsync<TEntity>(string statement, object values = null);
        T Execute<T>(Func<IDbConnection, T> action);

        /// <summary>
        /// Executes the action in an isolated transaction
        /// </summary>
        void Execute(IEnumerable<Action<IDbConnection, IDbTransaction>> actions);

        /// <summary>
        /// Executes the s in an isolated transaction
        /// </summary>
        void Execute(Action<IDbConnection, IDbTransaction> action);

        /// <summary>
        /// Creates an action by given <paramref name="statement"/>. Uses dapper serialization for <paramref name="values"/>.
        /// </summary>
        Action<IDbConnection, IDbTransaction> CreateCommand(string statement, object values = null);


        /// <summary>
        /// Executes a query with the given statement and tries to map the result to the given entity
        /// </summary>
        Task<TEntity> QueryAsync<TEntity>(string statement, object values = null);
        /// <summary>
        /// Executes a query with the given statement and tries to map the result to the given entity collection
        /// </summary>
        Task<IList<TEntity>> QueryManyAsync<TEntity>(string statement, object values = null);

        Task<T> InsertAsync<T>(T entity);
        Task<T> UpdateAsync<T>(T entity);
        Task<bool> DeleteAsync<T>(T entity);
    }


    public abstract class DapperDbContext : IDbContext
    {
        protected abstract IDbConnection OpenConnection();
        protected abstract IDbConnection CreateConnection();

        private DapperDbContext()
        {
            //Dapper.SqlMapper.AddTypeMap( typeof( string ), System.Data.DbType.AnsiString ); 
            Dapper.SqlMapper.AddTypeMap(typeof(Guid), System.Data.DbType.AnsiString);
        }

        /// <summary>
        /// Connection string
        /// </summary>
        protected string Connectionstring { get; }

        /// <summary>
        /// Creates new Instance
        /// </summary>
        /// <param name="connectionstring">Full filename</param>
        protected DapperDbContext(string connectionstring) : this()
        {
            Connectionstring = connectionstring;
        }

        /// <summary>
        /// Executes a write operation to the relational database via Dapper
        /// </summary>
        public T Execute<T>(Func<IDbConnection, T> action)
        {
            using (var connection = OpenConnection())
            {
                return action(connection);
            }
        }

        public async Task<T> InsertAsync<T>(T entity)
        {
            using (var connection = OpenConnection())
            {
                await connection.InsertAsync(entity);
                return entity;
            }
        }

        public async Task<bool> DeleteAsync<T>(T entity)
        {
            using (var connection = OpenConnection())
            {
                return await connection.DeleteAsync(entity);
            }
        }

        public async Task<T> UpdateAsync<T>(T entity)
        {
            using (var connection = OpenConnection())
            {
                await connection.UpdateAsync(entity);
                return entity;
            }
        }

        /// <summary>
        /// Executes a write operation to the relational database via Dapper
        /// </summary>
        public void Execute(Action<IDbConnection, IDbTransaction> action)
        {
            Execute(new[] { action });
        }

        /// <summary>
        /// Executes a write operation to the relational database via Dapper
        /// </summary>
        public void Execute(IEnumerable<Action<IDbConnection, IDbTransaction>> actions)
        {
            using (IDbConnection connection = OpenConnection())
            using (IDbTransaction transaction = connection.BeginTransaction())
            {
                try
                {
                    foreach (var action in actions)
                    {
                        action(connection, transaction);
                    }
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        /// <summary>
        /// Creates an dabase functional action with given statement and values
        /// </summary>
        /// <returns>Uses Dapper serialization</returns>
        public Action<IDbConnection, IDbTransaction> CreateCommand(string statement, object values)
        {
            return (dbConnection, dbTransaction) => dbConnection.Execute(statement, values, dbTransaction);
        }


        /// <summary>
        /// Sends given scarac execute statement (like Count(*)) to the database by given values
        /// </summary>
        /// <returns>Returns a single result</returns>
        public Task<TEntity> ExecuteAsync<TEntity>(string statement, object values = null)
        {
            using (var connection = OpenConnection())
            {
                return connection.ExecuteScalarAsync<TEntity>(statement, values);
            }
        }

        /// <summary>
        /// Sends given query to the database matched to the values
        /// </summary>
        /// <returns>Returns a single result</returns>
        public async Task<TEntity> QueryAsync<TEntity>(string statement, object values = null)
        {
            IList<TEntity> entities = await QueryManyAsync<TEntity>(statement, values);
            return entities.SingleOrDefault();
        }

        /// <summary>
        /// Sends given query to the database matched to the values
        /// </summary>
        /// <returns>Returns a collection result</returns>
        public async Task<IList<TEntity>> QueryManyAsync<TEntity>(string statement, object values = null)
        {
            using (var connection = OpenConnection())
            {
                IEnumerable<TEntity> result = await connection.QueryAsync<TEntity>(statement, values);
                return result.ToList();
            }
        }
    }
}
