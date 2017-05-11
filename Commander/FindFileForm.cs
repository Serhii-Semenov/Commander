using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.IO;

namespace Commander
{
    public partial class FindFileForm : Form
    {
        public Panel ControlPanel;
        Button CloseBtn, FindBtn;
        string path;
        public ListView FileList;
        TextBox dir, mask;
        public ListViewItem item;

        public delegate void AddListItem(string name);
        public AddListItem myDeligate;

        public FindFileForm(string StartPath)
        {
            path = StartPath;
            Height = 400;
            Width = 600;
            FileList = new ListView();
            FileList.Dock = DockStyle.Fill;
            FileList.View = View.Details;
            FileList.Columns.Add("Название", 550);

            this.Controls.Add(FileList);

            ControlPanel = new Panel();
            this.Controls.Add(ControlPanel);
            ControlPanel.Dock = DockStyle.Top;
            ControlPanel.Size = new Size(0, 50);

            CloseBtn = new Button();
            CloseBtn.Location = new Point(450, 3);
            CloseBtn.Size = new Size(72, 23);
            CloseBtn.Text = "Закрыть";
            ControlPanel.Controls.Add(CloseBtn);

            Label txtLabel = new Label();
            txtLabel.Location = new Point(0, 0);
            txtLabel.Size = new System.Drawing.Size(175, 23);
            txtLabel.Text = "Введите шаблон поиска";
            ControlPanel.Controls.Add(txtLabel);

            mask = new TextBox();
            mask.Location = new Point(0, 23);
            mask.Size = new System.Drawing.Size(185, 23);
            mask.Text = "*.*";
            ControlPanel.Controls.Add(mask);
            mask.KeyDown += Mask_KeyDown;

            Label txtLabel2 = new Label();
            txtLabel2.Location = new Point(190, 0);
            txtLabel2.Size = new System.Drawing.Size(175, 23);
            txtLabel2.Text = "Папка начала поиска:";
            ControlPanel.Controls.Add(txtLabel2);

            dir = new TextBox();
            dir.Location = new Point(190, 23);
            dir.Size = new System.Drawing.Size(185, 23);
            dir.Text = path;
            ControlPanel.Controls.Add(dir);
            dir.KeyDown += Mask_KeyDown;

            FindBtn = new Button();
            FindBtn.Location = new Point(450, 25);
            FindBtn.Size = new System.Drawing.Size(72, 23);
            FindBtn.Text = "Найти";
            ControlPanel.Controls.Add(FindBtn);

            FindBtn.MouseDown += FindBtn_MouseDown;
            FindBtn.Click += FindBtn_Click;
            CloseBtn.Click += CloseBtn_Click;

            //создаём делегат для добавления в ListView новой строки
            myDeligate = new AddListItem(AddListItemMethod);

            InitializeComponent();
        }

        private void Mask_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) FindBtn.PerformClick();
        }

        private void FindBtn_MouseDown(object sender, MouseEventArgs e)
        {
            //FileList.Items.Clear();
        }

        void AddListItemMethod(string name)
        {
            item = new ListViewItem(name);
            FileList.Items.Add(item);
        }
        void CloseBtn_Click(object sender, EventArgs e)
        {
            Close();
        }

        void FindBtn_Click(object sender, EventArgs e)
        {
            FileList.Items.Clear();

            string[] str = new string[2];
            str[0] = dir.Text;
            str[1] = mask.Text;

            ParameterizedThreadStart findThread = new ParameterizedThreadStart(ThreadFind);
            Thread thread1 = new Thread(findThread);
            thread1.IsBackground = true;
            thread1.Start((object)str);
        }

        void ThreadFind(object str)
        {
            try
            {
                string path = ((string[])str)[0];
                string mask = ((string[])str)[1];

                DirectoryInfo dirinfo = new DirectoryInfo(path);
                this.FindInDir(dirinfo, mask);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка доступа к папке", ex.Message);
            }
        }

        void FindInDir(DirectoryInfo dirinfo, string mask)
        {

            try
            {
                foreach (var file in dirinfo.GetFiles(mask)) // FileInfo
                {
                    this.Invoke(this.myDeligate, file.FullName); // Направляем наш делегат в основной поток
                }

                foreach (var subdir in dirinfo.GetDirectories())
                {
                    this.FindInDir(subdir, mask); // если в нашем каталоге есть подкаталоги то мы рапускаем рекурсию
                }
            }
            catch (Exception) { }
        }

        public FindFileForm()
        {
            InitializeComponent();
        }
    }
}
