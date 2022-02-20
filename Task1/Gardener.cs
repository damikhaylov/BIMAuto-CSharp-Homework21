using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Task1
{
    class Gardener
    // Класс реализует модель садовника
    {
        readonly Farmland farmLand;     // Внутреннее свойство класса содержит объект - участок земли, на котором
                                        // работает садовник
        readonly string name;           // Имя садовника
        int posX;                       // Текущая позиция садовника
        int posY;
        readonly int nextStepVectorX;   // Две переменных определяют вектор основного направления перемещения садовника
        readonly int nextStepVectorY;   // +1 по x - вправо, -1 - влево, +1 по y  - вверх, -1 - вниз.
        readonly int nextRowVector;     // Вектор при переходе на следующий ряд: +1 - с увеличением номера ряда,
                                        // -1 — с уменьшением

        public Gardener(Farmland farmland, string gardenerName, int startposx, int startposy,
                        int nextstepvectorx, int nextstepvectory, int nextrowvector)
        // Конструктор создаёт садовника, присваивая начальные значения внутренним свойствам
        {
            this.farmLand = farmland;
            this.name = gardenerName;
            this.posX = startposx;
            this.posY = startposy;
            this.nextStepVectorX = nextstepvectorx;
            this.nextStepVectorY = nextstepvectory;
            this.nextRowVector = nextrowvector;
            this.farmLand.AddGardenerToCodes(gardenerName);
        }
        public delegate void MoveBeforeDelegate(int x, int y);
        public event MoveBeforeDelegate MoveBefore; // Событие, которое должно инициироваться
                                                    // перед сменой позиции садовника
        public delegate void MoveAfterDelegate(string gardenerName, int x, int y);
        public event MoveAfterDelegate MoveAfter;   // Событие, которое должно инициироваться
                                                    // после смены позиции садовника
        public string Name { get => name; } // автосвойство возвращает имя садовника
        public int Move()
        // Метод выполняет перемещение садовника на новую позицию, если это возможно.
        // Метод возвращает 1 (успешная смена позиции), 0 (ожидание освобождения пути другим садовником),
        // -1 (невозможно двигаться дальше).
        {
            int newX, newY;
            GetNextPosCoords(out newX, out newY); // Определяются координаты следующего положения садовника
            if (newX != 0 && newY != 0) // Если они не нулевые (нулевые координаты - признак невозможности движения)
            {
                // Проверяется, не находится ли по этим координатам другой садовник
                // (с которым нежелательно встречаться в одной ячейке)
                if (!farmLand.CheckGardenerPresenceAt(newX, newY)) // Если не находится
                {// Выполняется смена позиции и ряд операций, сопутствующих смене позиции
                    DoActionsLeavingPosition(); // выполняется набор операций до смены позиции
                    posX = newX;
                    posY = newY;
                    DoActionsArrivingAtPosition(); // выполняется набор операций после смены позиции
                    return 1; // Возвращается признак успешной смены позиции (1)
                }
                else
                {
                    return 0; // Возвращается признак временной приостановки движения (0)
                              // (Ожидаем, может быть другой садовник освободит координаты для движения)
                }
            }
            else
            {
                return -1; // Если новые координаты недопустимы, возвращается признак невозможности дальнейшего движения
            }
        }
        void GetNextPosCoords(out int x, out int y)
        // Метод записывает координаты следующей позиции садовника в параметры x и y
        // Если дальнейшее движение невозможно в существующих границах сада, координаты на выходе будут равны 0
        {
            x = posX; // начальное значение координат соответствует текущей позиции
            y = posY;

            if (nextStepVectorY == 0) // Если основное движение садовника — по горизонтали
            {
                int gardenLeftInRow = farmLand.GetGardenLeftInRow(y);  // Левая граница небработанного сада в ряду
                int gardenRightInRow = farmLand.GetGardenRightInRow(y); // Правая граница небработанного сада в ряду
                // В ряду должны быть необработанные ячейки под сад, и координата х не должна достигать крайних
                // необработанных ячеек по направлению движения
                if (farmLand.GetGardenLeftInRow(y) != 0 && ((nextStepVectorX > 0 && x < gardenRightInRow) || (nextStepVectorX < 0 && x > gardenLeftInRow)))
                {
                    x = posX + nextStepVectorX; // В этом случае берётся очередная координата по основному направлению
                    y = posY;
                    return;
                }
                // В противном случае осуществляется смена ряда с проверкой, находится ли очередной ряд
                // всё еще в границах сада
                else if ((nextRowVector > 0 && y < farmLand.GardenBottom) || (nextRowVector < 0 && y > farmLand.GardenTop))
                {
                    // Координата соответствует новому ряду
                    y = posY + nextRowVector;
                    // Координата принимается соответствующей первой необработанной ячейке с садом в соответствующем
                    // направлении
                    x = (nextStepVectorX > 0) ? farmLand.GetGardenLeftInRow(y) : farmLand.GetGardenRightInRow(y);
                    if (x == 0) // Но если эта координата 0, значит в новом ряду нет необработанных ячеек
                    {
                        // и координата назначается просто по крайней границе сада в соответствующем направлении
                        // (предполагается, что следующим ходом опять произойдёт смена ряда)
                        x = (nextStepVectorX > 0) ? farmLand.GardenLeft : farmLand.GardenRight;
                    }
                    return;
                }
                // Если и смена ряда невозможна, возвращаются нулевые координаты (дальнейшее движение невозможно)
                else
                {
                    x = 0;
                    y = 0;
                    return;
                }
            }
            else if (nextStepVectorX == 0) // Если основное движение садовника — по вертикали
            {
                int gardenTopInColumn = farmLand.GetGardenTopInColumn(x); // Верхняя граница небработанного сада в ряду
                int gardenBottomInColumn = farmLand.GetGardenBottomInColumn(x); // Нижняя граница небработанного сада в ряду
                // В ряду должны быть необработанные ячейки под сад, и координата y не должна достигать крайних
                // необработанных ячеек по направлению движения
                if (gardenTopInColumn !=0 && ((nextStepVectorY > 0 && y < gardenBottomInColumn) || (nextStepVectorY < 0 && y > gardenTopInColumn)))
                {
                    x = posX;
                    y = posY + nextStepVectorY;  // В этом случае берётся очередная координата по основному направлению
                    return;
                }
                // В противном случае осуществляется смена ряда с проверкой, находится ли очередной ряд
                // всё еще в границах сада
                else if ((nextRowVector > 0 && x < farmLand.GardenRight) || (nextRowVector < 0 && x > farmLand.GardenLeft))
                {
                    // Координата соответствует новому ряду
                    x = posX + nextRowVector;
                    // Координата принимается соответствующей первой необработанной ячейке с садом в соответствующем
                    // направлении
                    y = (nextStepVectorY > 0) ? farmLand.GetGardenTopInColumn(x) : farmLand.GetGardenBottomInColumn(x);
                    if (y == 0) // Но если эта координата 0, значит в новом ряду нет необработанных ячеек
                    {
                        // и координата назначается просто по крайней границе сада в соответствующем направлении
                        // (предполагается, что следующим ходом опять произойдёт смена ряда)
                        y = (nextStepVectorY > 0) ? farmLand.GardenTop : farmLand.GardenBottom;
                    }
                    return;
                }
                // Если и смена ряда невозможна, возвращаются нулевые координаты (дальнейшее движение невозможно)
                else
                {
                    x = 0;
                    y = 0;
                    return;
                }
            }
        }
        void DoActionsLeavingPosition()
        // Метод выполняет набор операций, необходимых при покидании садовником текущей позиции
        {
            farmLand.ClearGardenerPresenceAt(posX, posY); // Удаляется признак присутствия садовника
                                                          // на покидаемой позиции на участке земли
            MoveBefore?.Invoke(posX, posY);               // Инициируется событие MoveBefore
        }
        public void DoActionsArrivingAtPosition()
        // Метод выполняет набор операций, необходимых при прибытии садовника на новую позицию
        {
            farmLand.SetGardenerPresenceAt(posX, posY); // Устанавливается признак присутствия садовника
                                                        // на новой позиции на участке земли
            MoveAfter?.Invoke(name, posX, posY);    // Инициируется событие MoveAfter
            farmLand.FarmedAt(name, posX, posY);    // Выполняется метод обработки участка земли
        }
    }
}
