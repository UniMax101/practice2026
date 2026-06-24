
public static class StringExtensions
{
    public static bool IsPalindrome(this string input)
    {
        string str = "";

        foreach (char c in input.ToLower())
        {
            if (!char.IsWhiteSpace(c) && !char.IsPunctuation(c))
            {
                str += c;
            }
        }
        if (str.Length == 0)
    {
      return false;
    }
        string reversed = "";

        for (int i = str.Length - 1; i >= 0; i--)
        {
            reversed += str[i];
        }

        return str == reversed;
    }
}
