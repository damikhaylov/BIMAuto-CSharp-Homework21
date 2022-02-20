using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Task1
{
    class Farmland
    // Класс реализует модель участка земли в двумерном массиве и операции с ним
    // !!!!! Во всех методах, принимающих и возвращающих координаты на участке земли отсчёт координат начинается с 1
    {
        int[,] matrix; // двумерный массив, соответствующий плану участка, каждая ячейка которого будет содержать код
                       // соответствующий типу земли, которая находится в данных координатах участка
        readonly int length; // длина участка
        readonly int width; // ширина участка

        // Словарь, содержащий соответствия типа земли и кода, соответствующего этому типу. Коды представляют собой
        // битовые флаги. 0 - пустырь, 1 - ещё не обработанная земля под сад, 2 - обработанная земля сада, 4 - в данной
        // клетке находится садовник. Применён словарь, а не enum, поскольку в словарь будут дописываться коды садовников.
        // Флаги могут сочетаться, для проверок, какие признаки имеет конкретная ячейка массива в дальнейшем используются
        // побитовые операции
        Dictionary<string, int> Codes
            = new Dictionary<string, int>() { { "Wasteland", 0 }, { "Garden", 1 }, { "Farmed", 2 }, { "Gardener", 4 } };


        int gardenTop, gardenBottom, gardenLeft, gardenRight; // Переменные для сохранения крайних границ
                                                              // всех участков с садом в пределах земельного участка

        public Farmland(int fieldlength, int fieldwidth)
        // Конструктор создаёт объект - участок земли с заданными длиной и шириной
        {
            length = fieldlength;
            width = fieldwidth;
            matrix = new int[width, length];
            // Весь массив заполняется кодом, соответствующим неиспользуемой пока земле (пустырю)
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < length; j++)
                {
                    matrix[i, j] = Codes["Wasteland"];
                }
            }
        }

        // Задаются автосвойства для чтения из вне параметров земельного участка
        public int Length { get => length; } 
        public int Width { get => width; }
        public int[,] Matrix { get => matrix; }
        public int GardenTop { get => gardenTop; }
        public int GardenBottom { get => gardenBottom; }
        public int GardenLeft { get => gardenLeft; }
        public int GardenRight { get => gardenRight; }

        public void SetGardenByCorners(int left, int top, int right, int bottom)
        // Метод заполняет участок массива кодированным обозначением сада.
        // Параметры метода - координаты прямоугольного участка, с отсчётом от 1
        {
            if (left >= 1 && top >= 1 && right <= length && bottom <= width)
            {
                if (left <= right && top <= bottom)
                {
                    for (int i = top - 1; i < bottom; i++)
                    {
                        for (int j = left - 1; j < right; j++)
                        {
                            matrix[i, j] = Codes["Garden"];
                        }
                    }
                }
                else
                {
                    throw new Exception("Верхняя и левая координаты должны быть меньше нижней и правой.");
                }
            }
            else
            {
                throw new Exception("Координаты сада не должны выходить за границы земельного участка.");
            }
            // После добавления участка с садом пересчитываются крайние границы всех участков с садом
            gardenTop = GetGardenTop();
            gardenBottom = GetGardenBottom();
            gardenLeft = GetGardenLeft();
            gardenRight = GetGardenRight();
        }
        int GetGardenTop()
        // Метод возвращает координату верхней границы сада, с отсчётом от 1
        {
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < length; j++)
                {
                    if (matrix[i, j] != Codes["Wasteland"])
                    {
                        return i + 1;
                    }
                }
            }
            return 0;
        }

        int GetGardenBottom()
        // Метод возвращает координату нижней границы сада, с отсчётом от 1
        {
            for (int i = width - 1; i >= 0; i--)
            {
                for (int j = 0; j < length; j++)
                {
                    if (matrix[i, j] != Codes["Wasteland"])
                    {
                        return i + 1;
                    }
                }
            }
            return 0;
        }
        int GetGardenLeft()
        // Метод возвращает координату левой границы сада, с отсчётом от 1
        {
            for (int i = 0; i < length; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    if (matrix[j, i] != Codes["Wasteland"])
                    {
                        return i + 1;
                    }
                }
            }
            return 0;
        }
        int GetGardenRight()
        // Метод возвращает координату правой границы сада, с отсчётом от 1
        {
            for (int i = length - 1; i >= 0; i--)
            {
                for (int j = 0; j < width; j++)
                {
                    if (matrix[j, i] != Codes["Wasteland"])
                    {
                        return i + 1;
                    }
                }
            }
            return 0;
        }

        public int GetGardenLeftInRow(int row)
        // Метод возвращает координату первой слева ячейки с необработанным садом в конкретном ряду
        {
            for (int i = GardenLeft - 1; i < GardenRight; i++)
            {
                if ((matrix[row - 1, i] & Codes["Garden"]) == Codes["Garden"])
                {
                    return i + 1;
                }
            }
            return 0;
        }
        public int GetGardenRightInRow(int row)
        // Метод возвращает координату первой справа ячейки с необработанным садом в конкретном ряду
        {
            for (int i = GardenRight - 1; i >= GardenLeft - 1; i--)
            {
                if ((matrix[row - 1, i] & Codes["Garden"]) == Codes["Garden"])
                {
                    return i + 1;
                }
            }
            return 0;
        }
        public int GetGardenTopInColumn(int column)
        // Метод возвращает координату первой сверху ячейки с необработанным садом в конкретной колонке
        {
            for (int i = GardenTop - 1; i < GardenBottom; i++)
            {
                if ((matrix[i, column - 1] & Codes["Garden"]) == Codes["Garden"])
                {
                    return i + 1;
                }
            }
            return 0;
        }
        public int GetGardenBottomInColumn(int column)
        // Метод возвращает координату первой снизу ячейки с необработанным садом в конкретной колонке
        {
            for (int i = GardenBottom - 1; i >= GardenTop - 1; i--)
            {
                if ((matrix[i, column - 1] & Codes["Garden"]) == Codes["Garden"])
                {
                    return i + 1;
                }
            }
            return 0;
        }
        public void AddGardenerToCodes(string name)
        // Метод добавляет садовника во внутренний словарь с кодами, назначая ему очередной код (битовый флаг)
        {
            if (!Codes.ContainsKey(name))
            {
                Codes.Add(name, Codes.Values.Max() * 2); // Код назначается как очередная степень 2
            }
        }
        public int GetLandCodeByName(string name)
        // Метод возвращает код типа земли из внутреннего словаря по имени этого типа. Если нет кода с таким именем,
        // возвращает -1
        {
            return (Codes.ContainsKey(name)) ? Codes[name] : -1;
        }
        public bool FarmedAt(string gardenerName, int x, int y)
        // Метод устанавливает для конкретной ячейки с координатами признак того, что она обработана конкретным
        // садовником. Операция выполняется и метод возвращает true только если ячейка содержит признак
        // необработанной земли под сад. В противном случае метод возвращает false.
        {
            if ((matrix[y - 1, x - 1] & Codes["Garden"]) == Codes["Garden"]) 
            {
                // Из кода ячейки удаляется признак необработанной земли под сад
                matrix[y - 1, x - 1] = matrix[y - 1, x - 1] ^ Codes["Garden"];
                // В код ячейки кода ячейки удаляется признак необработанной земли под сад
                matrix[y - 1, x - 1] = matrix[y - 1, x - 1] | Codes["Farmed"] | Codes[gardenerName];
                return true;
            }
            return false;
        }
        public bool CheckGardenerPresenceAt(int x, int y)
        // Метод проверяет, находится ли какой-либо садовник по заданным координатам и возвращает логическое значение
        {
            return ((matrix[y - 1, x - 1] & Codes["Gardener"]) == Codes["Gardener"]);
        }
        public void SetGardenerPresenceAt(int x, int y)
        // Метод добавляет в ячейку массива по заданным координатам признак присутствия на земле какого-либо садовника
        {
            matrix[y - 1, x - 1] = matrix[y - 1, x - 1] | Codes["Gardener"];
        }
        public void ClearGardenerPresenceAt(int x, int y)
        // Метод удаляет из ячейки массива по заданным координатам признак присутствия на земле какого-либо садовника
        {
            matrix[y - 1, x - 1] = matrix[y - 1, x - 1] ^ Codes["Gardener"];
        }
    }
}
