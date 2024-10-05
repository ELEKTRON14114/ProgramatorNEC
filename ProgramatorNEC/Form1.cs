using System;
using System.Windows.Forms;
using System.IO.Ports;
using System.Management;
using System.Linq;
using System.Drawing;
using System.Threading;
using FTD2XX_NET;
using System.IO;

namespace ProgramatorNEC
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        //Wszystkie udokumentowane polecenia 78k/0
        const byte CMD_Reset = 0x00;            //polecenie wewnętrznego resetu
        const byte CMD_SetCLK = 0x90;           //Ustawienie f oscylatora
        const byte CMD_SetEraseTime = 0x95;     //Ustawienie czasu kasowania (500ms..20s)
        const byte CMD_SetBaud = 0x9A;          //Ustawienie Baudrate następnej komunikacji
        const byte CMD_Prewrite = 0x48;         //Zamzywanie pamięci. Przygotowanie przed zapisem
        const byte CMD_Erase = 0x20;            //Kasowanie pamięci
        const byte CMD_HsWrite = 0x40;          //HighSpeed write, szybki zapis
        const byte CMD_CsWrite = 0x44;          //Continous write, zapis ciągły
        const byte CMD_InternalVerify = 0x18;   //Wewnętrzna weryfikacja
        const byte CMD_Verify = 0x11;           //veryfikacja całości
        const byte CMD_BlankCheck = 0x30;       //Sprwdzanie, czy pamięć jest pusta
        const byte CMD_GetSignature = 0xC0;     //Pozyskanie sygnatury pliku
        const byte CMD_GetStatus = 0x70;        //Pozyskanie statusu układu (co robi?)
        //inne stałe programowe
        const byte ACK = 0x3C; //ACK od układu

        const byte power = 16;  //C0 - !POWER
        const byte reset = 32;  //C1 - !RESET
        const byte VppHV = 64;  //C2 - Vpp HV 10V
        const byte Vpp5V = 128; //C3 - Vpp 5V

        FTDI Programator = new FTDI(); //nowy programator.
        uint test = 0; //zmienna potrzebna dla FTDI do odbierania i wysyłania
        int MCU_FlashSize = 0;
        byte[] Buffor = new byte[] { }; //bufor zawierający flash

        private void Form1_Load(object sender, EventArgs e)
        {
            AktualizujListePortow();
            Form2 Form2 = new Form2();
            Form2.ShowDialog();
            comboBox3.SelectedIndex = 8; //domyślny sposób komunikacji uart 8 impulsow
            comboBox4.SelectedIndex = 1; //domyślnie negocjuj, pozostaw 9600
            comboBox5.SelectedIndex = 1; //domyślnie 78k/0s
            Xconsole.Text += "NEC PROGRAMATOR by ELEKTRON v 1.0 \r\n";
            Xconsole.Text += "Ustawienia transmisji: domyślne (9600, none, 8, 1)\r\n";
            Xconsole.Text += "Impulsów programujących: " + comboBox3.Items[8].ToString() + "\r\n";
            button1.PerformClick();

        }

        private void button4_Click(object sender, EventArgs e)
        {
            AktualizujListePortow();
        }

        private void AktualizujListePortow()
        {
            comboBox1.Items.Clear();
            UInt32 ftdiDeviceCount = 0;
            Programator.GetNumberOfDevices(ref ftdiDeviceCount);
            if (ftdiDeviceCount > 0)
            {
                comboBox1.Enabled = true;
                FTDI.FT_DEVICE_INFO_NODE[] ftdiDeviceList = new FTDI.FT_DEVICE_INFO_NODE[ftdiDeviceCount];
                Programator.ResetPort();
                Programator.GetDeviceList(ftdiDeviceList);

                for (byte i = 0; i < ftdiDeviceCount; i++)
                {
                    comboBox1.Items.Add("[ " + i.ToString() + " ] " + ftdiDeviceList[i].Description + " | Serial #" + ftdiDeviceList[i].SerialNumber.ToString());
                }
                comboBox1.Text = comboBox1.Items[0].ToString();
            }
            else
            {
                comboBox1.Enabled = false;

                comboBox1.Items.Add("--- NO NEC PROG DETECTED ---");
                comboBox1.SelectedIndex = 0;
            }
        }





        private void button3_Click(object sender, EventArgs e)
        {
            //                         0      1     2    3     4     5     6     7      8     9    10    11    12    13    14    15    16   17     18
            //   byte[] odebrane; = { 0x3C, 0x10, 0x7F, 0x49, 0x7F, 0x7F, 0x80, 0xC4, 0x37, 0x38, 0x46, 0xB9, 0x31, 0x31, 0xB6, 0x20, 0x20, 0x00, 0x3C };
            //powyżej znajduje się testowy pkiet, który powinien zostać odebrany

            byte[] odebrane = new byte[30];

            // Programator.Write(new byte[] { 0xC0 }, 1, ref test);
            if (!SendCMD(CMD_GetSignature))
                return;
            Programator.Read(odebrane, 30, ref test); 
            //Zamiast purge działa lepiej odebranie większej ramki. Było 19, 30 jest spoko
            Xconsole.Text += "Odczytano sygnaturę: ";
            ///////////////Wpisanie całej sygnatury w pole////////////////////
            textBox1.Clear();
            for (byte i = 0; i < 18; i++)
            {
                textBox1.Text += odebrane[i].ToString("X");
                Xconsole.Text += odebrane[i].ToString("X");
            }
            Xconsole.Text += "\r\n";

            /////Uzupełnienie trzech kolejnych okienek
            textBox2.Text = "0x" + odebrane[0].ToString("X2");
            textBox3.Text = "0x" + odebrane[1].ToString("X2");
            textBox4.Text = "0x" + odebrane[2].ToString("X2");

            /////Obliczanie ostatniego adresu na podstawie sygnatury
            int osA = 0;
            osA = ((odebrane[5] & 0x7f) << 16);
            osA |= ((odebrane[4] & 0x7f) << 9);
            osA |= ((odebrane[3] & 0x7f) << 2);
            osA = osA >> 2;
            textBox5.Text = "0x" + osA.ToString("X2"); //zapisz jako hex

            /////obliczanie dostępnej pamięci ROM
            osA += 1; //uwzględnij bajt 0
            MCU_FlashSize = osA; //skopiuj rozmiar pmięci do globalnej zmiennej
            if (osA >= 1024) textBox6.Text = (osA / 1024).ToString() + " kB";
            else textBox6.Text = osA.ToString() + " B";
            String ChipName = "";
            for (byte i = 6; i < 14; i++)
            {
                ChipName += (char)(odebrane[i] & 0x7f);
            }
            textBox7.Text = "P" + ChipName; //µP

            comboBox2.Items.Clear();
            switch (ChipName)
            {
                case "D78F9116":
                    comboBox2.Items.Add("SOP-30");
                    comboBox2.Items.Add("DIP-28");
                    comboBox5.SelectedIndex = 1; //78k0S
                    break;
                case "D78F0134":
                case "D78F0138":
                    comboBox2.Items.Add("LQFP-64");
                    comboBox5.SelectedIndex = 0; //78k0
                    break;

                default:
                    comboBox2.Items.Add("unkown");
                    break;
            }
            comboBox2.SelectedItem = 0;
            comboBox2.Text = comboBox2.Items[0].ToString();
            GetFootprint();
            Xconsole.Text += "Układ : NEC " + textBox7.Text + "  (" + textBox6.Text + ")\r\n";
        }


        void GetFootprint()
        {
            label15.Visible = false;
            switch (comboBox2.SelectedItem.ToString())
            {
                case "SOP-30": pictureBox2.Image = imageList1.Images[0]; break;
                case "DIP-28": pictureBox2.Image = imageList1.Images[1]; break;
                case "LQFP-64": pictureBox2.Image = imageList1.Images[2]; break;
                default: pictureBox2.Image = null; label15.Visible = true; break;
            }
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            GetFootprint();
        }

        private void pictureBox2_DoubleClick(object sender, EventArgs e)
        {
            if (textBox7.Text == "" | comboBox2.Text == "" | comboBox2.Text == "unkown") return;
            Form ZoomDialog = new Form();
            PictureBox Obrazek = new PictureBox();

            ZoomDialog.Width = 500;
            ZoomDialog.Height = 550;
            ZoomDialog.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            ZoomDialog.Text = "Footprint ZOOM";
            ZoomDialog.FormClosed += (Msender, eventArgs) =>
            {
                Enabled = true; //przywróć aktywność głównego okna
                Obrazek.Dispose(); //wyczyść śmietnik
                ZoomDialog.Dispose();
            };
            /////////////////Tworzenie powiększenia/////////////////////
            Obrazek.Parent = ZoomDialog;
            Obrazek.Image = pictureBox2.Image;
            Obrazek.SizeMode = PictureBoxSizeMode.Zoom;
            Obrazek.Top = 0;
            Obrazek.Left = 0;
            Obrazek.Width = 460;
            Obrazek.Height = 460;
            ///////////////////Tworzenie mapy kolorów///////////////////

            Color[] mapaKoloru = { Color.Red, Color.Orange, Color.Green, Color.Lime, Color.Blue, Color.Magenta, Color.Gray };
            String[] nazwyPrzyciskow = { "Vdd", "Vpp", "Xtal", "!RES", "RX", "TX", "GND" };
            String[] opisyPrzyciskow = { "Napięcie zasilania 5V", "Napięcie Vpp (0/5/10V)", "Zewnętrzny oscylator", "Nie reset", "Pin RX mikrokontrolera", "Pin TX mikrokontrolera", "Masa" };
            for (byte i = 0; i < 7; i++)
            {
                Button tempButton = new Button();
                tempButton.Parent = ZoomDialog;
                tempButton.Font = new Font("Arial", 10, FontStyle.Bold);
                tempButton.Width = 50;
                tempButton.Height = 50;
                tempButton.Top = 460;
                tempButton.FlatStyle = FlatStyle.Flat;
                tempButton.Left = 35 + i * 60;
                tempButton.Text = nazwyPrzyciskow[i];
                tempButton.BackColor = mapaKoloru[i];
                toolTip1.SetToolTip(tempButton, opisyPrzyciskow[i]);
                tempButton.Name = "MapButton_" + i.ToString(); //zgredek jest wolny, dostał własną skarpetę
            }

            Enabled = false; //dezaktywuj główne okno
            ZoomDialog.Show();
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Xconsole.Clear();
        }

        private void wklejToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(Xconsole.Text);
        }

        private void kopiujToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(Xconsole.SelectedText);
        }

        private void Xconsole_TextChanged(object sender, EventArgs e)
        {
            Xconsole.SelectionStart = Xconsole.Text.Length;
            Xconsole.ScrollToCaret();
        }


        private void button2_Click_1(object sender, EventArgs e)
        {
            button2.Enabled = false;
            if (Programator.IsOpen)
            {
                Programator.SetBitMode(0, 0x20);//wyłącz wszystko
                Programator.Close();
                Xconsole.Text += ">>>> Rozłączono z programatorem \r\n";
                button2.Text = "POŁĄCZ";
                comboBox1.Enabled = true;
                comboBox3.Enabled = true;
                button2.Enabled = true;
                button3.Enabled = false;
                button11.Enabled = false;
            }
            else
            {
                comboBox1.Enabled = false;
                comboBox3.Enabled = false;
                button3.Enabled = true;
                button11.Enabled = true;
                button2.Text = "ROZŁĄCZ";
                String ProgName;
                Programator.OpenByIndex((uint)comboBox1.SelectedIndex);
                FTDI.FT_STATUS state = Programator.GetDescription(out ProgName);

                if (ProgName != "NEC_Prog" & state == FTDI.FT_STATUS.FT_OK)
                {
                    DialogResult result = MessageBox.Show("Próbujesz się połączyć z urządzeniem, które prawdopodobnie nie jest programatorem. Może to spowodować uszkodzenie tego sprzętu lub jego nieprawidłowe działanie. Czy chcesz kontynuować proces łączenia?", "WARNING!", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
                    if (result == DialogResult.No | result == DialogResult.Cancel)
                    {
                        comboBox1.Enabled = true;
                        comboBox3.Enabled = true;
                        button2.Text = "POŁĄCZ";
                        Programator.Close();
                        Xconsole.Text += ">>>> Anulowano operację połączenia z " + comboBox1.Text + " \r\n";
                        button2.Enabled = true;
                        button3.Enabled = false;
                        return;
                    }
                }
                //Programator.SetBitMode(power, 0x20);//włącz zaslanie

                ////////konfigurowanie portu szeregowego//////////
                Programator.SetBaudRate(9600);
                Programator.SetDataCharacteristics(FTDI.FT_DATA_BITS.FT_BITS_8, FTDI.FT_STOP_BITS.FT_STOP_BITS_1, FTDI.FT_PARITY.FT_PARITY_NONE);
                Programator.SetFlowControl(FTDI.FT_FLOW_CONTROL.FT_FLOW_NONE, 0, 0);
                Programator.SetTimeouts(200, 20000);

                /*                Xconsole.Text += ">>>> Próba nawiązania połączenia z układem zakończona niepowodzeniem!\r\n";
                                button2.Enabled = true;
                                button3.Enabled = false;
                                comboBox1.Enabled = true;
                                comboBox3.Enabled = true;
                                button2.Text = "POŁĄCZ";*/

            }
            button2.Enabled = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(MCU_FlashSize ==0)
            {
                MessageBox.Show("Nieznany rozmiar pamięci. Odcyztaj sygnaturę!", "INFO", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            if (openFileDialog1.ShowDialog() != DialogResult.OK | openFileDialog1.FileName == "")
                return;

            //////////////////////TWORZENIE TABELI NA WYMIAR//////////////////
            CreateMemoTable(MCU_FlashSize);

            ///załadowanie pliku do komórek
            LoadBINFile(openFileDialog1.FileName, MCU_FlashSize);
        }


        void LoadBINFile(String filePath, int OstatniAdres)
        {
            Array.Clear(Buffor, 0, Buffor.Length); //wyczyść bufor przed wczytaniem
            Buffor = File.ReadAllBytes(filePath); //załaduj bufor zawartością pliku
            FileStream s2 = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None); //blokowanie pliku przed nadpisaniem
           
            for (int y = 0; y < (0x3FFF / 16) + 1; y++) //OstatniAdres
                for (int x = 0; x < 16; x++)
                {
                    if (x + (y * 16) <= Buffor.Length)
                    {
                        byte znakPliku = Buffor[x + (y * 16)];  //pobierz z tabilicy pojedynczy bajt
                        DGV[x, y].Value = znakPliku.ToString("X2");
                        if (znakPliku == 255 & checkBox5.Checked) DGV[x, y].Style.ForeColor = Color.LightGray;
                        else DGV[x, y].Style.ForeColor = Color.Black;
                        if (znakPliku > 32 & znakPliku < 127) //jeśli jest to drukowalny znak, to go po prostu zapisz
                            DGV[16, y].Value += "" + System.Convert.ToChar(System.Convert.ToUInt32(DGV[x, y].Value.ToString(), 16));
                        else
                            DGV[16, y].Value += "."; //w przeciwnym razie zamień go na kropkę
                    }
                    else
                    {
                        DGV[x, y].Value = "FF"; //wypełnij puste miejsca FFami
                        DGV[16, y].Value += ".";
                    }
                }
        }

        void LoadHexFile(String filePath)
        {

        }

        private void button8_Click(object sender, EventArgs e)
        {
            if (!SendCMD(CMD_BlankCheck))
                return;
            Xconsole.Text += " > Sprawdzanie, czy urządzenie jest puste \r\n";

            Thread.Sleep(2000); //czas na sprawdzenie czy urządzenie jest puste
            switch (ReceiveStatus())
            {
                case 0x00:
                    MessageBox.Show("Urządzenie JEST puste", "INFO", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
                case 0x01:
                    MessageBox.Show("Urządzenie NIE jest puste", "INFO", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    break;
                case 0x10:
                    Xconsole.Text += "Trwa czyszczenie";
                    break;
                case 0x08:
                    MessageBox.Show("Błąd czyszczenia!", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
            }
        }

        void CreateMemoTable(int LastAdres)
        {

            DGV.Columns.Clear();
            DGV.Rows.Clear();
            DGV.RowTemplate.Height = 16; //tak mała wysokość ukrywa ikony (czarna strzałka)
            DGV.DefaultCellStyle.Font = new Font("Lucida Console", 9);
            for (byte column = 0; column < 16; column++)
            {
                DGV.Columns.Add(column.ToString(), column.ToString("X"));
                DGV.Columns[column].Width = 25;
                DGV.Columns[column].SortMode = DataGridViewColumnSortMode.NotSortable;
            }
            DGV.Columns.Add("ASCII", "ASCII"); //stworzenie kolumny ASCII
            DGV.Columns[16].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            DGV.Columns[16].SortMode = DataGridViewColumnSortMode.NotSortable;
            DGV.Rows.Add((LastAdres / 16)); //dodaj odpowiednią ilość wierszy
            DGV.RowHeadersWidth = 80; //szerokość pól adresowych
            for (int rows = 0; rows < (LastAdres / 16); rows++)
            {
                DGV.Rows[rows].HeaderCell.Value = "0000-" + (rows * 16).ToString("X4") + ":";
            }

            /////kolorowanie bloków
            if (checkBox1.Checked)
                try
                {

                    for (int y = 0; y < (LastAdres / 16) + 1; y++)
                        for (int x = 0; x < 16; x++)
                        {
                            int AdresKomorki = x + (y * 16);
                            int Start1 = int.Parse(textBox14.Text, System.Globalization.NumberStyles.HexNumber);
                            int End1 = int.Parse(textBox17.Text, System.Globalization.NumberStyles.HexNumber);

                            int Start2 = int.Parse(textBox13.Text, System.Globalization.NumberStyles.HexNumber);
                            int End2 = int.Parse(textBox16.Text, System.Globalization.NumberStyles.HexNumber);

                            int Start3 = int.Parse(textBox12.Text, System.Globalization.NumberStyles.HexNumber);
                            int End3 = int.Parse(textBox15.Text, System.Globalization.NumberStyles.HexNumber);

                            int Start4 = int.Parse(textBox19.Text, System.Globalization.NumberStyles.HexNumber);
                            int End4 = int.Parse(textBox18.Text, System.Globalization.NumberStyles.HexNumber);

                            if (AdresKomorki >= Start1 & AdresKomorki <= End1)
                            {
                                DGV[x, y].Style.BackColor = textBox14.BackColor;
                            }
                            if (AdresKomorki >= Start2 & AdresKomorki <= End2)
                            {
                                DGV[x, y].Style.BackColor = textBox13.BackColor;
                            }
                            if (AdresKomorki >= Start3 & AdresKomorki <= End3)
                            {
                                DGV[x, y].Style.BackColor = textBox12.BackColor;
                            }
                            if (AdresKomorki >= Start4 & AdresKomorki <= End4)
                            {
                                DGV[x, y].Style.BackColor = textBox19.BackColor;
                            }
                        }
                }
                catch
                {

                }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (!initMCU()) //ustaw clk i  czas kasowania. jeśli nie zakończy się sukcesem, przerwij operację
                return;
            DialogResult result = MessageBox.Show("Kasownie pamięci FLASH. Czy chcesz kontynuować?", "WARNING!", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
            if (result == DialogResult.No | result == DialogResult.Cancel) return;
            if (checkBox3.Checked)
            {
                if (!SendCMD(CMD_Prewrite))//prewrite
                    return;
                //Thread.Sleep(2000); //odczekanie na skasowanie całości
                for (int i = 0; i < 100; i++)
                {
                    Thread.Sleep(100);
                    byte status = ReceiveStatus();
                    if (status != 0x00 & status != 0x40) return;
                   // if (status==0x40) Xconsole.Text += ".";
                }
                Xconsole.Text += "\r\n"; //nowa linijka po znakach zamazywania
            }
            Xconsole.Text += " > Właściwe kasowanie\r\n";
            SendCMD(CMD_Erase); //erase
            for (byte i = 0; i < 50; i++)
            {
                Thread.Sleep((int)EraseTime.Value / 50); //odczekanie na skasowanie całości
                Xconsole.Text += ".";
            }
            Xconsole.Text += "\r\n";
            for (int x = 0; x < 200; x++)
            {
                Thread.Sleep(100);
                byte status = ReceiveStatus();
                switch (status)
                {
                    case 0x00:
                        Xconsole.Text += " > Zakończono Czyszczenie\r\n";
                        return;
                    case 0x01:
                        MessageBox.Show("Nie udało się wymazać flash!", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
/*                    case 0x80:
                        Xconsole.Text += ".";
                        break;*/
                    default:
                        Xconsole.Text += status.ToString("X2") + " \r\n";
                        break;
                }

            }
        }

        byte ReceiveStatus()
        {
            byte[] odebrane = new byte[3];
            Programator.Write(new byte[] { CMD_GetStatus }, 1, ref test);
            Programator.Read(odebrane, 3, ref test);
            if (odebrane[0] == ACK & odebrane[2] == ACK)
            { 
                switch(odebrane[1])
                {
                    case 0x00:
               //         Xconsole.Text += "OK\r\n";
                        break;
                    case 0x01: // Blank check error
                        Xconsole.Text += "!> Blank check error\r\n";
                        break;
                    case 0x02: // Verify error
                        Xconsole.Text += "!> Verify error\r\n";
                        break;
                    case 0x04: // Write failed
                        Xconsole.Text += "!>  Write failed\r\n";
                        break;
                    case 0x08: //  Erase failed
                        Xconsole.Text += "!> Erase failed\r\n";
                        break;
                    case 0x80: //  erase mode
                        Xconsole.Text += "E";
                        break;
                    case 0x40: //  Unexpected error
                        Xconsole.Text += "W";
                        break;
                    case 0x20: //  Unexpected error
                        Xconsole.Text += "V";
                        break;
                    case 0x10: //  Unexpected error
                        Xconsole.Text += "B";
                        break;
                    default:
                        Xconsole.Text += "?> Status 0x" + odebrane[1].ToString("X2") + "\r\n";
                        break;
                }
                return odebrane[1];
            }
            else
            {
                Xconsole.Text += "!> Błąd w komunikacij: Niepoprawny pakiet statusu! ";
                Xconsole.Text += odebrane[0].ToString("X2") + odebrane[1].ToString("X2") + odebrane[2].ToString("X2") + "\r\n";
                return 255;
            }
        }
        bool SendCMD(byte CMD)
        {
            byte[] tmp = new byte[1];
            byte[] odebrane = new byte[1];
            tmp[0] = CMD; //zapakuj bajt w pakiet 1-bajtowy (masło maślane)
            Programator.Write(tmp, 1, ref test);
            Programator.Read(odebrane, 1, ref test);
            if (odebrane[0] == ACK) return true; //jeśli odbierzesz ACK daj o tym znać
            else
            {
                Xconsole.Text += "!> Brak ACK (0x" + CMD.ToString("X2") + " : 0x" + odebrane[0].ToString("X2") + ")\r\n";
                return false;
            }
        }
        bool SendParam(byte B1, byte B2, byte B3, byte B4)
        {
            byte[] tmp = new byte[4];
            byte[] odebrane = new byte[1];
            tmp[0] = B1; //zapakuj bajty w pakiet
            tmp[1] = B2;
            tmp[2] = B3;
            tmp[3] = B4;
            Programator.Write(tmp, 4, ref test);
            Programator.Read(odebrane, 1, ref test);
            if (odebrane[0] == ACK) return true; //jeśli odbierzesz ACK daj o tym znać
            else
            {
                Xconsole.Text += "!> Brak ACK (0x" + tmp[0].ToString("X2") + tmp[1].ToString("X2") + tmp[2].ToString("X2") + tmp[3].ToString("X2") + " : 0x" + odebrane[0].ToString("X2") + ")\r\n";
                return false;
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            initMCU(); //ustaw clk i czas kasowania
            int ileTego = 0x3FFF;//(2 * 128)-1; //ile bajtów do zaprogramowania
            DialogResult result = MessageBox.Show("Zapis bufora do pamięci FLASH. Czy chcesz kontynuować?", "WARNING!", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
            if (result == DialogResult.No | result == DialogResult.Cancel) return;

            Byte[] Packet = new byte[128 + 4]; //rozmiar pakietu danych + dane kodujące
                                            // for (int z = 0; z < (ileTego/128)+1; z++)
            for (int z = 0; z < (ileTego / 128) + 1; z++)
            {
                Packet[0] = (byte)(((z * 128) >> 16) & 0xff);
                Packet[1] = (byte)(((z * 128) >> 8) & 0xff);
                Packet[2] = (byte)((z * 128) & 0xff);
                Packet[3] = 0x80; // 0x00=256 bajtów, 0x80 = 128 bajtów, 0xFF = 1 Bajt
                Array.Copy(Buffor, z * 128, Packet, 4, 128); //skopiuj dane do pakietu na miejsce od 4

                if (!SendCMD(CMD_HsWrite)) //Polecenie zapisu, nagłówek
                   return;
                Programator.Write(Packet, 128 + 4, ref test); //wysyłanie całego pakietu kodującego oraz danych
    //            Xconsole.Text += Packet[128].ToString("X2") + Packet[128].ToString("X2") + Packet[128+1].ToString("X2") + Packet[128+2].ToString("X2") + Packet[128+3].ToString("X2") + "\r\n";
                byte[] odebrane = new byte[1];
                Programator.Read(odebrane, 1, ref test);
                progressBar1.Maximum = 128; //0x3FFF;
                for (int x = 0; x < 100; x++)
                {
                    byte status = ReceiveStatus();
                    switch (status)
                    {
                        case 0x00:
                            Xconsole.Text += " > Przesłano pomyślnie pakiet " + (z + 1).ToString() + " z " + ((ileTego / 128) + 1).ToString() + "\r\n";
                            x = 100; //nietypowe wyjście z pętli
                            progressBar1.Value = z+1;
                            break;
                        case 0x04:
                            return;
                        default:
                         //   Xconsole.Text += status.ToString("X2") + " \r\n";
                            break;
                    }

                }
            }
            Xconsole.Text += " > Wewnętrzna weryfikacja...\r\n";
            if (!SendCMD(CMD_InternalVerify))//internal Verify
                return;
            Thread.Sleep(1000);
            for (int i = 0; i < 10; i++)
            {
                Thread.Sleep(1000);
                byte status = ReceiveStatus();
                switch (status)
                {
                    case 0x02:
                        MessageBox.Show("Wewnętrzna weryfikacja nie powiodła się!", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        progressBar1.Value = 0;
                        return;
                    case 0x00:
                        progressBar1.Value = progressBar1.Maximum;
                        Xconsole.Text += " > Programowanie zakończone sukcesem!\r\n";
                        MessageBox.Show("Programowanie zakończone sukcesem!", "INFO", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        progressBar1.Value = 0;
                        i = 200; //nietypowe wyjście z pętli
                        break;
                    default:
                        Xconsole.Text += status.ToString("X2") + " \r\n";
                        break;

                }
            }
            //  Xconsole.Text += " > Programowanie zakończone sukcesem!";
            //  MessageBox.Show("Programowanie zakończone sukcesem!", "INFO", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
                Programator.SetBitMode(power, 0x20);//włącz zaslanie
            else
                Programator.SetBitMode(0, 0x20);//wyłącz zaslanie
        }

        private void button10_Click(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                Programator.SetBitMode(power + reset, 0x20);//reset + zasilanie
                Programator.SetBitMode(power, 0x20);//reset + zasilanie
            }
            else
            {
                //  Programator.SetBitMode(0, 0x20);//wyłącz zaslanie
            }
        }

        private void button11_Click(object sender, EventArgs e)
        {
            //button2.Enabled = false;
            if (!Programator.IsOpen)
            {
                Programator.SetBitMode(0, 0x20);//wyłącz wszystko
                Programator.Close();
                Xconsole.Text += ">>>> Rozłączono z układem \r\n";
                return;
            }
            else
            {
                Programator.SetBitMode(power, 0x20);//włącz zaslanie
                Programator.SetBitMode(power + reset, 0x20);//!power, reset
                Programator.SetBitMode(VppHV + reset + power, 0x20); //vpp 10v, !reset, !power
                Programator.SetBitMode(VppHV + power, 0x20); //wyłącz reset

                byte impulsow = ((byte)comboBox3.SelectedIndex);
                for (byte i = 0; i < impulsow; i++) //wysyłanie impulsów vpp
                {
                    Programator.SetBitMode(Vpp5V + power, 0x20); //vpp = 5v
                    Programator.SetBitMode(VppHV + power, 0x20); //vpp = 10v
                }
                for (byte i = 0; i < 16; i++)
                {
                    Thread.Sleep(5);
                    if (SendCMD(CMD_Reset)) //jeśli odbierzesz ACK
                    {
                        Xconsole.Text += ">>>> Nawiązano połączenie z układem. Synchronizacja po: " + (i + 1).ToString() + " próbach. \n\r";
                     //   Clipboard.SetImage(imageList2.Images[1]);
                       // Xconsole.Paste();
                        return;
                    }
                }
                Xconsole.Text += ">>>> Próba nawiązania połączenia z układem zakończona niepowodzeniem!\r\n";
            }
        }

        bool initMCU() //przygotowanie scalaka do zapisu, kasowania itp
        {
            if (!SendCMD(CMD_SetCLK))//Polecenie ustawianie zegara
                return false;

            if (!SendParam(0x05, 0x00, 0x00, 0x04)) //5.000MHz
                return false;

            Xconsole.Text += " > Ustawiono zegar! ( f_CLK=5MHz)\r\n";

            if (!SendCMD(CMD_SetEraseTime)) //ustawianie czasu kasowania
                return false;
            byte mnoznik;
            if (EraseTime.Value < 1000) mnoznik = 0;
            if (EraseTime.Value < 10000) mnoznik = 1;
            else mnoznik = 2;
            byte d1, d2, d3;
            byte.TryParse(EraseTime.Value.ToString()[0] + "", out d1); //setki
            byte.TryParse(EraseTime.Value.ToString()[1] + "", out d2); //dziesiątki
            byte.TryParse(EraseTime.Value.ToString()[2] + "", out d3); //jedności

            if (!SendParam(d1, d2, d3, mnoznik)) 
                return false;

            Xconsole.Text += " > Ustawiono czas kasowania! ( t_ERASE = " + d1.ToString() + d2.ToString() + d3.ToString() + " x10^" + mnoznik.ToString() + " ) \r\n";
            
            if(checkBox4.Checked)
            {
                if (!SendCMD(CMD_SetBaud)) //ustawianie czasu kasowania
                    return false;
                byte baudrateCode = 3; //domyślny 9600
                switch (comboBox4.SelectedIndex)
                {
                    case 0://4,800 bps
                        baudrateCode = 2;
                        break;
                    case 1://19,200 bps
                        baudrateCode = 4;
                        break;
                    case 2://31,250 bps
                        baudrateCode = 5;
                        break;
                    case 3://38,400 bps
                        baudrateCode = 6;
                        break;
                    case 4://76,800 bps
                        baudrateCode = 7;
                        break;
                }
                if (!SendCMD(baudrateCode)) //zarządaj nowej prędkości
                    return false;
                Thread.Sleep(1000);
                Programator.SetBaudRate(Convert.ToUInt32(comboBox4.SelectedItem)); //zmień systemowy baudrate
                for (byte i = 0; i < 10; i++)
                {
                    Thread.Sleep(100);
                    if (SendCMD(CMD_Reset))
                    {
                        Xconsole.Text += " > Wynegocjonowano nowy baudrate: " + comboBox4.SelectedItem.ToString() + "\r\n";
                        return true;
                    }
                    Xconsole.Text += "!> Nie udało się Wynegocjować nowego baudrate: " + comboBox4.SelectedItem.ToString() + "\r\n";
                    Programator.SetBaudRate(9600);
                }
            }
            
            return true; //zakończona sukcesem
        }

        private void button9_Click(object sender, EventArgs e)
        {
            if (!SendCMD(CMD_Reset)) //Polecenie resetu
                return;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            Xconsole.Text += " > Weryfikowanie zawartości układu\r\n";
            int ileTego = 0x3FFF;

            Byte[] Packet = new byte[128]; //rozmiar pakietu danych
            byte[] odebrane = new byte[1];

            if (!SendCMD(CMD_Verify)) //Polecenie weryfikacji, nagłówek
                return;

            for (int z = 0; z < (ileTego / 128) + 1; z++)
            {
                Array.Copy(Buffor, z * 128, Packet, 0, 128); //skopiuj dane do pakietu


                Programator.Write(Packet, 128, ref test); //wysyłanie całego pakietu kodującego oraz danych
            //    Thread.Sleep(trackBar1.Value); //odczekanie na zaprogramowanie
                Xconsole.Text += " > Przesłano pakiet " + (z + 1).ToString() + " z " + ((ileTego / 128) + 1).ToString() + "\r\n";
            }
                Programator.Read(odebrane, 1, ref test);
                if (odebrane[0] != ACK)
                 return;
            Xconsole.Text += " > Przesłano pomyślnie pakiety \r\n";
            Thread.Sleep((int)EraseTime.Value); //odczekanie na zaprogramowanie
            byte status = ReceiveStatus();
                    switch (status)
                    {
                        case 0x00:
                            Xconsole.Text += " > pakiet OK \r\n";
                            break;
                        case 0x02:
                            Xconsole.Text += "!> Weryfikacja zakończona niepowodzeniem\r	\n"; break;
                        //                        return;
                        default:
                            Xconsole.Text += status.ToString("X2") + " \r\n";
                            break;
                    }
        }

        private void button12_Click(object sender, EventArgs e)
        {
            initMCU(); //ustaw clk i czas kasowania
            int ileTego = 0x3FFF;
            Byte[] Packet = new byte[1 + 4]; //rozmiar pakietu danych + dane kodujące
            for (int z = 0x08; z < ileTego + 1; z++)
            {
                Packet[0] = (byte)((z >> 16) & 0xff);
                Packet[1] = (byte)((z >> 8) & 0xff);
                Packet[2] = (byte)(z & 0xff);
                Packet[3] = 0xFF; // 0x00=256 bajtów, 0x80 = 128 bajtów, 0xFF = 1 Bajt
                Packet[4] = 0xFF; //pusty bajt sondujący

                if (!SendCMD(CMD_HsWrite)) //Polecenie zapisu, nagłówek
                    return;
                Programator.Write(Packet, 5, ref test); //wysyłanie całego pakietu kodującego oraz danych
                                                        //            Xconsole.Text += Packet[128].ToString("X2") + Packet[128].ToString("X2") + Packet[128+1].ToString("X2") + Packet[128+2].ToString("X2") + Packet[128+3].ToString("X2") + "\r\n";
                byte[] odebrane = new byte[20];
                Programator.Read(odebrane, 1, ref test);
                for (byte i = 0; i < 20; i++)
                {
                    Xconsole.Text += odebrane[i].ToString("X2") + " ";
                }
                Xconsole.Text += "\r\n";

              //  byte status = ReceiveStatus();
             //   Xconsole.Text += status.ToString("X2") + "\r\n";
            }
            Xconsole.Text += " > Wewnętrzna weryfikacja...\r\n";
            if (!SendCMD(CMD_InternalVerify))//internal Verify
                return;
            Thread.Sleep(1000);
            for (int i = 0; i < 10; i++)
            {
                Thread.Sleep(1000);
                byte status = ReceiveStatus();
                switch (status)
                {
                    case 0x02:
                        MessageBox.Show("Wewnętrzna weryfikacja nie powiodła się!", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        progressBar1.Value = 0;
                        return;
                    case 0x00:
                        progressBar1.Value = progressBar1.Maximum;
                        Xconsole.Text += " > Programowanie zakończone sukcesem!\r\n";
                        MessageBox.Show("Programowanie zakończone sukcesem!", "INFO", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        progressBar1.Value = 0;
                        i = 200; //nietypowe wyjście z pętli
                        break;
                    default:
                        Xconsole.Text += status.ToString("X2") + " \r\n";
                        break;

                }
            }
            //  Xconsole.Text += " > Programowanie zakończone sukcesem!";
            //  MessageBox.Show("Programowanie zakończone sukcesem!", "INFO", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void pictureBox3_DoubleClick(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.youtube.com/channel/UCCaZRkpVItUdJfTs9rtbFPg");
        }
    }
}