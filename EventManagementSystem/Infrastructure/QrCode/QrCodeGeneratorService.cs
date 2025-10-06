using EventManagementSystem.Domain.Interfaces;
using QRCoder;

namespace EventManagementSystem.Infrastructure.QrCode;

public class QRCodeGeneratorService : IQRCodeGeneratorService
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

    public string GenerateQRCodeAsBase64(string data, int qrCodeSize)
    {
        using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
        {
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.Q);
            using (Base64QRCode qrCode = new Base64QRCode(qrCodeData))
            {
                return qrCode.GetGraphic(qrCodeSize); // Returns base64 string
            }
        }
    }
}

