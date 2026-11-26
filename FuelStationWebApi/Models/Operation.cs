using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FuelStationWebApi.Models
{
    public class Operation
    {
        //ID операции
        [Display(Name = "Код операции")]
        public int OperationID { get; set; }
        //ID топлива
        public int FuelID { get; set; }
        //ID емкости
        public int TankID { get; set; }
        //Приход/Расход
        [Display(Name = "+Приход/-Расход")]
        public float? Inc_Exp { get; set; }
        //Дата операции
        [Display(Name = "Дата операции")]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }
        //ссылка на виды топлива
        //Атрибут JsonIgnore исключает свойство из обмена данными в формате JSON. Иначе клиент мог бы передать в теле
        //запроса вложенный объект fuel, и Entity Framework создал бы или изменил запись справочника вместе с операцией
        [JsonIgnore]
        public virtual Fuel Fuel { get; set; }
        //ссылка на емкости (см. комментарий выше)
        [JsonIgnore]
        public virtual Tank Tank { get; set; }

    }
}
