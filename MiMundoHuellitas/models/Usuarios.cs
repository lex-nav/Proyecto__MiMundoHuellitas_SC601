using System.Collections.Concurrent;

namespace MiMundoHuellitas.Models
{
    public static class Usuarios
    {
        
        private static readonly ConcurrentDictionary<string, string> _users =
            new ConcurrentDictionary<string, string>();

        public static bool Existe(string emailOrUser) =>
            _users.ContainsKey(Normalizar(emailOrUser));

        public static bool Agregar(string emailOrUser, string password) =>
            _users.TryAdd(Normalizar(emailOrUser), password);

        public static bool Validar(string emailOrUser, string password) =>
            _users.TryGetValue(Normalizar(emailOrUser), out var stored) && stored == password;

        private static string Normalizar(string s) => (s ?? "").Trim().ToLowerInvariant();
    }
}
