using MyHttpServer;
using System.Net;
using System.Text;
using System.Text.Json;
using MyHttpServer;


HttpServer server = new HttpServer();
server.Start();

Console.WriteLine("Введите 'exit' для остановки сервера...");

while (true)
{
    string command = Console.ReadLine();
    if (command?.ToLower() == "exit")
    {
        server.Stop();
        break;
    }
}

//string settingsJson = File.ReadAllText("settings.json");
//Settings setting = JsonSerializer.Deserialize<Settings>(settingsJson);

//// TODO: проверка на существование файла settings.json и в консоль если его нет

//string urlPrefix = $"http://{setting.Server.Host}:{setting.Server.Port}/{setting.Server.Path}";

////установка адресов прослушки
//server.Prefixes.Add(urlPrefix);


//server.Start();//начинаем прослушивать входящие подключения
//Console.WriteLine("Сервер запущен и слушает: " + urlPrefix);
//string command = Console.ReadLine();
//if (command == "exit") ;

////получаем контекст
//var context = await server.GetContextAsync();

//var response = context.Response;


//// TODO: проверка на существование файла hello.html и в консоль если его нет

//// отправляемый в ответ код html возвращает
//string htmlFileText = File.ReadAllText("hello.html");
    

//byte[] buffer = Encoding.UTF8.GetBytes(htmlFileText);
////получаем поток ответа и пишем в него ответ
//response.ContentLength64 = buffer.Length;
//using Stream output = response.OutputStream;
//// отправляем данные
//await output.WriteAsync(buffer);
//await output.FlushAsync();

//Console.WriteLine("Запрос обработан");

//server.Stop();
//Console.WriteLine("Сервер завершил работу");
//Console.ReadLine();