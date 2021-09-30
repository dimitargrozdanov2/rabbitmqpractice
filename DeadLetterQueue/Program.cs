using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace DeadLetterQueue
{
    class Program
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            //factory.UserName = "test";
            //factory.Password = "test";
            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.ExchangeDeclare("DLX", ExchangeType.Direct, true);
                    channel.QueueDeclare(queue: "deadLetter",
                                         durable: true,
                                         exclusive: false,
                                         autoDelete: false);
                    channel.QueueBind("deadLetter", "DLX", "");

                    var consumer = new EventingBasicConsumer(channel);

                    consumer.Received += (sender, eventArgs) =>
                    {
                        var message = System.Text.Encoding.UTF8.GetString(eventArgs.Body.ToArray());
                        var deathReasonBytes = eventArgs.BasicProperties.Headers["x-first-death-reason"] as byte[];
                        var deathReason = System.Text.Encoding.UTF8.GetString(deathReasonBytes);
                        Console.WriteLine($"Deadletter: {message}. Reason: {deathReason}");
                    };
                    channel.BasicConsume("deadLetter", true, consumer);

                    Console.ReadLine();

                    channel.Close();
                    connection.Close();
                }
            }

            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }
    }
}
