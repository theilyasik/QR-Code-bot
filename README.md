# QR Code Bot

Telegram-бот на C#/.NET 8, который превращает любой присланный пользователем текст в PNG с QR-кодом и отправляет его обратно в чат.

## Возможности
- Ответ на команду `/start` с приветствием и инструкциями.
- Генерация QR-кода из любого текстового сообщения.
- Отправка изображения QR-кода в виде фотографии в чат.

## Требования
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- Токен Telegram-бота, созданного через [@BotFather](https://t.me/BotFather)

## Быстрый старт
1. Клонируйте репозиторий:
   ```bash
   git clone <repository-url>
   cd QR-Code-bot
   ```
2. Установите переменную окружения с токеном бота:
   ```bash
   setx TELEGRAM_BOT_TOKEN "ваш_токен"
   ```
3. Восстановите зависимости и запустите бота:
   ```bash
   dotnet restore
   dotnet run
   ```

## Как это работает
- `Program.cs` инициализирует клиента Telegram API и запускает long polling с помощью `StartReceiving`.
- При получении текста вызывается `SendQrImageAsync`, которая генерирует QR-код с помощью пакета `QRCoder` и отправляет PNG в чат в качестве фотографии.
- Если переменная окружения `TELEGRAM_BOT_TOKEN` не задана, приложение завершит работу с сообщением в консоли.

## Основные зависимости
- [`Telegram.Bot`](https://www.nuget.org/packages/Telegram.Bot) — для работы с Telegram Bot API.
- [`QRCoder`](https://www.nuget.org/packages/QRCoder) — для генерации QR-кодов в формате PNG.

## Лицензия
Проект распространяется под лицензией MIT. См. файл [LICENSE](LICENSE).
