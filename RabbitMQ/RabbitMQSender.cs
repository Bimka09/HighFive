using RabbitMQ.Client;
using System;
using System.Text;

namespace HighFive.RabbitMQ
{
    public class RabbitMQSender: IRabbitMQSender
    {
        public void Send (string mail)
        {

			var factory = new ConnectionFactory() { Uri = new Uri("amqps://rdkgouxx:z6v6gcrnpR-B5yE_KQYWK-A9xEGGiofn@rattlesnake.rmq.cloudamqp.com/rdkgouxx") };
			using (var connection = factory.CreateConnection())
			using (var channel = connection.CreateModel())
			{
				var body = Encoding.UTF8.GetBytes(mail);

				channel.BasicPublish(exchange: "",
								routingKey: "SendReport",
								basicProperties: null,
								body: body);
			}

		}
    }
}
