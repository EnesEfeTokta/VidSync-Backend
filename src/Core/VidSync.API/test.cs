// Bu kodu bir kere çalıştırıp anahtarları alın
using System.Security.Cryptography;
using System;

public static class TestKeyGenerator
{
	// Çağırmak için: TestKeyGenerator.GenerateAndPrintKeys();
	public static void GenerateAndPrintKeys()
	{
		using var aes = Aes.Create();
		string key = Convert.ToBase64String(aes.Key);
		string iv = Convert.ToBase64String(aes.IV);

		Console.WriteLine($"Key: {key}"); // Bu çıktıyı kopyala
		Console.WriteLine($"IV: {iv}");   // Bu çıktıyı kopyala
	}
}