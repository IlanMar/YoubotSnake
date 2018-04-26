using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VRepClient
{
    public class SearchPath
    {
        public float sX;
        public float sY;
        int key = 0;
        SearchInGraph SiG;
        
        public PointF ExpertSystem(out int flag, float[] RobOdomData, List<Point> ListPoints)//функция задает движение змейкой
        {
            int ra0 = (int)(RobOdomData[0]*100);
            int ra1=(int)(RobOdomData[1]*100);
            int ra2=(int)(RobOdomData[2]*100);
            
            SiG = new SearchInGraph();
            if (ListPoints == null) //чтобы избежать ошибки когда ListPoints равен нулю
            {
                if (key > 2) { key = 3; }
                if (key <3){key =1;}
            }
            // 1 - старт  и определение точки 1, 1 ------------------------
            if (key ==0)
            {
                sX = ra0+100;
                sY = ra1;
                key = 1;
            }
            //2 - движение прямо (на право)  -->  ---------------------------
            if (ListPoints!=null & key == 1)//(RobOdomData[2]>1.4 & RobOdomData[2]<1.7))
            {
                if (ListPoints.Count < 5)
                {
                    sY = ra1+5;
                    sX = ra0 + 150;
                }
            }
            //3 - если тупик или путь слишком длинный и робот едет вправо то начиаем разворот на лево --> ---------РАЗВОРОТ В ЛЕВО------------
            if ((ListPoints==null || ListPoints.Count >20) & RobOdomData[2]>0 & key ==1 )
            {
                sX = ra0;
                sY = ra1 + 100;
                key = 2;
            }
            //4 - завершение разворота. Если робот едет вверх то опять повернуть налево
            if (key == 2 & ListPoints != null) 
            {
                if (ListPoints.Count < 4)
                {
                    sY = ra1+20;
                    sX = ra0 - 100f;
                    key = 3;//чтобы робот ехал на лево
                }
            }
            //5 - движение прямо (на лево)  <--  ------------Движение в ЛЕВО
            if (key == 3 & ListPoints != null) 
            {
                if (ListPoints.Count < 4)
                {
                    sY = ra1+5;//прибавляем 5 чтобы он не падал вниз когда едет вбок
                    sX = ra0 - 150f;
                }
            }
            //6 - если тупик или путь слишком длинный и робот едет влево то разворот вправо  <-- ---------- РАЗВОРОТ В  ПРАВО ---------
            if ((ListPoints == null || ListPoints.Count > 20) & RobOdomData[2] < 0 & key==3)
            {
                sX = ra0;
                sY = ra1 + 100f;
                key = 4;// чтобы после поворота он ехал направо
            }
            //7 - завершение разворота направо.  Если робот едет вверх то опять повернуть направо. 
            if (key == 4 & ListPoints != null)
            {
                if (ListPoints.Count < 4)
                {
                    sY = ra1+20;//прибавляем 20 читобы он ехал ровнее
                    sX = ra0 + 100f;
                    key = 1;
                }
            }
            //else
            //{
            //    PointF goall = new PointF(0, 0);//если все варианты не сработали отправляет 0, 0 вместо флага
            //    return goall;
            //}
            flag = 2;
            if (ListPoints == null & (key == 2 || key == 4))
            {
               // flag = 1;
                //if ((ListPoints.Count > 50) & (key == 2 || key == 4))
                //{
                //    flag = 1;
                //}
            }

            PointF goal = new PointF(0, 0);
            goal.X = ra0;
            goal.Y = ra1;//заплатка чтобы goal не равняласть 0
            goal.X=sX/10;
            goal.Y=sY/10;
           
           
            return goal;

        }
    }
}
