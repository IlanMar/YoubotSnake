using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRepAdapter;

namespace VRepClient
{
    public class tacticalLevel
    {
        public float[,] LaserData; //массив 683х2 х,у,z с ледара
        public float[,] RobLocData;// массив 1х3 ч,у,z местоположение робота
        public float[,] GetOfLaserData()
        {
            return LaserData;
        }//возвращает значения
        public float[,] GetOfRobLocData()
        {
            return RobLocData;
        }

        public void LedData(string var)// заполняем массив LaserData, даыми с ледара робота
        {
            string g = var;
            LaserData = new float[684, 3];

            if (g != "")
            {
                string someString = var;
                string[] words = someString.Split(new char[] { ';' });// words
                int h = 0;//вспомогательная переменная для преодразоания str в массиив


                for (int i = 0; i < 684; i++)//записываем данные с ледара в двухмерный массив 683 на 3, в сроках x y z
                {
                    for (int j = 0; j < 3; j++)
                    {
                        LaserData[i, j] = float.Parse(words[h], System.Globalization.CultureInfo.CreateSpecificCulture("en-US"));
                        if (LaserData[i, j] == 0) { LaserData[i, j] = 999; }
                        h++;
                    }
                }
            }

        }
        public void RodLocReceiving(string RobPos) //заполняем массив RobLocData[1,3] с данными о местоположением робота
        {
            RobLocData = new float[1, 3];
            if (RobPos != "")
            {
                // string someString = RobPos;
                string[] words = RobPos.Split(new char[] { ';' });//парсим строку в массив words
                for (int i = 0; i < 3; i++)
                {
                    RobLocData[0, i] = float.Parse(words[i], System.Globalization.CultureInfo.CreateSpecificCulture("en-US"));
                }
            }
        }

    }
}
