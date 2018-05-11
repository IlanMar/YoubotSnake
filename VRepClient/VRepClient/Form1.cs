using System;
using System.Drawing;
using System.Windows.Forms;
using VRepAdapter;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace VRepClient
{
    public partial class Form1 : Form

    {
        public MapForm f2 = new MapForm();
        public Form1()
        {
            InitializeComponent();
            f1 = this;
            
            f2.Show();

            f = this;
        }
        
        public RobotAdapter ra; //экземпляр класса ra - robot adapter
       public Drive RobDrive;
      public  SequencePoints SQ;//объект класса sequencePoints
       public Map map;//объект класса Map
       public SearchInGraph SiG;//объект класса поиска по графу
        //все что в абзаце ниже, удалить
        public tacticalLevel TactLevel = new tacticalLevel();
      //  public PotField PotFiel = new PotField();
        public KukaPotField KukaPotField = new KukaPotField();
        public int PotfieldButtonA = 0;//если кнопка нажате то методм PotField доступен
        public int KukaPotButtonB = 0;//если кнопка нажата то работает метод кука.
      //  public Bitmap Rob = new Bitmap(@"C:\Users\Илан\Pictures\Robot.jpg");
        public List<Point> ListPoints = new List<Point>();
        public RoomMap RM = new RoomMap(); 
        //мутим сканирование
        public SearchPath SP;//
        public List<Point> RobotsPoint1 = new List<Point>();//списки чтобы рисовать траекторию движения робота
        
     //   public OdomFromCam OFC = new OdomFromCam();
        
        /*enum ErrorCodes
        {
            simx_error_noerror = 0x000000,
            simx_error_novalue_flag = 0x000001,		// input buffer doesn't contain the specified command 
            simx_error_timeout_flag = 0x000002,		//command reply not received in time for simx_opmode_oneshot_wait operation mode 
            simx_error_illegal_opmode_flag = 0x000004,		//command doesn't support the specified operation mode
            simx_error_remote_error_flag = 0x000008,		// command caused an error on the server side 
            simx_error_split_progress_flag = 0x000010,		// previous similar command not yet fully processed (applies to simx_opmode_oneshot_split operation modes) 
            simx_error_local_error_flag = 0x000020,		// command caused an error on the client side //
            simx_error_initialize_error_flag = 0x000040		// simxStart was not yet called //
        };*/

        private void button1_Click(object sender, EventArgs e)
        {
            ra.Init();
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }
        int flag = 1;//флаг с номером действия 1-3 при сканировании карты
        int key =0; //для работы Астара мо змейкой
        public List<Point> ListPointsTemp=new List<Point>();//для работы Астара мо змейкой
                private void timer1_Tick(object sender, EventArgs e)
        {
            //pictureBox1.Invalidate();
            
                PaintEventArgs p = new PaintEventArgs(pictureBox1.CreateGraphics(), pictureBox1.Bounds); //Компонент на котором нужно рисовать и область на которой нужно рисовать
            pictureBox1_Paint(sender, p);
           // pictureBox1_Paint();
                 // this.Invalidate();
            f2.Invalidate();
            if(ra is VrepAdapter)
            {
                var vrep = ra as VrepAdapter;

                string Lidar = VRepFunctions.GetStringSignal(vrep.clientID, "Lidar");//str-данные с ледара
                string RobPos = VRepFunctions.GetStringSignal(vrep.clientID, "RobPos");//получение координат робота на сцене Врепа
                vrep.ReceiveLedData(Lidar);
                vrep.ReceiveOdomData(RobPos);
            }
            

            if (ra != null)
            {
                ra.Send(RobDrive);
            }
            
            if (RobDrive != null & ra != null && SQ!=null )//отправка одометрии в экземпляр класса drive
            {
                map.GlobListToGraph(map.GlobalMapList, map.RobOdData);
                float GoalPointX = Convert.ToSingle(textBox8.Text);
                float GoalPointY = Convert.ToSingle(textBox9.Text);
             
                Point start = new Point((int)(ra.RobotOdomData[0] * 10 + map.Xmax / 2), (int)(ra.RobotOdomData[1] * 10 + map.Ymax / 2));
                Point goal = new Point((int)GoalPointX*10+map.Xmax / 2, (int)GoalPointY*10+map.Ymax / 2);



            //    float[] test = OFC.getCamOdom();/////////////////////////////////////////////////////удалить эту строчку и объект класса OFC
                // ListPoints = null;
                //1 шаг 
                if (key == 0)
                {
                    goal.X = 100; goal.Y = 100;
                    ListPoints = SiG.FindPath(map.graph, start, goal); //SearchInGraph.FindPath(map.graph, start, goal);
                    key = 1;
                    ListPointsTemp = ListPoints;
                }
                //блок первый, выбираем левую нижнюю точку для поиска по карте.
                PointF H = new PointF(0, 0);
         
              
                    RM.NextRoom(out H, out flag, ra.RobotOdomData, flag);


                //Реализуем движение змейкой
                //2 шаг
                flag = 2;
                if (flag == 2)
                {
                    PointF snakePoint = SP.ExpertSystem(out flag, ra.RobotOdomData, ListPointsTemp);// получаем следующую точку змейки
                    Point snakeP = new Point((int)snakePoint.X + map.Xmax / 2, (int)snakePoint.Y + map.Ymax / 2);//точка подготовленная для масштабов карты
                    ListPoints = SiG.FindPath(map.graph, start, Point.Round(snakeP));//отправляем в А* эти точки
                }
                //if (snakePoint.X == 0 & snakePoint.Y == 0)
                //{
                //    //здесь взять из класса RoomMap следующюю комнату и поехать туда. Из RoomMap точка должна прийти в неподготовленном виде как в классе snakePoint
                //}

                Point driveH = new Point((int)H.X + map.Xmax / 2, (int)H.Y + map.Ymax / 2);//точка для движения к неисследованному кармуну на карте
                

                if (flag == 1)
                {
                    ListPoints = SiG.FindPath(map.graph, start, Point.Round(driveH));//отправляем в А* эти точки
                }
               
                ListPointsTemp = ListPoints;//запоминаем результат А* для следующего цикла

                if (ListPoints != null)
                {
                    SQ.GetNextPoint(ListPoints, ra.RobotOdomData[0], ra.RobotOdomData[1], ra.RobotOdomData[2], map.Xmax, map.Ymax);

                    //RobDrive.GetDrive(ra.RobotOdomData[0], ra.RobotOdomData[1], ra.RobotOdomData[2], SQ.CurrentPointX, SQ.CurrentPointY, map.Xmax,map.Ymax);//закоментили 12.01.18 чтобы отправлять туда точки от движения змейкой
                    RobDrive.GetDrive(ra.RobotOdomData[0], ra.RobotOdomData[1], ra.RobotOdomData[2], ListPoints[ListPoints.Count-1].X, ListPoints[ListPoints.Count-1].Y, map.Xmax, map.Ymax); 
                    ra.Send(RobDrive);
                }
                map.LedDataToList(ra.RobotLedData, ra.RobotOdomData);
                
            }
            if (ra != null & RobDrive != null)//вывод переменных из Робот Адаптера на форму
         {
             string OutLedData="";
             string OutOdomData = "";
             for (int i = 0; i < ra.RobotLedData.Length; i++) 
             {
                 OutLedData = OutLedData + ra.RobotLedData[i]+"; ";
             }
             for (int i = 0; i < ra.RobotOdomData.Length; i++)
             {
                 OutOdomData = OutOdomData + ra.RobotOdomData[i]+"; ";
             }

            // richTextBox1.Text = OutLedData;//закоменчен вывод данных одометрии
             richTextBox2.Text = OutOdomData;
             if (RobDrive != null)
             {
                 textBox2.Text = RobDrive.Phi.ToString();
                 textBox2.Invalidate();
                 textBox3.Text = RobDrive.RobotDirection.ToString();
                 textBox3.Invalidate();
                 textBox4.Text = RobDrive.TargetDirection.ToString();
                 textBox4.Invalidate();
                 textBox5.Text = RobDrive.DistToTarget.ToString();
                 textBox5.Invalidate();
                    if (ra is YoubotAdapter)
                    {
                        //richTextBox3.Text = OFC.CamOdomData[0].ToString();
                        //richTextBox3.Text = OFC.CamOdomData[1].ToString();
                        //richTextBox3.Text = OFC.CamOdomData[2].ToString();
                        //richTextBox3.Invalidate();
                       // richTextBox3.Text = OFC.txb_text.ToString();
                       // richTextBox3.Invalidate();
                    }
                }
         }
            
           
            
        }

    

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (ra != null)
            {
                ra.Deactivate();
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
           
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            PotfieldButtonA = 1;

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        public static Form1 f1;
        public static Form1 f;
        private void bt_tcp_test_Click(object sender, EventArgs e)
        {
            if (ra is YoubotAdapter)
            {
                var ya = ra as YoubotAdapter;
                ya.TCPconnect(tb_ip.Text);
            }
            else { MessageBox.Show("вы работаете с Врепом, а не с реальным роботом!"); }
        }

        public void ShowLedData(string s)
        {
            rtb_tcp.Invoke(new Action(() => rtb_tcp.Text = s));
        }
        public void ShowOdomData(string s)
        {
           // rtb_tcp2.Invoke(new Action(() => rtb_tcp2.Text = s));
        }

        private void btsend_Click(object sender, EventArgs e)
        {
       //     if (tc == null) 
        //    {

        //        return;
       //     }
       //     tc.Send(rtb_send.Text); 
        }

        private void rtb_send_TextChanged(object sender, EventArgs e)
        {

        }

        private void KukaPotButton_Click(object sender, EventArgs e)
        {

        }

        private void VrepAdapter_Click(object sender, EventArgs e)
        {
             ra =  new VrepAdapter();
           
        }

        private void YoubotAdapter_Click(object sender, EventArgs e)
        {
            ra = new YoubotAdapter();
        }

        private void Drive_Click(object sender, EventArgs e)
        {
            SQ = new SequencePoints();
            RobDrive = new Drive();
            map = new Map();
            SiG = new SearchInGraph();
            SP = new SearchPath();
        }

        private void richTextBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void rtb_tcp_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox8_TextChanged(object sender, EventArgs e)
        {

        }
        
        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            if (ra != null && SQ != null && Drive != null && map != null && map.graph != null)
         {          

            int yy = 0;
            int xx=0;
          //  this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
          //  if (10==timer1.Tick) { }               
               /*
              
                    for (int l = 0; l < map.Ymax + 1; l++)//отрисовываем сетку
                    {
                        e.Graphics.DrawLine(new Pen(Color.Black), 0, yy, pictureBox1.Width, yy);
                        // xx = xx + 50;
                        yy = yy + pictureBox1.Height / map.Ymax;
                    }
                    for (int l = 0; l < map.Xmax + 1; l++)
                    {
                        e.Graphics.DrawLine(new Pen(Color.Black), xx, 0, xx, pictureBox1.Height);
                        xx = xx + pictureBox1.Width / map.Xmax;
                    }
                    System.Drawing.SolidBrush drawBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Red);
             
           */
            
                int MapWidth = map.Xmax;
                int MapHeight = map.Ymax;
                int CellSize = 4;
                // Размер карты в пискелях.
                int mapWidthPxls = MapWidth * (CellSize + 1) + 1,
                    mapHeightPxls = MapHeight * (CellSize + 1) + 1;
                Bitmap mapImg = new Bitmap(mapWidthPxls, mapHeightPxls);
                Graphics g = Graphics.FromImage(mapImg);

                // Заливаем весь битмап:
                g.Clear(Color.White);

                // Рисуем сетку:
                for (int x = 0; x <= MapWidth; x++)
                    g.DrawLine(Pens.LightGray, x * (CellSize + 1), 0, x * (CellSize + 1), mapHeightPxls);
                for (int y = 0; y <= MapHeight; y++)
                    g.DrawLine(Pens.LightGray, 0, y * (CellSize + 1), mapWidthPxls, y * (CellSize + 1));
                PictureBox p = pictureBox1;
                if (p.Image != null)
                    p.Image.Dispose();

                pictureBox1.Image = mapImg;
                g.Dispose();
            
            
         for (int i = 0; i < map.Xmax; i++) //закрашиваем ячейки с препядствиями
         {
             for (int k = 0; k < map.Ymax; k++)
             {
                 if (map.graph[i, k] ==2)
                 {
                     int H = CellSize+1;//(int)(pictureBox1.Height / map.Ymax);
                     int W = CellSize + 1;//(int)(pictureBox1.Width / map.Xmax);
                     
                     SolidBrush blueBrush = new SolidBrush(Color.Blue);                   
                     Rectangle rect = new Rectangle((i) * W, pictureBox1.Height + ((-1) * k * H) , W, H);                  
                     e.Graphics.FillRectangle(blueBrush, rect);                  
                 }
                 /*
                 if (map.graph[i, k] ==0 )//закрашиваем пустые ячейки прозрачным цветом
                 {
                     int H = (int)(pictureBox1.Height / map.Ymax);
                     int W = (int)(pictureBox1.Width / map.Xmax);
                    
                     Color brushColor = Color.FromArgb(250 / 100 * 0, 255, 0, 0);
                     SolidBrush blueBrush = new SolidBrush(brushColor);
                     // Create rectangle.//ниже путаница со знаками, по Иксу двигается а по У нет
                     Rectangle rect = new Rectangle((i) * W, pictureBox1.Height + ((-1) * k * H), W, H);

                     // Fill rectangle to screen.
                     e.Graphics.FillRectangle(blueBrush, rect);

                 }*/
                 
             }
         }
         if (ListPoints != null)//ресуем получившийся маршрут
         {
             for (int i = 0; i < ListPoints.Count; i++)
             {
                 int H =CellSize+1; //(int)(pictureBox1.Height / map.Ymax);
                 int W = CellSize + 1;//(int)(pictureBox1.Width / map.Xmax);

                 SolidBrush blueBrush = new SolidBrush(Color.Red);
                 SolidBrush greenBrush = new SolidBrush(Color.Green);
                 Rectangle rect = new Rectangle((ListPoints[i].X) * W, pictureBox1.Height + ((-1) * ListPoints[i].Y * H), W, H);
                 Rectangle rectCurrentPoint = new Rectangle(((int)SQ.CurrentPointX) * W, pictureBox1.Height + ((-1) * (int)SQ.CurrentPointY * H), W, H);
                 
                 e.Graphics.FillRectangle(blueBrush, rect);
                 e.Graphics.FillRectangle(greenBrush, rectCurrentPoint);
             }

         }
     
         

                //рисуем путь робота
         Brush Green = new SolidBrush(Color.Green);
         Pen GreenPen = new Pen(Color.Green, 2);
                int H2 = CellSize + 1;// (int)(pictureBox1.Height / map.Ymax);
                int W2 = CellSize + 1; //(int)(pictureBox1.Width / map.Xmax);
                Point start = new Point((int)(ra.RobotOdomData[0] * 10 + map.Xmax / 2), (int)(ra.RobotOdomData[1] * 10 + map.Ymax / 2));//отрисовки местоположений роботов
                e.Graphics.FillEllipse(Green, (int)start.X * W2 - 2 * W2 - 0, pictureBox1.Height + ((-1) * (int)start.Y) * H2 - 2 * W2 - 0, 15, 15);
                RobotsPoint1.Add(new Point((int)start.X * W2 - 2 * W2, pictureBox1.Height + ((-1) * (int)start.Y) * H2 - 2 * W2));//задаю точку где был робот
                for (int i = 0; i < Form1.f1.RobotsPoint1.Count - 10; i++)
                {
                    Pen blackPen = new Pen(Color.Green, 2);
                    e.Graphics.DrawLine(blackPen, (int)Form1.f1.RobotsPoint1[i].X, (int)Form1.f1.RobotsPoint1[i].Y, (int)Form1.f1.RobotsPoint1[i + 10].X, (int)Form1.f1.RobotsPoint1[i + 10].Y);
                    //e.Graphics.DrawLine(YellowPen, (int)Form1.f1.RobotsPoint2[i].X, (int)Form1.f1.RobotsPoint2[i].Y, (int)Form1.f1.RobotsPoint2[i + 17].X, (int)Form1.f1.RobotsPoint2[i + 17].Y);
                   // e.Graphics.DrawLine(RedPen, (int)Form1.f1.RobotsPoint3[i].X, (int)Form1.f1.RobotsPoint3[i].Y, (int)Form1.f1.RobotsPoint3[i + 17].X, (int)Form1.f1.RobotsPoint3[i + 17].Y);
                    i = i + 9;
                }
            }
          //  e.Graphics.Clear(Color.Teal);
           // e.Graphics.Clear();
            if (map != null && map.invalidateform == true)//обновляем форму
            {
                pictureBox1.Invalidate();//вызов отрисовки на пикчербоксе перенести в более логичное мето
            }

        }
        private void pictureBox1_Click(object sender, EventArgs e)
        {
         

        }



        public Image Rob { get; set; }

        private void tb_ip_TextChanged(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            flag = 2;
        }

        private void richTextBox3_TextChanged(object sender, EventArgs e)
        {
            
        }

        private void rtb_tcp2_TextChanged(object sender, EventArgs e)
        {

        }

        private void label10_Click(object sender, EventArgs e)
        {

        }
    }
}
