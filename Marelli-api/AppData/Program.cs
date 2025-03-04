using System.Data.SqlClient; // ou Npgsql para PostgreSQL
//using Marelli.Business.Validations;
//using Marelli.Business.Services.FreeTime;
//using Marelli.Infra.Repositories.Interfaces;


public class Program
{
    public static void Main()
    {
        var connectionString = "Host=localhost;Port=5432;Pooling=true;Database=TesteVonbatra;User Id=postgres;Password=!098Cao10@;";

        using (var connection = new NpgsqlConnection(connectionString)) // Use NpgsqlConnection para PostgreSQL
        {
            connection.Open();

            // Consultar dados semanais
            var weekDataQuery = "SELECT main_id AS id, day, SucceedValue, FailValue FROM WeekData WHERE main_id = 1;";
            var weekData = new List<WeekDayData>();

            using (var command = new SqlCommand(weekDataQuery, connection)) // Use NpgsqlCommand para PostgreSQL
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        weekData.Add(new WeekDayData
                        {
                            Day = reader.GetString(1),
                            SucceedValue = reader.GetInt32(2),
                            FailValue = reader.GetInt32(3)
                        });
                    }
                }
            }

            // Consultar dados mensais
            var monthDataQuery = "SELECT main_id AS id, day, SucceedValue, FailValue FROM MonthData WHERE main_id = 1;";
            var monthData = new List<MonthDayData>();

            using (var command = new SqlCommand(monthDataQuery, connection)) // Use NpgsqlCommand para PostgreSQL
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        monthData.Add(new MonthDayData
                        {
                            Day = reader.GetInt32(1),
                            SucceedValue = reader.GetInt32(2),
                            FailValue = reader.GetInt32(3)
                        });
                    }
                }
            }

            // Formatar os dados no formato desejado
            var data = new[]
            {
                new
                {
                    id = 1,
                    week = weekData.ToArray(),
                    month = monthData.ToArray()
                }
            };

            // Exibir os dados formatados
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(data, Newtonsoft.Json.Formatting.Indented));
        }
    }

    public class WeekDayData
    {
        public string Day { get; set; }
        public int SucceedValue { get; set; }
        public int FailValue { get; set; }
    }

    public class MonthDayData
    {
        public int Day { get; set; }
        public int SucceedValue { get; set; }
        public int FailValue { get; set; }
    }
}
