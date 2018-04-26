using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
namespace VRepClient
{
   public class ObstaclesPoint
    {
        public float X;
        public float Y;
        public float weight=2f;
    }
   public class Map
    {
        
       public List<ObstaclesPoint> GlobalMapList = new List<ObstaclesPoint>();
       public float[] RobOdData; //только для вывода на форму картинкис роботом
       public bool invalidateform = false;
        public void LedDataToList(float[] LedData, float[] OdomData)
        {
            RobOdData = new float[OdomData.Length];
            RobOdData[0] = OdomData[0]; RobOdData[1] = OdomData[1]; RobOdData[2] = OdomData[2];//строчка только для вывода перемеще
            float[,] LedDataMass = new float[LedData.Length, 2];//массив с координатами точек относительно робота//убрать коменты
            float A = 180f/  LedData.Length;//угол между данными с ледара
            float x; // угол между катетом и гипотенузой
            float c; //длина гипотенузы
            float u=1;//для умножения X на -1 когда x становится больше 90 градусов
            for (int i=0; i < LedData.Length; i++) 
            {
                c = LedData[i];
                x = i * A;
                x=x*0.0174533f-OdomData[2];

                float LedRX = 0.344f * (float)Math.Cos(OdomData[2]);
               float LedRY = 0.344f * (float)Math.Sin(OdomData[2]);
                    LedDataMass[i, 0] = ((float)Math.Cos(x) * c)+OdomData[0]+LedRY;//значение по оси Х
                    LedDataMass[i, 1] = ((float)Math.Sin(x) * c)+OdomData[1]+LedRX;// значение по оси Y   
                    u = 1;
                    
            }
            GlobalMapList.Add(new ObstaclesPoint { X = 0f, Y = 0f, weight = 1 });//задаю две точки по умолчанию с обеих сторон от робота
           // GlobalMapList.Add(new ObstaclesPoint { X = -0.5f, Y = -0.5f, weight = 2});
            //для проверки зададим несколько препедствий спереди
          /*  GlobalMapList.Add(new ObstaclesPoint { X = 0f, Y = 1f, weight = 10 });
            GlobalMapList.Add(new ObstaclesPoint { X = 0f, Y = 1.1f, weight = 10 });
            GlobalMapList.Add(new ObstaclesPoint { X = 0f, Y = 0.9f, weight = 10 });
            GlobalMapList.Add(new ObstaclesPoint { X = 0.1f, Y = 1f, weight = 10 });
            GlobalMapList.Add(new ObstaclesPoint { X = -0.1f, Y = 1f, weight = 10 });
            GlobalMapList.Add(new ObstaclesPoint { X = 0.2f, Y = 1f, weight = 10 });
            GlobalMapList.Add(new ObstaclesPoint { X = 0.2f, Y = 1.1f, weight = 10 });
            GlobalMapList.Add(new ObstaclesPoint { X = 0.1f, Y = 1.1f, weight = 10 });
            GlobalMapList.Add(new ObstaclesPoint { X = -0.1f, Y = 1.1f, weight = 10 });*/
            float Xpel; 
            float Ypel; 
            float DistBetweenPoints=0;

            for (int g = 0; g < LedData.Length; g++)
            {
                float radius = 0;
                float h=4;//для запомнания кратчайшей точки в цикле
                for (int i = 0; i < GlobalMapList.Count; i++)
                {
                    Xpel = LedDataMass[g, 0] - GlobalMapList[i].X;
                    Ypel = LedDataMass[g, 1] - GlobalMapList[i].Y;
                    DistBetweenPoints = (float)Math.Abs(Math.Sqrt(Xpel * Xpel + Ypel * Ypel));
                   // radius = (float)(Math.Sqrt(LedDataMass[g, 0] * LedDataMass[g, 0] + LedDataMass[g, 1] * LedDataMass[g, 1]));
                    radius = LedData[g];// для отсеивания точек дальше 4-ех метров
                    if (DistBetweenPoints < h) 
                    {
                        h = DistBetweenPoints;
                    }
                    if (h > 0.01 && radius < 2 && i == GlobalMapList.Count-1&& radius>0.2 )//было 0.01
                    {
                       GlobalMapList.Add(new ObstaclesPoint { X = LedDataMass[g, 0], Y = LedDataMass[g, 1], weight =2 });
                      //ЗАКОМЕНЧЕНО ЧТОБЫ РОБОТ НЕ ЗАПОМИНАЛ ПРЕПЯТСТВИЯ 25.10.2016
                    }                 

                   
                    
                }
            }
            filterGlobalMapList(GlobalMapList, LedData, OdomData, LedDataMass);//отправка листа в функцию на отфильтровывание точек                  
          }

        void filterGlobalMapList(List<ObstaclesPoint> GlobalMapList, float[] LedData, float[] OdomData, float[,] LedDataMass) 
        {
           

            for (int i = 0; i < GlobalMapList.Count; i++)
            {
                for (int g = 0; g < LedData.Length; g++)
                {
                    float Tg = (float)Math.Atan(GlobalMapList[i].X / GlobalMapList[i].Y);
                    float Tg2 = (float)Math.Atan(LedDataMass[g, 0] / LedDataMass[g, 1]);
                    float dx = Math.Abs(GlobalMapList[i].X) - Math.Abs(LedDataMass[g, 0]);
                    float dy = Math.Abs(GlobalMapList[i].Y) - Math.Abs(LedDataMass[g, 1]);
                    float da = (float)Math.Abs(Tg - Tg2);

                    float Xpel = GlobalMapList[i].X - OdomData[0];
                    float Ypel = GlobalMapList[i].Y - OdomData[1];
                    float TargetonPoint = (float)Math.Atan2(Xpel, Ypel);
                    float TT = (float)Math.Abs(TargetonPoint - OdomData[2]);

                    float XpelP = LedDataMass[g, 0] - GlobalMapList[i].X;//для рсчета расстояния между точками
                    float YpelP = LedDataMass[g, 1] - GlobalMapList[i].Y;
                    float DistBetweenPoints = (float)Math.Abs(Math.Sqrt(XpelP * XpelP + YpelP * YpelP));

                   if ( LedData[g] < 2 && DistBetweenPoints>0.1 && da < 0.005 && TT<1.5  ) //проверка если старая точка заслоняется новой то ее удалить
                    {
                          // GlobalMapList.RemoveAt(i);//закоментированно удаление несуществующих точек, временно
                         //   break;
                    }
                    
                    //вся эта функция неправльная, она должна быть привязана к точке листа i а не к g
                    if ( LedData[g] > 2.1 && da < 0.002 && TT < 1.2) //если в данном направвлении точек нет то существующие удалить
                    {
                       //  GlobalMapList.RemoveAt(i);//эта функция почемуто удаляет точки при вращении
                       //  break;
                    }
                }
            }

        }

       // public float[,] graph;
        public float[,] graph;
        public int Ymax = 180;
        public int Xmax = 180;
        int oldXmax=0;
        int oldX = 0; int oldY = 0;
        
       public void GlobListToGraph(List<ObstaclesPoint> GlobalMapList,float[] OdomData )//метод для перевода листа в матрицу
        {
          
            graph = new float[Xmax, Ymax];//матрица которая является взвешенным графом
           int ymatrix = 0;
           int xmatrix = 0;
           bool key1 = false;
           bool key2 = false;
           for (int k=0; k < Xmax; k++) 
           {
               for (int k2 = 0; k2 < Ymax; k2++) 
               {
                   graph[k, k2] = 1;
               }
           }

               for (int i = 0; i < GlobalMapList.Count; i++)
               {
                   float Tx = GlobalMapList[i].X * 10;
                   float Ty = GlobalMapList[i].Y * 10;
                   xmatrix = (int)Math.Floor(Tx);
                   ymatrix = (int)Math.Floor(Ty);

                   graph[xmatrix + Xmax/2, ymatrix + Ymax/2] = GlobalMapList[i].weight;

               }           

        }
       
    }
}
