using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace NormalExchange
{
    class Subscriber
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
                    channel.ExchangeDeclare(exchange: "logs", type: ExchangeType.Fanout);

                    var arguments = new Dictionary<string, object>
                    {
                        { "x-dead-letter-exchange", "DLX" }
                    };
                    channel.QueueDeclare(queue: "normalQueue",
                                         durable: true,
                                         exclusive: false,
                                         autoDelete: false,
                                         arguments);
                    channel.QueueBind("normalQueue", "logs", "");

                    var message = GetMessage(args);
                    var body = Encoding.UTF8.GetBytes(message);
                    channel.BasicPublish(exchange: "logs",
                                         routingKey: "",
                                         basicProperties: null,
                                         body: body);
                    Console.WriteLine(" [x] Sent {0}", message);
                

                var consumer = new EventingBasicConsumer(channel);

                    consumer.Received += (sender, eventArgs) =>
                    {
                        var msg = System.Text.Encoding.UTF8.GetString(eventArgs.Body.ToArray());
                        Console.WriteLine($"msg sent: {msg}.");
                        channel.BasicReject(eventArgs.DeliveryTag, false);
                    };

                    channel.BasicConsume("normalQueue", true, consumer);
                    Console.ReadLine();

                    channel.Close();
                    connection.Close();
                }
            }

            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }

        private static string GetMessage(string[] args)
        {
            return ((args.Length > 0)
                   ? string.Join(" ", args)
                   : "info: Hello World!");
        }
    }
}
