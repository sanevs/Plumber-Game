using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ClientPlumber
{
    public partial class MainWindow : Window
    {
        public Player Player { get; set; } = new Player();
        private TcpClient Server { get; set; }
        private IList<Cell> Cells { get; set; }
        private IList<Cell> EnemyCells { get; set; }

        private const int Capacity = 25;
        private const int ImageMaxIndex = 6;
        private bool? MyMovement { get; set; }
        private bool CurMovement { get; set; } = true;

        public MainWindow()
        {
            InitializeComponent();

            new ConnectWindow(Player).ShowDialog();
            if (Player.Name is null)
            {
                Close();
                return;
            }

            Cells = ((CellCollection)DataContext).Cells;
            Generate();
            EnemyCells = ((CellCollection)DataContext).EnemyCells;
        }
        //Events\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Server = new TcpClient(Player.Ip, int.Parse(Player.Port));

                string order = await RecievePlayerOrder();
                Title = string.Concat(Title, order, Player.Name);
                if (MyMovement == true)
                {
                    SendMyArea();
                    RecieveEnemyArea(EnemyCells);
                }
                else
                {
                    RecieveEnemyArea(EnemyCells);
                    SendMyArea();
                }

                await RecieveFromServer();
            }
            catch (Exception ex) when (ex is SocketException ||
                                       ex is IOException ||
                                       ex is ObjectDisposedException ||
                                       ex is ArgumentNullException ||
                                       ex is EndOfStreamException)
            {
                MessageBox.Show(ex.Message, "Ошибка!",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }
        //bool isVis = true;
        //private void TextBlock_MouseEnter(object sender, MouseEventArgs e)
        //{
        //    ChangeVisibility(sender, Visibility.Hidden, false);
        //}
        //private void TextBlock_MouseLeave(object sender, MouseEventArgs e)
        //{
        //    ChangeVisibility(sender, Visibility.Visible, false);
        //}
        //void ChangeVisibility(object sender, Visibility visibility, bool val)
        //{
        //    if (isVis == val)
        //        return;
        //    TextBlock block = (TextBlock)sender;
        //    if (block.Visibility == visibility)
        //        return;
        //    block.Dispatcher.Invoke(() => block.Visibility = visibility);
        //    isVis = val;
        //}
        private async void Image_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {//rotate
            Cell cell = (Cell)((Image)sender).DataContext;

            if (!Server.Connected || cell.Index == 0 || MyMovement != CurMovement)
                return;

            BinaryWriter writer = new BinaryWriter(Server.GetStream(), Encoding.Unicode);
            await Task.Run(() => writer.Write($"{Cells.IndexOf(cell)}.{cell.Index}"));
        }
        private async void Image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {//move
            Cell cell = (Cell)((Image)sender).DataContext;
            if (!Server.Connected || cell.Index == 0 || MyMovement != CurMovement)
                return;

            const int line = 5;
            Cell emptyCell = Cells.Where(c => c.Index == 0).First();
            int index = Cells.IndexOf(cell);
            int emptyIndex = Cells.IndexOf(emptyCell);

            //можно перемещать только клетки, соседние от вентиля
            if (index != emptyIndex + 1 &&
                index != emptyIndex - 1 &&
                index != emptyIndex - line &&
                index != emptyIndex + line)
                return;
            // 4,5   9,10   14,15   19,20 клетки являются соседними по индексам но не по расположению
            if ((index + 1) % line == 0 &&
                emptyIndex % line == 0)
                return;
            if (index % line == 0 &&
                (emptyIndex + 1) % line == 0)
                return;

            BinaryWriter writer = new BinaryWriter(Server.GetStream(), Encoding.Unicode);
            await Task.Run(() => writer.Write(
                $"{index}.{cell.Index}.{emptyIndex}.{emptyCell.Index}"));
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //disconnect from server

            //Server.Close();
            //Server.Dispose();
        }

        //Methods\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\
        private void Generate()
        {
            Random random = new Random();

            //для каждой клетки устанавливается случайная картинка трубы
            Cells = Cells.Select(cell => {
                cell.Index = random.Next(1, ImageMaxIndex + 1);
                return cell;
            })
                .ToList();

            //для успешного построения трубопровода необходимы как минимум 2 изогнутые трубы
            if (Cells.Where(cell => cell.Index > 2).Count() < 2)
            {
                Cells[0].Index = random.Next(3, ImageMaxIndex + 1);
                Cells[random.Next(1, Cells.Count)].Index = random.Next(3, ImageMaxIndex + 1);
            }

            //последняя клетка будет без трубы, там будет вентиль, через который вода не проходит
            Cells.Last().Index = 0;

            Icon = Cells.ElementAt(random.Next(Cells.Count)).Image;
        }
        private async Task<string> RecievePlayerOrder()
        {
            try
            {
                BinaryReader reader = new BinaryReader(Server.GetStream(), Encoding.Unicode);
                MyMovement = await Task.Run(() => reader.ReadBoolean());
                return MyMovement.Value ? "1 / " : "2 / ";
            }
            catch (Exception) { throw; }
        }
        private async void SendMyArea()
        {
            BinaryWriter writer = new BinaryWriter(Server.GetStream(), Encoding.Unicode);
            byte[] cells = Cells
                .Select(c => (byte)c.Index)
                .ToArray();

            await Task.Run(() => writer.Write(cells));
        }
        private async void RecieveEnemyArea(IList<Cell> cells)
        {
            BinaryReader reader = new BinaryReader(Server.GetStream(), Encoding.Unicode);
            byte[] enemyCells = await Task.Run(() => reader.ReadBytes(Capacity));
            for (int i = 0; i < cells.Count; i++)
                cells[i].Index = enemyCells[i];
        }

        private async Task RecieveFromServer()
        {
            BinaryReader reader = new BinaryReader(Server.GetStream(), Encoding.Unicode);
            while (true)
            {
                try
                {
                    byte[] cells = await Task.Run(() => reader.ReadBytes(Capacity));
                    if (MyMovement == CurMovement)
                        FillCells(cells, Cells);
                    else
                        FillCells(cells, EnemyCells);
                    CurMovement = await Task.Run(() => reader.ReadBoolean());
                }
                catch (Exception) { throw; }
            }
        }
        void FillCells(byte[] from, IList<Cell> to)
        {
            for (int i = 0; i < from.Length; i++)
                to[i].Index = from[i];
        }
    }
}
