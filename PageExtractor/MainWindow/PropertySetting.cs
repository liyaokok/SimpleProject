using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MainWindow
{
    public partial class PropertySettingWindow : Form
    {
        int _maxDepth = -1;
        int _maxConnection = -1;

        public int MaxDepth
        {
            get 
            {
                return this._maxDepth;
            }
            set 
            {
                this._maxDepth = value;
            }
        }

        public int MaxConnection
        {
            get
            {
                return this._maxConnection;
            }
            set
            {
                this._maxConnection = value;
            }
        }


        public PropertySettingWindow()
        {
            InitializeComponent();
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            _maxDepth = int.Parse(textBox_MaxDepth.Text.Trim());
            _maxConnection = int.Parse(textBox_MaxConnection.Text.Trim());
            this.DialogResult = DialogResult.OK;
            Close();
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            Close();
        }

        private void PropertySettingWindow_Load(object sender, EventArgs e)
        {
            textBox_MaxDepth.Text = _maxDepth.ToString();
            textBox_MaxConnection.Text = _maxConnection.ToString();
        }
    }
}
