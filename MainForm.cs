using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Windows.Forms;

namespace WinPE_AdminTool
{
    public partial class MainForm : Form
    {
        // UI Controls
        private TabControl mainTabControl = null!;
        private TabPage tabUsers = null!;
        private TabPage tabExplorer = null!;
        private TabPage tabDrives = null!;

        // User Management Controls
        private ComboBox comboDrives = null!;
        private ListView listUsers = null!;
        private Button btnRefreshUsers = null!;
        private Button btnGrantAdmin = null!;
        private Button btnRevokeAdmin = null!;
        private Button btnRemoveRestrictions = null!;
        private Button btnCreateUser = null!;
        private Button btnDeleteUser = null!;
        private Label lblSelectedDriveInfo = null!;

        // File Explorer Controls
        private ComboBox comboExplorerDrives = null!;
        private TreeView treeFolders = null!;
        private ListView listFiles = null!;
        private TextBox txtCurrentPath = null!;
        private Button btnExplorerUp = null!;
        private Button btnResetPermissions = null!;
        private Button btnDeleteFile = null!;
        private Button btnOpenFile = null!;

        // Drive Info Controls
        private ListView listDrives = null!;
        private Button btnRefreshDrives = null!;

        public MainForm()
        {
            InitializeComponent();
            LoadDrives();
            RefreshUserList();
            RefreshDriveList();
        }

        private void InitializeComponent()
        {
            this.Text = "WinPE & Windows User & System Admin Suite";
            this.Size = new System.Drawing.Size(950, 650);
            this.StartPosition = FormStartPosition.CenterScreen;

            mainTabControl = new TabControl { Dock = DockStyle.Fill };

            // --- TAB 1: USER MANAGEMENT ---
            tabUsers = new TabPage("User Account Management");
            
            Panel topUserPanel = new Panel { Dock = DockStyle.Top, Height = 45 };
            Label lblDrive = new Label { Text = "Target Drive / OS:", Location = new System.Drawing.Point(12, 14), AutoSize = true };
            comboDrives = new ComboBox { Location = new System.Drawing.Point(120, 10), Width = 180, DropDownStyle = ComboBoxStyle.DropDownList };
            comboDrives.SelectedIndexChanged += ComboDrives_SelectedIndexChanged;
            
            btnRefreshUsers = new Button { Text = "Refresh Users", Location = new System.Drawing.Point(310, 8), Width = 110, Height = 28 };
            btnRefreshUsers.Click += (s, e) => RefreshUserList();

            lblSelectedDriveInfo = new Label { Text = "Mode: Active System", Location = new System.Drawing.Point(430, 14), AutoSize = true, Font = new System.Drawing.Font(this.Font, System.Drawing.FontStyle.Bold) };

            topUserPanel.Controls.AddRange(new Control[] { lblDrive, comboDrives, btnRefreshUsers, lblSelectedDriveInfo });

            listUsers = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true
            };
            listUsers.Columns.Add("Username", 150);
            listUsers.Columns.Add("Admin Privileges", 130);
            listUsers.Columns.Add("Status", 120);
            listUsers.Columns.Add("Password Required", 130);
            listUsers.Columns.Add("Comment / Description", 200);

            Panel actionPanel = new Panel { Dock = DockStyle.Right, Width = 210 };
            
            btnGrantAdmin = new Button { Text = "Grant Admin", Location = new System.Drawing.Point(15, 20), Width = 180, Height = 35 };
            btnGrantAdmin.Click += BtnGrantAdmin_Click;

            btnRevokeAdmin = new Button { Text = "Revoke Admin", Location = new System.Drawing.Point(15, 65), Width = 180, Height = 35 };
            btnRevokeAdmin.Click += BtnRevokeAdmin_Click;

            btnRemoveRestrictions = new Button { Text = "Remove All Restrictions", Location = new System.Drawing.Point(15, 110), Width = 180, Height = 40, BackColor = System.Drawing.Color.LightGreen };
            btnRemoveRestrictions.Click += BtnRemoveRestrictions_Click;

            btnCreateUser = new Button { Text = "Create New User", Location = new System.Drawing.Point(15, 175), Width = 180, Height = 35 };
            btnCreateUser.Click += BtnCreateUser_Click;

            btnDeleteUser = new Button { Text = "Delete User", Location = new System.Drawing.Point(15, 220), Width = 180, Height = 35, ForeColor = System.Drawing.Color.DarkRed };
            btnDeleteUser.Click += BtnDeleteUser_Click;

            actionPanel.Controls.AddRange(new Control[] { btnGrantAdmin, btnRevokeAdmin, btnRemoveRestrictions, btnCreateUser, btnDeleteUser });

            tabUsers.Controls.Add(listUsers);
            tabUsers.Controls.Add(actionPanel);
            tabUsers.Controls.Add(topUserPanel);

            // --- TAB 2: BUILT-IN FILE EXPLORER ---
            tabExplorer = new TabPage("Built-in File Explorer");

            Panel expTopPanel = new Panel { Dock = DockStyle.Top, Height = 45 };
            Label lblExpDrive = new Label { Text = "Drive:", Location = new System.Drawing.Point(10, 14), AutoSize = true };
            comboExplorerDrives = new ComboBox { Location = new System.Drawing.Point(60, 10), Width = 100, DropDownStyle = ComboBoxStyle.DropDownList };
            comboExplorerDrives.SelectedIndexChanged += ComboExplorerDrives_SelectedIndexChanged;

            btnExplorerUp = new Button { Text = "Up ⬆", Location = new System.Drawing.Point(170, 9), Width = 60, Height = 27 };
            btnExplorerUp.Click += BtnExplorerUp_Click;

            txtCurrentPath = new TextBox { Location = new System.Drawing.Point(240, 11), Width = 400, ReadOnly = true };

            btnResetPermissions = new Button { Text = "Take Ownership / Unlock Permissions", Location = new System.Drawing.Point(650, 8), Width = 230, Height = 30, BackColor = System.Drawing.Color.LightCyan };
            btnResetPermissions.Click += BtnResetPermissions_Click;

            expTopPanel.Controls.AddRange(new Control[] { lblExpDrive, comboExplorerDrives, btnExplorerUp, txtCurrentPath, btnResetPermissions });

            SplitContainer expSplit = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Vertical, SplitterDistance = 260 };
            
            treeFolders = new TreeView { Dock = DockStyle.Fill };
            treeFolders.BeforeExpand += TreeFolders_BeforeExpand;
            treeFolders.AfterSelect += TreeFolders_AfterSelect;

            listFiles = new ListView { Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true, GridLines = true };
            listFiles.Columns.Add("Name", 240);
            listFiles.Columns.Add("Size", 100);
            listFiles.Columns.Add("Type", 100);
            listFiles.Columns.Add("Date Modified", 140);
            listFiles.DoubleClick += ListFiles_DoubleClick;

            expSplit.Panel1.Controls.Add(treeFolders);
            expSplit.Panel2.Controls.Add(listFiles);

            Panel expBottomPanel = new Panel { Dock = DockStyle.Bottom, Height = 45 };
            btnOpenFile = new Button { Text = "Open File", Location = new System.Drawing.Point(10, 8), Width = 110, Height = 30 };
            btnOpenFile.Click += (s, e) => OpenSelectedFile();

            btnDeleteFile = new Button { Text = "Delete Selected", Location = new System.Drawing.Point(130, 8), Width = 130, Height = 30, ForeColor = System.Drawing.Color.Red };
            btnDeleteFile.Click += BtnDeleteFile_Click;

            expBottomPanel.Controls.AddRange(new Control[] { btnOpenFile, btnDeleteFile });

            tabExplorer.Controls.Add(expSplit);
            tabExplorer.Controls.Add(expTopPanel);
            tabExplorer.Controls.Add(expBottomPanel);

            // --- TAB 3: DRIVE SELECTION & DISK INFO ---
            tabDrives = new TabPage("Drive & System Manager");
            
            Panel driveTopPanel = new Panel { Dock = DockStyle.Top, Height = 45 };
            btnRefreshDrives = new Button { Text = "Refresh Drives List", Location = new System.Drawing.Point(12, 10), Width = 160, Height = 28 };
            btnRefreshDrives.Click += (s, e) => RefreshDriveList();
            driveTopPanel.Controls.Add(btnRefreshDrives);

            listDrives = new ListView { Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true, GridLines = true };
            listDrives.Columns.Add("Drive", 70);
            listDrives.Columns.Add("Label", 130);
            listDrives.Columns.Add("Type", 100);
            listDrives.Columns.Add("Format", 80);
            listDrives.Columns.Add("Free Space", 120);
            listDrives.Columns.Add("Total Size", 120);
            listDrives.Columns.Add("Windows System Found?", 160);

            tabDrives.Controls.Add(listDrives);
            tabDrives.Controls.Add(driveTopPanel);

            mainTabControl.TabPages.Add(tabUsers);
            mainTabControl.TabPages.Add(tabExplorer);
            mainTabControl.TabPages.Add(tabDrives);

            this.Controls.Add(mainTabControl);
        }

        #region Drive Loading & Management

        private void LoadDrives()
        {
            comboDrives.Items.Clear();
            comboExplorerDrives.Items.Clear();

            comboDrives.Items.Add("Active Live System");

            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady)
                {
                    comboDrives.Items.Add($"{drive.Name} [{drive.VolumeLabel}]");
                    comboExplorerDrives.Items.Add(drive.Name);
                }
            }

            if (comboDrives.Items.Count > 0) comboDrives.SelectedIndex = 0;
            if (comboExplorerDrives.Items.Count > 0) comboExplorerDrives.SelectedIndex = 0;
        }

        private void ComboDrives_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (comboDrives.SelectedItem == null) return;
            string selected = comboDrives.SelectedItem.ToString()!;
            if (selected.StartsWith("Active Live System"))
            {
                lblSelectedDriveInfo.Text = "Mode: Active Live System";
            }
            else
            {
                string driveLetter = selected.Substring(0, 2);
                string winDir = Path.Combine(driveLetter + "\\", "Windows");
                if (Directory.Exists(winDir))
                {
                    lblSelectedDriveInfo.Text = $"Mode: Offline Target ({driveLetter}) - Windows Installed";
                }
                else
                {
                    lblSelectedDriveInfo.Text = $"Mode: Target ({driveLetter}) - No Windows OS";
                }
            }
            RefreshUserList();
        }

        private void RefreshDriveList()
        {
            listDrives.Items.Clear();
            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                ListViewItem item = new ListViewItem(drive.Name);
                if (drive.IsReady)
                {
                    item.SubItems.Add(drive.VolumeLabel);
                    item.SubItems.Add(drive.DriveType.ToString());
                    item.SubItems.Add(drive.DriveFormat);
                    item.SubItems.Add(FormatBytes(drive.AvailableFreeSpace));
                    item.SubItems.Add(FormatBytes(drive.TotalSize));

                    bool hasWin = Directory.Exists(Path.Combine(drive.Name, "Windows", "System32"));
                    item.SubItems.Add(hasWin ? "YES (WinDir Detected)" : "No");
                }
                else
                {
                    item.SubItems.Add("Not Ready");
                    item.SubItems.Add(drive.DriveType.ToString());
                    item.SubItems.Add("-");
                    item.SubItems.Add("-");
                    item.SubItems.Add("-");
                    item.SubItems.Add("No");
                }
                listDrives.Items.Add(item);
            }
        }

        private string FormatBytes(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
            int counter = 0;
            decimal number = (decimal)bytes;
            while (Math.Round(number / 1024) >= 1)
            {
                number /= 1024;
                counter++;
            }
            return $"{number:n2} {suffixes[counter]}";
        }

        #endregion

        #region User Account Management

        private void RefreshUserList()
        {
            listUsers.Items.Clear();

            try
            {
                // Run 'net user' command to enumerate local accounts
                string output = RunCommand("net", "user");
                List<string> users = ParseNetUserOutput(output);

                string adminGroupOutput = RunCommand("net", "localgroup Administrators");

                foreach (string user in users)
                {
                    bool isAdmin = adminGroupOutput.Contains(user, StringComparison.OrdinalIgnoreCase);
                    
                    // Detailed query per user
                    string userDetail = RunCommand("net", $"user \"{user}\"");
                    
                    bool active = !userDetail.Contains("Account active              No", StringComparison.OrdinalIgnoreCase);
                    bool passReq = !userDetail.Contains("Password required           No", StringComparison.OrdinalIgnoreCase);

                    ListViewItem item = new ListViewItem(user);
                    item.SubItems.Add(isAdmin ? "Yes (Administrator)" : "No (Standard User)");
                    item.SubItems.Add(active ? "Enabled" : "Disabled / Locked");
                    item.SubItems.Add(passReq ? "Yes" : "No");
                    item.SubItems.Add(isAdmin ? "Full System Access" : "Restricted User");

                    if (!active) item.ForeColor = System.Drawing.Color.Gray;
                    else if (isAdmin) item.ForeColor = System.Drawing.Color.DarkBlue;

                    listUsers.Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error retrieving user accounts: {ex.Message}", "User Management Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private List<string> ParseNetUserOutput(string output)
        {
            List<string> list = new List<string>();
            string[] lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            bool capture = false;

            foreach (var line in lines)
            {
                if (line.StartsWith("-----"))
                {
                    capture = true;
                    continue;
                }
                if (capture && line.Contains("The command completed successfully", StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }
                if (capture)
                {
                    string[] parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var p in parts)
                    {
                        if (!string.IsNullOrWhiteSpace(p))
                            list.Add(p.Trim());
                    }
                }
            }
            return list;
        }

        private void BtnGrantAdmin_Click(object? sender, EventArgs e)
        {
            if (listUsers.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please select a user account first.", "Select User", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string username = listUsers.SelectedItems[0].Text;
            string res = RunCommand("net", $"localgroup Administrators \"{username}\" /add");
            MessageBox.Show($"Grant Admin Privileges Result:\n{res}", "Operation Status", MessageBoxButtons.OK, MessageBoxIcon.Information);
            RefreshUserList();
        }

        private void BtnRevokeAdmin_Click(object? sender, EventArgs e)
        {
            if (listUsers.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please select a user account first.", "Select User", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string username = listUsers.SelectedItems[0].Text;
            string res = RunCommand("net", $"localgroup Administrators \"{username}\" /delete");
            MessageBox.Show($"Revoke Admin Privileges Result:\n{res}", "Operation Status", MessageBoxButtons.OK, MessageBoxIcon.Information);
            RefreshUserList();
        }

        private void BtnRemoveRestrictions_Click(object? sender, EventArgs e)
        {
            if (listUsers.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please select a user account to unlock.", "Select User", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string username = listUsers.SelectedItems[0].Text;

            // Remove password expire, unlock account, enable account, remove restrictions
            string r1 = RunCommand("net", $"user \"{username}\" /active:yes");
            string r2 = RunCommand("net", $"user \"{username}\" /expires:never");
            string r3 = RunCommand("net", $"user \"{username}\" /passwordchg:yes");

            MessageBox.Show($"All Restrictions Removed for '{username}':\n- Account Activated: Yes\n- Account Expiration: Never\n- Password Change Allowed: Yes\n\nExecution Logs:\n{r1}\n{r2}", "Restrictions Lifted", MessageBoxButtons.OK, MessageBoxIcon.Information);
            RefreshUserList();
        }

        private void BtnCreateUser_Click(object? sender, EventArgs e)
        {
            using (Form prompt = new Form())
            {
                prompt.Width = 380;
                prompt.Height = 220;
                prompt.Text = "Create New User Account";
                prompt.StartPosition = FormStartPosition.CenterParent;
                prompt.FormBorderStyle = FormBorderStyle.FixedDialog;
                prompt.MaximizeBox = false;

                Label lblName = new Label { Left = 20, Top = 20, Text = "Username:", AutoSize = true };
                TextBox txtName = new TextBox { Left = 120, Top = 18, Width = 210 };

                Label lblPass = new Label { Left = 20, Top = 60, Text = "Password:", AutoSize = true };
                TextBox txtPass = new TextBox { Left = 120, Top = 58, Width = 210, PasswordChar = '*' };

                CheckBox chkAdmin = new CheckBox { Left = 120, Top = 95, Text = "Make Administrator", AutoSize = true, Checked = true };

                Button btnOk = new Button { Text = "Create", Left = 130, Width = 90, Top = 130, DialogResult = DialogResult.OK };
                Button btnCancel = new Button { Text = "Cancel", Left = 240, Width = 90, Top = 130, DialogResult = DialogResult.Cancel };

                prompt.Controls.AddRange(new Control[] { lblName, txtName, lblPass, txtPass, chkAdmin, btnOk, btnCancel });
                prompt.AcceptButton = btnOk;

                if (prompt.ShowDialog() == DialogResult.OK)
                {
                    string user = txtName.Text.Trim();
                    string pass = txtPass.Text;

                    if (string.IsNullOrEmpty(user))
                    {
                        MessageBox.Show("Username cannot be empty.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    string cmd = string.IsNullOrEmpty(pass) ? $"user \"{user}\" /add" : $"user \"{user}\" \"{pass}\" /add";
                    string res = RunCommand("net", cmd);

                    if (chkAdmin.Checked)
                    {
                        RunCommand("net", $"localgroup Administrators \"{user}\" /add");
                    }

                    MessageBox.Show($"User creation log:\n{res}", "User Created", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    RefreshUserList();
                }
            }
        }

        private void BtnDeleteUser_Click(object? sender, EventArgs e)
        {
            if (listUsers.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please select a user account to delete.", "Select User", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string username = listUsers.SelectedItems[0].Text;

            var confirm = MessageBox.Show($"Are you sure you want to PERMANENTLY DELETE user '{username}'?", "Confirm Deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirm == DialogResult.Yes)
            {
                string res = RunCommand("net", $"user \"{username}\" /delete");
                MessageBox.Show($"Delete result:\n{res}", "Result", MessageBoxButtons.OK, MessageBoxIcon.Information);
                RefreshUserList();
            }
        }

        #endregion

        #region Built-in File Explorer

        private void ComboExplorerDrives_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (comboExplorerDrives.SelectedItem == null) return;
            string drive = comboExplorerDrives.SelectedItem.ToString()!;
            LoadTreeDrive(drive);
        }

        private void LoadTreeDrive(string drivePath)
        {
            treeFolders.Nodes.Clear();
            DirectoryInfo rootDir = new DirectoryInfo(drivePath);
            TreeNode rootNode = new TreeNode(rootDir.Name) { Tag = rootDir.FullName };
            rootNode.Nodes.Add("..."); // Dummy node for expansion
            treeFolders.Nodes.Add(rootNode);
            rootNode.Expand();
        }

        private void TreeFolders_BeforeExpand(object? sender, TreeViewCancelEventArgs e)
        {
            if (e.Node == null || e.Node.Tag == null) return;
            string path = (string)e.Node.Tag;

            e.Node.Nodes.Clear();

            try
            {
                DirectoryInfo dir = new DirectoryInfo(path);
                foreach (DirectoryInfo subDir in dir.GetDirectories())
                {
                    TreeNode node = new TreeNode(subDir.Name) { Tag = subDir.FullName };
                    node.Nodes.Add("...");
                    e.Node.Nodes.Add(node);
                }
            }
            catch { }
        }

        private void TreeFolders_AfterSelect(object? sender, TreeViewEventArgs e)
        {
            if (e.Node == null || e.Node.Tag == null) return;
            string path = (string)e.Node.Tag;
            txtCurrentPath.Text = path;
            LoadDirectoryFiles(path);
        }

        private void LoadDirectoryFiles(string path)
        {
            listFiles.Items.Clear();

            try
            {
                DirectoryInfo dir = new DirectoryInfo(path);

                foreach (DirectoryInfo subDir in dir.GetDirectories())
                {
                    ListViewItem item = new ListViewItem(subDir.Name);
                    item.SubItems.Add("<DIR>");
                    item.SubItems.Add("Folder");
                    item.SubItems.Add(subDir.LastWriteTime.ToString("g"));
                    item.Tag = subDir.FullName;
                    item.ForeColor = System.Drawing.Color.DarkRed;
                    listFiles.Items.Add(item);
                }

                foreach (FileInfo file in dir.GetFiles())
                {
                    ListViewItem item = new ListViewItem(file.Name);
                    item.SubItems.Add(FormatBytes(file.Length));
                    item.SubItems.Add(file.Extension);
                    item.SubItems.Add(file.LastWriteTime.ToString("g"));
                    item.Tag = file.FullName;
                    listFiles.Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to read folder contents: {ex.Message}", "Access Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnExplorerUp_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtCurrentPath.Text)) return;
            DirectoryInfo? parent = Directory.GetParent(txtCurrentPath.Text);
            if (parent != null)
            {
                txtCurrentPath.Text = parent.FullName;
                LoadDirectoryFiles(parent.FullName);
            }
        }

        private void ListFiles_DoubleClick(object? sender, EventArgs e)
        {
            if (listFiles.SelectedItems.Count == 0) return;
            ListViewItem item = listFiles.SelectedItems[0];
            string path = (string)item.Tag;

            if (Directory.Exists(path))
            {
                txtCurrentPath.Text = path;
                LoadDirectoryFiles(path);
            }
            else if (File.Exists(path))
            {
                OpenSelectedFile();
            }
        }

        private void OpenSelectedFile()
        {
            if (listFiles.SelectedItems.Count == 0) return;
            string path = (string)listFiles.SelectedItems[0].Tag;
            try
            {
                Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not open file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnDeleteFile_Click(object? sender, EventArgs e)
        {
            if (listFiles.SelectedItems.Count == 0) return;
            string path = (string)listFiles.SelectedItems[0].Tag;

            var confirm = MessageBox.Show($"Delete '{Path.GetFileName(path)}'?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirm == DialogResult.Yes)
            {
                try
                {
                    if (File.Exists(path)) File.Delete(path);
                    else if (Directory.Exists(path)) Directory.Delete(path, true);

                    LoadDirectoryFiles(txtCurrentPath.Text);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Delete failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnResetPermissions_Click(object? sender, EventArgs e)
        {
            string targetPath = txtCurrentPath.Text;
            if (string.IsNullOrEmpty(targetPath) || (!Directory.Exists(targetPath) && !File.Exists(targetPath)))
            {
                MessageBox.Show("Please select a valid directory or file in the Explorer.", "Invalid Target", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                // Take ownership using takeown & icacls
                string tRes = RunCommand("takeown", $"/F \"{targetPath}\" /R /D Y");
                string iRes = RunCommand("icacls", $"\"{targetPath}\" /grant administrators:F /T /C");

                MessageBox.Show($"Ownership & Full Access Privileges granted to Administrators on:\n{targetPath}\n\nLog:\n{tRes}\n{iRes}", "Permissions Reset Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to take ownership: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region Helper Methods

        private string RunCommand(string command, string args)
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = command,
                    Arguments = args,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process proc = Process.Start(psi)!)
                {
                    string output = proc.StandardOutput.ReadToEnd();
                    string err = proc.StandardError.ReadToEnd();
                    proc.WaitForExit();
                    return output + (string.IsNullOrWhiteSpace(err) ? "" : "\nError: " + err);
                }
            }
            catch (Exception ex)
            {
                return $"Execution Exception: {ex.Message}";
            }
        }

        #endregion
    }
}
