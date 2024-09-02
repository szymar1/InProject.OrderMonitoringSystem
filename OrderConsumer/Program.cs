using Npgsql;
using OrderConsumer.Model;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace OrderConsumer
{
    public class OrderConsumer
    {
        private static string _connectionString = "Host=postgres;Username=postgres;Password=postgres;Database=ordersdb";

        public static void Main(string[] args)
        {
            var factory = new ConnectionFactory() { HostName = "rabbitmq" };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();
            channel.QueueDeclare(queue: "orders", durable: false, exclusive: false, autoDelete: false, arguments: null);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var order = JsonSerializer.Deserialize<Order>(message);

                if (order != null)
                {
                    SaveOrderToDatabase(order);
                    Console.WriteLine($"Order received and processed: {message}");
                    LogToFile($"Order received and processed: {message}");
                }
            };

            channel.BasicConsume(queue: "orders", autoAck: true, consumer: consumer);

            Console.WriteLine("Consumer started. Press [enter] to exit.");
            Console.ReadLine();
        }

        private static void SaveOrderToDatabase(Order order)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            using var cmd = new NpgsqlCommand("INSERT INTO orders (id, quantity, price, total_price) VALUES (@id, @quantity, @price, @total_price)", connection);
            cmd.Parameters.AddWithValue("id", order.Id);
            cmd.Parameters.AddWithValue("quantity", order.Quantity);
            cmd.Parameters.AddWithValue("price", order.Price);
            cmd.Parameters.AddWithValue("total_price", order.TotalPrice);

            cmd.ExecuteNonQuery();
        }

        private static void LogToFile(string message)
        {
            using var writer = new StreamWriter("Logs/logs.txt", true);
            writer.WriteLine($"{DateTime.Now}: {message}");
        }
    }
}
