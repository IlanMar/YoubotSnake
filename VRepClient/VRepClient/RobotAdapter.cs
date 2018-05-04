// VRep-YouBot Adapter, Margolin Ilan (MIREA 2016)
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VRepAdapter;

namespace VRepClient
{
    public class RobotAdapter
    {
        public virtual void Init(){}
        public virtual void Deactivate(){}
        public virtual void Send(Drive RobDrive) { }//отправка управляющих команд
    //    public virtual void SendOdom(float[] newOdom) { }//отправка управляющих команд
        public virtual void ReceiveLedData(string LedarData) { } /*получение данных ледара и закидывание их в массив*/
        public virtual void ReceiveOdomData(string OdometryData) { }//получение данных одометрии и закидывание их в массив

        public float[] RobotLedData;//сюда заносятся данные с ледара Врепа
        public float[] RobotOdomData;//сюда заносятся данные одометри Врепа

        public float right;
        public float left;
               
    }

    public class VrepAdapter : RobotAdapter //наследный класс для работы с Vrep
    {
        public int clientID = -1;
        int leftMotorHandle, rightMotorHandle, sensorHandle, leftMotorHandleA, rightMotorHandleA;
        float driveBackStartTime = -99000;
        float[] motorSpeeds = new float[4];

      
        public override void Init() 
        {
            clientID = VRepFunctions.Start("127.0.0.1", 7777);
            if (clientID != -1)
            {

                VRepFunctions.GetObjectHandle(clientID, "rollingJoint_fl", out leftMotorHandle);
                VRepFunctions.GetObjectHandle(clientID, "rollingJoint_rl", out leftMotorHandleA);
                VRepFunctions.GetObjectHandle(clientID, "rollingJoint_rr", out rightMotorHandle);
                VRepFunctions.GetObjectHandle(clientID, "rollingJoint_fr", out rightMotorHandleA);
                VRepFunctions.GetObjectHandle(clientID, "Proximity_sensor", out sensorHandle);

            }
        }
        public override void Send(Drive RobDrive)
        { /*youbot_connection.send(ToString(data));*/
            if (RobDrive != null)
            {
                right = RobDrive.right * (-5f);//20.02 удалет тип var//*(-2.5f);
                 left = RobDrive.left * (-5f);
                 //right = 0;//для проверки
              //  left = 0;
            }
            if (VRepFunctions.GetConnectionId(clientID) == -1) return;

            Byte sensorTrigger = (Byte)0;
            
                   
            int simulationTime = VRepFunctions.GetLastCmdTime(clientID);//??????
            if (simulationTime - driveBackStartTime < 3000)
                driveBackStartTime = simulationTime;
            {
                VRepFunctions.SetJointTargetVelocity(clientID, leftMotorHandle, left);
                VRepFunctions.SetJointTargetVelocity(clientID, leftMotorHandleA, left);
                VRepFunctions.SetJointTargetVelocity(clientID, rightMotorHandle, right);
                VRepFunctions.SetJointTargetVelocity(clientID, rightMotorHandleA, right);//right);
            }
        }
        public override void ReceiveLedData(string LedarData)
        { /* парсинг строки из Vrep "0.3, 0.1, -0.7" *//* base.Recieve("0.3, 0.1, +0.7");*/
         RobotLedData = new float[518];//более 1412 это бесконечность
         float[,] LaserDatatemporaryVrep;//временный массив с координатами видимых препядствий
         string g = LedarData;
         LaserDatatemporaryVrep = new float[684, 3];
         if (g != "")
         {
             string someString = LedarData;
             string[] words = someString.Split(new char[] { ';' });// words
             int h = 0;//вспомогательная переменная для преодразоания str в массиив
             
             for (int i = 0; i < 684; i++)//записываем данные с ледара в двухмерный массив 683 на 3, в сроках x y z
             {
                 for (int j = 0; j < 3; j++)
                 {
                     LaserDatatemporaryVrep[i, j] = float.Parse(words[h], System.Globalization.CultureInfo.CreateSpecificCulture("en-US"));
                     if (LaserDatatemporaryVrep[i, j] == 0) { LaserDatatemporaryVrep[i, j] = 500; }
                     h++;
                 }
             }

             int d=0;
             for (int i = 83; i < 601; i++) //в массив LaserDataVrep, длиной 516 высчитываем и добавляем расстояния до объектов
             {
                 RobotLedData[d] = (float)(Math.Sqrt(LaserDatatemporaryVrep[i, 0] * LaserDatatemporaryVrep[i, 0] + LaserDatatemporaryVrep[i, 1] * LaserDatatemporaryVrep[i, 1]));
                 d++;
             }


           

         }
            
        }
        public override void ReceiveOdomData(string OdometryData) 
        {
            RobotOdomData = new float[3];
            if (OdometryData != "")
            {
                // string someString = RobPos;
                string[] words = OdometryData.Split(new char[] { ';' });//парсим строку в массив words
                for (int i = 0; i < 3; i++)
                {
                    RobotOdomData[i] = float.Parse(words[i], System.Globalization.CultureInfo.CreateSpecificCulture("en-US"));

                }
              
                //textBox2.Text = h;
                //    RobLocDataKuka[2] = RobLocDataKuka[2] * -1;//
            }
        
        }

        public override void Deactivate()
        {
            VRepFunctions.Finish(clientID);
        }
    }

    public class YoubotAdapter : RobotAdapter //наследный класс для работы с Kuka Youbot
    {
        public TcpConnection tc;
        public void TCPconnect(string ip) 
        {
            tc = new TcpConnection(0, Form1.f1,
                   str => MessageBox.Show("Connected!"),
                   str =>
                   {
                   try
                   {
                       ProcessTCP(str);
                   }
                   catch { } // (Exception ex) { MessageBox.Show(ex.ToString()); }
                   },
                   str => MessageBox.Show("Disconnected!"));

            tc.Connect(ip, "7777");
        }
        private void ProcessTCP(string str)
        {
            if (str.StartsWith(".laser#"))
            {
                int ind = str.IndexOf("#");
                var LedData = str.Substring(ind + 1);

                Form1.f1.ShowLedData(LedData);
                //KukaPotField.LedDataKuka(s);//отправляем данные с лудара куки
                ReceiveLedData(LedData); //отправляем данные с ледара
            }
            if (str.Contains(".odom#"))
            {
                int ind = str.IndexOf("#");
                var OdometryData = str.Substring(ind + 1);
                //  KukaPotField.RodLocReceivingKuka(s);//отпровляем "s" данные одометрии
                Form1.f1.ShowOdomData(OdometryData);
                ReceiveOdomData(OdometryData); //отправляем данные одометрии
            }
            //ra.Receive(LedData, OdometryData); 
            if (tc == null)
            {

                return;
            }

        }
        void SendOdom(float[] newOdom)//отправляем обновленные координаты одометрии на куку
        {
            if (newOdom != null)
            {
                float x = newOdom[0];
                float y = newOdom[1];
                float a = newOdom[2];
                string p = "0.01";//чтобы текущие координаты обновлялить только на 10% относительно новых
                string control_str;
              // control_str = string.Format("LUA_UpdateScreenPose(", x, y, a, p, ")");
               control_str = string.Format(CultureInfo.InvariantCulture, "LUA_UpdateScreenPose({0}, {1}, {2}, {3})", y, x, a, p);
                if (control_str != null) tc.Send(control_str);//отправка новох значений одометрии на куку
            }
        }
        int key = 0;//ключ для уточнения одометрии
        public override void Send(Drive RobDrive) 
        {
            if (RobDrive != null)
            {
                right = RobDrive.right;
                left = RobDrive.left;
            }
            if (tc != null)// здесь происходит отправка задающих команд на куку
            {
                string control_str;
                 control_str = string.Format( "LUA_Base({0}, {1}, {2})", 0, 0, 0);
                 float Vrob = (right + left) / 2;
                 float Wrob = (right - left) / 1;
                 var speed = 0.1f;
                 //var k_slow = 0.1f;
                 var arg1 = Vrob; arg1 = (float)Math.Max(-speed, Math.Min(arg1, speed));//надо переделать эти выводы для адекватного вывода
                 var arg2 = 0;
                 var arg3 = Wrob; arg3 = Math.Max(-speed, Math.Min(arg3, speed));//возможно(left-right)
                 control_str = string.Format(CultureInfo.InvariantCulture, "LUA_Base({0}, {1}, {2})", arg1, arg2, arg3);

                // var b = KukaPotField.ObstDistKuka(KukaPotField.LaserDataKuka, KukaPotField.RobLocDataKuka);
                //if (b)
                //{
                
                    if (control_str != null) tc.Send(control_str);//отправляем команду на сервер куки
                //    }
            }
            /*youbot_connection.send(ToString(data));*/
        }
        public override void ReceiveLedData(string LedarData) 
        { /* парсинг строки из Vrep "0.3, 0.1, -0.7" *//*base.Recieve("0.3, 0.1, +0.7"); */
                        
            string g = LedarData;
            
           // float[] LaserDataKuka;
            if (g != "")
            {
                string someString = LedarData;
                string[] words = someString.Split(new char[] { ';' });// words
                int h = 0;//вспомогательная переменная для преобразоания str в массиив

               // LaserDataKuka = new float[words.Length];
                RobotLedData = new float[words.Length];
                for (int i = 0; i < words.Length; i++)//записываем данные с ледара в массив, в сроках x y z
                {
                    RobotLedData[i] = float.Parse(words[h], System.Globalization.CultureInfo.CreateSpecificCulture("en-US"));
                    h++;
                }
             }

        }
        public OdomFromCam OFC = new OdomFromCam();//класс в котором получаем координаты робота с камер
        int k = 0;// ключ точбы взять первою одометрию только один раз
        float[] ustavki = new float[3];//изначальные рассогласования фактического местоположения робота и камер

        public override void ReceiveOdomData(string OdometryData) 
        {
            RobotOdomData = new float[3];
            if (OdometryData != "")
            {
                // string someString = RobPos;
                string[] words = OdometryData.Split(new char[] { ';' });//парсим строку в массив words
                for (int i = 0; i < 3; i++)
                {
                    RobotOdomData[i] = float.Parse(words[i], System.Globalization.CultureInfo.CreateSpecificCulture("en-US"));

                }
                float alpha = RobotOdomData[0];

                RobotOdomData[0] = RobotOdomData[1];
                RobotOdomData[1] = alpha;

                var s = RobotOdomData[0].ToString() + " " + RobotOdomData[1].ToString() +" "+ RobotOdomData[2].ToString();

                Form1.f.Invoke(new Action(() => Form1.f.richTextBox2.Text = s   )); 
                    
                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                //здесь осуществить взятие текущих координат из сети
              //  if (RobotOdomData[1] < 2)
           //     {
                    float[] camodom = OFC.getCamOdom();

                    //float ledx = 0.1f * (float)Math.Cos(RobotOdomData[2]);//так как метка лежит не поцентру робота а чуть ближе к корме то пересчитываем полученные координаты
                    //float ledy = 0.1f * (float)Math.Sin(RobotOdomData[2]);

                    //camodom[0] = camodom[0] + ledx;
                    //camodom[1] = camodom[1] + ledy;

                    if (k == 0)
                    {
                        ustavki[0] = camodom[0];
                        ustavki[1] = camodom[1];
                        ustavki[2] = camodom[2];
                        k = 1;
                    }

                    float XG;// Х координата 
                    float YG;// Y координата
                    float AG;
                    XG = ustavki[0]- camodom[0];
                    YG =  camodom[1]- ustavki[1];
                    ////////////вывод координат с камер
                    var sb = XG.ToString() + " " + YG.ToString() + " " + camodom[2].ToString();
                    Form1.f.Invoke(new Action(() => Form1.f.rtb_tcp2.Text = sb));

                    // XG = XG * (-1);// умножаем на минс 1 чтобы инвертировать ось Y и она совпадала с глобальной осью координат Y
                    //для проверки сделаем навигациб только по камерам

                    //float usrednenieX = RobotOdomData[0] - XG;//высчитываем разницу между одометрий и камерами
                    //float usrednenieY = RobotOdomData[1] - YG;
                    //RobotOdomData[0] = RobotOdomData[0] - usrednenieX / 20;
                    //RobotOdomData[1] = RobotOdomData[1] - usrednenieY / 20;
                    //float usrednenieA = RobotOdomData[2] - camodom[2];

                    // RobotOdomData[2] = RobotOdomData[2] + usrednenieA / 20;// RobotOdomData[2] + usrednenieA / 50;
                    //if (RobotOdomData[1] < 2 && RobotOdomData[1] > 0.5 && RobotOdomData[0] > -1 && RobotOdomData[0] < 1)// если робот находится в обозначеном данными параметрами квадрате, то на юбота отправляется обновленная одометрия 

                    if (RobotOdomData[1] < 2 && RobotOdomData[1] > 0.3 && RobotOdomData[0] > -1 && RobotOdomData[0] < 1 )//& key2==0)
                    {
                        float[] newOdom = new float[3];
                        newOdom[0] =  XG;
                        newOdom[1] =  YG;
                        newOdom[2] =  camodom[2];
                     //   key2 = 1;

                      //SendOdom(newOdom);
                    }
                    //RobotOdomData[0] = XG;// camodom[0];
                    //RobotOdomData[1] = YG;// camodom[1];
                    //RobotOdomData[2] = camodom[2];
              //  }
               }
        }
        public override void Deactivate()
        {
            if (tc != null) tc.Disconnect("form closing", false);
        }
    }

}
