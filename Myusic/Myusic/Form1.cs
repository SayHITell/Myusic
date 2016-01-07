using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace Myusic
{
    public partial class Form1 : Form
    {
        int x = 0;
        int y = 0;
        bool haveHelpPanelShow = false;
        bool haveListPanelShow = false;
        public   string muname;
        public string singer;
        List<string> musicList;
        string[] musicNames;
        #region 窗体拖动代码
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HTCAPTION = 0x2;

        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();
        private void Form1_MouseDown_1(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
        }
        #endregion
        Form2 lyrForm = null;//歌词窗体
       public string pathname;
        private Thread thread1;
        set_Text setLrcText;
        set_Text setLableLrc;
        delegate void set_Text(string s);
        showgeci lrc = new showgeci();
        geci L = new geci();



        public Form1()
        {
            InitializeComponent();
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);   //   禁止擦除背景.
            SetStyle(ControlStyles.DoubleBuffer, true);   //   双缓冲 
        }


        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            int relx = MousePosition.X - this.Location.X;
            if ((relx <= 10 && !haveHelpPanelShow)||(relx>=230 && haveHelpPanelShow))
                helpPanelTimer.Start();
        }

        private void helpPanelTimer_Tick(object sender, EventArgs e)
        {
            if ((x <= -172 && haveHelpPanelShow) || (x >=0 && !haveHelpPanelShow) )
            {
                helpPanelTimer.Stop();
                haveHelpPanelShow = !haveHelpPanelShow;
            }
            else
            {
                if (haveHelpPanelShow)
                {
                    x = x - 160;
                    helpPanel.Location = new Point(x, 0);
                }
                else
                {
                    x = x + 160;
                    helpPanel.Location = new Point(x, 0);                   
                }
            }
        }

        private void dmButtonClose1_Click(object sender, EventArgs e)
        {
            SaveMusicList();
            this.Close();
        }

        private void listPanelTimer_Tick(object sender, EventArgs e)
        {
            if ((y <= -490 && haveListPanelShow) || (y >= 0 && !haveListPanelShow))
            {
                listPanelTimer.Stop();
                haveListPanelShow = !haveListPanelShow;
            }
            else
            {
                if (haveListPanelShow)
                {
                    y = y - 400;
                    listPanel.Location = new Point(172, y);
                }
                else
                {
                    y = y + 400;
                   listPanel.Location = new Point(172, y);
                }
            }
        }

        private void localList_Click(object sender, EventArgs e)
        {
            listPanelTimer.Start();
        }


        Boolean isPlay=false;//判断是否已经播放
        private void Form1_Load(object sender, EventArgs e)
        {
            clearlabel();
            timer1.Enabled = false;
            lyrForm = new Form2();
            timer1.Enabled = true;
            this.BackgroundImage = Gaussian.Transform("1.jpg");
            buttonPanel.BackColor = Color.FromArgb(90, 67, 97, 104);
            helpPanel.BackColor = Color.FromArgb(90, 244, 212, 215);
            listPanel.BackColor = Color.FromArgb(95, 251, 246, 233);
            setLrcText = new set_Text(set_lableText);
            setLableLrc = new set_Text(set_lableLrc);
                      
            getMusicList();
            buttomPicture.Image = Properties.Resources.logo;       
            thread1 = new Thread(new ThreadStart(SerchLrc));
            labeltimer.Enabled = true;
            try
            {
                this.axWindowsMediaPlayer1.settings.volume = 35;//初始化声音为35
                dmProgressBar2.DM_Value = 35;//初始化声音为35
                dmProgressBar1.Enabled = false;
                if (musicnumber != -1)
                {
                    playmusic(musicNames[musicnumber]);
                    pathname = System.IO.Path.GetDirectoryName(musicNames[0]);
                    isPlay = true;
                    setMusicList(1);
                }
              
            }
            catch(Exception)
            {
               // isPlay = false;
            }
            setlabelControl();              
                
        }

        private void addOne_Click(object sender, EventArgs e)
        {
            musicList = new List<string>();
            string[] preMusicList;
            string[] newMusicList;
            OpenFileDialog addOne = new OpenFileDialog();
            addOne.InitialDirectory = "c:\\";
            addOne.Filter = "mp3|*.mp3|wav|*.wav";
            addOne.FilterIndex = 1;
            addOne.Multiselect = true;
            if(addOne.ShowDialog() == DialogResult.OK)
            {
                int i=0; //循环变量
                int sameMusicNum = 0;
                if(musicNames == null)
                {
                    preMusicList = new string[addOne.FileNames.Length];
                    foreach(string fileName in addOne.FileNames)
                    {
                        preMusicList[i] = fileName;
                        i++;
                    }
                }
                else
                {
                    int j;
                    int musicNum = addOne.FileNames.Length + musicNames.Length;
                    preMusicList = new string[musicNum];
                    for (j = 0; j < musicNames.Length; j++)
                    {
                        preMusicList[j] = musicNames[j];
                    }
                    int k=0;
                    for (j = musicNames.Length; j < musicNum; j++)
                    {
                        preMusicList[j] = addOne.FileNames[k];
                        k++;
                    }
                }
                for (i = 0; i < preMusicList.Length; i++) 
                {
                    for (int j = i + 1; j < preMusicList.Length; j++) 
                    {
                        if (preMusicList[i] == preMusicList[j]) 
                        {
                            sameMusicNum++;
                            preMusicList[i] = "repeat";
                        }
                    }
                }
                newMusicList = new string[preMusicList.Length - sameMusicNum];
                int num = 0;
                for (i = 0; i < preMusicList.Length; i++) 
                {
                    if (preMusicList[i] != "repeat") 
                    {
                        newMusicList[num] = preMusicList[i];
                        num++;
                    }
                }
                musicNames = newMusicList;
                musicnumber = 0;
                for (i = 0; i < num; i++)
                {
                    musicList.Add(newMusicList[i]);
                }
                if (!isPlay)
                {
                    playmusic(musicNames[0]);
                } 
                setMusicList(1);
            }                     
        }//添加歌曲

        public void getMusicList()
        {
            string[] musicName;
            if (File.Exists("music.list") == true)
            {
                FileStream fs = new FileStream(string.Format("{0}", "music.list"), FileMode.Open);
                BinaryFormatter bf = new BinaryFormatter();
                this.musicList = ((List<string>)bf.Deserialize(fs));
                fs.Close();
                musicName = new string[musicList.Count];
                for (int i = 0; i < musicList.Count; i++)
                {
                    musicName[i] = musicList[i];
                }
                musicNames = musicName;
                musicNum.Text = "共有" + musicNames.Length + "首歌曲";
                musicnumber = 0;
            }
        }//获取歌列表

        int musicnumber= -1;//歌的数量
        public void SaveMusicList()//保存歌单
        {
            if (File.Exists(".\\music.list") == true)
            {
                File.Delete(".\\music.list");
            }
            if (musicnumber != -1)
            {
                
                SaveFileDialog sf = new SaveFileDialog();
                sf.FileName = "music.list";
                sf.RestoreDirectory = true;
                sf.FilterIndex = 1;
                FileStream fs = new FileStream(string.Format("{0}", sf.FileName), FileMode.Create);
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(fs, musicList);
                fs.Close();
            }
        }
        private int totalPages = 0;
        private int index = 0;
        private void setMusicList(int page)//设置在歌单list中显示
        {
            totalPages = musicNames.Length / 7+1;
            index = (page - 1) * 7;
            if (index + 0 < musicNames.Length) { label1.Text = getFileName(musicNames[index + 0]); }
            if (index + 1 < musicNames.Length) { label2.Text = getFileName(musicNames[index + 1]); }
            if (index + 2 < musicNames.Length) { label3.Text = getFileName(musicNames[index + 2]); }
            if (index + 3 < musicNames.Length) { label4.Text = getFileName(musicNames[index + 3]); }
            if (index + 4 < musicNames.Length) { label5.Text = getFileName(musicNames[index + 4]); }
            if (index + 5 < musicNames.Length) { label6.Text = getFileName(musicNames[index + 5]); }
            if (index + 6 < musicNames.Length) { label7.Text = getFileName(musicNames[index + 6]); }
            nowPage.Text = page.ToString()+"/";
            totalPage.Text = totalPages.ToString();
            musicNum.Text = "共有" + musicNames.Length + "首歌曲";
  
        }

        private void clearlabel()
        {
            label1.Text = "";
            label2.Text = "";
            label3.Text = "";
            label4.Text = "";
            label5.Text = "";
            label6.Text = "";
            label7.Text = "";

        }
        private string getFileName(string path) 
        {
            return System.IO.Path.GetFileNameWithoutExtension(path);
        }//得到文件的路径

        private void addMore_Click(object sender, EventArgs e)//添加文件夹
        {
            musicList = new List<string>();
            FolderBrowserDialog addMore = new FolderBrowserDialog();
            if (addMore.ShowDialog() == DialogResult.OK)
            {
                string foldPath = addMore.SelectedPath;     //获取到选择的文件夹路径
                string[] fileNames = Directory.GetFiles(foldPath);    //获取所有文件               
                if (fileNames.Length > 0)
                {
                    //遍历这个数组
                    for (int i = 0; i < fileNames.Length; i++)
                    {
                        //判断是否是音频文件
                        if (fileNames[i].Contains(".mp3") || fileNames[i].Contains(".wav"))
                        {
                            musicList.Add(fileNames[i]);
                        }
                    }
                }
                string[] musicName = new string[musicList.Count];
                for (int i = 0; i < musicList.Count; i++)
                {
                    musicName[i] = musicList[i];
                }
                musicNames = musicName;
                musicnumber = 0;
                if (!isPlay)
                {
                    playmusic(musicNames[0]);
                } 
                setMusicList(1);
            }                     
        }

        private void search_Click(object sender, EventArgs e)//查找歌曲
        {
            string searchName = searchTextBox.Text;
            string tem;
            int tranf = 0;
            if(searchName != "")
            {
                for (int i = 0; i < musicNames.Length; i++) 
                {
                    if (getFileName(musicNames[i]).Contains(searchName)) //模糊查找
                    {
                        tem = musicNames[i];
                        musicNames[i] = musicNames[tranf];
                        musicNames[tranf] = tem;
                        tranf++;
                    }
                    
                }
                clearlabel();
                setMusicList(1);
            }
        }

        private void searchTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)//如果输入的是回车键  
            {
                this.search_Click(sender, e);//触发button事件  
            }
        }
        /// <summary>
        /// 通过文件地址获取Tag(歌曲歌手图片(内置))
        /// </summary>
        /// <param name="MusicPath">歌曲路径</param>
        /// <returns></returns>
        public List<Image> GetPic_Music(string MusicPath)
        {
            List<Image> imgList = new List<Image>();
            try
            {
                Tags.ID3.ID3Info file = new Tags.ID3.ID3Info(MusicPath, true);
                foreach (Tags.ID3.ID3v2Frames.BinaryFrames.AttachedPictureFrame item in file.ID3v2Info.AttachedPictureFrames)
                {
                    System.Drawing.Image img = item.Picture; // 此段代码用于将获得的
                    imgList.Add(img);
                }
            }
            catch (Exception)
            {
                
            }
            return imgList;
        }

        private void dmProgressBar2_Click(object sender, EventArgs e)//声音控制条
        {
            this.axWindowsMediaPlayer1.settings.volume = (int)dmProgressBar2.DM_Value;
        }

        double currenttime=0;//当前播放音乐的时间
        double totaltime=0;//当前音乐的总时间
        double percent=0;
        private void getmusictime()
        {//获取音乐的时间
            currenttime = this.axWindowsMediaPlayer1.Ctlcontrols.currentPosition;
            totaltime = this.axWindowsMediaPlayer1.currentMedia.duration;
            if (totaltime != 0)
            {
                percent = currenttime / totaltime;
                dmProgressBar1.DM_Value = percent * 100;
            }
        }

       
      
        private void dmProgressBar1_Click(object sender, EventArgs e)//进度控制条
        {
            percent=dmProgressBar1.DM_Value / 100;
            this.axWindowsMediaPlayer1.Ctlcontrols.currentPosition = totaltime * percent;     
           
        }


        Boolean playornot = true;
        public void playmusic(string musicnamepath)
        {//播放音乐

            timer2.Enabled = true;
            if (thread1.IsAlive)
            {
                thread1.Abort(); //撤消thread1
            }
            thread1 = new Thread(new ThreadStart(SerchLrc));
            buttomName.Location = new Point(0, 0);
            name.Location = new Point(0, 0);
            dmProgressBar1.Enabled = true;
            var image = GetPic_Music(musicnamepath);
            this.buttomPicture.Image = image.Any() ? image[0] : Properties.Resources.logo;
            this.mainPicture.Image = image.Any() ? image[0] : Properties.Resources.logo;
            Image img = image.Any() ? image[0] : Gaussian.Transform("1.jpg");
            Bitmap bm = new Bitmap(img, 1000, 600);
            Image backimage = (Image)bm;
            this.BackgroundImage = Gaussian.Transform(backimage);

            this.axWindowsMediaPlayer1.URL = musicnamepath;   
            name.Text = this.axWindowsMediaPlayer1.currentMedia.name;
            buttomName.Text = this.axWindowsMediaPlayer1.currentMedia.name;
       
            pathname = System.IO.Path.GetDirectoryName(this.axWindowsMediaPlayer1.currentMedia.sourceURL);
            thread1.Start();
            int a = name.Right;
            prenamex = a;
            int b = buttomName.Right;
            prebtnamex = b;

            playornot = true;
            isPlay = true;
            musictimerctr.Enabled = true;

            if(playornot==true)
                start.Image = Properties.Resources.interrupt;

        }
        private void start_Click(object sender, EventArgs e)//点击播放音乐
        {
            if (musicnumber != -1)
            {
                if (playornot == false)
                {
                    this.axWindowsMediaPlayer1.Ctlcontrols.play();
                    musictimerctr.Enabled = true;

                    start.Image = Properties.Resources.interrupt;
                    playornot = true;
                    isPlay = true;

                }
                else
                {
                    this.axWindowsMediaPlayer1.Ctlcontrols.pause();
                    musictimerctr.Enabled = true;

                    start.Image = Properties.Resources.start;
                    playornot = false;
                    isPlay = false;
                }
            }
        }


        private void lyr_Click(object sender, EventArgs e)
        {
           
            if (lyrForm != null && lyrForm.Created)//只弹出一次歌词窗口
            {
                lyrForm.Focus();
                return;
            }
            lyrForm = new Form2();
            
             lyrForm.Show();             
        }


        int flag = 1;//标志判断
        int flagstyle = 1;//播放类别
        private void style_Click(object sender, EventArgs e)
        {
            
            if (flag == 1)//列表顺序
            {
                style.Image = Properties.Resources.order;
                flag = 2;
                flagstyle = 1;

               
            }
            else if (flag == 3)//随机播放
            {
                style.Image = Properties.Resources.random;
                flag = 1;
                flagstyle = 3;
                
            }
            else if (flag == 2)//循环播放
            {
                style.Image = Properties.Resources.circle;
                flag = 3;
                flagstyle = 2;
            }
        }
        

        Boolean isvoice = true;
        int vo = -1;
        private void voice_Click(object sender, EventArgs e)//调节声音大小
        {
            if (isvoice == true)
            {
                voice.Image = Properties.Resources.silence;
                this.axWindowsMediaPlayer1.settings.volume = 0;
                vo = (int)dmProgressBar2.DM_Value;
                dmProgressBar2.DM_Value = 0;
                dmProgressBar2.Enabled = false;
                isvoice = false;
            }
            else
            {
                voice.Image = Properties.Resources.voice;
                dmProgressBar2.Enabled = true;
                this.axWindowsMediaPlayer1.settings.volume = vo;
                dmProgressBar2.DM_Value = vo;
                isvoice = true;
            }
        }


        
        private void musictimerctr_Tick(object sender, EventArgs e)
        {
            if(isPlay==true){
                    getmusictime();
                    if(label8.Text!=null)
                         lyrForm.text = label8.Text;
                    if (this.axWindowsMediaPlayer1.Ctlcontrols.currentPositionString != "")
                    {
                        doneTime.Text = this.axWindowsMediaPlayer1.Ctlcontrols.currentPositionString + "/";
                  
                    }
                    else
                    {
                        doneTime.Text = "00:00/";
                       
                    }
                    totalTime.Text = this.axWindowsMediaPlayer1.currentMedia.durationString;
                    if (flagstyle == 1)//列表顺序播放
                    {
                        if (this.axWindowsMediaPlayer1.playState.ToString() == "wmppsStopped")
                        {

                            doneTime.Text = "00:00/";
                            musicnumber++;
                            if (musicnumber >= musicNames.Length)
                                musicnumber = 0;
                            playmusic(musicNames[musicnumber]);
                          
                           
                        }
                    }
                    else if (flagstyle == 2)//循环播放
                    {
                        if (this.axWindowsMediaPlayer1.playState.ToString() == "wmppsStopped")
                        {
                        doneTime.Text = "00:00/";
                        playmusic(musicNames[musicnumber]);
              
                        
                        }
                    }
                    else if (flagstyle == 3)//随机播放
                    {
                       
                        if (this.axWindowsMediaPlayer1.playState.ToString() == "wmppsStopped")
                        {
                            
                            Random ran = new Random();
                            musicnumber = ran.Next(0, musicNames.Length - 1);
                            doneTime.Text = "00:00/";
                            playmusic(musicNames[musicnumber]);
                           
                        }

                    }

            }  
        }

        private void dmButtonMin1_Click(object sender, EventArgs e)//最小化
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void next_Click(object sender, EventArgs e)//下一首
        {
            if (musicnumber != -1)
            {
                musicnumber++;
                if (musicnumber < musicNames.Length)
                {
                    playmusic(musicNames[musicnumber]);


                }
                else
                {
                    musicnumber = 0;
                    playmusic(musicNames[musicnumber]);
                }
            }
           
        }

        private void last_Click(object sender, EventArgs e)//上一首
        {
            if (musicnumber != -1)
            {
                musicnumber--;
                if (musicnumber >= 0)
                {
                    playmusic(musicNames[musicnumber]);

                }
                else
                {
                    musicnumber = musicNames.Length - 1;
                    playmusic(musicNames[musicnumber]);
                }
            }
        }


        int currentpage = 1;//当前页
        private void nextPage_Click(object sender, EventArgs e)//列表下一页
        {
            if(currentpage<totalPages){
                clearlabel();
                setMusicList(currentpage+1);
                currentpage++;
                nowPage.Text = currentpage.ToString()+"/";
            }
        }

        private void lastPage_Click(object sender, EventArgs e)//列表上一页
        {
            if (currentpage > 1)
            {
                clearlabel();
                setMusicList(currentpage - 1);
                currentpage--;
                nowPage.Text = currentpage.ToString()+"/";
            }
        }


        //label事件
        private void setlabelControl()
        {
            label1.MouseEnter += new EventHandler(label_MouseEnter);
            label2.MouseEnter += new EventHandler(label_MouseEnter);
            label3.MouseEnter += new EventHandler(label_MouseEnter);
            label4.MouseEnter += new EventHandler(label_MouseEnter);
            label5.MouseEnter += new EventHandler(label_MouseEnter);
            label6.MouseEnter += new EventHandler(label_MouseEnter);
            label7.MouseEnter += new EventHandler(label_MouseEnter);

            label1.MouseLeave += new EventHandler(lable_MouseLeave);
            label2.MouseLeave += new EventHandler(lable_MouseLeave);
            label3.MouseLeave += new EventHandler(lable_MouseLeave);
            label4.MouseLeave += new EventHandler(lable_MouseLeave);
            label5.MouseLeave += new EventHandler(lable_MouseLeave);
            label6.MouseLeave += new EventHandler(lable_MouseLeave);
            label7.MouseLeave += new EventHandler(lable_MouseLeave);

            label1.DoubleClick += new EventHandler(lable_DoubleClick);
            label2.DoubleClick += new EventHandler(lable_DoubleClick);
            label3.DoubleClick += new EventHandler(lable_DoubleClick);
            label4.DoubleClick += new EventHandler(lable_DoubleClick);
            label5.DoubleClick += new EventHandler(lable_DoubleClick);
            label6.DoubleClick += new EventHandler(lable_DoubleClick);
            label7.DoubleClick += new EventHandler(lable_DoubleClick);
           
        }

        private void label_MouseEnter(object sender, EventArgs e)
        {
            Label lb = (Label)sender;
            lb.SetBounds(lb.Left-14,lb.Top,585,60);
            lb.BringToFront();
            lb.Image = Properties.Resources.label;
        }
        private void lable_MouseLeave(object sender, EventArgs e)
        {
            Label lb = (Label)sender;
            lb.SetBounds(lb.Left+14, lb.Top, 557, 50);
            lb.SendToBack();
            lb.Image = null;
        }
        private void lable_DoubleClick(object sender, EventArgs e)
        {
                Label lb = (Label)sender;
                if (musicNames.Length > (currentpage - 1) * 7 + Convert.ToInt32(lb.Tag))
                {
                    playmusic(musicNames[(currentpage - 1) * 7 + Convert.ToInt32(lb.Tag)]);
                    musicnumber = (currentpage - 1) * 7 + Convert.ToInt32(lb.Tag);
                }
        }

        int namex = 0;
        int btnamex = 0;
        int prebtnamex;
        int prenamex;
        private void labeltimer_Tick(object sender, EventArgs e)
        {         
            if (name.Right > 426)
            {
                namex -= 10;
                name.Location = new Point(namex, 0);
            }
            else
            {
                labeltimer.Stop();
                labeltimer2.Start();
            }

            if (buttomName.Right > 176)
            {
                btnamex -= 3;
                buttomName.Location = new Point(btnamex, 0);
            }
        }

        private void labeltimer2_Tick(object sender, EventArgs e)
        {
            if (name.Right < prenamex)
            {
                namex += 10;
                name.Location = new Point(namex, 0);
            }
            else
            {
                labeltimer2.Stop();
                labeltimer.Start();
            }
            if (buttomName.Right < prebtnamex)
            {
                btnamex += 3;
                buttomName.Location = new Point(btnamex, 0);
            }
        }



        //显示歌词
        string[] Ltime = new string[200];//时间
        string[] Ltext = new string[200];//歌词
        bool timer = false;
        private void ChangeLable(string text)
        {
            Ltime = new string[200];
            Ltext = new string[200];
              if (text == "歌词找到并下载成功！" || text == "正在解析歌词...")
             {
            label8.Invoke(setLrcText, new object[] { text });
            lrc.getLrc(string.Format(".\\Lrc\\{0}.Lrc", L.returnPath()));
            Ltext = lrc.returnText();
            Ltime = lrc.returnTime();
            label8.Invoke(setLableLrc, new object[] { Ltext[0] });
            timer = true;
              }
              else
              {
                  label8.Invoke(setLableLrc, new object[] { text });
               }
        }

        string title;
        private void SerchLrc()
        {
            label8.Invoke(setLrcText, new object[] { "正在搜索歌词..." });
            try
            {
                title = axWindowsMediaPlayer1.currentMedia.getItemInfo("Title");
                string[] sArray = title.Split('-');
                title = sArray[sArray.Length - 1];
                ChangeLable(L.getLrc(title.Trim()));
            }
            catch (Exception)
            {


            }

        }
     
        private void set_lableText(string s)
        {
            label8.Text = s;
        }
        private void set_lableLrc(string text)
        {
            label8.Text = text;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (thread1.IsAlive) //判断thread1是否存在，不能撤消一个不存在的线程，否则会引发异常
            {
                thread1.Abort(); //撤消thread1
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
          
            if (timer == true)
            {
                timer2.Enabled = true;
            }
            else if (timer == false)
            {
                timer2.Enabled = false;
            }
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
             string time;
            time = this.axWindowsMediaPlayer1.Ctlcontrols.currentPositionString+":00";
            for (int i = 0; i < 100; i++)
            {
                if (time == Ltime[i])
                {
                    label8.Text = Ltext[i];
                }
            }
        }
        private void init()
        { 
            musicnumber = -1;
            musicNames = null;
            musictimerctr.Enabled = false;
            start.Image = Properties.Resources.start;
            playornot = true;
            isPlay = false;
            name.Text = "";
            buttomName.Text = "";
            doneTime.Text = "00:00/";
            totalTime.Text = "00:00";
            mainPicture.Image = Properties.Resources.logo;
            buttomPicture.Image = Properties.Resources.logo;
            musicNum.Text = "共有0首歌曲";
 
        }
        private void delete_MouseClick(object sender, MouseEventArgs e)
        {
            int k = 0;
            if (musicnumber != -1 || musicNames != null)
            {
                Console.WriteLine(musicnumber);
                Console.WriteLine(musicNames.Length);
                if (musicNames.Length == 1)
                {
                    musicList.RemoveAt(0);
                    this.axWindowsMediaPlayer1.Ctlcontrols.pause();
                    init();
                    SaveMusicList();
                    Form1_Load(sender, e);
                }
                else
                {
                    Console.WriteLine(musicnumber);
                    Console.WriteLine(musicNames.Length);
                    string[] musicName = new string[musicNames.Length - 1];
                    for (int i = 0; i < musicNames.Length; i++)
                    {
                        if (i != musicnumber)
                        {
                            musicName[k] = musicNames[i];
                            k++;
                        }
                    }
                    musicNames = musicName;
                    musicList.RemoveAt(musicnumber);
                    clearlabel();
                    setMusicList(1);
                    playmusic(musicNames[0]);
                }
            }
        }
    }        
  }

