using System.Security.Cryptography;
using GmintaDocs.Identidad.Application;

namespace GmintaDocs.Identidad.Infrastructure;

/// <summary>
/// Hasheador de contraseñas basado en PBKDF2 (HMAC-SHA256) usando sólo la BCL,
/// sin dependencias externas. El formato almacenado es
/// <c>{iteraciones}.{saltBase64}.{hashBase64}</c>.
/// </summary>
public sealed class HasheadorDeContrasenaPbkdf2 : IHasheadorDeContrasena
{
    private const int Iteraciones = 100_000;
    private const int TamanoSaltBytes = 16;
    private const int TamanoHashBytes = 32;

    public string Hashear(string contrasena)
    {
        var salt = RandomNumberGenerator.GetBytes(TamanoSaltBytes);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            contrasena, salt, Iteraciones, HashAlgorithmName.SHA256, TamanoHashBytes);

        return $"{Iteraciones}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    public bool Verificar(string contrasena, string hashAlmacenado)
    {
        var partes = hashAlmacenado.Split('.', 3);
        if (partes.Length != 3 || !int.TryParse(partes[0], out var iteraciones))
            return false;

        byte[] salt, esperado;
        try
        {
            salt = Convert.FromBase64String(partes[1]);
            esperado = Convert.FromBase64String(partes[2]);
        }
        catch (FormatException)
        {
            return false;
        }

        var calculado = Rfc2898DeriveBytes.Pbkdf2(
            contrasena, salt, iteraciones, HashAlgorithmName.SHA256, esperado.Length);

        return CryptographicOperations.FixedTimeEquals(calculado, esperado);
    }
}
