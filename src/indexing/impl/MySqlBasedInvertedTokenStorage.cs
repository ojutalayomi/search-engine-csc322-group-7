﻿using System.Text;
using MySql.Data.MySqlClient;
using SearchEngine_.indexing.api;
using SearchEngine_.indexing.models;

namespace SearchEngine_.indexing.impl
{
    internal class MySqlBasedInvertedTokenStorage : IInvertedIndexStorage
    {
        //externalize this connection string
        private static readonly string ConnectionString = Environment.GetEnvironmentVariable("MYSQL_CONNECTION_STRING") ?? "";

        public long GetPostingListSize(Token token)
        {

            using (MySqlConnection connection = new MySqlConnection(ConnectionString))
            {
                try
                {
                    connection.Open();
                    using (MySqlCommand command = new MySqlCommand("SELECT size FROM token_posting_list_size WHERE id = @tokenId", connection))
                    {
                        command.Parameters.AddWithValue("@tokenId", token.Id);
                        long size = Convert.ToInt64(command.ExecuteScalar());
                        return size;
                    }
                }
                catch (MySqlException ex)
                {
                    //handle the exception, log it, etc.
                    throw new Exception("Error connecting to the database", ex);
                }
            }
        }

        public Dictionary<int, long> GetPostingListSizes(int[] tokenIds)
        {
            var tokenSizes = new Dictionary<int, long>();

            if (tokenIds == null || tokenIds.Length == 0)
            {
                return tokenSizes; // Return an empty dictionary if no token IDs are provided
            }

            // 1. Generate the SQL query with a dynamic IN clause
            var placeholders = new string[tokenIds.Length];
            for (int i = 0; i < tokenIds.Length; i++)
            {
                placeholders[i] = $"@id{i}";
            }
            string inClause = string.Join(",", placeholders);

            string sql = $"SELECT id, size FROM token_posting_list_size WHERE id IN ({inClause})";

            // 2. Execute the query
            using (MySqlConnection connection = new MySqlConnection(ConnectionString))
            {
                try
                {
                    connection.Open();
                    using (MySqlCommand command = new MySqlCommand(sql, connection))
                    {
                        // 3. Add parameters for each token ID
                        for (int i = 0; i < tokenIds.Length; i++)
                        {
                            command.Parameters.AddWithValue($"@id{i}", tokenIds[i]);
                        }

                        // 4. Read the results and populate the dictionary
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int id = reader.GetInt32("id");
                                long size = reader.GetInt64("size");
                                tokenSizes[id] = size;
                            }
                        }
                    }
                }
                catch (MySqlException ex)
                {
                    // Handle the exception, log it, or rethrow
                    throw new Exception("Error retrieving posting list sizes from the database.", ex);
                }
            }

            return tokenSizes;
        }


        public List<DocumentIndex> MatchTokens(Token[] tokens) {
            int shortestPostlistTokenId = GetShortestPostingListTokenId(tokens);

            //reorder tokens array so that the shortestPostlistTokenId is first
            tokens = tokens.OrderBy(t => t.Id == shortestPostlistTokenId.ToString() ? 0 : 1).ToArray();
            string query = BuildMatchingAlgorithmQuery(tokens);
        
            using (MySqlConnection connection = new MySqlConnection(ConnectionString))
            {
                try
                {
                    connection.Open();
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        // Add parameters for each token ID
                        for (int i = 0; i < tokens.Length; i++)
                        {
                            command.Parameters.AddWithValue($"@token{i}", tokens[i].Id);
                        }

                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            List<DocumentIndex> results = new List<DocumentIndex>();
                            while (reader.Read())
                            {
                                DocumentIndex index = new DocumentIndex
                                {
                                    Id = reader["document_id"].ToString(),
                                    FrequencyDict = new Dictionary<string, long>(),
                                    totalTermCount = Convert.ToInt64(reader["total_term_count"]),
                                    DocumentLink = reader["doc_link"].ToString(),
                                    DocumentType = reader["doc_type"].ToString()
                                    
                                };

                                for (int i = 0; i < tokens.Length; i++)
                                {
                                    index.FrequencyDict[tokens[i].Value] = Convert.ToInt64(reader[$"tok{i + 1}"]);
                                }

                                results.Add(index);
                            }
                            return results;
                        }
                    }
                }
                catch (MySqlException ex)
                {
                    throw new Exception("Error executing query to match tokens", ex);
                }
            }
        }

        private int GetShortestPostingListTokenId(Token[] tokens)
        {
            string query = "SELECT id FROM token_posting_list_size WHERE id IN (" + string.Join(", ", tokens.Select(t => t.Id)) + ") ORDER BY size ASC LIMIT 1;";

            using (MySqlConnection connection = new MySqlConnection(ConnectionString))
            {
                try
                {
                    connection.Open();
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        // Add parameters for each token ID
                        for (int i = 0; i < tokens.Length; i++)
                        {
                            command.Parameters.AddWithValue($"@token{i}", tokens[i].Id);
                        }

                        // Execute the command and read the result
                        object result = command.ExecuteScalar();
                        if (result != null && int.TryParse(result.ToString(), out int tokenId))
                        {
                            return tokenId;
                        }
                        else
                        {
                            throw new Exception("No matching token found or invalid result.");
                        }
                    }
                }
                catch (MySqlException ex)
                {
                    throw new Exception("Error executing query to get shortest posting list token ID", ex);
                }
            }
        }

        private string BuildMatchingAlgorithmQuery(Token[] tokens)
        {
            int[] tokenIds = tokens.Select(t => int.Parse(t.Id)).ToArray();
            // Check if the tokenIds array is null or empty
            if (tokenIds == null || tokenIds.Length == 0)
            {
                throw new ArgumentException("Token IDs cannot be null or empty.");
            }

            // Use a StringBuilder for efficient string concatenation
            var selectBuilder = new StringBuilder();
            selectBuilder.AppendLine("SELECT d.document_id AS document_id, d.document_link AS doc_link,  d.document_type AS doc_type, SUM(d.total_term_count) as total_term_count, COUNT(*) AS total_freq");

            // Dynamically create the SUM(CASE...) expressions in the specified order
            for (int i = 0; i < tokenIds.Length; i++)
            {
                selectBuilder.AppendLine(
                    $", SUM(CASE WHEN p.token_id = @token" + i + " THEN p.frequency ELSE 0 END) AS tok" + (i + 1)
                );
            }

            var whereClause = "WHERE p.token_id IN (" + string.Join(", ", tokenIds.Select((id, i) => $"@token{i}")) + ")";

            // Construct the full query
            string fullQuery = $@"
            {selectBuilder.ToString()}
            FROM posting_list p
            INNER JOIN (
                SELECT document_id, document_link, document_type, total_term_count
                FROM posting_list c
                INNER JOIN documents f ON c.document_id = f.id 
                WHERE c.token_id = @token0
                
            ) AS d ON p.document_id = d.document_id
            {whereClause}
            GROUP BY document_id
            HAVING total_freq = {tokenIds.Length}
            LIMIT 0, 1000;";

            return fullQuery;
        }


        public long GetTotalCorpusSize()
        {
            string query = "SHOW TABLE STATUS LIKE 'your_table_name';";
            long rowCount = 0;
            using (MySqlConnection connection = new MySqlConnection(ConnectionString))
            {
                try
                {
                    connection.Open();
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // The 'Rows' column is the 5th column (index 4) in the SHOW TABLE STATUS result set
                                // Or you can use GetInt64 for a more direct approach
                                rowCount = reader.GetInt64("Rows");
                            }
                        }
                    }
                }
                catch (MySqlException ex)
                {
                    // Handle the exception (e.g., log it or throw a custom exception)
                    Console.WriteLine($"Error retrieving table status: {ex.Message}");
                    throw new Exception("Unable to get approximate row count.", ex);
                }
            }
            return rowCount;
        }
    

        public void StoreIndex(DocumentIndex index)
        {
            //batch update into a temporary table
            string tempTableName = BatchUpdateIntoTemporaryTokenFrequencyTable(index.FrequencyDict);

            using (MySqlConnection connection = new MySqlConnection(ConnectionString))
            {
                try
                {
                    connection.Open();
                    using var tx = connection.BeginTransaction();

                    //sum up all token counts in index
                    index.totalTermCount = index.FrequencyDict.Values.Sum();

                    // 1) Map existing tokens
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.Transaction = tx;
                        cmd.CommandText = $"UPDATE {tempTableName} t JOIN inverted_index_table i ON t.value = i.value SET t.token_id = i.id";
                        cmd.ExecuteNonQuery();
                    }

                    // 2) Insert missing tokens
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.Transaction = tx;
                        cmd.CommandText = $"INSERT INTO inverted_index_table (value) SELECT t.value FROM {tempTableName} t LEFT JOIN inverted_index_table i ON t.value = i.value WHERE i.id IS NULL";
                        cmd.ExecuteNonQuery();
                    }

                    // 3) Map again to fill token_id
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.Transaction = tx;
                        cmd.CommandText = $"UPDATE {tempTableName} t JOIN inverted_index_table i ON t.value = i.value SET t.token_id = i.id WHERE t.token_id IS NULL";
                        cmd.ExecuteNonQuery();
                    }

                    // 4) Insert document row
                    long docId;
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.Transaction = tx;
                        cmd.CommandText = "INSERT INTO documents (document_link, total_term_count, document_type) VALUES (@doc_link, @doc_term_count, @doc_type);";
                        cmd.Parameters.AddWithValue("@doc_link", index.DocumentLink);
                        cmd.Parameters.AddWithValue("@doc_term_count", index.totalTermCount);
                        cmd.Parameters.AddWithValue("@doc_type", index.DocumentType);
                        cmd.ExecuteNonQuery();
                        docId = cmd.LastInsertedId;
                    }

                    // 5) Insert posting list rows
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.Transaction = tx;
                        cmd.CommandText = $"INSERT INTO posting_list (document_id, token_id, frequency) SELECT @doc_id, token_id, frequency FROM {tempTableName} WHERE token_id IS NOT NULL";
                        cmd.Parameters.AddWithValue("@doc_id", docId);
                        cmd.ExecuteNonQuery();
                    }

                    // 6) Update posting list sizes
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.Transaction = tx;
                        cmd.CommandText = $"INSERT INTO token_posting_list_size(id, size) SELECT token_id, 1 FROM {tempTableName} WHERE token_id IS NOT NULL ON DUPLICATE KEY UPDATE size = size + 1";
                        cmd.ExecuteNonQuery();
                    }

                    // 7) Drop temp table
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.Transaction = tx;
                        cmd.CommandText = $"DROP TEMPORARY TABLE {tempTableName}";
                        cmd.ExecuteNonQuery();
                    }

                    tx.Commit();
                }
                catch (MySqlException ex)
                {
                    //handle the exception, log it, etc.
                    throw new Exception("Error executing batch update into temporary token frequency table", ex);

                }

            }
        }

        private string BatchUpdateIntoTemporaryTokenFrequencyTable(IDictionary<string, long> tokenFrequencyTable)
        {
            string tempTableName = $"temp_token_freq_{Guid.NewGuid().ToString().Replace("-", "")}";
            using (MySqlConnection connection = new MySqlConnection(ConnectionString))
            {
                try
                {
                    connection.Open();

                    string sql = $"CREATE TEMPORARY TABLE {tempTableName} (id INT PRIMARY KEY AUTO_INCREMENT, value VARCHAR(200) UNIQUE, frequency INT, token_id INT); ";
                    var tempTableCreationCommand = new MySqlCommand(sql, connection);
                    tempTableCreationCommand.ExecuteNonQuery();
                    int batchSize = 800;
                    for (int i = 0; i < tokenFrequencyTable.Count; i += batchSize)
                    {
                        var batch = tokenFrequencyTable.Skip(i).Take(batchSize).ToList();

                        // Build the SQL INSERT statement with multiple value sets
                        var sqlBuilder = new StringBuilder($"INSERT INTO {tempTableName} (value, frequency) VALUES ");
                        var parameters = new List<MySqlParameter>();

                        for (int j = 0; j < batch.Count; j++)
                        {
                            // Append a set of parameterized values
                            sqlBuilder.Append($"( @Value{j}, @Frequency{j})");
                            if (j < batch.Count - 1)
                            {
                                sqlBuilder.Append(", ");
                            }

                            // Add parameters for each value

                            parameters.Add(new MySqlParameter($"@Value{j}", batch[j].Key));
                            parameters.Add(new MySqlParameter($"@Frequency{j}", batch[j].Value));
                        }
                        var command = connection.CreateCommand();
                        command.CommandText = sqlBuilder.ToString();
                        command.Parameters.Clear();
                        command.Parameters.AddRange(parameters.ToArray());

                        command.ExecuteNonQuery();
                    }
                }
                catch (MySqlException e)
                {
                    throw new Exception("Error executing batch update into temporary token frequency table", e);
                }
            }
            return tempTableName;
        }
    }
}
