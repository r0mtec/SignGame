﻿using Newtonsoft.Json;
using SGame.AboutUser;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace SGame.Forms
{
    public partial class WaitGameForm : Form
    {
        private ManageUser manageUser;
        private MainForm? mainForm;
        public WaitGameForm(ManageUser manageUser, MainForm parentForm, string connectingIp)
        {
            this.manageUser = manageUser;
            this.mainForm = parentForm;
            InitializeComponent();
            ConnectingToHost(connectingIp);
        }
        private List<string> Parse(string otv)
        {
            List<string> words = new List<string>();
            string temp = "";
            for(int i = 0; i < otv.Length; i++)
            {
                if (otv[i] == ' ')
                {
                    words.Add(temp);
                    temp = "";
                }
                else temp += otv[i];
            }
            words.Add(temp);
            return words;
        }
        private bool Consist(List<string> words, List<string> wordToFind)
        {
            int count = wordToFind.Count;
            foreach(string word in words)
            {
                foreach(string check in wordToFind)
                {
                    if(word == check)
                    {
                        count--;
                        wordToFind.Remove(check);
                        break;
                    }
                }
            }
            return count == 0;
        }
        private async void ConnectingToHost(string connectingIp)
        {
            // Получаем IP-адрес из текстового поля
            string ip = connectingIp;
            const int port = 8080;

            IPEndPoint tcpEndPoint;

            try
            {
                // Попытка использовать введенный IP-адрес, если он корректен
                tcpEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            }
            catch
            {
                // TO DO: вызывать метод у мэин формы чтобы переносило обратно в форму choseGameFrom
                return;
            }

            // Создаем новый сокет для обмена данными
            var tcpSocket = new TcpClient();

            // Сериализуем объект пользователя в формат JSON
            string json = JsonConvert.SerializeObject(manageUser.User);

            // Преобразуем JSON-строку в массив байт
            var message = Encoding.UTF8.GetBytes(json);

            // Асинхронное подключение к серверу
            await tcpSocket.ConnectAsync(tcpEndPoint);

            // Отправка данных на сервер
            await tcpSocket.GetStream().WriteAsync(message, 0, message.Length);

            // Буфер для приема данных от сервера
            var buffer = new byte[256];
            var size = 0;

            // Цикл для ожидания новых сообщений от сервера
            while (true)
            {
                // Асинхронное чтение данных от сервера
                try
                {
                    size = await tcpSocket.GetStream().ReadAsync(buffer, 0, buffer.Length);
                }
                catch
                {
                    messageLabel.Text = "Сервер отключился";
                    break;
                }

                if (size == 0) break;


                // Обработка полученных данных, вывод на форму
                string receivedMessage = Encoding.UTF8.GetString(buffer, 0, size);
                List<string> parseReceivedMessage = Parse(receivedMessage);
                if (Consist(parseReceivedMessage, new List<string> { "count" }))
                {
                    countPlayersLabel.Invoke((MethodInvoker)delegate
                    {
                        countPlayersLabel.Text = parseReceivedMessage[0] + "/6";
                    });
                    
                }
                if (receivedMessage != "Прибавить всем баллы")
                {
                    messageLabel.Invoke((MethodInvoker)delegate
                    {
                        messageLabel.Text = receivedMessage;
                    });
                }
                else
                {
                    manageUser.User.ChangeScores(5);
                    json = JsonConvert.SerializeObject(manageUser.User);
                    message = Encoding.UTF8.GetBytes(json);
                    await tcpSocket.GetStream().WriteAsync(message, 0, message.Length);
                }
            }

            // Завершаем соединение и закрываем сокет
            tcpSocket.Close();
        }
    }
}