﻿using System.Net;
namespace SignGame;
public class User
{
    /// <summary>
    /// Никнейм пользователя
    /// </summary>
    public string? Name { get; private set; }
    /// <summary>
    /// Количество очков пользователя
    /// </summary>
    public int Scores { get; private set; }
    /// <summary>
    /// Айпи пользователя
    /// </summary>
    public string Ip { get; private set; }
    /// <summary>
    /// Конструктор
    /// </summary>
    /// <param name="name">Никнейм</param>
    /// <param name="scores">Очки</param>
    public User(string name, int scores) 
    {
        Name = name;
        Scores = scores;
        Ip = GetLocalIpAddress();
    }

    private string GetLocalIpAddress()
    {
        string hostName = Dns.GetHostName();
        IPHostEntry ipEntry = Dns.GetHostEntry(hostName);

        foreach (IPAddress ipAddress in ipEntry.AddressList)
        {
            if (ipAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                return ipAddress.ToString();
            }
        }
        return null;
    }
    /// <summary>
    /// Изменение очков у пользователя
    /// </summary>
    /// <param name="change">Сумма изменения очков</param>
    public void ChangeScores(int change) 
    {
        Scores += change;
    }
}
