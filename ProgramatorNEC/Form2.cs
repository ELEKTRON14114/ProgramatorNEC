using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProgramatorNEC
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }
        const int ByFamily = 1;
        const int Manual = 2;
        int SelMethod;

        private void button3_Click(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                tabControl1.SelectedIndex = 1;
            }
            else
            {
                tabControl1.SelectedIndex = 2;
            }
            SelMethod = tabControl1.SelectedIndex;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedIndex = 0;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedIndex = 0;
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            byte FlashSize =0;
            int RAMSize=0;
            String Pakage;
            String Link;

            switch(treeView1.SelectedNode.Tag)
            {
                case "F9026":
                    FlashSize = 16;
                    RAMSize = 512;
                    Pakage = "DIP-42, QFP-44, LQFP-44";
                    Link = "https://www.alldatasheet.com/datasheet-pdf/view/111476/NEC/UPD78F9026A.html";
                    break;
                case "F9046":
                    FlashSize = 16;
                    RAMSize = 512;
                    Pakage = "LQFP-44";
                    Link = "https://www.elenota.pl/datasheet-pdf/29514/NEC/UPD78F9046?sid=1fb1efb00abdb055bd1cd09ff8fcef06";
                    break;
                case "F9116":
                    FlashSize = 16;
                    RAMSize = 256;
                    Pakage = "DIP-28, SOP-30";
                    Link = "https://www.alldatasheet.com/datasheet-pdf/view/1695377/RENESAS/UPD78F9116.html";
                    break;
                case "F9136":
                    FlashSize = 16;
                    RAMSize = 256;
                    Pakage = "DIP-28, SOP-30";
                    Link = "https://www.renesas.com/en/document/dst/upd78f9136-preliminary-product-information";
                    break;
                case "F9156":
                    FlashSize = 16;
                    RAMSize = 256;
                    Pakage = "??";
                    Link = "??";
                    break;
                case "F9177":
                    FlashSize = 24;
                    RAMSize = 512;
                    Pakage = "QFP-44, LQFP-44, TQFP-48";
                    Link = "https://www.alldatasheet.com/datasheet-pdf/view/7116/NEC/UPD78F9177.html";
                    break;
                case "F9801":
                    FlashSize = 16;
                    RAMSize = 256;
                    Pakage = "QFP-44";
                    Link = "??";
                    break;
                case "F9842":
                    FlashSize = 16;
                    RAMSize = 256;
                    Pakage = "LQFP-44";
                    Link = "https://www.renesas.com/en/document/mat/upd789842-subseries-users-manual";
                    break;
                default:
                    FlashSize = 0;
                    RAMSize = 0;
                    Pakage = "---";
                    Link = "";
                    break;
            }
            if ((string)treeView1.SelectedNode.Tag != null)
            {
                label7.Text = "μPD78" + (string)treeView1.SelectedNode.Tag;
                label15.Text = label7.Text; //po prostu przepisz.
                button1.Enabled = true;
                button1.BackColor = Color.FromArgb(0, 192, 192);
            }
            else
            {
                label7.Text = "---";
                label15.Text = "---";
                button1.Enabled = false;
                button1.BackColor = Color.Gray;
            }
            label9.Text = FlashSize.ToString() + " kB";
            label8.Text = RAMSize.ToString() + " B";
            label6.Text = Pakage;
            linkLabel1.Text = Link;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedIndex = 3;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedIndex = SelMethod; //powróć do karty, skąd przybyłeś
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(linkLabel1.Text);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            DialogResult = System.Windows.Forms.DialogResult.OK;
        }
    }
}
