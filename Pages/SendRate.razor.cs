using HighFive.Data;
using HighFive.DB;
using Microsoft.AspNetCore.Components;
using System;
using System.ComponentModel.DataAnnotations;
using Fluxor;
using HighFive.Store;

namespace HighFive.Pages
{
    public class SendRateModel : ComponentBase
    {
        [Required(ErrorMessage = "Обязательное поле")]
        public string FirstName { get; set; }
        [Required(ErrorMessage = "Обязательное поле")]
        public string MiddleName { get; set; }
        [Required(ErrorMessage = "Обязательное поле")]
        public string LastName { get; set; }
        [Required(ErrorMessage = "Обязательное поле")]
        [Range(1, 10, ErrorMessage = "Возможны значения от 1 до 10")]
        public int Rate { get; set; }
        [Required(ErrorMessage = "Обязательное поле")]
        public string currentAdress { get; set; }
        public string Rewiew { get; set; }

        private ConnectDB ConnectDB = new ConnectDB("User ID=postgres;Password=k1t2i3f4;Host=localhost;Port=5432;Database=HighFive;");
        public SendRateModel()
        {

        }
        
        public void SendUserInput()
        {
            Console.WriteLine("");
            var clientInput = new ClientInput()
            {
                first_name = FirstName,
                middle_name = MiddleName,   
                last_name = LastName,
            };
            clientInput = ConnectDB.CheckUser(clientInput);
        }

    }

    public partial class SendRate
    {

        [Inject]
        private IState<GeolocationBase> GeolocationBase { get; set; }

    }
}
