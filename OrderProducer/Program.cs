using OrderProducer.Model;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace OrderConsumer
{

    public class OrderProducer
    {
        private static readonly Random _random = new Random();
        private static int _orderId = 1;

        public static async Task Main(string[] args)
        {
            var factory = new ConnectionFactory() { HostName = "rabbitmq" };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();
            channel.QueueDeclare(queue: "orders", durable: false, exclusive: false, autoDelete: false, arguments: null);

            while (true)
            {
                var order = new Order
                {
                    Id = _orderId++,
                    Quantity = _random.Next(1, 10),
                    Price = Math.Round((decimal)_random.NextDouble() * 100, 2)
                };

                var orderJson = JsonSerializer.Serialize(order);
                var body = Encoding.UTF8.GetBytes(orderJson);

                channel.BasicPublish(exchange: "", routingKey: "orders", basicProperties: null, body: body);

                Console.WriteLine($"Order sent: {orderJson}");
                LogToFile($"Order sent: {orderJson}");

                await Task.Delay(_random.Next(5000, 30000)); // Random interval between 5 and 30 seconds
            }
        }

        private static void LogToFile(string message)
        {
            using var writer = new StreamWriter("Logs/logs.txt", true);
            writer.WriteLine($"{DateTime.Now}: {message}");
        }
    }
}