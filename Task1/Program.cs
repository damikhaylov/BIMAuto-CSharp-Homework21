using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Task1
{
    class Program
    {
        static object locker = new object();
        static int pause = 25; // Величина задержки в мс для передвижения садовников в цикле
        static bool isOneOfGardenerFinished = false; // Признак того, что хотя бы один из садовников закончил работу
        static void Main(string[] args)
        {
            /*
            ***** Условие задачи *****
            
            Имеется пустой участок земли (двумерный массив) и план сада, который необходимо реализовать. Эту задачу 
            выполняют два садовника, которые не хотят встречаться друг с другом. Первый садовник начинает работу
            с верхнего левого угла сада и перемещается слева направо, сделав ряд, он спускается вниз. Второй садовник
            начинает работу с нижнего правого угла сада и перемещается снизу вверх, сделав ряд, он перемещается влево.
            Если садовник видит, что участок сада уже выполнен другим садовником, он идет дальше. Садовники должны 
            работать параллельно. Создать многопоточное приложение, моделирующее работу садовников.
            */
            /*
            ***** Реализация задачи *****

            Задача реализована с отображением движения садовников и обработки сада в консоли. Предусмотрена возможность
            задать сложный контур плана сада, неограниченное количество раз задавая границы прямоугольных участков,
            занятых садом (участки могут пересекаться или быть изолированными). Садовники никогда не встречаются 
            в одной ячейке.
            */

            // Запрашиваем у пользователя размеры земельного участка, ограничивая их размерами консоли
            Console.WriteLine("задайте параметры земельного участка:\n");
            int length = InputInt("Ведите длину земельного участка", 1, Console.LargestWindowWidth / 2);
            int width = InputInt("Ведите длину земельного участка", 1, Console.LargestWindowHeight - 10);

            // Если текущие размеры консоли не вместят карту земельного участка, увеличиваем их
            if (Console.WindowWidth < length * 2)
            {
                Console.WindowWidth = length * 2;
            }
            if (Console.WindowHeight < width + 10)
            {
                Console.WindowHeight = width + 10;
            }

            // Создаём новый объект, моделирующий земельный участок в двумерном массиве
            Farmland farmland = new Farmland(length, width);
            // Создаём новый объект - карту земельного участка для отображения в консоли
            Map map = new Map(length, width);
            // Добавляем в объект карты условные обозначения разных типов земли на земельном участке
            map.AddLegendSymbol(farmland.GetLandCodeByName("Wasteland"),
                new LegendSymbol() { Description = "Пустырь", Symbol = '~', Color = ConsoleColor.DarkGray });
            map.AddLegendSymbol(farmland.GetLandCodeByName("Garden"),
                new LegendSymbol() { Description = "Сад", Symbol = 'ш', Color = ConsoleColor.Green });
            map.AddLegendSymbol(farmland.GetLandCodeByName("Farmed"),
                new LegendSymbol() { Description = "Посадки", Symbol = '*', Color = ConsoleColor.Red });

            // Очищаем консоль и отрисовываем в ней карту пустого земельного участка
            Console.Clear();
            map.DrawMap(farmland.Matrix);

            // В цикле запрашиваем у пользователя границы прямоугольных участков сада.
            int addGarden = 0;
            Console.WriteLine("Задайте расположение сада в пределах земельного участка. " +
                "Сад может состоять из нескольких пересекающихся или изолированных прямоугольников. " +
                "Начало координат земельного участка — в верхнем левом углу.\n");
            do
            {
                int gardenLeft = InputInt("Ведите координату x левого верхнего угла сада", 1, length);
                int gardenTop = InputInt("Ведите координату y левого верхнего угла сада", 1, width);
                int gardenRight = InputInt("Ведите координату x правого нижнего угла сада", gardenLeft, length);
                int gardenBottom = InputInt("Ведите координату y правого нижнего угла сада", gardenTop, width);
                farmland.SetGardenByCorners(gardenLeft, gardenTop, gardenRight, gardenBottom);
                Console.Clear();
                map.DrawMap(farmland.Matrix);
                addGarden = InputInt("Чтобы добавить ещё один участок с садом, введите 1, чтобы начать обработку - 0", 0, 1);
            } while (addGarden == 1);

            // Создаём объекты - первого  и второго садовника
            Gardener firstGardener =
                new Gardener(farmland, "Gardener1", farmland.GetGardenLeftInRow(farmland.GardenTop), farmland.GardenTop, 1, 0, 1);
            Gardener secondGardener =
                new Gardener(farmland, "Gardener2", farmland.GardenRight, farmland.GetGardenBottomInColumn(farmland.GardenRight), 0, -1, -1);

            // Добавляем в условные обозначения карты обозначения самих садовников и их посадок на территории сада
            map.AddLegendSymbol(farmland.GetLandCodeByName("Gardener1"),
                new LegendSymbol() { Description = "Садовник 1", Symbol = 'Y', Color = ConsoleColor.Red });
            map.AddLegendSymbol((farmland.GetLandCodeByName("Farmed") | farmland.GetLandCodeByName("Gardener1")),
                new LegendSymbol() { Description = "Посадки садовника 1", Symbol = '*', Color = ConsoleColor.Red });

            map.AddLegendSymbol(farmland.GetLandCodeByName("Gardener2"),
                new LegendSymbol() { Description = "Садовник 2", Symbol = 'T', Color = ConsoleColor.Yellow });
            map.AddLegendSymbol((farmland.GetLandCodeByName("Farmed") | farmland.GetLandCodeByName("Gardener2")),
                new LegendSymbol() { Description = "Посадки садовника 1", Symbol = '*', Color = ConsoleColor.Yellow });

            // Перерисовываем карту, уже с отображением садовников на стартовых позициях
            Console.Clear();
            map.DrawMap(farmland.Matrix);

            // Добавляем в события садовников, которые будут инициироваться при их передвижении с места на место,
            // методы, которые будут синхронно вносить изменения в отображаемую карту
            firstGardener.MoveBefore += ClearGardener;
            firstGardener.MoveAfter += DrawGardener;

            secondGardener.MoveBefore += ClearGardener;
            secondGardener.MoveAfter += DrawGardener;

            // Вручную вызываем для каждого из садовников метод, выполняющий действия по прибытию на новую позицию,
            // чтобы этот метод отработал появление садовников на стартовой позиции
            firstGardener.DoActionsArrivingAtPosition();
            secondGardener.DoActionsArrivingAtPosition();

            // Создаём и запускаем поток для второго садовника (используем анонимный метод, чтобы передать параметр)
            ThreadStart threadStart = new ThreadStart(() => DoJobByGardener(secondGardener));
            Thread thread = new Thread(threadStart);
            thread.Start();

            // Запускаем работу первого садовника
            DoJobByGardener(firstGardener);

            // В конце работы программы ожидаем от пользователя нажатия клавишы для завершения работы
            map.SetCursorUnderMap();
            Console.WriteLine("Конец. Для завершения работы нажмите любую клавишу.");
            Console.ReadKey();


            void DoJobByGardener(Gardener gardener)
            // Метод осуществляет работу садовником до тех пор, пока его перемещение в пределах сада возможно
            {
                int GardenerMovingPossible;
                {
                    while (true)
                    {
                        Thread.Sleep(pause); // Используется задержка, чтобы перемещение было наглядным
                                             // Блокируется от одновременного выполнения фрагмент кода, осуществляющий работу с общими
                                             // ресурсами — массивом ячеек земельного учатска, в который вносятся измененния при каждом
                                             // ходе садовника, и с картой земельного участка в консоли, на которой синхронно отражаются
                                             // эти изменения
                        lock (locker)
                        {
                            // Вызывается метод, осуществляющий движение садовника, и возвращающий код, который
                            // показывает, осуществлено ли движение, сделана ли пауза для пропуска другого садовника
                            // через целевую ячейку, или движение уже в принципе невозможно
                            GardenerMovingPossible = gardener.Move();
                            if (GardenerMovingPossible == -1) // Если возвращён признак невозможности движения
                            {
                                isOneOfGardenerFinished = true; // Указываем программе, что один из садовников
                                                                // закончил работу
                                break;
                            } else if (GardenerMovingPossible == 0 && isOneOfGardenerFinished)
                            // Также завершаем цикл, если садовник "упёрся" в другого садовника, который уже
                            // завершил работу
                            {
                                break;
                            }
                        }
                    } 
                }
            }

            void DrawGardener(string gardenerName, int x, int y)
            // Метод отрисовывает садовника в заданной позиции
            {
                map.DrawAt(farmland.GetLandCodeByName(gardenerName), x, y);
            }
            void ClearGardener(int x, int y)
            // Метод удаляет символ садовника из заданной позиции, заменяя его на символ того типа земли,
            // которая там находится
            {
                map.DrawAt(farmland.Matrix[y - 1, x - 1], x, y);
            }
        }
        static int InputInt(string que, int min, int max)
        // Метод запрашивает у пользователя целое число с заданным в параметре вопросом и проверяет его
        // на ошибки ввода и соответствие заданному диапазону
        {
            int input;
            while (true)
            {
                Console.Write($"{que} <min:{min} max:{max}>: ");
                try
                {
                    input = Convert.ToInt32(Console.ReadLine());
                    if (input <= max)
                    {
                        if (input >= min)
                        {
                            return input;
                        }
                        else
                        {
                            Console.WriteLine($"Значение должно быть больше либо равно {min}.");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Значение не должно превышать {max}.");
                    }
                }
                catch (Exception Ex)
                {
                    Console.WriteLine($"\nОшибка! {Ex.Message}");
                }
            }
        }
    }
}