using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Web;
using System.Net.Http;
using Newtonsoft.Json;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Converters;
using Telegram.Bot.Types;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;

namespace HomeWork9
{
    
    class Program
    {
        string path = @"D:\Skillbox\C#\HomeWork9\HomeWork9\HomeWork9\bin\Download\";
        static TelegramBotClient bot = new TelegramBotClient("5163846817:AAGFNgDOMOpTWXvCjLcnpEb4wt3fPyqmPuI");
        

        static void Main(string[] args)
        {
            var cts = new CancellationTokenSource();

            Program p = new Program();

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { } 
            };
            bot.StartReceiving(
                        p.HandleUpdateAsync,
                        HandleErrorAsync,
                        receiverOptions,
                        cancellationToken: cts.Token);

            var me = bot.GetMeAsync();

            Console.WriteLine($"Start listening for @{me.Id}");
            Console.ReadLine();

            cts.Cancel();
        }

        //Метод скачивания файлов
        static async void Download(string fileId,string fileName, string path)
        {
            var file = await bot.GetFileAsync(fileId);
            FileStream fs = new FileStream(path + fileName, FileMode.Create);
            await bot.DownloadFileAsync(file.FilePath, fs);

            fs.Close();

            fs.Dispose();
        }

        private static Task HandleErrorAsync(ITelegramBotClient arg1, Exception arg2, CancellationToken arg3)
        {
            throw new NotImplementedException();
        }

        //Метод, обрабатывающий сообщения
        async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken cancellationToken)
        {

            if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message && update?.Message?.Text != null)
            {
                await HandleMessage(bot, update.Message);
                return;
            }
            if (update.Message.Type == Telegram.Bot.Types.Enums.MessageType.Document)
            {
                Console.WriteLine($"FileId : {update.Message.Document.FileId}");
                Console.WriteLine($"FileName : {update.Message.Document.FileName}");
                Console.WriteLine($"FileSize : {update.Message.Document.FileSize}");

                Download(update.Message.Document.FileId, update.Message.Document.FileName,path);
            }
           

        }

        //Метод, принимающий сообщения
        async Task HandleMessage(ITelegramBotClient bot, Message message)
        {

            if (message.Text == "/start")
            {
                await bot.SendTextMessageAsync(message.Chat.Id, $"Привет, {message.Chat.Username}.\nСкидывай сюда файлы и я их сохраню." +
                    $"\nПосмотреть загруженные файлы можно с помощью команды: /list" +
                    "\nСкачать выбранный файл можно с помощью команды: \n/upload");
            }

            else if (message.Text == "/list")
            {
                DirectoryInfo files = new DirectoryInfo(path);
                var FileListName = files.GetFiles().ToList();

                if (FileListName.Count == 0)
                {
                    await bot.SendTextMessageAsync(message.Chat.Id, "Файлов пока нет");
                }

                for (int i = 0; i < FileListName.Count; i++)
                {
                    await bot.SendTextMessageAsync(message.Chat.Id, FileListName[i].Name);
                }
            }

            else if (message.Text.Contains("/upload"))
            {
                var send = message.Text.Substring(6);
                Upload(send, message.Chat.Id);
            }


            else
            {
                await bot.SendTextMessageAsync(message.Chat.Id, "Команда не распознана");
            }
        }

        /// <summary>

        /// Upload

        /// </summary>

        /// <param name="fileName"></param>

        /// <param name="id"></param>

        static async void Upload(string fileName, long id)
        {
            using (FileStream filestream = System.IO.File.OpenRead(fileName))
            {
                InputOnlineFile inputOnlineFile = new InputOnlineFile(filestream, fileName);

                await bot.SendDocumentAsync(id, inputOnlineFile);
            }
        }
    }

    
}

