using Project.API.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Project.API.Services
{
    public class NLPService : INLPService
    {
        private static readonly Dictionary<string, int> NumberTable = new Dictionary<string, int>
        {
            { "sıfır", 0 }, { "bir", 1 }, { "iki", 2 }, { "üç", 3 }, { "dört", 4 }, { "beş", 5 },
            { "altı", 6 }, { "yedi", 7 }, { "sekiz", 8 }, { "dokuz", 9 }, { "on", 10 },
            { "yirmi", 20 }, { "otuz", 30 }, { "kırk", 40 }, { "elli", 50 }, { "altmış", 60 },
            { "yetmiş", 70 }, { "seksen", 80 }, { "doksan", 90 },
            { "yüz", 100 }, { "bin", 1000 }, { "milyon", 1000000 }
        };

        public string WordsToNumber(string input)
        {
            int currentNumber = 0; //Elimizdeki sayı
            int total = 0; //Cebimizdeki sayı

            int number;

            string suffix = string.Empty;

            string output = string.Empty;

            List<string> words = input.ToLower().Split(' ').ToList();

            foreach (string word in words)
            {
                //Birleşik olmayan sayı
                if (NumberTable.ContainsKey(word))
                {
                    int value = NumberTable[word];

                    (currentNumber, total) = ExtractNumber(value, currentNumber, total);

                    if (word == "sıfır")
                    {
                        output += NumberTable[word] + " ";
                    }
                }
                //Birleşik olan sayı
                else if (NumberTable.Keys.Any(key => word.Contains(key)))
                {
                    string integratedNumber = word;

                    List<int> numbersInString = new List<int>();

                    //String içerisindeki sayıları regex kullanarak ayır
                    numbersInString = RegexDivider(numbersInString, integratedNumber);

                    //Harf Harf Arat
                    (currentNumber, total, integratedNumber) = SearchByLetter(currentNumber, total, numbersInString, integratedNumber);

                    // Kalan ek kısmını ekle
                    suffix = integratedNumber;
                }
                //Direkt Sayı Olarak Yazılan
                else if (int.TryParse(word, out number))
                {
                    (currentNumber, total) = ExtractNumber(number, currentNumber, total);

                    if (word == "0")
                    {
                        output += word + " ";
                    }
                }
                // Sayı olmayan kelimeler
                else
                {
                    //Eğer sayısal bir değer gelmediyse sayı tamamlandı
                    total += currentNumber;

                    //Cebimizdeki sayı 0 dan büyük ise çıktıya ekle
                    if (total > 0)
                    {
                        output += total + " ";
                    }

                    //Son eki ekle (Örnek: onaltıda => da)
                    if (!string.IsNullOrWhiteSpace(suffix))
                    {
                        output += suffix + " ";
                    }

                    //Sayıları sıfırla
                    currentNumber = 0;
                    total = 0;

                    //Son eki sıfırla
                    suffix = string.Empty;

                    //Kelimeyi çıktıya ekle
                    output += word + " ";
                }
            }

            return output.Trim();
        }

        public (int currentNumber, int total) ExtractNumber(int value, int currentNumber, int total)
        {
            // Eğer değer yüz,bin,milyon ise bunu önceki sayısal değer ile çarparak ekle
            if (value == 100 || value == 1000 || value == 1000000)
            {
                //Eğer elimizdeki sayı 0 ise ön değerini 1 olarak, değil ise elimizdeki sayıyı dışarıdan gelen sayı ile çarparak elimizdeki sayıya ata
                currentNumber = (currentNumber == 0 ? 1 : currentNumber) * value;

                //Eğer cebimizdeki sayımız sıfır değil ve elimizdeki sayı yüz-bin-milyon ise toplam sayı ile çarp (Örnek: 87216)
                if (total != 0 && (currentNumber == 100 || currentNumber == 1000 || currentNumber == 1000000))
                {
                    total = total * currentNumber;
                }
                else
                {
                    total += currentNumber;
                }

                currentNumber = 0;
            }
            else
            {
                //Eğer yüzlük-binlik-milyonluk değer değilse mevcut sayıya direkt ekle
                currentNumber += value;
            }

            return (currentNumber, total);
        }

        public List<int> RegexDivider(List<int> numbersInString, string integratedNumber)
        {
            // Regex kullanarak sayıyı ayır
            MatchCollection matches = Regex.Matches(integratedNumber, @"\d+");

            foreach (Match match in matches)
            {
                if (match.Success)
                {
                    numbersInString.Add(int.Parse(match.Value));
                }
            }

            return numbersInString;
        }

        public (int currentNumber, int total, string integratedNumber) SearchByLetter(int currentNumber, int total, List<int> numbersInString, string integratedNumber)
        {
            // Doğru sıralamada alabilmek için harf harf arat
            for (int i = 1; i <= integratedNumber.Length; i++)
            {
                string substring = integratedNumber.Substring(0, i);

                //
                if (NumberTable.TryGetValue(substring, out int value))
                {
                    int subValue = NumberTable[substring];

                    (currentNumber, total) = ExtractNumber(subValue, currentNumber, total);

                    //Alınan sayıyı çıkararak kelimeyi baştan tara
                    integratedNumber = integratedNumber.Remove(0, i);
                    i = 0;
                }
                // Tek kelimede sayı-harf karışık ise
                else if (int.TryParse(substring, out int parsedNumber) && numbersInString.Contains(parsedNumber))
                {
                    int numberInString = numbersInString.FirstOrDefault(x => x == Convert.ToInt32(substring));

                    (currentNumber, total) = ExtractNumber(numberInString, currentNumber, total);

                    //Kullanılan harfleri - sayıları string'ten çıkar
                    integratedNumber = integratedNumber.Remove(0, i);
                    i = 0;
                }
            }

            return (currentNumber, total, integratedNumber);
        }

    }
}
