using dnlib.DotNet;
using Embedix.Core;
using Embedix.Core.dnlib.Loading;
using Embedix.GUI.Utility;
using System.Diagnostics;
using System.Text;

namespace Embedix.GUI.View
{
    public partial class FormMain : Form
    {
        private string _file = string.Empty;
        private List<string> _libraries = new List<string>();

        public FormMain()
        {
            InitializeComponent();
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
        }

        private void helpLatestRelease_Click(object sender, EventArgs e)
        {
            OpenUrl("https://github.com/soheil-js/Embedix/releases");
        }

        private void helpSourceCode_Click(object sender, EventArgs e)
        {
            OpenUrl("https://github.com/soheil-js/Embedix");
        }

        private void helpAbout_Click(object sender, EventArgs e)
        {
            string coreVersion = CoreVersion.Get();
            string appVersion = AppVersion.Get();

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Developed by 'Soheil Jashnsaz'");
            sb.AppendLine($"Core Version: v{coreVersion}");
            sb.AppendLine($"App Version: v{appVersion}");

            MessageBox.Show(sb.ToString(), this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using OpenFileDialog open = new OpenFileDialog();
            open.Title = "Choose your file";
            open.Filter = "All File(*.exe, *.dll)|*.exe;*.dll|Execute File(*.exe)|*.exe|Library File(*.dll)|*.dll";
            if (open.ShowDialog() == DialogResult.OK)
            {
                _file = open.FileName;
                txtName.Text = Path.GetFileName(open.FileName);
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            using OpenFileDialog open = new OpenFileDialog();
            open.Title = "Choose your file";
            open.Filter = "Library File(*.dll)|*.dll";
            open.Multiselect = true;
            if (open.ShowDialog() == DialogResult.OK)
            {
                var files = open.FileNames;
                foreach (var file in files)
                {
                    AddToLibraries(file);
                }
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearLibraries();
        }

        private void btnRun_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(_file))
            {

                var automatically = chkAutomatically.Checked;
                var writePdb = chkWritePdb.Checked;

                Task.Run(() => {

                    if (automatically)
                    {
                        ClearLibraries();
                        GetRefs(_file);
                    }

                    if (_libraries.Count > 0)
                    {
                        using AssemblyLoader assembly = new AssemblyLoader(_file);
                        AssemblyContext context = assembly.GetAssemblyContext();
                        Embedder.Execute(context, _libraries.ToArray());
                        string? address = Address(_file);
                        assembly.Write(address, writePdb);

                        if (Path.GetExtension(_file) == ".dll")
                        {
                            var name = Path.GetFileNameWithoutExtension(_file);
                            var dir = Path.GetDirectoryName(_file);
                            var file = Path.Combine(dir!, name);
                            string runtimeconfig = $"{file}.runtimeconfig.json";
                            if (File.Exists(runtimeconfig))
                            {
                                string exe = $"{file}.exe";
                                try
                                {
                                    File.Copy(exe, Address(exe)!);
                                    File.Copy(runtimeconfig, Address(runtimeconfig)!);
                                }
                                catch
                                {
                                }
                            }
                        }

                        Invoke((MethodInvoker)delegate
                        {
                            MessageBox.Show("Assembly embedding completed successfully.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        });
                    }
                    else
                    {
                        Invoke((MethodInvoker)delegate
                        {
                            MessageBox.Show("No libraries selected for embedding.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        });
                    }
                });
            }
            else
                MessageBox.Show("Please select an assembly to process.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void chkAutomatically_CheckedChanged(object sender, EventArgs e)
        {
            ClearLibraries();
            btnAdd.Enabled = !chkAutomatically.Checked;
            btnClear.Enabled = !chkAutomatically.Checked;
        }

        private void txtName_DragDrop(object sender, DragEventArgs e)
        {
            var files = GetDragDropFiles(e);
            if (files.Length > 0)
            {
                string path = files[0].ToString();
                string? extension = Path.GetExtension(path);
                if (!string.IsNullOrWhiteSpace(extension))
                {
                    if (extension == ".exe" || extension == ".dll")
                    {
                        base.Activate();
                        _file = path;
                        txtName.Text = Path.GetFileName(path);
                    }
                }
            }
        }

        private void txtName_DragEnter(object sender, DragEventArgs e)
        {
            SetDragDropEffects(e, true);
        }

        private void lbLibraries_DragDrop(object sender, DragEventArgs e)
        {
            var files = GetDragDropFiles(e);
            foreach (var file in files)
            {
                string path = file.ToString();
                string? extension = Path.GetExtension(path);
                if (!string.IsNullOrWhiteSpace(extension))
                {
                    if (extension == ".dll")
                    {
                        base.Activate();
                        AddToLibraries(path);
                    }
                }
            }
        }

        private void lbLibraries_DragEnter(object sender, DragEventArgs e)
        {
            SetDragDropEffects(e, false);
        }

        private string[] GetDragDropFiles(DragEventArgs e)
        {
            List<string> files = new List<string>();
            try
            {
                Array? array = e.Data?.GetData(DataFormats.FileDrop) as Array;
                if (array != null)
                {
                    foreach (var file in array)
                    {
                        string? path = file?.ToString();

                        if (string.IsNullOrWhiteSpace(path))
                            continue;

                        if (files.Contains(path))
                            continue;

                        files.Add(path);
                    }
                }
            }
            catch
            {
            }
            return files.ToArray();
        }

        private void SetDragDropEffects(DragEventArgs e, bool IsManifestModule = false)
        {
            if (e.Data != null)
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop) && (IsManifestModule || !chkAutomatically.Checked))
                {
                    e.Effect = DragDropEffects.Copy;
                    return;
                }
            }
            e.Effect = DragDropEffects.None;
        }

        private void AddToLibraries(string path)
        {
            _libraries.Add(path);
            Invoke((MethodInvoker)delegate
            {
                lbLibraries.Items.Add(Path.GetFileName(path));
            });
        }

        private void ClearLibraries()
        {
            _libraries.Clear();
            Invoke((MethodInvoker)delegate
            {
                lbLibraries.Items.Clear();
            });
        }

        private string? Address(string path)
        {
            string? directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                string embedixDirectory = Path.Combine(directory, "Embedix");
                Directory.CreateDirectory(embedixDirectory);
                string file = Path.Combine(embedixDirectory, Path.GetFileName(path));

                if (File.Exists(file))
                    File.Delete(file);

                return file;
            }
            return null;
        }

        private void GetRefs(string file)
        {
            using AssemblyLoader assembly = new AssemblyLoader(file);
            string? dir = Path.GetDirectoryName(file);
            var refs = assembly.ModuleDefMD.GetAssemblyRefs();
            foreach (AssemblyRef asmRef in refs)
            {
                if (!string.IsNullOrEmpty(dir))
                {
                    string path = $"{Path.Combine(dir, asmRef.Name)}.dll";
                    if (File.Exists(path) && !_libraries.Contains(path))
                    {
                        AddToLibraries(path);
                        GetRefs(path);
                    }
                }
            }
        }

        public static bool OpenUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return false;

            Uri? uri;
            if (!Uri.TryCreate(url.Trim(), UriKind.Absolute, out uri))
                return false;

            if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
                return false;

            try
            {
                using (Process.Start(new ProcessStartInfo(uri.AbsoluteUri)
                {
                    UseShellExecute = true
                }))
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
