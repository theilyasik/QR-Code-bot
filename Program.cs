using QRCoder;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

// Читаем токен бота из переменной окружения
var token = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN");
if (string.IsNullOrWhiteSpace(token))
{
    Console.WriteLine("Не найден токен в переменной окружения TELEGRAM_BOT_TOKEN. Завершаем работу.");
    return;
}

// Инициализация клиента Telegram Bot API
var botClient = new TelegramBotClient(token);

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, eventArgs) =>
{
    // Позволяем корректно завершить работу при нажатии Ctrl+C
    eventArgs.Cancel = true;
    cts.Cancel();
};

// Настройка получения обновлений через long polling
var receiverOptions = new ReceiverOptions
{
    // Получаем все типы обновлений (по умолчанию сообщения)
    AllowedUpdates = Array.Empty<UpdateType>()
};

// Стартуем long polling
botClient.StartReceiving(HandleUpdateAsync, HandleErrorAsync, receiverOptions, cancellationToken: cts.Token);

var me = await botClient.GetMe();
Console.WriteLine($"Бот @{me.Username} запущен. Нажмите Ctrl+C для остановки.");

try
{
    // Ожидаем завершения работы приложения
    await Task.Delay(-1, cts.Token);
}
catch (TaskCanceledException)
{
    Console.WriteLine("Бот остановлен.");
}

// Обработка входящих обновлений
async Task HandleUpdateAsync(ITelegramBotClient client, Update update, CancellationToken cancellationToken)
{
    if (update.Type != UpdateType.Message || update.Message is null)
        return;

    var message = update.Message;

    if (message.Text == "/start")
    {
        await client.SendMessage(
            chatId: message.Chat.Id,
            text: "Привет! Пришли мне текст — я отвечу QR-кодом.",
            cancellationToken: cancellationToken);
        return;
    }

    if (!string.IsNullOrEmpty(message.Text))
    {
        // Пользователь прислал текст
        await SendQrImageAsync(client, message.Chat.Id, message.Text, cancellationToken);
        return;
    }

    // Другие типы сообщений не поддерживаются
    await client.SendMessage(
        chatId: message.Chat.Id,
        text: "Я понимаю только текст 🙃",
        cancellationToken: cancellationToken);
}

// Простая обработка ошибок long polling
Task HandleErrorAsync(ITelegramBotClient client, Exception exception, CancellationToken cancellationToken)
{
    var errorMessage = exception switch
    {
        ApiRequestException apiRequestException =>
            $"Telegram API Error:\n[{apiRequestException.ErrorCode}] {apiRequestException.Message}",
        _ => exception.ToString()
    };

    Console.WriteLine(errorMessage);
    return Task.CompletedTask;
}

// Генерация QR-кода и отправка фото пользователю
async Task SendQrImageAsync(ITelegramBotClient client, long chatId, string content, CancellationToken cancellationToken)
{
    using var qrStream = GenerateQrCode(content);
    qrStream.Position = 0;

    var file = new InputFileStream(qrStream, "qr-code.png");

    await client.SendPhoto(
        chatId: chatId,
        photo: file,
        caption: "Вот ваш QR-код",
        cancellationToken: cancellationToken);
}

// Генерирует PNG с QR-кодом в памяти
MemoryStream GenerateQrCode(string content)
{
    // Используем QRCoder для создания QR с простыми настройками
    var qrGenerator = new QRCodeGenerator();
    var qrData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
    var qrCode = new PngByteQRCode(qrData);
    var qrBytes = qrCode.GetGraphic(pixelsPerModule: 20);

    return new MemoryStream(qrBytes);
}
