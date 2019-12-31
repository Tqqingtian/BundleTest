using System.IO;
using System.Security.Cryptography;
using System.Text;

public static class MD5Helper
{
    public static string FileMD5(string filePath)
    {
        byte[] retVal;
        using (FileStream file = new FileStream(filePath, FileMode.Open))
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            retVal = md5.ComputeHash(file);
        }
        return retVal.ToHex("x2");
    }
}
public static class ByteHelper
{
    public static string ToHex(this byte[] bytes, string format)
    {
        StringBuilder stringBuilder = new StringBuilder();
        foreach (byte b in bytes)
        {
            stringBuilder.Append(b.ToString(format));
        }
        return stringBuilder.ToString();
    }
}

public static class StringHelper
{
    public static byte[] ToByteArray(this string str)
    {
        byte[] byteArray = Encoding.Default.GetBytes(str);
        return byteArray;
    }
}
