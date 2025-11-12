namespace FamilyTree
{
    public static class Gender
    {
        private static readonly Dictionary<int, string> Genders = new Dictionary<int, string>
    {
        {1, "Мужской"},
        {2, "Женский"}
    };

        public static string GetGenderName(int genId)
        {
            return Genders.ContainsKey(genId) ? Genders[genId] : "Неизвестно";
        }

        public static int GetGenderNumber(string genName)
        {
            foreach (var gender in Genders)
            {
                if (gender.Value == genName)
                    return gender.Key;
            }
            return -1;
        }

        public static bool IsValidGenderId(int genId)
        {
            return Genders.ContainsKey(genId);
        }
    }
}
