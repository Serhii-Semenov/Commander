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
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Commander
{
    public partial class Form1 : Form
    {
        BackgroundWorker aWorker;
        public string AppDirectory { get; private set; } = AppDomain.CurrentDomain.BaseDirectory;
        //long sizeDir = 0;

        #region Elements
        Panel ButtonPanel; // нижняя панель для кнопок
        Button EditBtn, ViewBtn, MkDirBtn, DelBtn, MovBtn, CopyBtn;

        SplitContainer mainSplitContainer;

        Panel LeftTopPanel, btnLeftPanel, btnLeftPathPanel;
        Label LeftPath;
        ComboBox LeftDisk;
        ListView LeftListView;
        public string FileName { get; private set; }

        Panel RightTopPanel, btnRightPanel, btnRightPathPanel;
        Label RightPath;
        ComboBox RightDisk;
        ListView RightListView;

        MenuStrip menuRU;

        List<Button> LeftDiscDriverBtn = new List<Button>();
        List<Button> RightDiscDriverBtn = new List<Button>();

        HotKeys[] hks;

        ProgressBar actionProgressBar;
        #endregion

        #region The Clock
        Label clockLb;
        public delegate void DelegateForTime(Label label); // 
        DelegateForTime DelClock; //
        Thread clockS_thread;
        #endregion

        #region Parametrs
        string FileOnFocus = "";
        string PathOnFocus = "";
        bool LeftPathOnFocus;
        #endregion

        public Form1()
        {
            InitializeComponent();

            #region HotKeys Initialization
            hks = new HotKeys[6];
            for (int j = 0; j < 6; j++)
                hks[j] = new HotKeys();

            hks[0].Key = Keys.F3;
            hks[0].HotKeyPressed += viewFile; // просмотр файла исспользуя NotePad

            hks[1].Key = Keys.F4;
            hks[1].HotKeyPressed += editFile; // редактирование файла исспользуя NotePad

            hks[2].Key = Keys.F5;
            //hks[2].HotKeyPressed += copyFileOrDir; // копирование Файла/Папки
            hks[2].HotKeyPressed += CopyBtn_Click;

            hks[3].Key = Keys.F6;
            hks[3].HotKeyPressed += moveFile; // перемещение Файла/Папки

            hks[4].Key = Keys.F7;
            hks[4].HotKeyPressed += createDir; // создание каталога

            hks[5].Key = Keys.F8;
            hks[5].HotKeyPressed += deleteFileOrDir; // удаление Файла/Папки(включая подкаталоги)
            #endregion

            #region mainSplitContainer
            this.Text = "File Manager";
            this.Size = new Size(840, 500);
            this.StartPosition = FormStartPosition.CenterScreen;

            mainSplitContainer = new SplitContainer();
            Controls.Add(mainSplitContainer);
            mainSplitContainer.Dock = DockStyle.Fill;
            mainSplitContainer.SplitterDistance = 420; // расстояние слева от разделителя

            #endregion 

            #region Left
            LeftListView = new ListView();
            LeftListView.Dock = DockStyle.Fill;
            LeftListView.FullRowSelect = true;
            LeftListView.GridLines = true;
            LeftListView.MultiSelect = false;
            LeftListView.View = View.Details;
            LeftListView.Columns.Add("Название", 250);
            LeftListView.Columns.Add("Тип", 50);
            LeftListView.Columns.Add("Размер", 100);
            mainSplitContainer.Panel1.Controls.Add(LeftListView);

            LeftTopPanel = new Panel();
            mainSplitContainer.Panel1.Controls.Add(LeftTopPanel);
            LeftTopPanel.Dock = DockStyle.Top;
            LeftTopPanel.Padding = new Padding(0, 5, 17, 0);
            LeftTopPanel.Size = new Size(0, 28);

            btnLeftPathPanel = new Panel();
            btnLeftPathPanel.Dock = DockStyle.Bottom;
            btnLeftPathPanel.Size = new Size(0, 22);
            btnLeftPathPanel.Padding = new Padding(5, 5, 0, 0);
            mainSplitContainer.Panel1.Controls.Add(btnLeftPathPanel);

            LeftPath = new Label();
            LeftPath.Dock = DockStyle.Left;
            btnLeftPathPanel.Controls.Add(LeftPath);

            btnLeftPanel = new Panel();
            btnLeftPanel.Dock = DockStyle.Left;
            LeftTopPanel.Controls.Add(btnLeftPanel);

            LeftDisk = new ComboBox();
            LeftDisk.Dock = DockStyle.Right;
            LeftDisk.Margin = new Padding(10);
            LeftDisk.Size = new Size(89, 21);
            LeftTopPanel.Controls.Add(LeftDisk);
            #endregion

            #region Right
            RightListView = new ListView();
            RightListView.Dock = DockStyle.Fill;
            RightListView.FullRowSelect = true;
            RightListView.GridLines = true;
            RightListView.MultiSelect = false;
            RightListView.View = View.Details;
            RightListView.Columns.Add("Название", 250);
            RightListView.Columns.Add("Тип", 50);
            RightListView.Columns.Add("Размер", 100);
            mainSplitContainer.Panel2.Controls.Add(RightListView);

            RightTopPanel = new Panel();
            mainSplitContainer.Panel2.Controls.Add(RightTopPanel);
            RightTopPanel.Dock = DockStyle.Top;
            RightTopPanel.Padding = new Padding(0, 5, 17, 0);
            RightTopPanel.Size = new Size(0, 28);

            btnRightPathPanel = new Panel();
            btnRightPathPanel.Dock = DockStyle.Bottom;
            btnRightPathPanel.Size = new Size(0, 22);
            btnRightPathPanel.Padding = new Padding(5, 5, 0, 0);
            mainSplitContainer.Panel2.Controls.Add(btnRightPathPanel);

            RightPath = new Label();
            RightPath.Dock = DockStyle.Left;
            btnRightPathPanel.Controls.Add(RightPath);

            btnRightPanel = new Panel();
            btnRightPanel.Dock = DockStyle.Left;
            RightTopPanel.Controls.Add(btnRightPanel);

            RightDisk = new ComboBox();
            RightDisk.Dock = DockStyle.Right;
            RightDisk.Margin = new Padding(10);
            RightDisk.Size = new Size(89, 21);
            RightTopPanel.Controls.Add(RightDisk);
            #endregion

            #region Panel for Functional Buttons
            ButtonPanel = new Panel();
            ButtonPanel.BackColor = Color.Azure;
            Controls.Add(ButtonPanel);
            ButtonPanel.Dock = DockStyle.Bottom;
            ButtonPanel.Size = new Size(100, 30);
            #endregion

            #region Buttons : EditBtn, ViewBtn, MkDirBtn, DelBtn, MovBtn, CopyBtn;
            EditBtn = new Button();
            EditBtn.Size = new Size(85, 23);
            EditBtn.Location = new Point(88, 3);
            EditBtn.Text = "F4 Правка";
            ButtonPanel.Controls.Add(EditBtn);

            ViewBtn = new Button();
            ViewBtn.Size = new Size(85, 23);
            ViewBtn.Location = new Point(2, 3);
            ViewBtn.Text = "F3 Просмотр";
            ButtonPanel.Controls.Add(ViewBtn);

            MkDirBtn = new Button();
            MkDirBtn.Size = new Size(85, 23);
            MkDirBtn.Location = new Point(368, 3);
            MkDirBtn.Text = "F7 Каталог";
            ButtonPanel.Controls.Add(MkDirBtn);

            DelBtn = new Button();
            DelBtn.Size = new Size(85, 23);
            DelBtn.Location = new Point(454, 3);
            DelBtn.Text = "F8 Удаление";
            ButtonPanel.Controls.Add(DelBtn);

            MovBtn = new Button();
            MovBtn.Size = new Size(105, 23);
            MovBtn.Location = new Point(263, 3);
            MovBtn.Text = "F6 Перемещение";
            ButtonPanel.Controls.Add(MovBtn);

            CopyBtn = new Button();
            CopyBtn.Size = new Size(90, 23);
            CopyBtn.Location = new Point(173, 3);
            CopyBtn.Text = "F5 Копировать";
            ButtonPanel.Controls.Add(CopyBtn);
            #endregion

            #region ProgressBar
            actionProgressBar = new ProgressBar();
            actionProgressBar.Location = new Point(454 + 85 + 4, 3);
            actionProgressBar.Size = new Size(175, 24);
            ButtonPanel.Controls.Add(actionProgressBar);
            actionProgressBar.Visible = false;
            #endregion

            #region ToolStripMenuItem
            ToolStripMenuItem Menu = new ToolStripMenuItem() { Text = "Дополнительные команды" };
            Menu.DropDownItems.AddRange
            (
                new ToolStripItem[]
                {
                    new ToolStripMenuItem(){ Text = "Создать txt-файл", ShortcutKeys = Keys.Alt|Keys.N, ShowShortcutKeys = true},
                    new ToolStripSeparator(){ },
                    new ToolStripMenuItem(){ Text = "Размер папки", ShortcutKeys = Keys.Alt|Keys.S, ShowShortcutKeys = true},
                    new ToolStripMenuItem(){ Text = "Поиск файлов", ShortcutKeys = Keys.Alt|Keys.F7, ShowShortcutKeys = true},
                    new ToolStripSeparator(){ },
                    new ToolStripMenuItem(){ Text = "Выход", ShortcutKeys = Keys.Alt|Keys.F4, ShowShortcutKeys = true}
                }
            );

            ToolStripMenuItem helpMenuRu = new ToolStripMenuItem() { Text = "Справка" };
            helpMenuRu.DropDownItems.AddRange
            (
                new ToolStripItem[]
                {
                    new ToolStripMenuItem(){ Text = "О программе ...", ShortcutKeys = Keys.F1, ShowShortcutKeys = true}
                }
            );

            menuRU = new MenuStrip()
            {
                Location = new Point(0, 0),
                Size = new Size(292, 24),
                Visible = true
            };

            menuRU.Items.AddRange(
                new ToolStripItem[] { Menu, helpMenuRu, }
                );

            Controls.Add(menuRU);
            #endregion

            #region Buttons Of Driver load in ComboBox
            int i = 0;
            string[] drivers = Directory.GetLogicalDrives();
            foreach (string drive in drivers)
            {
                LeftDisk.Items.Add(drive);
                RightDisk.Items.Add(drive);

                try
                {
                    LeftDiscDriverBtn.Add(new Button());
                    LeftDiscDriverBtn.Last().Location = new Point(2 + i * 32, 0);
                    LeftDiscDriverBtn.Last().Size = new Size(32, 22);
                    LeftDiscDriverBtn.Last().Text = drive.ToString();
                    LeftDiscDriverBtn.Last().Click += LeftDiscDriver_Click;
                    this.btnLeftPanel.Controls.Add(this.LeftDiscDriverBtn.Last());

                    RightDiscDriverBtn.Add(new Button());
                    RightDiscDriverBtn.Last().Location = new Point(2 + i * 32, 0);
                    RightDiscDriverBtn.Last().Size = new Size(32, 22);
                    RightDiscDriverBtn.Last().Text = drive.ToString();
                    RightDiscDriverBtn.Last().Click += RightDiscDriver_Click;
                    this.btnRightPanel.Controls.Add(this.RightDiscDriverBtn.Last());

                    i++;
                }
                catch { }
            }
            LeftDisk.SelectedIndex = 0;
            RightDisk.SelectedIndex = drivers.Length - 1;
            #endregion

            #region Events
            RightDisk.SelectedIndexChanged += RightDisk_SelectedIndexChanged;
            LeftDisk.SelectedIndexChanged += LeftDisk_SelectedIndexChanged;

            LeftListView.Click += LeftListView_Click;
            RightListView.Click += RightListView_Click;
            LeftListView.DoubleClick += ListView_DoubleClick;
            RightListView.DoubleClick += ListView_DoubleClick;

            Menu.DropDownItems[3].Click += findFile;
            Menu.DropDownItems[2].Click += calcSizeDir;

            MkDirBtn.Click += createDir;
            MovBtn.Click += moveFile;
            ViewBtn.Click += viewFile;
            EditBtn.Click += editFile;
            CopyBtn.Click += CopyBtn_Click;
            DelBtn.Click += deleteFileOrDir;


            #endregion

            #region for The Time
            DelClock = new DelegateForTime(StartTime);
            // указываем метод делегату
            clockLb = new Label();
            clockLb.Dock = DockStyle.Right;
            clockLb.Size = new Size(100, 20);
            clockLb.Font = new Font("Time New Roman", 11, FontStyle.Italic);
            clockLb.TextAlign = ContentAlignment.MiddleCenter;
            clockLb.BackColor = Color.LightBlue;

            ButtonPanel.Controls.Add(clockLb);
            this.Load += Form1_Load;
            #endregion

            #region Worker
            aWorker = new BackgroundWorker();
            aWorker.WorkerReportsProgress = true;
            aWorker.RunWorkerCompleted += AWorker_RunWorkerCompleted;
            //aWorker.DoWork += copyFileOrDir;
            aWorker.DoWork += AWorker_DoWork;
            aWorker.ProgressChanged += AWorker_ProgressChanged;
            #endregion
        }

        private void AWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            actionProgressBar.Visible = false;
            ListViewUpdate(true);
            ListViewUpdate(false);
        }

        private void AWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            actionProgressBar.Value = Math.Min(e.ProgressPercentage, 100);
        }

        //private void AWorker_DoWork(object sender, DoWorkEventArgs e)
        //{
        //    // Сначала пройтись по всем файлам и собрать их суммарный размер
        //    var ourDir = Path.Combine(LeftPathOnFocus ? LeftPath.Text : RightPath.Text, FileOnFocus);
        //    if (Directory.Exists(ourDir)) MessageBox.Show("Это директория!");
        //    else MessageBox.Show("Это файл!");

        //    var filesSize = DirGetSize(ourDir);

        //    var fileName = (string)e.Argument;
        //    var fsize = new FileInfo(Path.Combine(LeftPathOnFocus ? LeftPath.Text : RightPath.Text, FileOnFocus)).Length;
        //    var bytesForPercent = fsize / 100;
        //    var buffer = new byte[bytesForPercent];

        //    using (var inputFs = new FileStream(Path.Combine(LeftPathOnFocus ? LeftPath.Text : RightPath.Text, FileOnFocus), FileMode.Open, FileAccess.Read))
        //    using (var outputFs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
        //    {
        //        int counter = 0;
        //        while (inputFs.Position < inputFs.Length)
        //        {
        //            var wasRead = inputFs.Read(buffer, 0, (int)bytesForPercent);
        //            outputFs.Write(buffer, 0, wasRead);
        //            counter++;
        //            if (counter <= actionProgressBar.Maximum)
        //                aWorker.ReportProgress(counter);
        //        }
        //    }
        //}
        private void AWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            // Сначала пройтись по всем файлам и собрать их суммарный размер
            var ourDir = Path.Combine(LeftPathOnFocus ? LeftPath.Text : RightPath.Text, FileOnFocus);
            if (Directory.Exists(ourDir))
            {
                //MessageBox.Show("Это директория!");
                var targetDir = (string)e.Argument; // Директория цели
                string forFind = targetDir.Substring(targetDir.LastIndexOf('\\'), targetDir.Length - targetDir.LastIndexOf('\\'));

                string fileName; 

                var filesSize = DirGetSize(ourDir); // Получить размер директории

                var DD = new DirectoryInfo(ourDir); 

                foreach (var di in DD.EnumerateDirectories("*", SearchOption.AllDirectories))
                {
                    string fullPathDir = di.FullName;
                    string s = fullPathDir.Substring(di.FullName.IndexOf(forFind) + forFind.Length);
                    var tempDir = targetDir + s;
                    if (Directory.Exists(tempDir) != true)
                        Directory.CreateDirectory(tempDir);
                }

                var buffer = new byte[10000000];
                var bytesForPercent = filesSize / 100;
                int step = 1000000;
                int counter = 0;

                foreach (var fi in DD.EnumerateFiles("*", SearchOption.AllDirectories))
                {
                    string file = fi.FullName;
                    string s = file.Substring(fi.FullName.IndexOf(forFind) + forFind.Length);
                    fileName = targetDir + s;

                    using (var inputFs = new FileStream(fi.FullName, FileMode.Open, FileAccess.Read))
                    using (var outputFs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                    {
                        while (inputFs.Position < inputFs.Length)
                        {
                            var wasRead = inputFs.Read(buffer, 0, step);
                            outputFs.Write(buffer, 0, wasRead);

                            counter++;
                            if (counter <= actionProgressBar.Maximum) aWorker.ReportProgress(counter);
                        }
                    }
                }       
            }
            else
            {
                //MessageBox.Show("Это файл!");
                var fileName = (string)e.Argument; // Имя файла и путь куда он должен записаться
                var fsize = new FileInfo(Path.Combine(LeftPathOnFocus ? LeftPath.Text : RightPath.Text, FileOnFocus)).Length;
                var bytesForPercent = fsize / 100;
                var buffer = new byte[bytesForPercent];

                using (var inputFs = new FileStream(Path.Combine(LeftPathOnFocus ? LeftPath.Text : RightPath.Text, FileOnFocus), FileMode.Open, FileAccess.Read))
                using (var outputFs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                {
                    int counter = 0;
                    while (inputFs.Position < inputFs.Length)
                    {
                        var wasRead = inputFs.Read(buffer, 0, (int)bytesForPercent);
                        outputFs.Write(buffer, 0, wasRead);
                        counter++;
                        if (counter <= actionProgressBar.Maximum)
                            aWorker.ReportProgress(counter);
                    }
                }
            }
        }

        private Int64 DirGetSize(string ourDir)
        {
            Int64 size = 0;
            try
            {
                foreach (var file in Directory.GetFiles(ourDir, "*", SearchOption.AllDirectories))
                {
                    // FileInfo f = new FileInfo(file);
                    size += new FileInfo(file).Length;
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
            return size;
        }

        private void CopyBtn_Click(object sender, EventArgs e)
        {
            FileName = FileOnFocus;
            AppDirectory = LeftPathOnFocus? RightPath.Text: LeftPath.Text;
            if (FileName == null) return;
            actionProgressBar.Visible = true;
            actionProgressBar.Maximum = 100;
            actionProgressBar.Value = 0;
            var destinationFileName = Path.Combine(AppDirectory, Path.GetFileName(FileName));
            //var destinationFileName = AppDirectory;
            aWorker.RunWorkerAsync(destinationFileName);
        }

        private void RightListView_Click(object sender, EventArgs e)
        {
            if (RightListView.Focused)
            {
                FileOnFocus = RightListView.SelectedItems[0].SubItems[0].Text;
                PathOnFocus = Path.Combine(RightPath.Text, RightListView.SelectedItems[0].Text);
                LeftPathOnFocus = false;
            }
        }

        private void LeftListView_Click(object sender, EventArgs e)
        {
            if (LeftListView.Focused)
            {
                FileOnFocus = LeftListView.SelectedItems[0].SubItems[0].Text;
                PathOnFocus = Path.Combine(LeftPath.Text, LeftListView.SelectedItems[0].Text);
                LeftPathOnFocus = true;
            }
        }

        private void RightDiscDriver_Click(object sender, EventArgs e)
        {
            int i = 0;
            Button b = (Button)sender;
            foreach (var v in RightDisk.Items)
            {
                if (b.Text == v.ToString())
                {
                    RightDisk.SelectedIndex = i;
                    return;
                }
                i++;
            }
        }

        private void LeftDiscDriver_Click(object sender, EventArgs e)
        {
            int i = 0;
            Button b = (Button)sender;
            foreach (var v in LeftDisk.Items)
            {
                if (b.Text == v.ToString())
                {
                    LeftDisk.SelectedIndex = i;
                    return;
                }
                i++;
            }
        }

        private void deleteFileOrDir(object sender, EventArgs e)
        {
            try
            {
                string sourcePath, destinationPath;
                sourcePath = LeftPathOnFocus ? Path.Combine(LeftPath.Text, LeftListView.SelectedItems[0].Text) : Path.Combine(RightPath.Text, RightListView.SelectedItems[0].Text);
                destinationPath = LeftPathOnFocus ? Path.Combine(RightPath.Text, LeftListView.SelectedItems[0].Text) : Path.Combine(LeftPath.Text, RightListView.SelectedItems[0].Text);

                if (sourcePath == destinationPath)
                {
                    CreateView frm = new CreateView();
                    frm.Text = "Преименнование";
                    frm.txtLable.Text = "Введите новое имя";
                    frm.dir.Text = LeftListView.SelectedItems[0].Text;
                    frm.okBtn.Text = "Пререименовать";
                    if (frm.ShowDialog() != DialogResult.OK)
                    {
                        return;
                    }
                    else
                    {
                        destinationPath = Path.Combine(LeftPathOnFocus ? RightPath.Text : LeftPath.Text, frm.dir.Text);
                    }
                }
                if ((LeftPathOnFocus ? LeftListView.SelectedItems[0].SubItems[1].Text : RightListView.SelectedItems[0].SubItems[1].Text) == "Файл")
                    File.Delete(sourcePath);
                else Directory.Delete(sourcePath, true);
            }
            catch { MessageBox.Show("Невозможно Удалить! ", "Ошибка"); }
            finally
            {
                ListViewUpdate(true);
                ListViewUpdate(false);
            }
        }

        private void copyFileOrDir(object sender, EventArgs e)
        {
            //lock(this)
            try
            {
                string fileName, destFile;
                string sourcePath, destinationPath, targetPath = "";

                fileName = LeftPathOnFocus ? LeftListView.SelectedItems[0].Text : RightListView.SelectedItems[0].Text;
                sourcePath = LeftPathOnFocus ? LeftPath.Text : RightPath.Text;
                targetPath = LeftPathOnFocus ? RightPath.Text : LeftPath.Text;

                destinationPath = LeftPathOnFocus ? RightPath.Text : LeftPath.Text;
                destFile = Path.Combine(targetPath, fileName);

                if ((LeftPathOnFocus ? LeftListView.SelectedItems[0].SubItems[1].Text : RightListView.SelectedItems[0].SubItems[1].Text) == "Файл")
                    File.Copy(Path.Combine(sourcePath, fileName), Path.Combine(destinationPath, fileName), true);
                else
                {
                    sourcePath = Path.Combine(sourcePath, (LeftPathOnFocus ? LeftListView.SelectedItems[0].Text : RightListView.SelectedItems[0].Text));
                    targetPath = Path.Combine(destinationPath, LeftPathOnFocus ? LeftListView.SelectedItems[0].Text : RightListView.SelectedItems[0].Text);
                    perebor_updates(sourcePath, targetPath);
                    /*
                    targetPath = Path.Combine(destinationPath, LeftPathOnFocus ? LeftListView.SelectedItems[0].Text : RightListView.SelectedItems[0].Text);
                    Directory.CreateDirectory(targetPath);
                    sourcePath = Path.Combine(sourcePath, (LeftPathOnFocus ? LeftListView.SelectedItems[0].Text : RightListView.SelectedItems[0].Text));
                    if (System.IO.Directory.Exists(sourcePath))
                    {
                        string[] files = System.IO.Directory.GetFiles(sourcePath);

                        // Copy the files and overwrite destination files if they already exist.
                        foreach (string s in files)
                        {
                            // Use static Path methods to extract only the file name from the path.
                            fileName = System.IO.Path.GetFileName(s);
                            destFile = System.IO.Path.Combine(targetPath, fileName);
                            System.IO.File.Copy(s, destFile, true);
                        }
                        if (LeftPathOnFocus) RightPath.Text = destinationPath;
                        else LeftPath.Text = destinationPath;
                    }
                    else
                    {
                        MessageBox.Show("Source path does not exist!");
                    }
                     * */
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Ошибка"); }
            finally
            {
                ListViewUpdate(true);
                ListViewUpdate(false);
            }
        }

        private void editFile(object sender, EventArgs e)
        {
            try
            {
                if ((LeftPathOnFocus ? LeftListView.SelectedItems[0].SubItems[1].Text : RightListView.SelectedItems[0].SubItems[1].Text) != "Файл")
                {
                    return;
                }

                string s = FileOnFocus;
                if (s != "")
                {
                    ProcessStartInfo psi = new ProcessStartInfo("Notepad++.exe");
                    psi.WindowStyle = ProcessWindowStyle.Normal;
                    psi.Arguments = (LeftPathOnFocus ? LeftPath.Text : RightPath.Text) + @"\" + s;
                    Process.Start(psi);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                FileOnFocus = "";
            }
        }

        void perebor_updates(string begin_dir, string end_dir)
        {
            //Берём нашу исходную папку
            DirectoryInfo dir_inf = new DirectoryInfo(begin_dir);
            //Перебираем все внутренние папки
            foreach (DirectoryInfo dir in dir_inf.GetDirectories())
            {
                //Проверяем - если директории не существует, то создаём;
                if (Directory.Exists(end_dir + "\\" + dir.Name) != true)
                {
                    Directory.CreateDirectory(end_dir + "\\" + dir.Name);
                }

                //Рекурсия (перебираем вложенные папки и делаем для них то-же самое).
                perebor_updates(dir.FullName, end_dir + "\\" + dir.Name);
            }

            //Перебираем файлики в папке источнике.
            foreach (string file in Directory.GetFiles(begin_dir))
            {
                //Определяем (отделяем) имя файла с расширением - без пути (но с слешем "\").
                string filik = file.Substring(file.LastIndexOf('\\'), file.Length - file.LastIndexOf('\\'));
                //Копируем файлик с перезаписью из источника в приёмник.
                File.Copy(file, end_dir + "\\" + filik, true);
            }
        }

        private void viewFile(object sender, EventArgs e)
        {
            try
            {
                if ((LeftPathOnFocus ? LeftListView.SelectedItems[0].SubItems[1].Text : RightListView.SelectedItems[0].SubItems[1].Text) != "Файл")
                {
                    ProcessStartInfo proc = new ProcessStartInfo("explorer.exe", LeftPathOnFocus ? LeftListView.SelectedItems[0].Text : RightListView.SelectedItems[0].Text);
                    Process.Start(proc);
                    return;
                }
                string s = FileOnFocus;
                if (s != "")
                {
                    ProcessStartInfo psi = new ProcessStartInfo("Notepad++.exe");
                    psi.WindowStyle = ProcessWindowStyle.Normal;
                    psi.Arguments = " -ro " + (LeftPathOnFocus ? LeftPath.Text : RightPath.Text) + @"\" + s;
                    Process.Start(psi);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                FileOnFocus = "";
            }
        }

        void Form1_Load(object sender, EventArgs e)
        {
            clockS_thread = new Thread(LabelTime); // создаем поток
            clockS_thread.IsBackground = true;
            clockS_thread.Priority = ThreadPriority.Lowest;
            clockS_thread.Start();

            LeftDisk_SelectedIndexChanged(null, null);
            RightDisk_SelectedIndexChanged(null, null);
        }

        void StartTime(Label label)
        {
            // выводим всегда две цыфры
            // ( 00 : 00 )
            string s = DateTime.Now.Hour.ToString("00");
            s += " : ";
            s += DateTime.Now.Minute.ToString("00");
            s += " : ";
            s += DateTime.Now.Second.ToString("00");
            label.Text = s;
            Thread.Sleep(1);
        }

        void LabelTime()
        {
            while (true)
                if (this.InvokeRequired)
                    try
                    {
                        Invoke(DelClock, clockLb);
                    }
                    catch (Exception) { }
        }

        void moveFile(object sender, EventArgs e)
        {
            try
            {
                string sourcePath, destinationPath;
                sourcePath = LeftPathOnFocus ? Path.Combine(LeftPath.Text, LeftListView.SelectedItems[0].Text) : Path.Combine(RightPath.Text, RightListView.SelectedItems[0].Text);
                destinationPath = LeftPathOnFocus ? Path.Combine(RightPath.Text, LeftListView.SelectedItems[0].Text) : Path.Combine(LeftPath.Text, RightListView.SelectedItems[0].Text);

                if (sourcePath == destinationPath)
                {
                    CreateView frm = new CreateView();
                    frm.Text = "Преименнование";
                    frm.txtLable.Text = "Введите новое имя";
                    frm.dir.Text = LeftListView.SelectedItems[0].Text;
                    frm.okBtn.Text = "Пререименовать";
                    if (frm.ShowDialog() != DialogResult.OK)
                    {
                        return;
                    }
                    else
                    {
                        destinationPath = Path.Combine(LeftPathOnFocus ? RightPath.Text : LeftPath.Text, frm.dir.Text);
                    }
                }
                if ((LeftPathOnFocus ? LeftListView.SelectedItems[0].SubItems[1].Text : RightListView.SelectedItems[0].SubItems[1].Text) == "Файл")
                    File.Move(sourcePath, destinationPath);
                else Directory.Move(sourcePath, destinationPath);
            }
            catch { MessageBox.Show("Невозможно переместить файл! ", "Ошибка"); }
            finally
            {
                ListViewUpdate(true);
                ListViewUpdate(false);
            }
        }

        void createDir(object sender, EventArgs e)
        {
            string path;
            CreateView frm = new CreateView();
            try
            {
                if (frm.ShowDialog() == DialogResult.OK)
                {
                    path = Path.Combine((LeftPathOnFocus ? LeftPath.Text : RightPath.Text), frm.dir.Text);
                    Directory.CreateDirectory(path);
                }
                ListViewUpdate(LeftPathOnFocus);
            }
            catch { MessageBox.Show("Невозможно создать каталог!", "Ошибка"); }
        }

        void calcSizeDir(object sender, EventArgs e)
        {
            try
            {
                string SourceFile;
                long result = 0;
                SourceFile = Path.Combine(
                    LeftPathOnFocus ? LeftPath.Text : RightPath.Text,
                LeftPathOnFocus ? LeftListView.SelectedItems[0].Text : RightListView.SelectedItems[0].Text);
                if ((LeftPathOnFocus ? LeftListView.SelectedItems[0].SubItems[1].Text : RightListView.SelectedItems[0].SubItems[1].Text) == "Папка")
                {
                    foreach (var file in Directory.GetFiles(SourceFile, "*", SearchOption.AllDirectories))
                    {
                        FileInfo f = new FileInfo(file);
                        result += f.Length;
                    }
                    ListViewItem.ListViewSubItem item = new ListViewItem.ListViewSubItem(
                        LeftPathOnFocus ? LeftListView.SelectedItems[0] : RightListView.SelectedItems[0], 
                        result.ToString());
                    if (LeftPathOnFocus) LeftListView.SelectedItems[0].SubItems.Insert(2, item);
                    else RightListView.SelectedItems[0].SubItems.Insert(2, item);
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Невозможно подсчитать размер! Недостаточно прав доступа ...", "Ошибка");
            }
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F12) MessageBox.Show("Q");
        }

        void findFile(object sender, EventArgs e)
        {
            FindFileForm frm = new FindFileForm(PathOnFocus);
            frm.Show();
        }

        void ListView_DoubleClick(object sender, EventArgs e)
        {
            ListView list;
            Label lbl;
            bool flag;
            if (sender.GetHashCode() == LeftListView.GetHashCode())
            {
                list = LeftListView;
                flag = true;
                lbl = LeftPath;
            }
            else
            {
                list = RightListView;
                flag = false;
                lbl = RightPath;
            }

            if (list.SelectedItems[0].SubItems[1].Text == "Папка")
            {
                lbl.Text = Path.Combine(lbl.Text, list.SelectedItems[0].Text);
            }
            else if (list.SelectedItems[0].SubItems[1].Text == "Файл")
            {
                if (Path.GetExtension(list.SelectedItems[0].Text.ToLower().Trim()) == ".exe" ||
                    Path.GetExtension(list.SelectedItems[0].Text.ToLower().Trim()) == ".jpg" ||
                    Path.GetExtension(list.SelectedItems[0].Text.ToLower().Trim()) == ".png" ||
                    Path.GetExtension(list.SelectedItems[0].Text.ToLower().Trim()) == ".txt" ||
                    Path.GetExtension(list.SelectedItems[0].Text.ToLower().Trim()) == ".bmp"
                    ) // получает расширение
                {
                    try
                    {
                        Process proc = new Process();
                        proc.StartInfo.FileName = Path.Combine(lbl.Text, list.SelectedItems[0].Text);
                        proc.Start();
                    }
                    catch (Exception ex) { MessageBox.Show(ex.Message); }
                    return;
                }
            }
            else
            {
                try
                {
                    lbl.Text = Directory.GetParent(lbl.Text).ToString();
                }
                catch (Exception) { }
            }
            ListViewUpdate(flag);

        }

        void LeftDisk_SelectedIndexChanged(object sender, EventArgs e)
        {
            LeftPath.Text = LeftDisk.Text;
            ListViewUpdate(true);
        }

        void RightDisk_SelectedIndexChanged(object sender, EventArgs e)
        {
            RightPath.Text = RightDisk.Text;
            ListViewUpdate(false);
        }

        private void ListViewUpdate(bool flag)
        {
            try
            {
                long result = 0;
                ListView list;
                string path;
                if (flag)
                {
                    list = LeftListView;
                    path = LeftPath.Text;
                }
                else {
                    list = RightListView;
                    path = RightPath.Text;
                }
                list.Items.Clear();
                list.View = View.Details;
                list.Items.Add(new ListViewItem(".."));
                list.Items[0].SubItems.Add("");

                string[] directories = Directory.GetDirectories(path);
                foreach (string directory in directories)
                {
                    ListViewItem item = new ListViewItem(Path.GetFileName(directory));
                    item.SubItems.Add("Папка");
                    list.Items.Add(item);
                }

                string[] files = Directory.GetFiles(path, "*", SearchOption.TopDirectoryOnly);
                foreach (string file in files)
                {
                    ListViewItem item = new ListViewItem(Path.GetFileName(file));
                    FileInfo f = new FileInfo(file);
                    result = f.Length;
                    item.SubItems.Add("Файл");
                    item.SubItems.Add(result.ToString());
                    list.Items.Add(item);
                }
            }
            catch { }
        }
    }
}
