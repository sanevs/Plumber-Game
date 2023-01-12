using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace ClientPlumber
{
    public class Cell : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private BitmapSource image;
        public BitmapSource Image
        {
            get => image;
            set
            {
                if (image == value)
                    return;
                image = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Image)));
            }
        }
        private int index;
        public int Index
        {
            get => index;
            set
            {
                if (index == value && index != 0)
                    return;
                index = value;
                SetImage(index);
            }
        }
        private void SetImage(int index)
        {
            Bitmap bitmap = (Bitmap)System.Drawing.Image.
                FromFile($"../../Resources/{index}.png", true);
            Image = Imaging.CreateBitmapSourceFromHBitmap(
                bitmap.GetHbitmap(),
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
        }
    }
}
