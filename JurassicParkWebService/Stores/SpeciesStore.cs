using JurassicParkWebService.Entities;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;

namespace JurassicParkWebService.Stores;

public interface ISpeciesStore : IStore<Species> {
    IList<Species> Search(string? name = null);
}

internal sealed class SpeciesStore : Store<Species>, ISpeciesStore {
    private readonly IDatabaseConfiguration _databaseConfiguration;

    public SpeciesStore(IDatabaseConfiguration databaseConfiguration) : base(databaseConfiguration) {
        _databaseConfiguration = databaseConfiguration;
    }

    public IList<Species> Search(string? name = null) {
        var speciesList = new List<Species>();
        using (var connection = new SqlConnection(_databaseConfiguration.ConnectionString)) {
            connection.Open();

            var sql = $"SELECT {SelectFieldList} FROM {EntityName}";

            if (name != null) {
                sql += " WHERE Name = @Name";
            }

            var command = new SqlCommand(sql, connection);

            if (name != null) {
                command.Parameters.AddWithValue("name", name);
            }

            using (var reader = command.ExecuteReader()) {
                while (reader.Read()) {
                    speciesList.Add(CreateEntityFromReader(reader));
                }
            }
        }

        return speciesList;
    }
}