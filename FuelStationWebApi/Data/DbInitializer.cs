using FuelStationWebApi.Models;
using System;
using System.Linq;
//Класс для инициализации базы данных путем заполнения ее таблиц тестовым набором записей
namespace FuelStationWebApi.Data
{
    public static class DbInitializer
    {
        public static void Initialize(FuelsContext db)
        {
            db.Database.EnsureCreated();

            // Проверка занесены ли виды топлива
            if (db.Fuels.Any())
            {
                return;   // База данных инициализирована
            }

            // Тестовые данные имитируют работу топливной базы.
            // Единицы измерения: объем емкости и приход/расход - литры, масса емкости - килограммы,
            // плотность топлива - кг/л (при температуре 15-20 °C). Значения ориентировочные.
            int operations_number = 300;   // количество операций
            int days_number = 90;          // за сколько последних суток выполнены операции

            Random randObj = new(1);

            //Заполнение таблицы видов топлива: название и плотность
            Fuel petrol92 = new() { FuelType = "Бензин Аи-92", FuelDensity = 0.735F };
            Fuel petrol95 = new() { FuelType = "Бензин Аи-95", FuelDensity = 0.750F };
            Fuel petrol98 = new() { FuelType = "Бензин Аи-98", FuelDensity = 0.760F };
            Fuel dieselSummer = new() { FuelType = "Дизельное топливо летнее", FuelDensity = 0.840F };
            Fuel dieselWinter = new() { FuelType = "Дизельное топливо зимнее", FuelDensity = 0.830F };
            Fuel dieselArctic = new() { FuelType = "Дизельное топливо арктическое", FuelDensity = 0.820F };
            Fuel jetKerosene = new() { FuelType = "Керосин авиационный ТС-1", FuelDensity = 0.780F };
            Fuel lampKerosene = new() { FuelType = "Керосин осветительный КО-25", FuelDensity = 0.820F };
            Fuel fuelOil40 = new() { FuelType = "Мазут топочный М-40", FuelDensity = 0.945F };
            Fuel fuelOil100 = new() { FuelType = "Мазут топочный М-100", FuelDensity = 0.965F };
            Fuel heatingOil = new() { FuelType = "Топливо печное бытовое", FuelDensity = 0.860F };
            Fuel crudeOil = new() { FuelType = "Нефть Urals", FuelDensity = 0.865F };
            Fuel ethanol = new() { FuelType = "Этанол топливный", FuelDensity = 0.789F };
            Fuel lpg = new() { FuelType = "Пропан-бутан СПБТ", FuelDensity = 0.540F };
            Fuel[] fuels =
            [
                petrol92, petrol95, petrol98, dieselSummer, dieselWinter, dieselArctic, jetKerosene,
                lampKerosene, fuelOil40, fuelOil100, heatingOil, crudeOil, ethanol, lpg
            ];
            db.Fuels.AddRange(fuels);

            //Заполнение таблицы емкостей: тип, материал, объем (л), масса пустой емкости (кг) и хранимое топливо.
            //Каждая емкость хранит один вид топлива, а один вид топлива может храниться в нескольких емкостях.
            //РВС и РГС - вертикальные и горизонтальные стальные резервуары (число в названии - объем в м³),
            //АЦ - автоцистерна. Массы однотипных емкостей немного различаются.
            (string Type, string Material, float Volume, float Weight, Fuel Fuel)[] tankData =
            [
                ("РВС-1000", "Сталь", 1_000_000, 41_500, petrol92),
                ("РВС-700", "Сталь", 700_000, 28_300, petrol92),
                ("РВС-700", "Сталь", 700_000, 28_500, petrol95),
                ("РВС-400", "Сталь", 400_000, 16_800, petrol95),
                ("РВС-200", "Сталь", 200_000, 9_800, petrol98),
                ("РВС-1000", "Сталь", 1_000_000, 41_900, dieselSummer),
                ("РВС-700", "Сталь", 700_000, 28_100, dieselSummer),
                ("РВС-400", "Сталь", 400_000, 16_900, dieselWinter),
                ("РВС-200", "Сталь", 200_000, 9_600, dieselArctic),
                ("РВС-400", "Сталь", 400_000, 17_000, jetKerosene),
                ("РВС-700", "Сталь", 700_000, 28_400, fuelOil40),
                ("РВС-1000", "Сталь", 1_000_000, 42_300, fuelOil100),
                ("РВС-1000", "Сталь", 1_000_000, 42_000, crudeOil),
                ("РВС-1000", "Сталь", 1_000_000, 41_700, crudeOil),
                ("РВС-700", "Сталь", 700_000, 28_200, crudeOil),
                ("РГС-50", "Сталь", 50_000, 6_600, petrol92),
                ("РГС-25", "Сталь", 25_000, 3_350, petrol98),
                ("РГС-50", "Сталь", 50_000, 6_500, dieselSummer),
                ("РГС-25", "Сталь", 25_000, 3_400, dieselWinter),
                ("РГС-25", "Сталь", 25_000, 3_300, jetKerosene),
                ("РГС-10", "Сталь", 10_000, 1_750, lampKerosene),
                ("РГС-50", "Сталь", 50_000, 6_600, heatingOil),
                ("РГС-10", "Нержавеющая сталь", 10_000, 1_600, ethanol),
                ("РГС-50", "Сталь", 50_000, 6_700, petrol95),
                ("Газгольдер 25 м³", "Сталь", 25_000, 8_200, lpg),
                ("Газгольдер 10 м³", "Сталь", 10_000, 3_900, lpg),
                ("Автоцистерна АЦ-10", "Алюминий", 10_000, 1_700, petrol92),
                ("Автоцистерна АЦ-10", "Сталь", 10_000, 2_600, dieselSummer),
                ("Ж/д цистерна 60 м³", "Сталь", 60_000, 23_800, dieselSummer),
                ("Ж/д цистерна 60 м³", "Сталь", 60_000, 24_600, fuelOil40),
                ("Ж/д цистерна 60 м³", "Сталь", 60_000, 24_100, fuelOil100),
                ("Еврокуб 1000 л", "Полиэтилен", 1_000, 62, dieselWinter),
                ("Бочка 200 л", "Сталь", 200, 19, petrol95),
                ("Бочка 200 л", "Сталь", 200, 18, heatingOil),
                ("Канистра 20 л", "Полиэтилен", 20, 1.3F, petrol92)
            ];
            Tank[] tanks = new Tank[tankData.Length];
            for (int i = 0; i < tanks.Length; i++)
            {
                tanks[i] = new Tank
                {
                    TankType = tankData[i].Type + " №" + (i + 1),   // название емкости с ее номером
                    TankMaterial = tankData[i].Material,
                    TankVolume = tankData[i].Volume,
                    TankWeight = tankData[i].Weight
                };
            }
            db.Tanks.AddRange(tanks);

            //Заполнение таблицы операций.
            //Операции моделируют работу базы: когда остаток топлива в емкости падает до 25% ее объема,
            //емкость пополняется до 85-95% объема (приход); в остальных случаях из нее отпускается топливо
            //порциями по 5-25% объема (расход). Поэтому остаток в любой емкости (сумма ее приходов и расходов)
            //никогда не бывает отрицательным и не превышает объем емкости. В начале периода все емкости пусты,
            //так что первая операция по каждой емкости - ее начальное заполнение.
            float[] stocks = new float[tanks.Length];   // текущий остаток топлива в каждой емкости, л

            //Даты операций - случайные сутки за последние days_number дней. Операции создаются в хронологическом
            //порядке, поэтому код операции растет вместе с ее датой.
            DateTime today = DateTime.Now.Date;
            DateTime[] dates = new DateTime[operations_number];
            for (int i = 0; i < dates.Length; i++)
            {
                dates[i] = today.AddDays(-randObj.Next(1, days_number + 1));
            }
            Array.Sort(dates);

            foreach (DateTime operationDate in dates)
            {
                int tankIndex = randObj.Next(tanks.Length);
                float volume = tankData[tankIndex].Volume;
                float amount;   // приход - положительное число, расход - отрицательное
                if (stocks[tankIndex] <= 0.25F * volume ||
                    (stocks[tankIndex] <= 0.70F * volume && randObj.NextDouble() < 0.15))
                {
                    // приход: емкость заполняется до 85-95% объема
                    amount = volume * (0.85F + 0.10F * randObj.NextSingle()) - stocks[tankIndex];
                }
                else
                {
                    // расход: отпускается 5-25% объема емкости
                    amount = -volume * (0.05F + 0.20F * randObj.NextSingle());
                }
                // округление: до десятков литров для емкостей от 1000 л, для остальных - до литров
                float step = volume >= 1000 ? 10 : 1;
                amount = MathF.Round(amount / step) * step;
                stocks[tankIndex] += amount;
                db.Operations.Add(new Operation
                {
                    Fuel = tankData[tankIndex].Fuel,
                    Tank = tanks[tankIndex],
                    Inc_Exp = amount,
                    Date = operationDate
                });
            }

            //сохранение изменений в базу данных, связанную с объектом контекста
            //(все записи сохраняются одним вызовом в одной транзакции, поэтому база не окажется заполненной частично)
            db.SaveChanges();
        }

    }

}
