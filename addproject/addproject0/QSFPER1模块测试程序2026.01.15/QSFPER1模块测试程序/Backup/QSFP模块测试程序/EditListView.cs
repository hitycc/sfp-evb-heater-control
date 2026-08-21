using System;
using System.Windows.Forms;
using System.Drawing;

namespace Fibertower_Common
{
    /// <summary>
    /// Summary description for SMK_EditListView.
    /// </summary>
    public class EditListView : ListView
    {
        private ListViewItem lV;
        private int X = 0;
        private int Y = 0;
        private string subItemText;
        private int subItemSelected = 0;
        private System.Windows.Forms.TextBox editBox = new System.Windows.Forms.TextBox();


        public EditListView()
        {
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.list_MouseDown);
            // 
            this.DoubleClick += new System.EventHandler(this.list_DoubleClick);
            editBox.Size = new System.Drawing.Size(0, 0);
            editBox.Location = new System.Drawing.Point(0, 0);
            this.Controls.AddRange(new System.Windows.Forms.Control[] { this.editBox });
            editBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.EditOver);
            editBox.LostFocus += new System.EventHandler(this.FocusOver);
            editBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            editBox.BackColor = Color.White;
            editBox.BorderStyle = BorderStyle.None;
            editBox.Hide();
            editBox.Text = " ";
        }

        private void EditOver(object sender, System.Windows.Forms.KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                lV.SubItems[subItemSelected].Text = editBox.Text;
                editBox.Hide();
            }

            if (e.KeyChar == 27)
                editBox.Hide();
        }

        private void FocusOver(object sender, System.EventArgs e)
        {                   
                lV.SubItems[subItemSelected].Text = editBox.Text;
                editBox.Hide();                 
        }

        private void list_DoubleClick(object sender, EventArgs e)
        {
            // Check the subitem clicked .
            int nStart = X;
            int spos = 0;
            int epos = 0;
            for (int i = 0; i < this.Columns.Count; i++)
            {                
                spos = epos;
                epos += this.Columns[i].Width;
                if (nStart > spos && nStart < epos)
                {
                    subItemSelected = i;
                    break;
                }
            }
            if ((subItemSelected == 2) || (subItemSelected == 3) || (subItemSelected == 4))
            {                  
                subItemText = lV.SubItems[subItemSelected].Text;         
            }
            else
            {
                return;
            }
            string colName = this.Columns[subItemSelected].Text;
            Rectangle r = new Rectangle(spos, lV.Bounds.Y, epos, lV.Bounds.Bottom);
            editBox.Size = new System.Drawing.Size(epos - spos, lV.Bounds.Bottom - lV.Bounds.Top);
            editBox.Location = new System.Drawing.Point(spos, lV.Bounds.Y);
            editBox.Show();
            editBox.Text = subItemText;
            editBox.SelectAll();
            editBox.Focus();
        }

        private void list_MouseDown(object sender, MouseEventArgs e)
        {
            lV = this.GetItemAt(e.X, e.Y);
            X = e.X;
            Y = e.Y;
        }
    }
}
