using Microsoft.AspNetCore.Components;
using RabbitMQ.Client;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace HighFive.Pages
{
    public class GetReportModel: ComponentBase
    {
        [Required(ErrorMessage = "Обязательное поле")]
        public string adressMail { get; set; }
		public string status { get; set; }

		public void SendMessage()
		{
			try
			{
				status = "";
				var factory = new ConnectionFactory() { Uri = new Uri("amqps://rdkgouxx:z6v6gcrnpR-B5yE_KQYWK-A9xEGGiofn@rattlesnake.rmq.cloudamqp.com/rdkgouxx") };
				using (var connection = factory.CreateConnection())
				using (var channel = connection.CreateModel())
				{
					var body = Encoding.UTF8.GetBytes(adressMail);

					channel.BasicPublish(exchange: "",
								   routingKey: "SendReport",
								   basicProperties: null,
								   body: body);
				}
				status = "Запрос на получение отчета отправлен";
			}
			catch
            {
				status = "Ошибка при отправке запроса";
			}
		}
	}
}
