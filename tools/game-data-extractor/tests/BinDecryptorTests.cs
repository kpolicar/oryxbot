using FluentAssertions;
using OryxBot.GameDataExtractor.Extraction;

namespace OryxBot.GameDataExtractor.Tests;

public class BinDecryptorTests
{
    [Fact]
    public void DecryptBinFile_InvalidFile_ThrowsException()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        File.WriteAllBytes(tempFile, new byte[] { 0x01, 0x02 });

        // Act
        Action act = () => BinDecryptor.DecryptBinFile(tempFile);

        // Assert
        act.Should().Throw<Exception>();
        
        // Cleanup
        File.Delete(tempFile);
    }
}
