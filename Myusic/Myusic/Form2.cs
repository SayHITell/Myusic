using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;


namespace Myusic
{
    public partial class Form2 : Form
    {
       
        
      
        public string text;
        Form1 f = new Form1();
          
        public Form2()
        {
            InitializeComponent();
           
            int ScreenWidth = Screen.PrimaryScreen.WorkingArea.Width;
            int ScreenHeight = Screen.PrimaryScreen.WorkingArea.Height;
            this.SetBounds(60, f.Height + (ScreenHeight - f.Height) / 2, ScreenWidth-120, (ScreenHeight - f.Height) / 2);
                
        }
     
      
     
        private void dmButtonClose1_Click(object sender, EventArgs e)
        {
            this.Close();
            this.Dispose();
        }

        private void lyrForm_Load(object sender, EventArgs e)
        {
           
            label1.SetBounds(0,0,this.Width,this.Height); 
            lrctimer1.Enabled = true;
            dmButtonClose1.SetBounds(this.Width - dmButtonClose1.Width, 0, dmButtonClose1.Width, dmButtonClose1.Height);      
        }


        private void lrctimer1_Tick(object sender, EventArgs e)
        {
            label1.Text = text;
        }

        private void label1_MouseEnter(object sender, EventArgs e)
        {
            label1.BackColor = Color.Wheat;
            dmButtonClose1.BackColor = Color.Wheat;
        }

        private void label1_MouseLeave(object sender, EventArgs e)
        {
            label1.BackColor = Color.Transparent;
            dmButtonClose1.BackColor = Color.Transparent;
        }

        
    }
}
