using System.Windows;
using System.Windows.Controls;
using static Nihon.Static.Data;

namespace Nihon.UserControlView;

public partial class ScriptItem : UserControl
{
    public ScriptItem() => this.InitializeComponent();

    public static readonly DependencyProperty ScriptProperty = DependencyProperty.Register(
    "Script", typeof(ScriptObject), typeof(ScriptItem), new PropertyMetadata(default(ScriptObject)));

    public ScriptObject Script
    {
        get => (ScriptObject)this.GetValue(ScriptProperty);

        set
        {
            this.SetValue(ScriptProperty, value);

            this.ExecuteButton.Tag = value.Id;

            this.Title.Text = value.Title;
            this.Description.Text = value.Description;
            this.ScriptType.Text = value.Category;
        }
    }

    public event RoutedEventHandler Executed;

    private void ExecButton_Click(object sender, RoutedEventArgs e) => Executed?.Invoke(this, e);
}
