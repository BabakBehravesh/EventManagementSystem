using QRCoder;
using System.Drawing;           
using System.Drawing.Imaging;   
using System.IO;               

public interface IQRCodeGenerator
{
    byte[] GenerateQRCode(string data);
    byte[] GenerateQRCode(string data, int pixelsPerModule, string darkColorHex, string lightColorHex);
}

public class QRCodeGeneratorService : IQRCodeGenerator
{
    public byte[] GenerateQRCode(string data)
    {
        return GenerateQRCode(data, 20, "#000000", "#FFFFFF");
    }


    public byte[] GenerateQRCode(string data, int pixelsPerModule, string darkColorHex, string lightColorHex)
    {
        using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
        {
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.Q);
            using (BitmapByteQRCode qrCode = new BitmapByteQRCode(qrCodeData))
            {
                return qrCode.GetGraphic(
                    pixelsPerModule,
                    darkColorHex,
                    lightColorHex
                    );
            }
        }
    }
}

