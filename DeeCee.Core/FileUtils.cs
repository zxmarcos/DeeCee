namespace DeeCee.Core;

public static class FileUtils
{
    public static void ReadFileToBuffer(string filePath, byte[] buffer)
    {
        if (buffer == null)
            throw new ArgumentNullException(nameof(buffer));

        using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        {
            int totalRead = 0;
            while (totalRead < buffer.Length)
            {
                int bytesRead = fs.Read(buffer, totalRead, buffer.Length - totalRead);
                if (bytesRead == 0)
                {
                    // throw new EndOfStreamException("O arquivo terminou antes de preencher todo o buffer.");
                    return;
                }
                
                totalRead += bytesRead;
            }
        }
    }
}