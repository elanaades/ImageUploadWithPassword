using System.Data.SqlClient;

namespace April_17_Homework_Image_Upload_w_Password.Data
{
    public class DatabaseManager
    {
        private string _connectionString;
        public DatabaseManager(string connectionString)
        {
            _connectionString = connectionString;
        }
        public void AddImage(Image image)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "INSERT INTO Images (FileName, Password, Views) VALUES (@filename, @password, 0); SELECT SCOPE_IDENTITY();";
            cmd.Parameters.AddWithValue("@filename", image.FileName);
            cmd.Parameters.AddWithValue("@password", image.Password);
            conn.Open();
            image.Id = (int)(decimal)cmd.ExecuteScalar();
        }

        public Image GetImage(int id)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM  Images WHERE id = @id";
            cmd.Parameters.AddWithValue("@id", id);
            conn.Open();
            var reader = cmd.ExecuteReader();
            if(!reader.Read())
            {
                return null;
            }
            return new Image
            {
                Id = (int)reader["Id"],
                FileName = (string)reader["FileName"],
                Password = (string)reader["Password"],
                Views = (int)reader["Views"],
            };
        }

        public void IncrementViewCount(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "Update Images SET Views = Views + 1 WHERE Id = @id";
            cmd.Parameters.AddWithValue("@id", id);
            connection.Open();
            cmd.ExecuteNonQuery();
        }
    }
}