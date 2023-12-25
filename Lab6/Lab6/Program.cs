/*Задание№1
Зарегистрируйтесь на сайте https://openweathermap.org/ для получения ключа (API key) к API от сервиса погоды.
Создайте структуру Weather, содержащую свойства Country(страна), Name(город или название местности), Temp(температура воздуха), Description(описание погоды).
Используя API, получите не менее 50 значений текущей погоды в разных точках мира.
Используйте запрос вида: 
https://api.openweathermap.org/data/2.5/weather?lat={Широта}&lon={Долгота}&appid={API key}
, где:
Широта - дробная величина в диапазоне от -90 до 90. 
Долгота - дробная величина в диапазоне от -180 до 180.
API key - ключ, полученный при регистрации на сайте https://openweathermap.org/.
Значения Широты и Долготы изменяйте случайным образом в заданных диапазонах, если для полученной координаты нет значения Country или Name, следует сгенерировать новые координаты.
На основе полученных данных создайте и заполните коллекцию структур Weather.
С помощью LINQ запросов к созданной коллекции, получите и выведите на консоль следующие данные:

Страну с максимальной и минимальной температурой.
Среднюю температуру в мире.
Количество стран в коллекции.
Первую найденную страну и название местности, в которых Description принимает значение: "clear sky","rain","few clouds"
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Newtonsoft.Json.Linq;

namespace Lab6
{
    struct Weather
    {
        public string Country { get; set; }
        public string Name { get; set; }
        public double Temp { get; set; }
        public string Description { get; set; }

        public void Print()
        {
            Console.WriteLine($"Country: {Country}");
            Console.WriteLine($"Name: {Name}");
            Console.WriteLine($"Temp: {Temp}");
            Console.WriteLine($"Description: {Description}");
        }
    }
    internal class Program
    {
        
        private static readonly string API_KEY = "5e9529dfc67ed27293e38ca71793f3c4"; //ключ для доступа к данным о погоде через API

        static void Main(string[] args)
        {
            List<Weather> list = FetchWeatherList(50); //создаём и заполняем список погод

            foreach (var weather in list)
            {
                weather.Print();
            }

            PrintInfo(list);
        }

        private static List<Weather> FetchWeatherList(int num) //генерируем случайную широту/долготу
        {
            List<Weather> weatherList = new List<Weather>(); //создание нового списка
            Random random = new Random();

            for (int i = 0; i < num; ++i)
            {
                double latitude = random.NextDouble() * 180 - 90; //генерация случайной широты в пределах от -90 до 90 градусов.
                double longitude = random.NextDouble() * 360 - 180; //генерация случайной долготы в пределах от -180 до 180 градусов.
                string apiUrl = $"https://api.openweathermap.org/data/2.5/weather?lat={latitude}&lon={longitude}&appid={API_KEY}"; //составляем URL-адрес для отправки запроса к API сайта
                string jsonData = FetchData(apiUrl);  //вызов метода для получения данных о погоде в виде JSON
                JObject json = JObject.Parse(jsonData); //Разбор JSON-данных с использованием библиотеки Newtonsoft.Json.Linq;

                if (json["sys"] != null && json["sys"]["country"] != null) //Проверка, что в JSON-данных присутствует информация о стране и ее коде
                {
                    string country = json["sys"]["country"].ToString(); //страна
                    string name = json["name"].ToString(); //место
                    double temp = Convert.ToDouble(json["main"]["temp"]); //температура
                    string description = json["weather"][0]["description"].ToString(); //описание погоды

                    weatherList.Add(new Weather //добавление нового экземпляра в список
                    {
                        Country = country,
                        Name = name,
                        Temp = temp,
                        Description = description
                    });
                }
                else
                {
                    Console.WriteLine("We can not find country lat={0}, long={1}", latitude, longitude);
                }
            }

            return weatherList;
        }

        private static string FetchData(string apiUrl) //метод отправляет HTTP-запрос к заданному URL-адресу и возвращает полученные данные в виде строки
        {
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = client.GetAsync(apiUrl).Result; //Отправка асинхронного GET-запроса к заданному URL-адресу и ожидание получения ответа
                if (response.IsSuccessStatusCode) //проверка успешности (200-"OK")
                {
                    return response.Content.ReadAsStringAsync().Result; //возвращает содержимое ответа в виде строки
                }
                else
                {
                    throw new Exception($"HTTP error: {response.StatusCode}");
                }
            }
        }

        private static void PrintInfo(List<Weather> list) //функция печати
        {
            var maxTempCountry = (from w in list
                                 orderby w.Temp
                                 select w).FirstOrDefault();
                /*list.OrderByDescending(x => x.Temp).FirstOrDefault(); //поиск страны с максимальной температурой*/
            Console.WriteLine($"Country with max temp: {maxTempCountry.Country}");


            var minTempCountry = list.OrderBy(x => x.Temp).FirstOrDefault(); //поиск страны с минимальной температурой
            Console.WriteLine($"Country with min temp: {minTempCountry.Country}");


            var averageTemp = list.Average(w => w.Temp); //вычисление средней температуры
            Console.WriteLine($"Average temp: {averageTemp:F2} °C");


            var countryCount = list.Select(w => w.Country).Distinct().Count(); //кол-во стран
            Console.WriteLine($"Count of countries: {countryCount}");


            var locationWithDescription = list //поиск первой страны и местоположения с определенным описанием погоды в списке 
                .Where(w => w.Description == "clear sky" || w.Description == "rain" || w.Description == "few clouds")
                .Select(w => new { w.Country, w.Name })
                .FirstOrDefault();
            Console.WriteLine($"First country and location with the description 'clear sky', 'rain', or 'few clouds': {locationWithDescription?.Country}, {locationWithDescription?.Name}");

        }
    }
}


