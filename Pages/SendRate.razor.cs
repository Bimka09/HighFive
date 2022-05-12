using Blazored.SessionStorage;
using HighFive.Data;
using HighFive.DB;
using Microsoft.AspNetCore.Components;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace HighFive.Pages
{
    public class SendRateModel : ComponentBase, IDisposable
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
        public string Rewiew { get; set; }
        public string address { get; set; }
        public string orgName { get; set; }
        public string status { get; set; }
        [Inject] public ISessionStorageService sessionStorage { get; set; }

        private bool disposed = false;

        private ConnectDB ConnectDB = new ConnectDB("User ID=postgres;Password=k1t2i3f4;Host=localhost;Port=5432;Database=HighFive;");
        public SendRateModel()
        {
            
        }

        public void SendUserInputAsync()
        {

            var clientInput = new ClientInput()
            {
                first_name = FirstName,
                middle_name = MiddleName,   
                last_name = LastName,
                rate = Rate,
                rewiew = Rewiew
            };
            try
            {
                clientInput = ConnectDB.CheckUser(clientInput);
                clientInput.organization_id = ConnectDB.CheckPlace(new PlaceInfo { adress = address, name = orgName }).id;
                ConnectDB.RecordRate(clientInput);
                FirstName = "";
                MiddleName = "";
                LastName = "";
                Rate = 0;
                Rewiew = "";
                status = "Отзыв успешно отправлен";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                status = "Ошибка при отправке запроса";
            }
            StateHasChanged();
            //sessionStorage.SetItemAsync("address", status);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    ConnectDB.Dispose();// Освобождаем управляемые ресурсы
                }
                // освобождаем неуправляемые объекты
                disposed = true;
            }
        }
        public void Dispose()
        {
            Dispose(true);
        }

    }

}
