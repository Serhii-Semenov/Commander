using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Commander
{
    public partial class CreateView : Form
    {
        public TextBox dir;
        public Label txtLable;
        public Button okBtn, cancelBtn;

        public CreateView()
        {
            this.Text = "Создание каталога";
            this.Height = 110;
            this.Width = 300;
            txtLable = new Label();
            txtLable.Location = new Point(0, 0);
            txtLable.Size = new System.Drawing.Size(175, 23);
            txtLable.Text = "Имя нового каталога:";
            this.Controls.Add(txtLable);

            dir = new TextBox();
            dir.Location = new Point(0, 23);
            dir.Size = new System.Drawing.Size(185, 23);
            dir.Text = "Новая папка";
            this.Controls.Add(dir);

            okBtn = new Button();
            okBtn.Text = "Создать";
            okBtn.Location = new Point(0, 47);
            okBtn.Size = new System.Drawing.Size(100, 23);
            okBtn.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Controls.Add(okBtn);

            cancelBtn = new Button();
            cancelBtn.Text = "Отмена";
            cancelBtn.Location = new Point(100, 47);
            cancelBtn.Size = new System.Drawing.Size(100, 23);
            cancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Controls.Add(cancelBtn);

            InitializeComponent();
        }
    }
}
