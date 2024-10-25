using static System.Net.Mime.MediaTypeNames;
using System.Drawing;

static void Main()
{
    string imagePath = @"C:\Users\yazha\Downloads\test.jpg"; // 替换为你的图片路径  
    string inputString = "这个真的很重要 "; // 替换为你的字符串  
    string outputPath = "output_image.png"; // 输出图片路径  

    Bitmap image = new Bitmap(imagePath);
    Bitmap asciiImage = ConvertImageToAsciiImage(image, inputString);
    asciiImage.Save(outputPath);

    Console.WriteLine("ASCII art image saved to " + outputPath);
}

static Bitmap ConvertImageToAsciiImage(Bitmap image, string inputString)
{
    int width = image.Width;
    int height = image.Height;
    Bitmap asciiImage = new Bitmap(width, height);

    using (Graphics g = Graphics.FromImage(asciiImage))
    {
        g.Clear(Color.White);
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixel;


        for (int y = 0; y < height; y += 14)
        {
            int i = 0;
            for (int x = 0; x < width; x += 14)
            {
                Color pixelColor = image.GetPixel(x, y);
                char asciiChar = inputString[i % inputString.Length];

                using (Brush brush = new SolidBrush(pixelColor))
                {
                    var font = new System.Drawing.Font("Courier New", 10, FontStyle.Bold);
                    g.DrawString(asciiChar.ToString(), font, brush, new PointF(x, y));
                }
                i++;
            }
        }
    }

    return asciiImage;
}