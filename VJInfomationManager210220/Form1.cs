using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Text.RegularExpressions;
using MySql.Data.MySqlClient;
using System.Globalization;

namespace VJInfomationManager210220
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            LoadOptions();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            SaveOptions();
            InputData.Server = this.textBox5.Text;
            InputData.Database = this.textBox6.Text;
            InputData.UID = this.textBox7.Text;
            InputData.Password = this.textBox8.Text;
            new DataServer().CreateTable();
            OpenFileDialog open = new OpenFileDialog();
            open.Filter = "txt file|*.txt";
            open.ShowDialog();
            string[] inputlist = new string[0];
            try
            {
                inputlist = File.ReadAllLines(@open.FileName);
            }
            catch
            {
                MessageBox.Show("File lỗi!");
            }
            int currentthread = 1;
            int Success = 0;
            int Failed = 0;
            Thread t1 = new Thread(delegate ()
            {
                for (int a = 0; a < inputlist.Length; a++)
                {
                    try
                    {
                        Invoke((MethodInvoker)(() =>
                        {
                            this.button1.Text = (a + 1).ToString() + "/" + inputlist.Length.ToString() + " Success:" + Success.ToString() + " Failed:" + Failed;
                            Application.DoEvents();
                        }));
                        while (true)
                        {
                            if (currentthread < InputData.MaxThread)
                                break;
                            Thread.Sleep(250);
                        }
                        currentthread++;
                        string[] currentinfo = inputlist[a].Split('|');
                        string flightcode = currentinfo[0];
                        string firstname = currentinfo[1].Split(',')[0].Replace(" ", "");
                        string lastname = "";
                        string[] handlelastname = currentinfo[1].Split(',')[1].Split(' ');
                        for (int b = 0; b < handlelastname.Length; b++)
                        {
                            handlelastname[b]=handlelastname[b].Replace(" ", "");
                            if (handlelastname[b]!=null && handlelastname[b]!="")
                            {
                                lastname += handlelastname[b];
                                if (b + 1 != handlelastname.Length)
                                    lastname += " ";
                            }
                        }

                        MatchCollection coll = Regex.Matches(currentinfo[2], @"(\d{2}/\d{2}/\d{4}) ([A-Z]{1,3} - [A-Z]{1,3}) ([A-Z]{1,5})");
                        string dateflight1 = coll[0].Groups[1].Value;
                        string flight1 = coll[0].Groups[2].Value;
                        string flightcode1= coll[0].Groups[3].Value;

                        string dateflight2 = "";
                        string flight2 = "";
                        string flightcode2 = "";
                        string seats = "";
                        try
                        {
                            coll = Regex.Matches(currentinfo[3], @"(\d{2}/\d{2}/\d{4}) ([A-Z]{1,3} - [A-Z]{1,3}) ([A-Z]{1,5})");
                            dateflight2 = coll[0].Groups[1].Value;
                            flight2 = coll[0].Groups[2].Value;
                            flightcode2 = coll[0].Groups[3].Value;
                            seats = currentinfo[4];
                        }
                        catch
                        {
                            seats = currentinfo[3];
                        }
                        Thread t2 = new Thread(delegate ()
                        {
                            try
                            {
                                int lastinsertedid = new DataServer().InsertNewData(flightcode, firstname, lastname, dateflight1, flight1, flightcode1, dateflight2, flight2, flightcode2, seats);
                                Success++;
                            }
                            catch { Failed++; }
                            currentthread--;
                        });
                        t2.Start();
                    }
                    catch { }
                }
                Invoke((MethodInvoker)(() =>
                {
                    this.button1.Text = "Hoàn thành Success:" + Success.ToString() + " Failed:" + Failed;
                    Application.DoEvents();
                }));
                //Thread.Sleep(1000);
                //Invoke((MethodInvoker)(() =>
                //{
                //    this.button1.Text = "Nhập dữ liệu vào database";
                //    Application.DoEvents();
                //}));
            });
            t1.Start();

        }
        List<VietJetInfomation> ListVJIFiltered;
        private void Button2_Click(object sender, EventArgs e)
        {
            SaveOptions();
            dataGridView1.Rows.Clear();
            List<VietJetInfomation> ListAllVJI = new DataServer().LoadAllData();
            List<VietJetInfomation2> ListAllVJI2 = new DataServer().LoadAllDataVJ2();
            ListVJIFiltered = new List<VietJetInfomation>();
            MessageBox.Show(ListAllVJI.Count.ToString() + " " + ListAllVJI2.Count.ToString());
            //var VJIJoin = ListAllVJI.Join(ListAllVJI2, arg => arg.CustomerIsCode, arg => arg.CustomerIsCode, (first, second) => new { CustomerIsCode = first.CustomerIsCode, FirstName = first.FirstName, LastName = first.LastName, DateFlight1t1 = first.DateFlight1t1, Flight1 = first.Flight1, Verify1 = first.Verify1, DateFlight2t1 = first.DateFlight2t1, Flight2 = first.Flight2, Verify2 = first.Verify2, Seats = first.Seats, DateFlight1t2 = second.DateFlight1t2, FlightCode1 = second.FlightCode1, DateFlight2t2 = second.DateFlight2t2, FlightCode2 = second.FlightCode2, Email = second.Email, EmailStandardizedSuccess = second.EmailStandardizedSuccess, EmailStandardized = second.EmailStandardized, Phone = second.Phone, PhoneStandardizedSuccess = second.PhoneStandardizedSuccess, PhoneStandardized = second.PhoneStandardized, PhoneNetwork = second.PhoneNetwork, Confirm = second.Confirm, PaymentStatus = second.PaymentStatus });
            for (int a = 0; a < ListAllVJI2.Count; a++)
            {
                int index = ListAllVJI.IndexOf(ListAllVJI.Where(p => p.CustomerIsCode == ListAllVJI2[a].CustomerIsCode).FirstOrDefault());
                MessageBox.Show(index.ToString() + " " + a.ToString());
                if (index < 0)
                    continue;
                ListAllVJI[index].DateFlight1t2 = ListAllVJI2[a].DateFlight1t2;
                ListAllVJI[index].FlightCode1 = ListAllVJI2[a].FlightCode1;
                ListAllVJI[index].DateFlight2t2 = ListAllVJI2[a].DateFlight2t2;
                ListAllVJI[index].FlightCode2 = ListAllVJI2[a].FlightCode2;
                ListAllVJI[index].Email = ListAllVJI2[a].Email;
                ListAllVJI[index].EmailStandardizedSuccess = ListAllVJI2[a].EmailStandardizedSuccess;
                ListAllVJI[index].EmailStandardized = ListAllVJI2[a].EmailStandardized;
                ListAllVJI[index].Phone = ListAllVJI2[a].Phone;
                ListAllVJI[index].PhoneStandardizedSuccess = ListAllVJI2[a].PhoneStandardizedSuccess;
                ListAllVJI[index].PhoneStandardized = ListAllVJI2[a].PhoneStandardized;
                ListAllVJI[index].PhoneNetwork = ListAllVJI2[a].PhoneNetwork;
                ListAllVJI[index].Confirm = ListAllVJI2[a].Confirm;
                ListAllVJI[index].PaymentStatus = ListAllVJI2[a].PaymentStatus;
            }
            //MessageBox.Show(VJIJoin.Count().ToString());
            bool FirstNameCountryVietNam = checkBox1.Checked;
            string[] ListFirstNameCountryVietNam = File.ReadAllLines(Application.StartupPath + "//firstnamevn.txt");


            bool FirstNameOtherCountry = checkBox2.Checked;
            bool TimeLineChecked = checkBox3.Checked;
            string FromTime = textBox1.Text;
            string ToTime = textBox2.Text;

            bool Conf= checkBox14.Checked;
            bool Canx = checkBox15.Checked;

            bool OneWayTrip = checkBox11.Checked;
            bool TwoWayTrip = checkBox12.Checked;

            bool CodeFrom = checkBox4.Checked;
            string CodeFrom1 = textBox3.Text;
            bool CodeTo = checkBox5.Checked;
            string CodeTo1 = textBox4.Text;

            bool NoGetFirstName = checkBox16.Checked;
            string NoFirstNameGet = textBox9.Text;

            bool NoGetLastName= checkBox17.Checked;
            string NoLastNameGet = textBox10.Text;

            bool OnlyEmailStandardizedSuccess = checkBox18.Checked;
            bool OnlyPhoneStandardizedSuccess = checkBox19.Checked;

            bool EmailBlackListChecked =checkBox20.Checked;
            bool PhoneBlackListChecked =checkBox21.Checked;
            string[] EmailBlackList = File.ReadAllLines(Application.StartupPath + "//emailblacklist.txt");
            string[] PhoneBlackList = File.ReadAllLines(Application.StartupPath + "//phoneblacklist.txt");

            bool GetHaveEmail = checkBox22.Checked;
            bool GetHavePhone = checkBox23.Checked;

            int loop = 1;

            for (int a = 0; a < ListAllVJI.Count; a++)
            {
                VietJetInfomation CurrentVJI = ListAllVJI[a];
                if (FirstNameCountryVietNam && !FirstNameOtherCountry)
                {
                    bool IsFirstNameVietNam = false;
                    for (int b = 0; b < ListFirstNameCountryVietNam.Length; b++)
                    {
                        if (CurrentVJI.FirstName.ToLower() == ListFirstNameCountryVietNam[b].ToLower())
                        {
                            IsFirstNameVietNam = true;
                            break;
                        }
                    }
                    if (!IsFirstNameVietNam)
                        continue;
                }else if(!FirstNameCountryVietNam && FirstNameOtherCountry)
                {
                    bool IsFirstNameVietNam = false;
                    for (int b = 0; b < ListFirstNameCountryVietNam.Length; b++)
                    {
                        if (CurrentVJI.FirstName.ToLower() == ListFirstNameCountryVietNam[b].ToLower())
                        {
                            IsFirstNameVietNam = true;
                            break;
                        }
                    }
                    if (IsFirstNameVietNam)
                        continue;
                }
                else if (FirstNameCountryVietNam && FirstNameOtherCountry){ }
                else if (!FirstNameCountryVietNam && !FirstNameOtherCountry) { continue; }

                if (TimeLineChecked)
                {
                    bool InTimeLine = false;
                    try
                    {
                        if (DateTime.ParseExact(CurrentVJI.DateFlight1t1, "dd/MM/yyyy", CultureInfo.InvariantCulture) > DateTime.ParseExact(FromTime, "dd/MM/yyyy", CultureInfo.InvariantCulture) && DateTime.ParseExact(CurrentVJI.DateFlight1t1, "dd/MM/yyyy", CultureInfo.InvariantCulture) < DateTime.ParseExact(ToTime, "dd/MM/yyyy", CultureInfo.InvariantCulture))
                        {
                            InTimeLine = true;
                        }
                    }
                    catch { }
                    try {
                        if (DateTime.ParseExact(CurrentVJI.DateFlight2t1, "dd/MM/yyyy", CultureInfo.InvariantCulture) > DateTime.ParseExact(FromTime, "dd/MM/yyyy", CultureInfo.InvariantCulture) && DateTime.ParseExact(CurrentVJI.DateFlight2t1, "dd/MM/yyyy", CultureInfo.InvariantCulture) < DateTime.ParseExact(ToTime, "dd/MM/yyyy", CultureInfo.InvariantCulture))
                        {
                            InTimeLine = true;
                        }
                    }
                    catch { }
                    if (!InTimeLine)
                        continue;
                }

                if(Conf && !Canx)
                {
                    if (CurrentVJI.Verify1 != "CONF")
                        continue;
                }else if(!Conf && Canx)
                {
                    if (CurrentVJI.Verify1 != "CANX")
                        continue;
                }else if(Conf && Canx){ }
                else if(!Conf && !Canx){ continue; }

                if (OneWayTrip && !TwoWayTrip)
                {
                    if (CurrentVJI.DateFlight1t1 == "" || CurrentVJI.DateFlight1t1 == null)
                        continue;
                    if (CurrentVJI.DateFlight2t1 != "")
                        if (CurrentVJI.DateFlight2t1 != null)
                            continue;
                }
                else if (!OneWayTrip && TwoWayTrip)
                {
                    if (CurrentVJI.DateFlight2t1 == "" || CurrentVJI.DateFlight2t1 == null)
                        continue;
                }
                else if (OneWayTrip && TwoWayTrip)
                {
                    //if (CurrentVJI.DateFlight1 == "" || CurrentVJI.DateFlight1 == null)
                    //    if (CurrentVJI.DateFlight2 == "" || CurrentVJI.DateFlight2 == null)
                    //        continue;
                }
                else if (!OneWayTrip && !TwoWayTrip)
                {
                    continue;
                }

                if (CodeFrom) {
                    bool match = false;
                    string[] ListCodeFrom1 = CodeFrom1.Split(',');
                    for(int c = 0; c < ListCodeFrom1.Length; c++)
                    {
                        if(Regex.IsMatch(CurrentVJI.Flight1, ListCodeFrom1[c] + " -") || Regex.IsMatch(CurrentVJI.Flight2, ListCodeFrom1[c] + " -"))
                        {
                            match = true;
                            break;
                        }
                    }
                    if (!match)
                        continue;
                }
                if (CodeTo)
                {
                    bool match = false;
                    string[] ListCodeTo1 = CodeTo1.Split(',');
                    for (int c = 0; c < ListCodeTo1.Length; c++)
                    {
                        if (Regex.IsMatch(CurrentVJI.Flight1, "- " + ListCodeTo1[c]) || Regex.IsMatch(CurrentVJI.Flight2, "- " + ListCodeTo1[c]))
                        {
                            match = true;
                            break;
                        }
                    }
                    if (!match)
                        continue;
                }

                if (NoGetFirstName)
                    if (CurrentVJI.FirstName == NoFirstNameGet)
                        continue;

                if (NoGetLastName)
                    if (CurrentVJI.LastName != NoLastNameGet)
                        continue;
                if (OnlyEmailStandardizedSuccess)
                    if (!Convert.ToBoolean(CurrentVJI.EmailStandardizedSuccess))
                        continue;

                if (OnlyPhoneStandardizedSuccess)
                    if (!Convert.ToBoolean(CurrentVJI.PhoneStandardizedSuccess))
                        continue;

                if (EmailBlackListChecked)
                {
                    if (Convert.ToBoolean(CurrentVJI.EmailStandardizedSuccess))
                    {
                        bool IsBlackList = false;
                        for (int d = 0; d < EmailBlackList.Length; d++)
                        {
                            if(CurrentVJI.EmailStandardized==EmailBlackList[d])
                            {
                                IsBlackList = true;
                                break;
                            }
                        }
                        if (IsBlackList)
                            continue;
                    }
                }
                if (PhoneBlackListChecked)
                    if (Convert.ToBoolean(CurrentVJI.PhoneStandardizedSuccess))
                    {
                        bool IsBlackList = false;
                        for (int d = 0; d < PhoneBlackList.Length; d++)
                        {
                            if (CurrentVJI.PhoneStandardized == PhoneBlackList[d])
                            {
                                IsBlackList = true;
                                break;
                            }
                        }
                        if (IsBlackList)
                            continue;
                    }
                if (GetHaveEmail)
                    if (CurrentVJI.Email == null || CurrentVJI.Email == "")
                        continue;

                if(GetHavePhone)
                    if (CurrentVJI.Phone == null || CurrentVJI.Phone == "")
                        continue;

                dataGridView1.Rows.Add(loop.ToString(), CurrentVJI.CustomerIsCode, CurrentVJI.FirstName, CurrentVJI.LastName, CurrentVJI.DateFlight1t1, CurrentVJI.Flight1, CurrentVJI.Verify1, CurrentVJI.DateFlight2t1, CurrentVJI.Flight2, CurrentVJI.Verify2, CurrentVJI.Seats, CurrentVJI.DateFlight1t2, CurrentVJI.FlightCode1, CurrentVJI.DateFlight2t2, CurrentVJI.FlightCode2, CurrentVJI.Email, CurrentVJI.EmailStandardizedSuccess, CurrentVJI.EmailStandardized, CurrentVJI.Phone, CurrentVJI.PhoneStandardizedSuccess, CurrentVJI.PhoneStandardized, CurrentVJI.PhoneNetwork, CurrentVJI.Confirm, CurrentVJI.PaymentStatus);
                loop++;
                ListVJIFiltered.Add(new VietJetInfomation { CustomerIsCode = CurrentVJI.CustomerIsCode, FirstName = CurrentVJI.FirstName, LastName = CurrentVJI.LastName, DateFlight1t1 = CurrentVJI.DateFlight1t1, Flight1 = CurrentVJI.Flight1, Verify1 = CurrentVJI.Verify1, DateFlight2t1 = CurrentVJI.DateFlight2t1, Flight2 = CurrentVJI.Flight2, Verify2 = CurrentVJI.Verify2, DateFlight1t2 = CurrentVJI.DateFlight1t2, FlightCode1 = CurrentVJI.FlightCode1, DateFlight2t2 = CurrentVJI.DateFlight2t2, Email = CurrentVJI.Email, EmailStandardizedSuccess = CurrentVJI.EmailStandardizedSuccess, EmailStandardized = CurrentVJI.EmailStandardized, Phone = CurrentVJI.Phone, PhoneStandardizedSuccess = CurrentVJI.PhoneStandardizedSuccess, PhoneStandardized = CurrentVJI.PhoneStandardized, PhoneNetwork = CurrentVJI.PhoneNetwork, Confirm = CurrentVJI.Confirm, PaymentStatus = CurrentVJI.PaymentStatus });
            }
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            SaveOptions();
            string[] Export = new string[0];
            bool FlightCode = checkBox6.Checked;
            bool FirstName = checkBox7.Checked;
            bool LastName = checkBox8.Checked;
            bool Flight = checkBox9.Checked;
            bool Seats = checkBox10.Checked;
            bool Date = checkBox13.Checked;
            for(int a = 0; a < ListVJIFiltered.Count; a++)
            {
                Array.Resize(ref Export, Export.Length + 1);
                if (FlightCode)
                    Export[Export.Length - 1] += ListVJIFiltered[a].CustomerIsCode + "|";
                if(FirstName)
                    Export[Export.Length - 1] += ListVJIFiltered[a].FirstName + "|";
                if (LastName)
                    Export[Export.Length - 1] += ListVJIFiltered[a].LastName + "|";
                if (Date)
                    Export[Export.Length - 1] += ListVJIFiltered[a].DateFlight1t1 + "|";
                if (Flight)
                    Export[Export.Length - 1] += ListVJIFiltered[a].Flight1 + "|";
                if (Date && ListVJIFiltered[a].DateFlight2t1!= null && ListVJIFiltered[a].DateFlight2t1 != "")
                    Export[Export.Length - 1] += ListVJIFiltered[a].DateFlight2t1 + "|";
                if (Flight && ListVJIFiltered[a].Flight2 != null && ListVJIFiltered[a].DateFlight2t1 != "")
                    Export[Export.Length - 1] += ListVJIFiltered[a].Flight2 + "|";
                if (Seats)
                    Export[Export.Length - 1] += ListVJIFiltered[a].Seats + "|";
                Export[Export.Length - 1].Remove(Export[Export.Length - 1].Length - 1);
            }
            File.WriteAllLines(Application.StartupPath + "//Export.txt", Export);
        }
        public void LoadOptions()
        {
            try
            {
                string[] Options = File.ReadAllLines(Application.StartupPath + "//option");
                InputData.MaxThread = Convert.ToInt32(Options[0]);
                checkBox1.Checked = Convert.ToBoolean(Options[1]);
                checkBox2.Checked = Convert.ToBoolean(Options[2]);
                checkBox3.Checked = Convert.ToBoolean(Options[3]);
                checkBox4.Checked = Convert.ToBoolean(Options[4]);
                checkBox5.Checked = Convert.ToBoolean(Options[5]);
                checkBox6.Checked = Convert.ToBoolean(Options[6]);
                checkBox7.Checked = Convert.ToBoolean(Options[7]);
                checkBox8.Checked = Convert.ToBoolean(Options[8]);
                checkBox9.Checked = Convert.ToBoolean(Options[9]);
                checkBox10.Checked = Convert.ToBoolean(Options[10]);
                checkBox11.Checked = Convert.ToBoolean(Options[11]);
                checkBox12.Checked = Convert.ToBoolean(Options[12]);
                checkBox13.Checked = Convert.ToBoolean(Options[13]);
                checkBox14.Checked = Convert.ToBoolean(Options[14]);
                checkBox15.Checked = Convert.ToBoolean(Options[15]);
                textBox1.Text = Options[16];
                textBox2.Text = Options[17];
                textBox3.Text = Options[18];
                textBox4.Text = Options[19];
                textBox5.Text = Options[20];
                textBox6.Text = Options[21];
                textBox7.Text = Options[22];
                textBox8.Text = Options[23];
                textBox9.Text = Options[24];
                textBox10.Text = Options[25];
                checkBox16.Checked = Convert.ToBoolean(Options[26]);
                checkBox17.Checked = Convert.ToBoolean(Options[27]);
                checkBox18.Checked = Convert.ToBoolean(Options[28]);
                checkBox19.Checked = Convert.ToBoolean(Options[29]);
            }
            catch { }
        }
        public void SaveOptions()
        {
            InputData.Server = this.textBox5.Text;
            InputData.Database = this.textBox6.Text;
            InputData.UID = this.textBox7.Text;
            InputData.Password = this.textBox8.Text;
            string[] Options = new string[30];
            Options[0] = Convert.ToString(InputData.MaxThread);
            Options[1] = Convert.ToString(checkBox1.Checked);
            Options[2] = Convert.ToString(checkBox2.Checked);
            Options[3] = Convert.ToString(checkBox3.Checked);
            Options[4] = Convert.ToString(checkBox4.Checked);
            Options[5] = Convert.ToString(checkBox5.Checked);
            Options[6] = Convert.ToString(checkBox6.Checked);
            Options[7] = Convert.ToString(checkBox7.Checked);
            Options[8] = Convert.ToString(checkBox8.Checked);
            Options[9] = Convert.ToString(checkBox9.Checked);
            Options[10] = Convert.ToString(checkBox10.Checked);
            Options[11] = Convert.ToString(checkBox11.Checked);
            Options[12] = Convert.ToString(checkBox12.Checked);
            Options[13] = Convert.ToString(checkBox13.Checked);
            Options[14] = Convert.ToString(checkBox14.Checked);
            Options[15] = Convert.ToString(checkBox15.Checked);
            Options[16] = textBox1.Text;
            Options[17] = textBox2.Text;
            Options[18] = textBox3.Text;
            Options[19] = textBox4.Text;
            Options[20] = textBox5.Text;
            Options[21] = textBox6.Text;
            Options[22] = textBox7.Text;
            Options[23] = textBox8.Text;
            Options[24] = textBox9.Text;
            Options[25] = textBox10.Text;
            Options[26] = Convert.ToString(checkBox16.Checked);
            Options[27] = Convert.ToString(checkBox17.Checked);
            Options[28] = Convert.ToString(checkBox18.Checked);
            Options[29] = Convert.ToString(checkBox19.Checked);
            File.WriteAllLines(Application.StartupPath + "//option", Options);
        }

        private void Button4_Click(object sender, EventArgs e)
        {
            SaveOptions();
            if (new DataServer().CheckConnection())
            {
                MessageBox.Show("Kết nối thành công !");
            }
            else MessageBox.Show("Kết nối thất bại !");
        }

        private void Button5_Click(object sender, EventArgs e)
        {
            SaveOptions();
            InputData.Server = this.textBox5.Text;
            InputData.Database = this.textBox6.Text;
            InputData.UID = this.textBox7.Text;
            InputData.Password = this.textBox8.Text;
            //bool EmailStandardized = checkBox18.Checked;
            //bool PhoneStandardized = checkBox19.Checked;
            new DataServer().CreateTable2();
            OpenFileDialog open = new OpenFileDialog();
            open.Filter = "txt file|*.txt";
            open.ShowDialog();
            string[] inputlist = new string[0];
            try
            {
                inputlist = File.ReadAllLines(@open.FileName);
            }
            catch
            {
                MessageBox.Show("File lỗi!");
            }
            int currentthread = 0;
            int Success = 0;
            int Failed = 0;
            Thread t1 = new Thread(delegate ()
            {
                for (int a = 0; a < inputlist.Length; a++)
                {
                    try
                    {
                        Invoke((MethodInvoker)(() =>
                        {
                            this.button5.Text = (a + 1).ToString() + "/" + inputlist.Length.ToString() + " Success:" + Success.ToString() + " Failed:" + Failed;
                            Application.DoEvents();
                        }));
                        while (true)
                        {
                            if (currentthread < InputData.MaxThread)
                                break;
                            Thread.Sleep(250);
                        }
                        currentthread++;
                        string[] currentinfo = inputlist[a].Split('|');
                        string flightcode = currentinfo[0];
                        string firstname = currentinfo[1];
                        string lastname = currentinfo[2];
                        string dateflight1 = currentinfo[3];
                        string flightcode1 = currentinfo[4];

                        string dateflight2 = "";//currentinfo[5];
                        string flightcode2 = "";// currentinfo[6];
                        string email = "";// currentinfo[7];
                        bool emailstandardizedsuccess = false;
                        string emailstandardized = "";
                        string phone = "";//currentinfo[8];
                        bool phonestandardizedsuccess = false;
                        string phonestandardized = "";
                        string phonenetwork = "";
                        string confirm = "";// currentinfo[9];
                        string paymentstatus = "";// currentinfo[10];

                        if (Regex.IsMatch(currentinfo[5], @"\d{2}/\d{2}/\d{4}"))
                        {
                            dateflight2 = currentinfo[5];
                            flightcode2 = currentinfo[6];
                            email = currentinfo[7];
                            phone = currentinfo[8];
                            confirm = currentinfo[9];
                            paymentstatus = currentinfo[10];
                        }
                        else
                        {
                            email = currentinfo[5];
                            phone = currentinfo[6];
                            confirm = currentinfo[7];
                            paymentstatus = currentinfo[8];
                        }
                        emailstandardized = EmailStandardizedHandle(email);
                        if (emailstandardized != "")
                            emailstandardizedsuccess = true;
                        phonestandardized = PhoneStandardizedHandle(phone);
                        if (phonestandardized != "")
                            phonestandardizedsuccess = true;
                        if (phonestandardizedsuccess)
                            phonenetwork = HandlePhoneNetwork(phonestandardized);

                        Thread t2 = new Thread(delegate ()
                        {
                        try
                        {
                            int lastinsertedid = new DataServer().InsertNewData2(flightcode, firstname, lastname, dateflight1, flightcode1, dateflight2, flightcode2, email,emailstandardizedsuccess, emailstandardized, phone,phonestandardizedsuccess, phonestandardized,phonenetwork, confirm, paymentstatus);
                                Success++;
                            }
                            catch (Exception d)
                            {
                                //MessageBox.Show(d.Message);
                                Failed++;
                            }
                            currentthread--;
                        });
                        t2.Start();
                    }
                    catch { }
                }
                while (true)
                {
                    if (currentthread ==0)
                        break;
                    Thread.Sleep(250);

                }
                Invoke((MethodInvoker)(() =>
                {
                    this.button5.Text = "Hoàn thành Success:" + Success.ToString() + " Failed:" + Failed;
                    Application.DoEvents();
                }));
                //Thread.Sleep(1000);
                //Invoke((MethodInvoker)(() =>
                //{
                //    this.button5.Text = "Nhập dữ liệu 2";
                //    Application.DoEvents();
                //}));
            });
            t1.Start();

        }
        public string PhoneStandardizedHandle(string phone)
        {
            string returnphone = "";
            phone = phone.Replace(" ", "");
            string[] ListFirstNumberAccept = File.ReadAllLines(Application.StartupPath + "\\emailphoneruler.txt");
            char[] RemoveChar = ListFirstNumberAccept[0].ToCharArray();
            bool AcceptThisPhone = false;
            for (int a = 2; a < ListFirstNumberAccept.Length; a++)
            {
                string[] ListNetwork = ListFirstNumberAccept[a].Split(',');
                for(int b = 1; b < ListNetwork.Length; b++)
                {
                    if (Regex.IsMatch(phone, "^" + ListNetwork[b]))
                    {
                        AcceptThisPhone = true;
                        break;
                    }
                }
                if (AcceptThisPhone)
                    break;
            }
            if (AcceptThisPhone)
            {
                try
                {
                    MatchCollection coll = Regex.Matches(phone, @"^84(\d{9}$)");
                    phone = "0" + coll[0].Groups[1].Value;
                }
                catch { }
                if (phone.Length == 10)
                    returnphone = phone;
            }
            return returnphone;
        }
        public string HandlePhoneNetwork(string phone)
        {
            string PhoneNetwork = "Other";
            string[] ListFirstNumberAccept = File.ReadAllLines(Application.StartupPath + "\\emailphoneruler.txt");
            //char[] RemoveChar = ListFirstNumberAccept[0].ToCharArray();
            bool IsANetwork = false;
            for (int a = 2; a < ListFirstNumberAccept.Length; a++)
            {
                string[] ListNetwork = ListFirstNumberAccept[a].Split(',');
                for (int b = 1; b < ListNetwork.Length; b++)
                {
                    if (Regex.IsMatch(phone, "^" + ListNetwork[b]))
                    {
                        IsANetwork = true;
                        PhoneNetwork = ListNetwork[0];
                        break;
                    }
                }
                if (IsANetwork)
                    break;
            }
            return PhoneNetwork;
        }
        public string EmailStandardizedHandle(string email)
        {
            string returnemail = "";
            char[] ListCharacterNotAccept = File.ReadAllLines(Application.StartupPath + "\\emailphoneruler.txt")[1].ToCharArray();
            if (email.Split('@').Length==2)
                if(email.Split('.').Length >=2)
                {
                    bool AcceptThisEmail = true;
                    for(int a=0;a<ListCharacterNotAccept.Length;a++)
                        if (email.Split(ListCharacterNotAccept[a]).Length > 1)
                        {
                            AcceptThisEmail = false;
                            break;
                        }
                    if (AcceptThisEmail)
                        returnemail = email;
                }
            return returnemail;
        }


        private void tabPage2_Click(object sender, EventArgs e)
        {

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }
    }
    class InputData
    {
        public static string Server;
        public static string Database;
        public static string UID;
        public static string Password;
        public static int MaxThread;
    }
    class DataServer
    {
        public bool CheckConnection()
        {
            bool result = false;
            MySqlConnection conn = new MySqlConnection("SERVER=" + InputData.Server + ";DATABASE=" + InputData.Database + ";UID=" + InputData.UID + ";PASSWORD=" + InputData.Password + ";CHARSET=utf8;");
            try
            {
                conn.Open();
                result = true;
                conn.Close();
            }
            catch { }
            return result;
        }
        public void CreateTable()
        {
            MySqlConnection conn = new MySqlConnection("SERVER=" + InputData.Server + ";DATABASE=" + InputData.Database + ";UID=" + InputData.UID + ";PASSWORD=" + InputData.Password + ";CHARSET=utf8;");
            conn.Open();
            MySqlCommand cmd = new MySqlCommand();
            cmd.CommandText = "CREATE TABLE IF NOT EXISTS VietJetInfomation(id INT(10) NOT NULL UNIQUE AUTO_INCREMENT,flightcode VARCHAR(100) NOT NULL UNIQUE,firstname VARCHAR(100),lastname VARCHAR(100),dateflight1 VARCHAR(100),flight1 VARCHAR(100),flightcode1 VARCHAR(100),dateflight2 VARCHAR(100),flight2 VARCHAR(100),flightcode2 VARCHAR(100),seats VARCHAR(100)) ENGINE = InnoDB";
            cmd.Connection = conn;
            cmd.ExecuteNonQuery();
            conn.Close();
        }
        public void CreateTable2()
        {
            MySqlConnection conn = new MySqlConnection("SERVER=" + InputData.Server + ";DATABASE=" + InputData.Database + ";UID=" + InputData.UID + ";PASSWORD=" + InputData.Password + ";CHARSET=utf8;");
            conn.Open();
            MySqlCommand cmd = new MySqlCommand();
            cmd.CommandText = "CREATE TABLE IF NOT EXISTS VietJetInfomation2(id INT(10) NOT NULL UNIQUE AUTO_INCREMENT,customeriscode VARCHAR(100) NOT NULL UNIQUE,firstname VARCHAR(100),lastname VARCHAR(100),dateflight1 VARCHAR(100),flightcode1 VARCHAR(100),dateflight2 VARCHAR(100),flightcode2 VARCHAR(100),email VARCHAR(100),emailstandardizedsuccess VARCHAR(100),emailstandardized VARCHAR(100),phone VARCHAR(100),phonestandardizedsuccess VARCHAR(100),phonestandardized VARCHAR(100),phonenetwork VARCHAR(100),confirm VARCHAR(100),paymentstatus VARCHAR(100)) ENGINE = InnoDB";
            cmd.Connection = conn;
            cmd.ExecuteNonQuery();
            conn.Close();
        }

        public int InsertNewData(string flightcode, string firstname, string lastname, string dateflight1, string flight1, string flightcode1, string dateflight2, string flight2, string flightcode2, string seats)
        {
            MySqlConnection conn = new MySqlConnection("SERVER=" + InputData.Server + ";DATABASE=" + InputData.Database + ";UID=" + InputData.UID + ";PASSWORD=" + InputData.Password + ";CHARSET=utf8;");
            conn.Open();
            MySqlCommand cmd = new MySqlCommand();
            cmd.CommandText = "INSERT INTO VietJetInfomation(flightcode,firstname,lastname,dateflight1,flight1,flightcode1,dateflight2,flight2,flightcode2,seats) VALUES ('" + flightcode + "','" + firstname + "','" + lastname + "','" + dateflight1 + "','" + flight1 + "','" + flightcode1 + "','" + dateflight2 + "','" + flight2 + "','" + flightcode2 + "','" + seats + "')";
            cmd.Connection = conn;
            cmd.ExecuteNonQuery();
            int LastInsertedId = Convert.ToInt32(cmd.LastInsertedId);
            conn.Close();
            return LastInsertedId;
        }
        public int InsertNewData2(string customeriscode, string firstname, string lastname, string dateflight1, string flightcode1, string dateflight2, string flightcode2, string email, bool emailstandardizedsuccess, string emailstandardized, string phone, bool phonestandardizedsuccess, string phonestandardized,string phonenetwork, string confirm,string paymentstatus)
        {
            MySqlConnection conn = new MySqlConnection("SERVER=" + InputData.Server + ";DATABASE=" + InputData.Database + ";UID=" + InputData.UID + ";PASSWORD=" + InputData.Password + ";CHARSET=utf8;");
            conn.Open();
            MySqlCommand cmd = new MySqlCommand();
            cmd.CommandText = "INSERT INTO VietJetInfomation2(customeriscode,firstname,lastname,dateflight1,flightcode1,dateflight2,flightcode2,email,emailstandardizedsuccess,emailstandardized,phone,phonestandardizedsuccess,phonestandardized,phonenetwork,confirm,paymentstatus) VALUES ('" + customeriscode + "','" + firstname + "','" + lastname + "','" + dateflight1 + "','" + flightcode1 + "','" + dateflight2 + "','" + flightcode2 + "','" + email + "','" + Convert.ToString(emailstandardizedsuccess) + "','"+emailstandardized+"','" + phone + "','"+Convert.ToString(phonestandardizedsuccess)+"','" + phonestandardized + "','" + phonenetwork + "','" + confirm + "','" + paymentstatus + "')";
            //MessageBox.Show(cmd.CommandText);
            cmd.Connection = conn;
            cmd.ExecuteNonQuery();
            int LastInsertedId = Convert.ToInt32(cmd.LastInsertedId);
            conn.Close();
            return LastInsertedId;
        }

        public List<VietJetInfomation> LoadAllData()
        {
            List<VietJetInfomation> ListVietJetInfomation = new List<VietJetInfomation>();
            MySqlConnection conn = new MySqlConnection("SERVER=" + InputData.Server + ";DATABASE=" + InputData.Database + ";UID=" + InputData.UID + ";PASSWORD=" + InputData.Password + ";CHARSET=utf8;");
            conn.Open();
            string[] result = new string[0];
            string cmd = "SELECT id,flightcode,firstname,lastname,dateflight1,flight1,flightcode1,dateflight2,flight2,flightcode2,seats FROM VietJetInfomation";
            MySqlCommand cmd1 = new MySqlCommand();
            cmd1.CommandText = cmd;
            cmd1.Connection = conn;
            MySqlDataReader dr = cmd1.ExecuteReader();
            while (dr.Read())
            {
                VietJetInfomation vji = new VietJetInfomation();
                vji.Id = Convert.ToInt32(dr["id"].ToString());
                vji.CustomerIsCode = dr["flightcode"].ToString();
                vji.FirstName= dr["firstname"].ToString();
                vji.LastName = dr["lastname"].ToString();
                vji.DateFlight1t1 = dr["dateflight1"].ToString();
                vji.Flight1 = dr["flight1"].ToString();
                vji.Verify1 = dr["flightcode1"].ToString();
                vji.DateFlight2t1 = dr["dateflight2"].ToString();
                vji.Flight2 = dr["flight2"].ToString();
                vji.Verify2 = dr["flightcode2"].ToString();
                vji.Seats = dr["seats"].ToString();
                ListVietJetInfomation.Add(vji);
            }
            conn.Close();
            return ListVietJetInfomation;
        }
        public List<VietJetInfomation2> LoadAllDataVJ2()
        {
            List<VietJetInfomation2> ListVietJetInfomation2 = new List<VietJetInfomation2>();
            MySqlConnection conn = new MySqlConnection("SERVER=" + InputData.Server + ";DATABASE=" + InputData.Database + ";UID=" + InputData.UID + ";PASSWORD=" + InputData.Password + ";CHARSET=utf8;");
            conn.Open();
            string[] result = new string[0];
            string cmd = "SELECT id,customeriscode,firstname,lastname,dateflight1,flightcode1,dateflight2,flightcode2,email,emailstandardizedsuccess,emailstandardized,phone,phonestandardizedsuccess,phonestandardized,phonenetwork,confirm,paymentstatus FROM VietJetInfomation2";
            MySqlCommand cmd1 = new MySqlCommand();
            cmd1.CommandText = cmd;
            cmd1.Connection = conn;
            MySqlDataReader dr = cmd1.ExecuteReader();
            while (dr.Read())
            {
                VietJetInfomation2 vji2 = new VietJetInfomation2();
                vji2.Id = Convert.ToInt32(dr["id"].ToString());
                vji2.CustomerIsCode = dr["customeriscode"].ToString();
                vji2.FirstName = dr["firstname"].ToString();
                vji2.LastName = dr["lastname"].ToString();
                vji2.DateFlight1t2 = dr["dateflight1"].ToString();
                vji2.FlightCode1 = dr["flightcode1"].ToString();
                vji2.DateFlight2t2 = dr["dateflight1"].ToString();
                vji2.FlightCode2 = dr["flightcode2"].ToString();
                vji2.Email = dr["email"].ToString();
                vji2.EmailStandardizedSuccess = dr["emailstandardizedsuccess"].ToString();
                vji2.EmailStandardized = dr["emailstandardized"].ToString();
                vji2.Phone = dr["phone"].ToString();
                vji2.PhoneStandardizedSuccess = dr["phonestandardizedsuccess"].ToString();
                vji2.PhoneStandardized = dr["phonestandardized"].ToString();
                vji2.PhoneNetwork = dr["phonenetwork"].ToString();
                vji2.Confirm = dr["confirm"].ToString();
                vji2.PaymentStatus = dr["paymentstatus"].ToString();
                ListVietJetInfomation2.Add(vji2);
            }
            conn.Close();
            return ListVietJetInfomation2;
        }

        //public string ReadIdMax()
        //{
        //    MySqlConnection conn = new MySqlConnection("SERVER=" + InputData.Server + ";DATABASE=" + InputData.Database + ";UID=" + InputData.UID + ";PASSWORD=" + InputData.Password + ";CHARSET=utf8;");
        //    conn.Open();
        //    string result = null;
        //    string cmd = "SELECT id FROM VietJetInfomation WHERE id=(SELECT max(id) FROM VietJetInfomation)";
        //    MySqlCommand cmd1 = new MySqlCommand();
        //    cmd1.CommandText = cmd;
        //    cmd1.Connection = conn;
        //    MySqlDataReader dr = cmd1.ExecuteReader();
        //    while (dr.Read())
        //    {
        //        result = dr["id"].ToString();
        //        break;
        //    }
        //    conn.Close();
        //    return result;
        //}
        //public void Update(string id, string flightcode, string firstname, string lastname, string dateflight1, string flight1, string flightcode1, string dateflight2, string flight2, string flightcode2, string seats)
        //{
        //    MySqlConnection conn = new MySqlConnection("SERVER=" + InputData.Server + ";DATABASE=" + InputData.Database + ";UID=" + InputData.UID + ";PASSWORD=" + InputData.Password + ";CHARSET=utf8;");
        //    conn.Open();
        //    MySqlCommand cmd = new MySqlCommand();
        //    cmd.CommandText = "UPDATE VietJetInfomation SET flightcode='" + flightcode + "',firstname='" + firstname + "',lastname='" + lastname + "',dateflight1='" + dateflight1 + "',flight1='" + flight1 + "',flightcode1='" + flightcode1 + "',dateflight2='" + dateflight2 + "',flight2='" + flight2 + "',flightcode2='" + flightcode2 + "',seats='" + seats + "' WHERE id=" + id;
        //    cmd.Connection = conn;
        //    cmd.ExecuteNonQuery();
        //    conn.Close();
        //}
        //public bool DeleteID(string id)
        //{
        //    bool success = false;
        //    try
        //    {
        //        MySqlConnection conn = new MySqlConnection("SERVER=" + InputData.Server + ";DATABASE=" + InputData.Database + ";UID=" + InputData.UID + ";PASSWORD=" + InputData.Password + ";CHARSET=utf8;");
        //        conn.Open();
        //        MySqlCommand cmd = new MySqlCommand();
        //        cmd.CommandText = "DELETE FROM VietJetInfomation WHERE id=" + id;
        //        cmd.Connection = conn;
        //        cmd.ExecuteNonQuery();
        //        conn.Close();
        //        success = true;
        //    }
        //    catch { }
        //    return success;
        //}
        //public VietJetInfomation GetVietJetInfomation(string id)
        //{
        //    VietJetInfomation vji = new VietJetInfomation();
        //    MySqlConnection conn = new MySqlConnection("SERVER=" + InputData.Server + ";DATABASE=" + InputData.Database + ";UID=" + InputData.UID + ";PASSWORD=" + InputData.Password + ";CHARSET=utf8;");
        //    conn.Open();
        //    string[] result = new string[0];
        //    string cmd = "SELECT flightcode,firstname,lastname,dateflight1,flight1,flightcode1,dateflight2,flight2,flightcode2,seats FROM VietJetInfomation WHERE id=" + id;
        //    MySqlCommand cmd1 = new MySqlCommand();
        //    cmd1.CommandText = cmd;
        //    cmd1.Connection = conn;
        //    MySqlDataReader dr = cmd1.ExecuteReader();
        //    while (dr.Read())
        //    {
        //        vji.Id = Convert.ToInt32(id);
        //        vji.CustomerIsCode = dr["flightcode"].ToString();
        //        vji.FirstName = dr["firstname"].ToString();
        //        vji.DateFlight1 = dr["dateflight1"].ToString();
        //        vji.Flight1 = dr["flight1"].ToString();
        //        vji.Verify1 = dr["flightcode1"].ToString();
        //        vji.DateFlight2 = dr["dateflight2"].ToString();
        //        vji.Flight2 = dr["flight2"].ToString();
        //        vji.Verify2 = dr["flightcode2"].ToString();
        //        vji.Seats = dr["seats"].ToString();
        //        break;
        //    }
        //    conn.Close();
        //    return vji;
        //}
        //public VietJetInfomation2 GetVietJetInfomation2(string customeriscode)
        //{
        //    VietJetInfomation2 vji2 = new VietJetInfomation2();
        //    MySqlConnection conn = new MySqlConnection("SERVER=" + InputData.Server + ";DATABASE=" + InputData.Database + ";UID=" + InputData.UID + ";PASSWORD=" + InputData.Password + ";CHARSET=utf8;");
        //    conn.Open();
        //    //string[] result = new string[0];
        //    string cmd = "SELECT id,dateflight1,flightcode1,dateflight2,flightcode2,email,emailstandizedsuccess,emailstandized,phone,phonestandizedsuccess,phonestandized,confirm,paymentstatus FROM VietJetInfomation WHERE customeriscode=" + customeriscode;
        //    MySqlCommand cmd1 = new MySqlCommand();
        //    cmd1.CommandText = cmd;
        //    cmd1.Connection = conn;
        //    MySqlDataReader dr = cmd1.ExecuteReader();
        //    while (dr.Read())
        //    {
        //        vji2.Id = Convert.ToInt32(dr["id"].ToString());
        //        vji2.CustomerIsCode = customeriscode;
        //        vji2.DateFlight1 = dr["dateflight1"].ToString();
        //        vji2.FlightCode1 = dr["flightcode1"].ToString();
        //        vji2.DateFlight2 = dr["dateflight2"].ToString();
        //        vji2.FlightCode2 = dr["flightcode2"].ToString();
        //        vji2.Email = dr["email"].ToString();
        //        vji2.EmailStandardizedSuccess = dr["emailstandizedsuccess"].ToString();
        //        vji2.EmailStandardized = dr["emailstandized"].ToString();
        //        vji2.Phone = dr["phone"].ToString();
        //        vji2.PhoneStandardizedSuccess = dr["phonestandizedsuccess"].ToString();
        //        vji2.PhoneStandardized = dr["phonestandized"].ToString();
        //        vji2.PhoneNetwork = dr["phonenetwork"].ToString();
        //        vji2.Confirm= dr["confirm"].ToString();
        //        vji2.PaymentStatus= dr["paymentstatus"].ToString();
        //        break;
        //    }
        //    conn.Close();
        //    return vji2;
        //}


    }
    class VietJetInfomation
    {
        public int Id;
        public string CustomerIsCode;
        public string FirstName;
        public string LastName;
        public string DateFlight1t1;
        public string Flight1;
        public string Verify1;
        public string DateFlight2t1;
        public string Flight2;
        public string Verify2;
        public string Seats;
        public string DateFlight1t2;
        public string FlightCode1;
        public string DateFlight2t2;
        public string FlightCode2;
        public string Email;
        public string EmailStandardizedSuccess;
        public string EmailStandardized;
        public string Phone;
        public string PhoneStandardizedSuccess;
        public string PhoneStandardized;
        public string PhoneNetwork;
        public string Confirm;
        public string PaymentStatus;

    }
    //class VietJetInfomation1
    //{
    //    public int Id;
    //    public string FlightCode;
    //    public string FirstName;
    //    public string LastName;
    //    public string DateFlight1;
    //    public string Flight1;
    //    public string FlightCode1;
    //    public string DateFlight2;
    //    public string Flight2;
    //    public string FlightCode2;
    //    public string Seats;
    //}
    class VietJetInfomation2
    {
        public int Id;
        public string CustomerIsCode;
        public string FirstName;
        public string LastName;
        public string DateFlight1t2;
        public string FlightCode1;
        public string DateFlight2t2;
        public string FlightCode2;
        public string Email;
        public string EmailStandardizedSuccess;
        public string EmailStandardized;
        public string Phone;
        public string PhoneStandardizedSuccess;
        public string PhoneStandardized;
        public string PhoneNetwork;
        public string Confirm;
        public string PaymentStatus;
    }
}
