using MySql.Data.MySqlClient;
<<<<<<< HEAD
=======
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

>>>>>>> 2e5dba8f14fa4cbcb1ed9d7bdd65d7befb3ae391


namespace SearchEngine_.helpers
{
    public class StorageHelper
    {
<<<<<<< HEAD
        public static void InitializeStorage()
        {
            var connectionString = Environment.GetEnvironmentVariable("MYSQL_CONNECTION_STRING") ?? string.Empty;
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                Console.WriteLine("MYSQL_CONNECTION_STRING not set; skipping storage initialization.");
                return;
            }

=======
        public static string ConnectionString = Environment.GetEnvironmentVariable("MYSQL_CONNECTION_STRING") ?? "";

        public void InitializeStorage()
        {
>>>>>>> 2e5dba8f14fa4cbcb1ed9d7bdd65d7befb3ae391
            string sql = @"
CREATE TABLE IF NOT EXISTS `inverted_index_table` (
  `id` int NOT NULL AUTO_INCREMENT,
  `value` varchar(200) NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `value_UNIQUE` (`value`),
  KEY `ID_VALUE` (`id`,`value`)
) ENGINE=InnoDB AUTO_INCREMENT=16 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE IF NOT EXISTS `posting_list` (
  `id` int NOT NULL AUTO_INCREMENT,
  `document_id` int DEFAULT NULL,
  `token_id` int DEFAULT NULL,
  `frequency` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `DOCUMENT_ID_TK_ID` (`token_id`,`document_id`),
  KEY `DOCUMENT_ID` (`document_id`),
  KEY `TOKEN_ID` (`token_id`)
) ENGINE=InnoDB AUTO_INCREMENT=273 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE IF NOT EXISTS `token_posting_list_size` (
  `id` int NOT NULL AUTO_INCREMENT,
  `size` int DEFAULT '0',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=15 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE IF NOT EXISTS `documents` (
  `id` int NOT NULL AUTO_INCREMENT,
  `document_link` varchar(255) NOT NULL,
  `document_type` varchar(100) DEFAULT NULL,
  `total_term_count` int DEFAULT '0',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=44 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

";

<<<<<<< HEAD
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
=======
            using (MySqlConnection connection = new MySqlConnection(ConnectionString))
            {
                try
>>>>>>> 2e5dba8f14fa4cbcb1ed9d7bdd65d7befb3ae391
                {
                    connection.Open();
                    using (MySqlCommand command = new MySqlCommand(sql, connection))
                    {
                        // ExecuteNonQuery is used for commands that do not return data, like CREATE TABLE.
                        command.ExecuteNonQuery();
                        Console.WriteLine("All tables created successfully.");
                    }
                }
<<<<<<< HEAD
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while initializing storage: {ex.Message}");
                // Do not crash the app; continue running without DB.
=======
                catch (MySqlException ex)
                {
                    Console.WriteLine($"An error occurred while initializing storage: {ex.Message}");
                    // It is often a good practice to log the full exception details
                    // and re-throw if it's a critical error.
                    throw;
                }
>>>>>>> 2e5dba8f14fa4cbcb1ed9d7bdd65d7befb3ae391
            }
        }
    }
}