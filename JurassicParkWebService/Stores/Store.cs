using System;
using System.Collections.Generic;
using System.Linq;
using JurassicParkWebService.Entities;
using JurassicParkWebService.Extensions;
using Microsoft.Data.SqlClient;

namespace JurassicParkWebService.Stores;

public interface IStore<T> where T : IdentifiableEntity {
    void Add(T entity);
    void Update(T entity);
    T? Get(int id);
    void Delete(int id);
}

internal abstract class Store<T> : IStore<T> where T : IdentifiableEntity, new() {
    private readonly IDatabaseConfiguration _databaseConfiguration;

    protected Store(IDatabaseConfiguration databaseConfiguration) {
        _databaseConfiguration = databaseConfiguration;
        EntityName = typeof(T).Name;
        
        var properties = typeof(T).GetProperties();
        SelectFieldList = string.Join(",", properties.Select(x => $"{EntityName}.{x.Name}"));
    }

    protected readonly string EntityName;

    public virtual void Add(T entity) {
        using (var connection = new SqlConnection(_databaseConfiguration.ConnectionString)) {
            connection.Open();

            var properties = typeof(T).GetProperties().Where(x => x.Name != nameof(IdentifiableEntity.Id)).ToList();
            var fieldList = string.Join(",", properties.Select(x => x.Name).ToList());
            var parameterList = string.Join(",", properties.Select(x => "@" + x.Name).ToList());

            var sql = $"INSERT INTO {EntityName} ({fieldList}) output INSERTED.ID VALUES ({parameterList})";
            var command = new SqlCommand(sql, connection);

            command.Parameters.AddFromProperties(entity);

            var id = (int)command.ExecuteScalar();
            entity.Id = id;
        }
    }

    public virtual void Update(T entity) {
        using (var connection = new SqlConnection(_databaseConfiguration.ConnectionString)) {
            connection.Open();

            var properties = typeof(T).GetProperties().Where(x => x.Name != nameof(IdentifiableEntity.Id)).ToList();
            var setList = string.Join(",", properties.Select(x => $"{x.Name}=@{x.Name}"));

            var sql = $"UPDATE {EntityName} SET {setList} WHERE Id=@Id";
            var command = new SqlCommand(sql, connection);

            command.Parameters.AddFromProperties(entity);

            command.Parameters.AddWithValue("Id", entity.Id);

            command.ExecuteNonQuery();
        }
    }

    public T? Get(int id) {
        using (var connection = new SqlConnection(_databaseConfiguration.ConnectionString)) {
            connection.Open();

            var sql = $"SELECT {SelectFieldList} FROM {EntityName} WHERE ID = @Id";
            var command = new SqlCommand(sql, connection);

            command.Parameters.AddWithValue("Id", id);
            
            using (var reader = command.ExecuteReader()) {
                return reader.Read() 
                    ? CreateEntityFromReader(reader)
                    : null;
            }
        }
    }

    protected IList<T> Search(IDictionary<string, string> searchParameters) {
        var entities = new List<T>();
        using (var connection = new SqlConnection(_databaseConfiguration.ConnectionString)) {
            connection.Open();

            var sql = $"SELECT {SelectFieldList} FROM {EntityName}";

            if (searchParameters.Any()) {
                sql += " WHERE ";
                sql += string.Join(" AND ", searchParameters.Keys.Select(x => $"{x}=@{x}"));
            }

            var command = new SqlCommand(sql, connection);

            if (searchParameters.Any()) {
                foreach (var (key, value) in searchParameters) {
                    command.Parameters.AddWithValue(key, value);
                }
            }

            using (var reader = command.ExecuteReader()) {
                while (reader.Read()) {
                    entities.Add(CreateEntityFromReader(reader));
                }
            }
        }

        return entities;
    }

    public void Delete(int id) {
        using (var connection = new SqlConnection(_databaseConfiguration.ConnectionString)) {
            connection.Open();

            var sql = $"DELETE FROM {EntityName} WHERE ID = @Id";
            var command = new SqlCommand(sql, connection);

            command.Parameters.AddWithValue("Id", id);

            command.ExecuteNonQuery();
        }
    }

    protected string SelectFieldList { get; }

    protected static T CreateEntityFromReader(SqlDataReader reader) {
        var entity = new T();
        var index = 0;
        foreach (var property in typeof(T).GetProperties()) {
            var value = reader.GetValue(index++);

            if (property.PropertyType.IsEnum) {
                value = Enum.Parse(property.PropertyType, (string)value);
            }

            property.SetValue(entity, value);
        }

        return entity;
    }
}