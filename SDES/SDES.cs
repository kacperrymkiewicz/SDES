using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDES
{
    internal class SDES
    {
        private readonly string _key;
        private string _key1;
        private string _key2;

        public SDES(string key)
        {
            _key = key;
            GenerateKeys();
        }

        private void GenerateKeys()
        {
            string permutedKey = Permute(_key, [2, 4, 1, 6, 3, 9, 0, 8, 7, 5]);
            string left = permutedKey.Substring(0, 5);
            string right = permutedKey.Substring(5, 5);

            left = LeftShift(left, 1);
            right = LeftShift(right, 1);

            _key1 = Permute(left + right, [5, 2, 6, 3, 7, 4, 9, 8]);

            left = LeftShift(left, 2);
            right = LeftShift(right, 2);

            _key2 = Permute(left + right, [5, 2, 6, 3, 7, 4, 9, 8]);
        }

        public string Encrypt(string plaintext)
        {
            string initialPermutation = Permute(plaintext, [1, 5, 2, 0, 3, 7, 4, 6]);
            string left = initialPermutation.Substring(0, 4);
            string right = initialPermutation.Substring(4, 4);

            string fk1 = F(right, _key1);
            string xorResult = XOR(left, fk1);

            left = right;
            right = xorResult;

            string fk2 = F(right, _key2);
            xorResult = XOR(left, fk2);

            string preOutput = xorResult + right;
            return Permute(preOutput, [3, 0, 2, 4, 6, 1, 7, 5]);
        }

        private string F(string right, string subKey)
        {
            string expanded = Permute(right, [3, 0, 1, 2, 1, 2, 3, 0]);
            string xorResult = XOR(expanded, subKey);

            string leftHalf = xorResult.Substring(0, 4);
            string rightHalf = xorResult.Substring(4, 4);

            string sboxOutput = SBox(leftHalf, new[,] {
                { "01", "00", "11", "10" },
                { "11", "10", "01", "00" },
                { "00", "10", "01", "11" },
                { "11", "01", "11", "10" }
            }) + SBox(rightHalf, new[,] {
                { "00", "01", "10", "11" },
                { "10", "00", "01", "11" },
                { "11", "00", "01", "00" },
                { "10", "01", "00", "11" }
            });

            return Permute(sboxOutput, [1, 3, 2, 0]);
        }

        private string SBox(string input, string[,] sbox)
        {
            int row = Convert.ToInt32(input[0].ToString() + input[3], 2);
            int col = Convert.ToInt32(input[1].ToString() + input[2], 2);
            return sbox[row, col];
        }

        private string XOR(string a, string b)
        {
            return string.Concat(a.Zip(b, (x, y) => x == y ? '0' : '1'));
        }

        private string LeftShift(string input, int shifts)
        {
            return input.Substring(shifts) + input.Substring(0, shifts);
        }

        private string Permute(string input, int[] permutationTable)
        {
            return string.Concat(permutationTable.Select(index => input[index]));
        }
    }


    internal class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine("Enter an 8-bit plaintext:");
                string plaintext = Console.ReadLine()!;
                Console.WriteLine("Enter a 10-bit key:");
                string key = Console.ReadLine()!;

                if (!IsValidBinaryInput(plaintext, 8) || !IsValidBinaryInput(key, 10))
                {
                    Console.WriteLine("Invalid input. Ensure the plaintext is 8 bits and the key is 10 bits.\n");
                    continue;
                }

                var sdes = new SDES(key);
                string encrypted = sdes.Encrypt(plaintext);

                Console.WriteLine($"Encrypted text: {encrypted}\n");
            }

        }

        private static bool IsValidBinaryInput(string input, int expectedLength)
        {
            return input.Length == expectedLength && input.All(c => c == '0' || c == '1');
        }
    }
}
