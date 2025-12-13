using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using NCneticCore;
using NCneticCore.View;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace NCneticStandalone
{
    public class MainForm : Form
    {
        // Core UI components
        private RichTextBox editor = new RichTextBox();
        private GLControl glControl = new GLControl();

        // Simulation & rendering
        private ncView view;
        private ncJob job = new ncJob();
        private ncMachine machine = new ncMachine();

        // Playback controls
        private TrackBar trackBar = new TrackBar();

        // Manual split container (code/editor vs 3D view)
        private SplitContainer split = new SplitContainer();

        // Manual menu (optional)
        //private MenuStrip menu = new MenuStrip();
        //private ToolStripMenuItem fileMenu = new ToolStripMenuItem("File");
        //private ToolStripMenuItem openItem = new ToolStripMenuItem("Open");
        //private ToolStripMenuItem reloadItem = new ToolStripMenuItem("Reload");
        //private ToolStripMenuItem exitItem = new ToolStripMenuItem("Exit");

        // Designer-generated components (hooked via InitializeComponent)
        private MenuStrip menuStrip1;
        private ToolStrip toolStrip1;
        private ToolStripButton toolStripButton1;
        private ToolStripButton toolStripButton2;
        private ToolStripButton toolStripButton3;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem openToolStripMenuItem;
        private ToolStripMenuItem saveToolStripMenuItem;
        private ToolStripMenuItem saveAsToolStripMenuItem;
        private ToolStripMenuItem exitToolStripMenuItem;
        private SplitContainer splitContainer1;
        private string currentFile = string.Empty;

        // Timer for play/pause
        private Timer _timer;
        private bool _playing;
        private bool _syncingSelection;

        public MainForm()
        {
            // First, wire up any Designer controls
            InitializeComponent();

            // Window settings
            Text = "NCnetic Standalone";
            Width = 1000;
            Height = 700;

            // Set up manual split+controls
            split.Dock = DockStyle.Fill;
            split.SplitterDistance = 350;
            split.Panel1.Controls.Add(editor);
            split.Panel2.Controls.Add(glControl);
            editor.Dock = DockStyle.Fill;
            glControl.Dock = DockStyle.Fill;

            Controls.Add(split);
            Controls.Add(trackBar);
            //Controls.Add(menu);

            // Manual menu setup
            //menu.Items.Add(fileMenu);
            //fileMenu.DropDownItems.Add(openItem);
            //fileMenu.DropDownItems.Add(reloadItem);
            //fileMenu.DropDownItems.Add(new ToolStripSeparator());
            //fileMenu.DropDownItems.Add(exitItem);

            // TrackBar for stepping
            trackBar.Dock = DockStyle.Bottom;
            trackBar.Minimum = 0;
            trackBar.ValueChanged += TrackBar_ValueChanged;
            editor.SelectionChanged += Editor_SelectionChanged;

           // openItem.Click += OpenItem_Click;
          //  reloadItem.Click += ReloadItem_Click;
          //  exitItem.Click += (s, e) => Close();

            // Initialize view & OpenTK
            view = new ncView(new ncViewOptions());
            view.IniGraphicContext(glControl.Handle);
            view.ViewPortLoad(glControl.Width, glControl.Height);
            HookViewEvents();

            // Set up playback timer
            _timer = new Timer { Interval = 200 };
            _timer.Tick += (s, e) => Step();
        }

        private void HookViewEvents()
        {
            glControl.Resize += (s, e) => view.ViewChangeSize(glControl.Width, glControl.Height);
            glControl.Paint += (s, e) =>
            {
                view.ViewPortPaint();
                glControl.SwapBuffers();
            };
            view.Refresh += (s, e) => glControl.Invalidate();
            view.MoveSelected += (s, e) =>
            {
                int selId = job.MoveList.FindIndex(m => m.MoveGuid == e.guid);
                if (selId >= 0)
                {
                    SelectMoveIndex(selId);
                }
            };
        }

        private void TrackBar_ValueChanged(object sender, EventArgs e)
        {
            SelectMoveIndex(trackBar.Value, false);
        }

        private void HighlightLine(int line)
        {
            if (line >= 0 && line < editor.Lines.Length)
            {
                int idx = editor.GetFirstCharIndexFromLine(line);
                _syncingSelection = true;
                editor.SelectionStart = idx;
                editor.ScrollToCaret();
                _syncingSelection = false;
            }
        }

        private void Editor_SelectionChanged(object sender, EventArgs e)
        {
            if (_syncingSelection || !job.MoveList.Any()) return;

            int line = editor.GetLineFromCharIndex(editor.SelectionStart);
            int selId = job.MoveList.FindIndex(m => m.Line == line);

            if (selId >= 0)
            {
                SelectMoveIndex(selId);
            }
        }

        private void SelectMoveIndex(int index, bool updateTrackBar = true)
        {
            if (!job.MoveList.Any() || index < 0 || index >= job.MoveList.Count)
                return;

            view.SelectMove(job.MoveList[index]);
            HighlightLine(job.MoveList[index].Line);

            if (updateTrackBar && trackBar.Value != index)
            {
                trackBar.Value = index;
            }
        }

        private void LoadFile(string file)
        {
            editor.Text = File.ReadAllText(file);
            job = new ncJob { FileName = file, Text = editor.Text };
            job.Process(machine);
            job.EndProcessing += (s, e) =>
            {
                view.LoadJob(job);
                view.Recenter();
                trackBar.Maximum = Math.Max(0, job.MoveList.Count - 1);
                trackBar.Value = 0;
                if (job.MoveList.Any())
                {
                    SelectMoveIndex(0);
                }
            };
        }

        private void OpenItem_Click(object sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog
            {
                Filter = "NC Files|*.nc;*.tap|All Files|*.*"
            };

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                currentFile = dlg.FileName;
                LoadFile(currentFile);
            }
        }

        private void ReloadItem_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(currentFile) && File.Exists(currentFile))
                LoadFile(currentFile);
        }

        private void InitializeComponent()
        {
            menuStrip1 = new MenuStrip();
            toolStrip1 = new ToolStrip();
            toolStripButton1 = new ToolStripButton();
            toolStripButton2 = new ToolStripButton();
            toolStripButton3 = new ToolStripButton();
            fileToolStripMenuItem = new ToolStripMenuItem();
            openToolStripMenuItem = new ToolStripMenuItem();
            saveToolStripMenuItem = new ToolStripMenuItem();
            saveAsToolStripMenuItem = new ToolStripMenuItem();
            exitToolStripMenuItem = new ToolStripMenuItem();
            splitContainer1 = new SplitContainer();
            menuStrip1.SuspendLayout();
            toolStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.SuspendLayout();
            SuspendLayout();
            // 
            // menuStrip1
            // 
            menuStrip1.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem });
            menuStrip1.Location = new System.Drawing.Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new System.Drawing.Size(1189, 24);
            menuStrip1.TabIndex = 0;
            menuStrip1.Text = "menuStrip1";
            //
            // toolStrip1
            //
            toolStrip1.Items.AddRange(new ToolStripItem[] { toolStripButton1, toolStripButton2, toolStripButton3 });
            toolStrip1.Location = new System.Drawing.Point(0, 24);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new System.Drawing.Size(1189, 25);
            toolStrip1.TabIndex = 1;
            toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButton1
            // 
            toolStripButton1.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolStripButton1.Image = Properties.Resources.play;
            toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            toolStripButton1.Name = "toolStripButton1";
            toolStripButton1.Size = new System.Drawing.Size(23, 22);
            toolStripButton1.Text = "toolStripButton1";
            toolStripButton1.Click += toolStripButton1_Click;
            // 
            // toolStripButton2
            // 
            toolStripButton2.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolStripButton2.Image = Properties.Resources.pause;
            toolStripButton2.ImageTransparentColor = System.Drawing.Color.Magenta;
            toolStripButton2.Name = "toolStripButton2";
            toolStripButton2.Size = new System.Drawing.Size(23, 22);
            toolStripButton2.Text = "toolStripButton2";
            toolStripButton2.Click += toolStripButton2_Click;
            //
            // toolStripButton3
            //
            toolStripButton3.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripButton3.ImageTransparentColor = System.Drawing.Color.Magenta;
            toolStripButton3.Name = "toolStripButton3";
            toolStripButton3.Size = new System.Drawing.Size(66, 22);
            toolStripButton3.Text = "Step back";
            toolStripButton3.ToolTipText = "Step backward one line";
            toolStripButton3.Click += toolStripButton3_Click;
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { openToolStripMenuItem, saveToolStripMenuItem, saveAsToolStripMenuItem, exitToolStripMenuItem });
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            fileToolStripMenuItem.Text = "File";
            // 
            // openToolStripMenuItem
            // 
            openToolStripMenuItem.Name = "openToolStripMenuItem";
            openToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            openToolStripMenuItem.Text = "Open";
            openToolStripMenuItem.Click += openToolStripMenuItem_Click;
            // 
            // saveToolStripMenuItem
            // 
            saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            saveToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            saveToolStripMenuItem.Text = "Save";
            // 
            // saveAsToolStripMenuItem
            // 
            saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            saveAsToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            saveAsToolStripMenuItem.Text = "Save As";
            // 
            // exitToolStripMenuItem
            // 
            exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            exitToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            exitToolStripMenuItem.Text = "Exit";
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.Location = new System.Drawing.Point(0, 49);
            splitContainer1.Name = "splitContainer1";
            splitContainer1.Size = new System.Drawing.Size(1189, 571);
            splitContainer1.SplitterDistance = 396;
            splitContainer1.TabIndex = 2;
            // 
            // MainForm
            // 
            ClientSize = new System.Drawing.Size(1189, 620);
            Controls.Add(splitContainer1);
            Controls.Add(toolStrip1);
            Controls.Add(menuStrip1);
            MainMenuStrip = menuStrip1;
            Name = "MainForm";
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();


        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Designer "Open" routes here
            OpenItem_Click(sender, e);
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            // Play/Pause toggle
            if (_playing) _timer.Stop();
            else _timer.Start();
            _playing = !_playing;
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            // Single‑step
            Step();
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            Step(-1);
        }

        private void Step(int delta = 1)
        {
            if (!job.MoveList.Any()) return;

            int nextIndex = Math.Clamp(trackBar.Value + delta, 0, job.MoveList.Count - 1);
            SelectMoveIndex(nextIndex);
        }
    }
}