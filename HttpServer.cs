using System;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace MyHttpServer
{
    public class HttpServer
    {
        private HttpListener _listener;
        private bool _isRunning;
        private string _basePath = "";

        public HttpServer()
        {
            _listener = new HttpListener();
        }

        public void Start()
        {
            try
            {
                if (!File.Exists("settings.json"))
                {
                    Console.WriteLine("Ошибка: Файл settings.json не найден! Текущая папка: " + Directory.GetCurrentDirectory());
                    return;
                }

                string settingsJson = File.ReadAllText("settings.json");
                Settings setting = JsonSerializer.Deserialize<Settings>(settingsJson);

                // Сохраняем Path из настроек, чтобы корректно отрезать его при маршрутизации
                _basePath = "/" + setting.Server.Path.Trim('/');
                if (_basePath == "/") _basePath = "";

                string urlPrefix = $"http://{setting.Server.Host}:{setting.Server.Port}{_basePath}/";

                _listener.Prefixes.Clear();
                _listener.Prefixes.Add(urlPrefix);

                _listener.Start();
                _isRunning = true;
                Console.WriteLine("Сервер запущен и слушает: " + urlPrefix);

                _ = ListenAsync();
            }
            catch (HttpListenerException ex)
            {
                Console.WriteLine($"Ошибка доступа к сети (нужны права администратора?): {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Критическая ошибка при запуске: {ex.Message}");
            }
        }

        public void Stop()
        {
            if (!_isRunning) return;

            _isRunning = false;
            _listener.Stop();
            _listener.Close();
            Console.WriteLine("Сервер завершил работу");
        }

        private async Task ListenAsync()
        {
            while (true)
            {
                if (!_isRunning) break;

                try
                {
                    var context = await _listener.GetContextAsync();
                    _ = ProcessRequestAsync(context);
                }
                catch (HttpListenerException) { break; }
                catch (ObjectDisposedException) { break; }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка прослушивания: {ex.Message}");
                }
            }
        }

        private async Task ProcessRequestAsync(HttpListenerContext context)
        {
            var response = context.Response;
            var request = context.Request;

            try
            {
                // 1. Извлекаем локальный путь из URL-адреса
                string localPath = request.Url.AbsolutePath;

                // Убираем базовый путь (например, /connection), если он есть
                if (!string.IsNullOrEmpty(_basePath) && localPath.StartsWith(_basePath))
                {
                    localPath = localPath.Substring(_basePath.Length);
                }

                // Убираем начальный слеш, чтобы получить относительный путь к файлу
                localPath = localPath.TrimStart('/');

                // 2. Если запрашивается корень, отдаем главную страницу
                if (string.IsNullOrEmpty(localPath))
                {
                    localPath = "Search2.html";
                }

                // 3. Проверяем существование запрашиваемого файла
                if (!File.Exists(localPath))
                {
                    Console.WriteLine($"Ошибка: Файл {localPath} не найден!");
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    response.Close();
                    return;
                }

                // 4. Читаем файл как массив байтов (безопасно для картинок)
                byte[] buffer = File.ReadAllBytes(localPath);

                // 5. Устанавливаем правильный Content-Type
                if (localPath.EndsWith(".css")) response.ContentType = "text/css";
                else if (localPath.EndsWith(".html")) response.ContentType = "text/html";
                else if (localPath.EndsWith(".png")) response.ContentType = "image/png";
                else if (localPath.EndsWith(".jpg") || localPath.EndsWith(".jpeg")) response.ContentType = "image/jpeg";

                response.ContentLength64 = buffer.Length;

                using Stream output = response.OutputStream;
                await output.WriteAsync(buffer);
                await output.FlushAsync();

                Console.WriteLine($"Запрос обработан: {localPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка обработки запроса: {ex.Message}");
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                response.Close();
            }
        }
    }
}