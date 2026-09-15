using System.Text.Json;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Program
{
    class Program
    {
        static ITelegramBotClient client = new TelegramBotClient("Token");
        static async Task Main(string[] args)
        {
            LoadAdminsFromFile();
            await SkipOldUpdates();
            client.StartReceiving(Update, Error);
            await Task.Delay(-1);
        }

        // Список ID администраторов
        static List<long> adminIds = new List<long> { 1991980696 };

        //Булевый метод который возвращает айди админов
        static bool IsAdmin(long userId)
        {
            return adminIds.Contains(userId);
        }
        static string commands = "/help - ℹ️ О боте / Помощь\n" +
                                  "/btn2 - 📦 Кнопка 2\n";

        static ReplyKeyboardMarkup keyboard = new ReplyKeyboardMarkup(new[]
                {
                        new KeyboardButton[] { "ℹ️ 1", "📦 2"},
                        new KeyboardButton[]{ "📍 3", "🔍 4" },
                    })
        {
            ResizeKeyboard = true
        };
        static ReplyKeyboardMarkup AdminKeyboard = new ReplyKeyboardMarkup(new[]
                {
                        new KeyboardButton[] { "ℹ️ 1", "📦 2"},
                        new KeyboardButton[]{ "📍 3", "🔍 4" },
                    })
        {
            ResizeKeyboard = true
        };

        // Обработка полученных обновлений, которых может быть много :)
        async static Task Update(ITelegramBotClient client, Update update, CancellationToken token)
        {
            var message = update.Message;

            if (message == null)
                return;
            if (message.Chat.Type == ChatType.Group || message.Chat.Type == ChatType.Supergroup)
            {
                return;
            }
            // Проверка на команду /start
            if (message.Text == "/start")
            {
                if (!IsAdmin(message.Chat.Id))
                {
                    await client.SendMessage(message.Chat.Id,
                    "Название Бота \n" + $"Выберите, что вас интересует: \n\n{commands}",
                    replyMarkup: keyboard);
                }
                else
                {
                    await client.SendMessage(message.Chat.Id,
                    "Название Бота \n" + $"✨Выберите, что вас интересует: \n\n{commands}",
                    replyMarkup: AdminKeyboard);
                }
            }
            else
            {
                switch (message.Text)
                {
                    case "/help":
                    case "ℹ️ О боте / Помощь":
                        if (!IsAdmin(message.Chat.Id))
                        {
                            await client.SendMessage(message.Chat.Id, $"{commands}", replyMarkup: keyboard);
                        }
                        else
                        {
                            await client.SendMessage(message.Chat.Id, $"{commands}", replyMarkup: AdminKeyboard);
                        }
                        break;
                    default:
                            await client.SendMessage(message.Chat.Id, "Извините, я не понимаю эту команду 🤖\nВведите /help, чтобы посмотреть список доступных команд.");
                        break;
                }
            }
        }

        // Обработка ошибок которых не должно быть в проекте :D
        private static Task Error(ITelegramBotClient client, Exception exception, HandleErrorSource source, CancellationToken token)
        {
            Console.WriteLine($"Ошибка: {exception.Message}");
            return Task.CompletedTask;
        }
        private static async Task SkipOldUpdates()
        {
            try
            {
                var updates = await client.GetUpdates();
                if (updates.Any())
                {
                    // Устанавливаем offset на следующий после последнего UpdateId
                    var lastUpdateId = updates.Last().Id;
                    await client.GetUpdates(offset: lastUpdateId + 1);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при установке offset: {ex.Message}");
            }
        }
        
        static async void AddAdmin(long chatId, long adminId)
        {
            if (adminIds.Contains(adminId))
            {
                await client.SendMessage(chatId, $"👤 Пользователь с ID {adminId} уже является админом.", parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
                return;
            }

            adminIds.Add(adminId);
            SaveAdminsToFile(); // если сохраняешь в JSON
            await client.SendMessage(chatId, $"✅ Пользователь с ID {adminId} добавлен в список админов.", parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
        }
        static async void RemoveAdmin(long chatId, long adminId)
        {
            if (adminIds.Contains(adminId))
            {
                adminIds.Remove(adminId);
                SaveAdminsToFile();
                await client.SendMessage(chatId, $"✅ Пользователь с ID {adminId} удален из списка Админов.", parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
                return;
            }
            else
                await client.SendMessage(chatId, $"✅ Пользователя с ID {adminId} нет в списке Администраторов.", parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
        }
        static void SaveAdminsToFile()
        {
            string json = JsonSerializer.Serialize(adminIds);
            System.IO.File.WriteAllText("admins.json", json);
        }
        static void LoadAdminsFromFile()
        {
            string path = "admins.json";

            if (System.IO.File.Exists(path))
            {
                string json = System.IO.File.ReadAllText(path);
                var loaded = JsonSerializer.Deserialize<List<long>>(json);
                if (loaded != null)
                    adminIds = loaded;
            }
        }
    }
}