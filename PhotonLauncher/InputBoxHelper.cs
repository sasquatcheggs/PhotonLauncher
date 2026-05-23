using System.Windows.Forms;

namespace LuxonLauncher
{
    public static class InputBoxHelper
    {
        public static string Show(string title, string prompt, string defaultValue = "")
        {
            var form = new Form
            {
                Text = title,
                Size = new System.Drawing.Size(400, 160),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            };

            var label = new Label
            {
                Text = prompt,
                Location = new System.Drawing.Point(10, 15),
                Size = new System.Drawing.Size(360, 25),
                AutoSize = false
            };

            var textBox = new TextBox
            {
                Text = defaultValue,
                Location = new System.Drawing.Point(10, 45),
                Size = new System.Drawing.Size(360, 25)
            };

            var okButton = new Button
            {
                Text = "OK",
                Location = new System.Drawing.Point(200, 80),
                Size = new System.Drawing.Size(80, 25),
                DialogResult = DialogResult.OK
            };

            var cancelButton = new Button
            {
                Text = "Cancel",
                Location = new System.Drawing.Point(290, 80),
                Size = new System.Drawing.Size(80, 25),
                DialogResult = DialogResult.Cancel
            };

            form.Controls.AddRange(new Control[] { label, textBox, okButton, cancelButton });
            form.AcceptButton = okButton;
            form.CancelButton = cancelButton;

            return form.ShowDialog() == DialogResult.OK ? textBox.Text : defaultValue;
        }
    }
}