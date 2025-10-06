namespace EventManagementSystem.Domain.Interfaces;

public interface IQRCodeGeneratorService
{
    byte[] GenerateQRCode(string data);
    byte[] GenerateQRCode(string data, int pixelsPerModule, string darkColorHex, string lightColorHex);
    string GenerateQRCodeAsBase64(string data, int qRCodeSize);
}
