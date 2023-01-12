using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerPlumber
{
    class Program
    {
        const int Length = 5;
        IList<TcpClient> Clients { get; } = new List<TcpClient>(2);
        IList<byte[]> Areas { get; } = new List<byte[]>(2) { null, null };
        bool Movement { get; set; } = true;

        static void Main(string[] args) => new Program().Run();
        void Run()
        {
            TcpListener listener = new TcpListener(IPAddress.Parse(
                File.ReadAllText("../../../ip.txt")), 
                int.Parse(File.ReadAllText("../../../port.txt")));
            listener.Start();
            Console.WriteLine($"Server is listening on  {listener.LocalEndpoint}");
            Connect(listener);
            Connect(listener);
            listener.Stop();
            Console.WriteLine("Server stopped listen");

            SendPlayerOrder(Clients[0], true);
            SendPlayerOrder(Clients[1], false);

            RecieveAndSendOther(clientFrom: Clients[0], clientTo: Clients[1], indexFrom: 0);
            RecieveAndSendOther(clientFrom: Clients[1], clientTo: Clients[0], indexFrom: 1);
            Console.WriteLine($"Player's 1 turn");

            //Disconnect();

            Queue<Task> tasks = new Queue<Task>();
            while (true)
            {
                foreach (TcpClient client in Clients)
                    tasks.Enqueue(Task.Run(() => OperateClient(client)));
                Task.WhenAll(tasks.Dequeue(), tasks.Dequeue()).Wait();

                if (CheckWin(0)) //проверка 1 игрока на победу
                    break;
                if (CheckWin(1)) //проверка 2 игрока на победу
                    break;
            }
            Console.WriteLine("Нажмите любую клавишу для выхода, спасибо за игру");
            Console.ReadKey();
            Disconnect();
        }
        private void Connect(TcpListener listener)
        {
            TcpClient client = listener.AcceptTcpClient();
            lock (Clients)
                Clients.Add(client);
            Console.WriteLine($"Client {client.Client.RemoteEndPoint} connected");
        }
        void SendPlayerOrder(TcpClient client, bool isFirst)
        {
            if (!client.Connected)
                return;
            BinaryWriter writer = new BinaryWriter(client.GetStream(), Encoding.Unicode);
            writer.Write(isFirst);
        }
        void RecieveAndSendOther(TcpClient clientFrom, TcpClient clientTo, int indexFrom)
        {
            if (!clientFrom.Connected || !clientTo.Connected)
                return;
            BinaryReader reader = new BinaryReader(clientFrom.GetStream(), Encoding.Unicode);
            Areas[indexFrom] = reader.ReadBytes(Length * Length);

            BinaryWriter writer = new BinaryWriter(clientTo.GetStream(), Encoding.Unicode);
            writer.Write(Areas[indexFrom]);
        }
        async Task OperateClient(TcpClient client)
        {
            BinaryReader reader = new BinaryReader(client.GetStream(), Encoding.Unicode);
            //"cellIndex.imageIndex"
            string cellInfo;
            try
            {
                cellInfo = await Task.Run(() => reader.ReadString());
            }
            catch (Exception)
            {
                throw;
            }
            //текущий ход игрока
            byte[] indexes = cellInfo.Split('.')
                .Select(str => byte.Parse(str))
                .ToArray();
            if (indexes.Length == 4)
                Move(indexes, Clients.IndexOf(client));
            else
                Rotate(indexes, Clients.IndexOf(client));

            SendAll();
            Movement = !Movement;
            Console.WriteLine($"Player's {(Movement ? 1 : 2)} turn");
        }
        void SendAll()
        {
            for (int i = 0; i < Clients.Count; i++)
            {
                BinaryWriter writer = new BinaryWriter(Clients[i].GetStream(), Encoding.Unicode);
                
                writer.Write(Areas[Movement ? 0 : 1]);
                writer.Write(!Movement);
            }
        }

        private void Move(byte[] indexes, int areaIndex)
        {
            indexes[3] = indexes[1];
            indexes[1] = 0;

            Areas[areaIndex][indexes[0]] = indexes[1];
            Areas[areaIndex][indexes[2]] = indexes[3];
        }
        private void Rotate(byte[] indexes, int areaIndex)
        {
            switch (indexes[1])
            {
                case 0:
                    return;
                case 1:
                    indexes[1] = 2;
                    break;
                case 2:
                    indexes[1] = 1;
                    break;
                case 3:
                case 4:
                case 5:
                    indexes[1]++;
                    break;
                case 6:
                    indexes[1] = 3;
                    break;
                default:
                    break;
            }
            Areas[areaIndex][indexes[0]] = indexes[1];
        }

        bool CheckWin(int index)
        {
            //если первая или последняя клетка пустая, трубопровод невозможно проложить
            if (Areas[index].First() == 0 || Areas[index].Last() == 0)
                return false;
            //если первая клетка не имеет слева и внизу трубы, трубопровод невозможно проложить
            if (Areas[index].First() != 1 && Areas[index].First() != 5)
                return false;
            //если последняя клетка не имеет справа и сверху трубы, трубопровод невозможно проложить
            if (Areas[index].Last() != 1 && Areas[index].Last() != 3)
                return false;

            byte[,] area = new byte[Length, Length];
            for (int i = 0; i < Length; i++)
                for (int j = 0; j < Length; j++)
                    area[i, j] = Areas[index][i * Length + j];
 
            for (int i = 0; i < Length; i++)
                for (int j = 0; j < Length; j++)
                {
                    if ((j != Length - 1) && (i != Length - 1))
                    {
                        if ((area[i, j] == 1 || area[i, j] == 3 /*|| area[i, j] == 4*/) &&
                            (area[i, j + 1] == 1 || area[i, j + 1] == 5 /*|| area[i, j + 1] == 6*/))
                            continue;
                        if ((area[i, j] == 2 /*|| area[i, j] == 4*/ || area[i, j] == 5) &&
                           (area[i + 1, j] == 2 || area[i + 1, j] == 3 /*|| area[i + 1, j] == 6*/))
                        {
                            i++;
                            j--;
                        }
                        else
                            return false;
                    }
                    //крайняя правая клетка
                    else if ((j == Length - 1) && (i != Length - 1))
                    {
                        if ((area[i, j] == 2 /*|| area[i, j] == 4*/ || area[i, j] == 5) &&
                            (area[i + 1, j] == 2 /*|| area[i + 1, j] == 3 || area[i + 1, j] == 6*/))
                        {
                            i++;
                            j--;
                        }
                        else
                            return false;
                    }
                    //крайняя нижняя клетка
                    else if ((i == Length - 1) && (j != Length - 1) &&
                            (area[i, j] == 1 || area[i, j] == 3 /*|| area[i, j] == 4*/) &&
                             area[i, j + 1] != 1 /*&& area[i, j + 1] != 5 && area[i, j + 1] != 6*/)
                        return false;
                    else if(area[i, j] != 1 && area[i, j] != 3) //последняя клетка
                        return false;
                }


            Console.WriteLine($"{index + 1} игрок выиграл!");
            return true;

            //Area[cellIndex] = imageIndex
            //0 - пустая
            //1 - -  left/right
            //2 -  | up/down
            //3 - |_ up/right
            //4 - |` right/down
            //5 - `| down/left
            //6 - _| left/up

            //r - 1,3,4
            //l - 1,5,6
            //d - 2,4,5
            //up - 2,3,6
        }
        void Disconnect()
        {
            foreach (TcpClient client in Clients)
            {
                client.Close();
                client.Dispose();
            }
        }
    }
}
