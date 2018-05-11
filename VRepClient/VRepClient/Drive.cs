using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VRepClient
{
    public class Drive
    {
        public float right, left, Phi;
        public float TargetDirection;
        public float RobotDirection;//переменная для вывода на форму чрез форму
        public float DistToTarget; 
        public void GetDrive(float RobX, float RobY, float RobA, float GoalPointX, float GoalPointY, float Xmax, float Ymax) 
        {
            GoalPointX = GoalPointX * 0.1f;
            GoalPointY = GoalPointY * 0.1f;
            Xmax = Xmax * 0.1f;
            Ymax = Ymax * 0.1f;
            RobX = RobX + Xmax/2;
            RobY = RobY + Ymax/2;
            RobotDirection = RobA;
            //float GoalPointX = -1;
           // float GoalPointY = 1;
            //определяем относительное направление цели цели
            float Xpel = GoalPointX - RobX;
            float Ypel = GoalPointY - RobY;
             TargetDirection = (float)Math.Atan2(Xpel, Ypel);//надо просто RobA-
            DistToTarget = (float)Math.Sqrt(Xpel * Xpel + Ypel * Ypel);
                        
               // Phi = TargetDirection - RobA;

            if (TargetDirection - RobA < Math.PI && TargetDirection - RobA > -Math.PI)
            {
                Phi = TargetDirection - RobA;
            }
            else
            {
                if ((Math.PI * 2) > Math.Abs((float)(Math.PI * 2 + TargetDirection - RobA)))//если угол между точками больше двух ПИ
                {
                    // Phi = (-1) * (float)(Math.PI * 2 + TargetDirection - RobA);
                    Phi = (float)(Math.PI * 2 + TargetDirection - RobA);
                }
                else
                {
                    Phi = (TargetDirection - RobA - (float)(Math.PI * 2));
                }
            }
                //определям сильно ли отклонен робот от цели направляем ее к ней
                if (Phi > 0.4 || Phi < -0.4)
                {
                    if (Phi > 0.4) { right = -1; left = 1; }
                    //right = -2 * (-1) * (float)Phi; left = (-2 * (float)Phi);
                    if (Phi < -0.4) { right = 1; left = -1; }
                }
                if ( Phi < 0.4 || Phi > -0.4)
                {

                    if (Phi < 0.4 && Phi>0) { right = 1 - Phi*1.4f; left = 1; }
                    if (Phi > -0.4&& Phi<0) { right = 1; left = 1- Phi *1.4f* -1; }
                    // if (Phi > 0.02 && Phi<0.1) { right = 0.5f; left = 1; }
                    //right = -2 * (-1) * (float)Phi; left = (-2 * (float)Phi);
                  //  if (Phi < -0.02&& Phi>-0.1) { right = 1; left = 0.5f; }
                }
                if (Phi == 0) 
                {
                    right = 1; left = 1;
                }
                
            
            
            //if(Phi<0.02 && Phi >-0.02)
            //    { right = 1; left = 1; }

            if (DistToTarget < 0.03)
            {
                right = 0;
                left = 0;
            }
           // right = 0;//10; left = 10;///
           // right = 0; left = 0;///
      //  right = 0;// left = 3;///
        }


    }
}
