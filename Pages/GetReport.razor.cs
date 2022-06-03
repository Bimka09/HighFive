using Darnton.Blazor.DeviceInterop.Geolocation;
using HighFive.RabbitMQ;
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
		[Inject] public IRabbitMQSender mQSender { get; set; }
		[Inject] public IGeolocationService GeolocationService { get; set; }

		public void SendMessage()
		{
			try
			{
				status = "";
				mQSender.Send(adressMail);
				status = "Запрос на получение отчета отправлен";
			}
			catch
            {
				status = "Ошибка при отправке запроса";
			}
		}
	}
}
