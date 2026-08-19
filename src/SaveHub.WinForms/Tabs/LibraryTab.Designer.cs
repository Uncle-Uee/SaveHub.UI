namespace SaveHub.WinForms.Tabs;

partial class LibraryTab
{
    private System.ComponentModel.IContainer components = null;

    private Button _btnRefresh;
    private Button _btnRebuild;
    private Label _lblSummary;
    private TreeView _tree;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            components?.Dispose();
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        _btnRefresh = new Button();
        _btnRebuild = new Button();
        _lblSummary = new Label();
        _tree = new TreeView();
        SuspendLayout();
        //
        // _btnRefresh
        //
        _btnRefresh.Text = "Refresh";
        _btnRefresh.Location = new Point(12, 12);
        _btnRefresh.Size = new Size(90, 28);
        _btnRefresh.Click += Library_Refresh;
        //
        // _btnRebuild
        //
        _btnRebuild.Text = "Rebuild Index";
        _btnRebuild.Location = new Point(108, 12);
        _btnRebuild.Size = new Size(110, 28);
        _btnRebuild.Click += Library_Rebuild;
        //
        // _lblSummary
        //
        _lblSummary.Text = "No library loaded yet.";
        _lblSummary.Location = new Point(228, 18);
        _lblSummary.AutoSize = true;
        //
        // _tree
        //
        _tree.Location = new Point(12, 48);
        _tree.Size = new Size(760, 630);
        _tree.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        //
        // LibraryTab
        //
        Controls.Add(_btnRefresh);
        Controls.Add(_btnRebuild);
        Controls.Add(_lblSummary);
        Controls.Add(_tree);
        Size = new Size(784, 690);
        ResumeLayout(false);
        PerformLayout();
    }
}
