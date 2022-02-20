using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Task1
{
    class LegendSymbol
    // Класс реализует символ условного обозначения, отображаемый в консоли
    {
        public string Description { get; set; }
        public char Symbol { get; set; }
        public ConsoleColor Color { get; set; }
    }
    class Map
    // Класс реализует карту земельного участка, отображаемую в консоли
    {
        readonly int length, width; // длина и ширина карты

        const int kX = 2; // коэффициент отображения "клеток" карты по ширине консоли
        const int kY = 1; // коэффициент отображения "клеток" карты по высоте консоли

        // Словарь для хранения символов условных обозначений, ключами к которым будут значения матрицы земельного участка
        static Dictionary<int, LegendSymbol> Legend2 = new Dictionary<int, LegendSymbol>();

        public Map(int length, int width)
        // Конструктор создаёт карту с параметрами длина и ширина
        {
            this.length = length;
            this.width = width;
        }
        public void AddLegendSymbol(int code, LegendSymbol legendsymbol)
        // Метод добавляет символ условного обозначения в словарь
        {
            if (!Legend2.ContainsKey(code))
            {
                Legend2.Add(code, legendsymbol);
            }
        }
        public void DrawMap(int[,] matrix)
        // Метод отрисовывает в консоли карту двумерного массива
        {
            ConsoleColor color = Console.ForegroundColor; // запоминается текущее значение цвета текста для консоли
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < length; j++)
                {
                    // В метод DrawAt передаётся значение кода символа для словаря условных обозначений, а также
                    // координаты символа, которые переводятся в систему отсчёта от 1
                    DrawAt(matrix[i, j], j + 1, i + 1);
                 }
            }
            Console.ForegroundColor = color; // возвращается временно изменявшееся значение цвета текста для консоли
            SetCursorUnderMap(); // курсор устанавливается в позицию ниже карты
        }
        public void DrawAt(int code, int x, int y)
        // Метод отрисовывает в консоли символ, находя его параметры в словаре условных обозначений по коду
        // Символ отрисовывается в позиции с заданными координатами. Координаты в параметрах отсчитываются от 1
        {
            Console.ForegroundColor = Legend2[code].Color;
            Console.SetCursorPosition(kX * (x - 1), kY * (y - 1)); 
            Console.Write(Legend2[code].Symbol);
        }
        public void SetCursorUnderMap()
        // Метод устанавливает курсор в позицию ниже карты
        {
            Console.SetCursorPosition(0, kY * (width + 1));
        }
    }
}
