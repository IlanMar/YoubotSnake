using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace VRepClient
{
    public class KukaPotField
    {

        public float[] LaserDataKuka; //массив 683х2 х,у,z с ледара
        public float[] RobLocDataKuka;// массив 1х3 ч,у,z местоположение робота
        public float RV;
        public float FBV;
        public float Fx;
        public void LedDataKuka(string var)// заполняем массив LaserData, даыми с ледара робота// сюда отправить данные с ледара куки
        {
            string g = var;


            if (g != "")
            {
                string someString = var;
                string[] words = someString.Split(new char[] { ';' });// words
                int h = 0;//вспомогательная переменная для преобразоания str в массиив

                LaserDataKuka = new float[words.Length];
                for (int i = 0; i < words.Length; i++)//записываем данные с ледара в массив, в сроках x y z
                {
                    LaserDataKuka[i] = float.Parse(words[h], System.Globalization.CultureInfo.CreateSpecificCulture("en-US"));
                    h++;
                    /*
                   for (int j = 0; j < 3; j++) 
                    {
                        LaserData[i, j] = float.Parse(words[h], System.Globalization.CultureInfo.CreateSpecificCulture("en-US"));
                        if (LaserData[i, j] == 0) { LaserData[i, j] = 999; }
                        h++;
                    }*/
                }
            }

        }
        public void RodLocReceivingKuka(string RobPos) //заполняем массив RobLocData[3] с данными о местоположением робота
        {
            RobLocDataKuka = new float[3];
            if (RobPos != "")
            {
                // string someString = RobPos;
                string[] words = RobPos.Split(new char[] { ';' });//парсим строку в массив words
                for (int i = 0; i < 3; i++)
                {
                    RobLocDataKuka[i] = float.Parse(words[i], System.Globalization.CultureInfo.CreateSpecificCulture("en-US"));

                }
                RobLocDataKuka[2] = RobLocDataKuka[2] * -1;
            }
        }



        double[] DistData = new double[171];
        float left = 0;
        float right = 0;
        double Phi; //пеленг на цель
        double RobDirect;//направление робота
        double TargetDirection;
        double DistToTarget;//расстояние до цели
        //public tacticalLevel TactLevel = new tacticalLevel();
        float Xold = 0;
        float Yold = 0;
        
        public bool ObstDistKuka(float[] M, float[] RobLoc)
        {
            if (M == null || RobLoc == null) return false;
            //здесь мы определяем ближайшее препядствие к роботу
            //M это laser data
            int h = 0;
            int k = 0;
            double MinDist = 3;
            for (int i = 0; i < LaserDataKuka.Length-1; i++)
            {
                //float[,] m = TactLevel.GetOfLaserData();
                //DistData[i] = M[i];

                if (LaserDataKuka[i] < MinDist)
                {
                    MinDist = LaserDataKuka[i]; h = i;

                }
            }
            /*
            int h = 0;
            int k = 0;
            for (k = 0; k < M.Length; k++)
            {
                if (DistData[k] < MinDist)
                {
                    MinDist = DistData[k]; h = k;

                }

            }*/
            Fx = Math.Abs(1 / (1 + (float)MinDist * (float)MinDist));//формуна Fx=k/r^2
            if (h < 85) { Fx = Math.Abs(Fx); }//коэффициэнты для правого и левого колес
            if (h > 85) { Fx = -Fx; }
            
            float Xrob = RobLoc[0];
            float Yrob = RobLoc[1];
            float Yfin = 0;// координаты точки пеленга
            float Xfin = 3;
            float Xpel = Xfin - Xrob;
            float Ypel = Yfin - Yrob;
            TargetDirection = Math.Atan2(Ypel, Xpel);
            DistToTarget = Math.Sqrt(Xpel * Xpel + Ypel * Ypel);
            //вычисляем направление движения робота
            if (Xold != 0)
            {
                float RobDirectX = Xrob - Xold;
                float RobDirectY = Yrob - Yold;
                RobDirect = Math.Atan2(RobDirectY, RobDirectX);
                if (TargetDirection - RobDirect < Math.PI)
                {
                    Phi = TargetDirection - RobDirect;
                }
                else
                {
                    Phi = (-1) * (Math.PI * 2 - TargetDirection + RobDirect);
                }

            }
            Xold = Xrob;
            Yold = Yrob;


            /*
                 if (Phi > 0.3 || Phi < -0.3 && MinDist>1) //поворачивает в стороону цели если отклонение большое
                 {
                     if (Phi > 0) { right = 3; left = -3; }
                     //right = -2 * (-1) * (float)Phi; left = (-2 * (float)Phi);
                     if (Phi < 0) { right = -3; left = 3; }
                     
                 }*/

            if (DistToTarget > 0.4)//едет к цели при небольшом отклонении
            {//дифференциальный регулятор
                float y = 1f;//0.1f;//коэффициэнт для ухода от препядствий
                float Vdes = -1;
                float omdes = 2f * (float)Phi;
                float d = 0.4f;//расстояние между колесами
                float Vr = (Vdes + d * omdes );
                float Vl = (Vdes - d * omdes );
                float rw = 0.1f;//радиус колеса
                float R = Vr / rw;
                float L = Vl / rw;
                RV = (L - R) ; //RotateVelocity
                  FBV = (R + L) / 2; //FrontBackVelocity
              //  right = (float)Math.Round(Vr, 2);//умножить на 0.1 для настоящей куки
                //left = (float)Math.Round(Vl, 2);//умножить на 0.1 для настоящей куки
                 right = R;//умножить на 0.1 для настоящей куки
                left = L;//умножить на 0.1 для настоящей куки
            }

            if (DistToTarget < 0.4)
            {
                FBV = 0;
                RV = 0;
            }
            var speed = 0.1f;
            var k_slow = 0.1f;
            var arg1 = -1*FBV*0.1 * k_slow;
            arg1 = Math.Max(-speed, Math.Min(arg1, speed));//надо переделать эти выводы для адекватного вывода
            var arg2 = 0;
            var arg3 =-1*RV*0.2  * k_slow; 
            arg3 = Math.Max(-speed, Math.Min(arg3, speed));//возможно(left-right)
        //    control_str = string.Format(CultureInfo.InvariantCulture, "LUA_Base({0}, {1}, {2})", arg1, arg2, arg3);
            return true;
        }

        public string control_str;
    }
}

