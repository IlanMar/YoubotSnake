using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VRepClient
{
    public class RoomMap
    {
        public int[,] rmap = new int[8, 8];
       // int flag = 1;
        public void NextRoom(out PointF outData1, out int outData2, float[] RobOdomData, int flag)
        {
            if (0 < RobOdomData[0] & RobOdomData[0]<6 & 0 < RobOdomData[1] & RobOdomData[1]<6) //отмечать еденицой те квадраты где есть робот
            {
                rmap[(int)RobOdomData[0], (int)RobOdomData[1]] = 1;
                rmap[(int)RobOdomData[0]+1, (int)RobOdomData[1]] = 1;
                rmap[(int)RobOdomData[0], (int)RobOdomData[1]+1] = 1;
                rmap[(int)RobOdomData[0]+1, (int)RobOdomData[1]+1] = 1;

            }

        int y = 0;
        int x = 0;

        for ( y =0; y<8; y++)//выбераем непосещенный квадрат ближайший к началу координат
        {
            for (x =0; x<8; x++)
            {
                if (rmap[x, y] == 0)
                    break;
            }
            if (x < 8)
                break;
        }
           

            PointF H = new PointF(0, 0);
            H.X = x; H.Y = y;

            if (flag == 1)
            {
                //если робот доехал до координат квадрата то отправляем флаг =2
                if (H.X - RobOdomData[0] < 0.5 & H.X - RobOdomData[0] > -0.5 & H.Y - RobOdomData[1] < .5 & H.Y - RobOdomData[1] > -.5)
                {
                    flag = 2;
                }
                else
                    flag = 1;
            }
          outData1 =  H; // Необходимо инициализировать выходной параметр
          outData2 = flag; // Необходимо инициализировать выходной параметр
           //return result;
        }
   
    }
}
