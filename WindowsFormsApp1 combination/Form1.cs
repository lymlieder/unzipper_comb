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

namespace WindowsFormsApp1_combination
{
    public partial class Form1 : Form
    {
        static string resultFileAddress, fileType, head1, head2, head3, head4, head5, head6, unzipName;
        static bool fileAdd1_ok, fileAdd2_ok, fileAdd3_ok, resultName_ok, checkTip, head1_ok, head2_ok, head3_ok, head4_ok, head5_ok, head6_ok;
        static byte[] head1_Byte, head2_Byte, head3_Byte, head4_Byte, head5_Byte, head6_Byte, searchResult;
        static int massageCount;

        struct DataLineStruct
        {
            public byte commenValue;
            public byte bbSecondValue;
            public List<Int64> positionList;
        }

        static FileStream resultWriter, fileReader1, fileReader2, finalWriter;//, tempWriter;//, tempReader;

        static StreamWriter resultWriterForText, resultRecoder;

        static byte fileAddressLength = 0;//文件最大地址位组数，最大为4，最小为1

        static bool searchLoop = true;

        static int judgeLength = 3;
        static uint cursorRange = 256;
        static uint cursorStep = 1;
        static string searchHead;

        static List<DataLineStruct> dataLineListA, dataLineListB;

        private void Form1_Load(object sender, EventArgs e)
        {
            load();
            checkForPara();
            string[] array = { ".bin", ".txt" };
            type_comboBox1.DataSource = array;
            type_comboBox1.SelectedIndex = 0;
            fileType = type_comboBox1.SelectedItem.ToString();

            string[] arraySearch = { "SRWF" };
            search_comboBox1.DataSource = arraySearch;
            search_comboBox1.SelectedIndex = 0;
            searchHead = search_comboBox1.SelectedItem.ToString();
            unzipName_textBox4.Text = resultFileName_textBox.Text;
            unzipper_textBox1.Text = fileOutPut_textBox3.Text;
            if (detailShow_textBox.TextLength > 0)
                recoder.Enabled = true;
            else
                recoder.Enabled = false;

            checkTip = false;
            unzipName_textBox4.Text = resultFileName_textBox.Text + "_unzipper.bin";
            massageCount = 0;
        }

        private void checkForPara()
        {
            fileAdd1_ok = true;
            fileAdd2_ok = true;
            fileAdd3_ok = true;
            resultName_ok = true;
            errorProvider1.Clear();
            if(fileInPut_textBox1.TextLength == 0)
            {
                fileAdd1_ok = false;
                errorProvider1.SetError(label8, "请选择地址");
            }
            if (fileInPut_textBox2.TextLength == 0)
            {
                fileAdd2_ok = false;
                errorProvider1.SetError(label9, "请选择地址");
            }
            if(fileOutPut_textBox3.TextLength == 0)
            {
                resultName_ok = false;
                errorProvider1.SetError(label10, "请选择地址");
            }
            if(resultFileName_textBox.TextLength == 0)
            {
                resultName_ok = false;
                errorProvider1.SetError(label6, "请输入名称");
            }
        }

        private void search_comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
           searchHead = search_comboBox1.SelectedItem.ToString();
        }

        private void recoder_Click(object sender, EventArgs e)
        {
            if (detailShow_textBox.TextLength > 0)
            {
                start.Enabled = false;
                string address = fileOutPut_textBox3.Text + resultFileName_textBox.Text + "_Result.txt";
                FileStream recoder = new FileStream(address, FileMode.Create, FileAccess.Write);
                resultRecoder = new StreamWriter(recoder, Encoding.Unicode);
                resultRecoder.Write(detailShow_textBox.Text);
                resultRecoder.Close();
                recoder.Close();
                start.Enabled = true;
            }
        }

        private void search_Click(object sender, EventArgs e)
        {
            start.Enabled = false;
            if (search_textBox1.TextLength > 0)
                searchHead = search_textBox1.Text;
            else
                searchHead = search_comboBox1.SelectedItem.ToString();
            searchPara();
            start.Enabled = true;
        }

        private void searchPara()
        {
            fileReader1 = new FileStream(fileInPut_textBox1.Text, FileMode.Open, FileAccess.Read);
            fileReader2 = new FileStream(fileInPut_textBox2.Text, FileMode.Open, FileAccess.Read);
            //共用量
            byte[] searchHeadByte = Encoding.Default.GetBytes(searchHead);//待对比量，不用重置
            byte[] searchTemp = new byte[searchHeadByte.Count()];
            long readerPosition = 0;
            int number = 1;
            //找旧头
            while(number>0)
            {
                fileReader1.Position = readerPosition;
                number = fileReader1.Read(searchTemp, 0, searchHeadByte.Count());
                if (byteArrayEquals(searchTemp, searchHeadByte))
                {//找到头，找全部
                    fileReader1.Position = readerPosition;
                    int resultLength = searchHeadByte.Count() + System.Text.Encoding.Default.GetByteCount("-4E88J-GW-BXD17-20170817-Vsp0.30");
                    searchResult = new byte[resultLength];
                    fileReader1.Read(searchResult, 0, resultLength);//存结果
                    textBox2.Text = System.Text.Encoding.Default.GetString(searchResult);//searchResult.ToString();//显示结果
                    break;
                }
                readerPosition++;
            }
            if(number == 0)
                textBox2.Text = "未找到";
            readerPosition = 0;
            number = 1;

            fileReader1.Close();
            //找新头
            while (number > 0)
            {
                fileReader2.Position = readerPosition;
                number = fileReader2.Read(searchTemp, 0, searchHeadByte.Count());
                if (byteArrayEquals(searchTemp, searchHeadByte))
                {//找到头，找全部
                    fileReader2.Position = readerPosition;
                    int resultLength = searchHeadByte.Count() + System.Text.Encoding.Default.GetByteCount("-4E88J-GW-BXD17-20170817-Vsp0.30");
                    searchResult = new byte[resultLength];
                    fileReader2.Read(searchResult, 0, resultLength);//存结果
                    textBox3.Text = System.Text.Encoding.Default.GetString(searchResult);//searchResult.ToString();//显示结果
                    break;
                }
                readerPosition++;
            }
            if (number == 0)
                textBox3.Text = "未找到";
            fileReader2.Close();
        }

        private bool byteArrayEquals(byte[] array1, byte[] array2)
        {
            if (array1.Count() != array2.Count())
                return false;
            for(long i = 0; i < array1.Count(); i++)
            {
                if (array1[i] != array2[i])
                    return false;
            }
            return true;
        }
        private void fileInPut1_Leave(object sender, EventArgs e)
        {
            checkForPara();
        }

        private void fileInPut2_Leave(object sender, EventArgs e)
        {
            checkForPara();
        }

        private void fileOutPut_textBox3_Leave(object sender, EventArgs e)
        {
            checkForPara();
        }

        private void resultFileName_textBox_Leave(object sender, EventArgs e)
        {
            unzipName_textBox4.Text = resultFileName_textBox.Text + "_unzipper.bin";
            checkForPara();
        }

        private void resultFileName_textBox_Enter(object sender, EventArgs e)
        {
            checkForPara();
        }

        private void fileInPut1_Enter(object sender, EventArgs e)
        {
            checkForPara();
        }

        private void fileInPut2_Enter(object sender, EventArgs e)
        {
            checkForPara();
        }

        private void fileOutPut_Enter(object sender, EventArgs e)
        {
            checkForPara();
        }

        private void fileOutPut_Leave(object sender, EventArgs e)
        {
            checkForPara();
        }

        private void fileInPut_textBox1_Enter(object sender, EventArgs e)
        {
            checkForPara();
        }

        private void fileInPut_textBox1_Leave(object sender, EventArgs e)
        {
            checkForPara();
        }

        private void fileInPut_textBox2_Enter(object sender, EventArgs e)
        {
            checkForPara();
        }

        private void fileInPut_textBox2_Leave(object sender, EventArgs e)
        {
            checkForPara();
        }

        private void unzipName_textBox4_KeyUp(object sender, KeyEventArgs e)
        {
            if (unzipName_textBox4.TextLength == 0)
                unzipName_textBox4.Text = resultFileName_textBox.Text + "_unzipper.bin";
            if (unzipName_textBox4.TextLength == 0)
                errorProvider1.SetError(label13, "请输入名称");
        }

        private void unzipName_textBox4_Leave(object sender, EventArgs e)
        {
            if (unzipName_textBox4.TextLength == 0)
                unzipName_textBox4.Text = resultFileName_textBox.Text + "_unzipper.bin";
            if (unzipName_textBox4.TextLength == 0)
                errorProvider1.SetError(label13, "请输入名称");
        }

        private void fileOutPut_textBox3_Enter(object sender, EventArgs e)
        {
            checkForPara();
        }

        private void type_comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            fileType = type_comboBox1.SelectedItem.ToString();
        }

        private void detailShow_textBox_TextChanged(object sender, EventArgs e)
        {
            if (detailShow_textBox.TextLength > 0)
            {
                recoder.Enabled = true;
            }
            else
                recoder.Enabled = false;
        }

        private void resultFileName_textBox_TextChanged(object sender, EventArgs e)
        {
            checkForPara();
        }

        private void fileOutPut_Click(object sender, EventArgs e)
        {
            errorProvider1.Clear();
            fileAdd3_ok = false;
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
                fileOutPut_textBox3.Text = folderBrowserDialog1.SelectedPath + @"\";
            checkForPara();
        }

        private void fileInPut2_Click(object sender, EventArgs e)
        {
            errorProvider1.Clear();
            openFileDialog1.Filter = null;//"bin files (*.bin) | *.bin";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.RestoreDirectory = true;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
                fileInPut_textBox2.Text = openFileDialog1.FileName.ToString();
            checkForPara();
        }

        private void fileInPut1_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = null;//"bin files (*.bin) | *.bin";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.RestoreDirectory = true;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
                fileInPut_textBox1.Text = openFileDialog1.FileName.ToString();
            checkForPara();
        }

        public Form1()
        {
            InitializeComponent();
        }

        private void start_Click(object sender, EventArgs e)
        {
            checkForPara();
            /*if (fileAdd1_ok == false)
                errorProvider1.SetError(label8, "请输入正确地址");
            if (fileAdd2_ok == false)
                errorProvider1.SetError(label9, "请输入正确地址");
            if (fileAdd3_ok == false)
                errorProvider1.SetError(label10, "请输入正确地址");
            if (resultName_ok == false)
                errorProvider1.SetError(label11, "请输入名称");
            if (head1_ok == false)
                errorProvider2.SetError(label1, "请输入");
            if (head2_ok == false)
                errorProvider2.SetError(label2, "请输入");
            if (head3_ok == false)
                errorProvider2.SetError(label3, "请输入");
            if (head4_ok == false)
                errorProvider2.SetError(label4, "请输入");
            if (head5_ok == false)
                errorProvider2.SetError(label5, "请输入");
            if (head6_ok == false)
                errorProvider2.SetError(label6, "请输入");*/
            if (fileAdd1_ok == true && fileAdd2_ok == true && fileAdd3_ok == true && resultName_ok == true)
            {
                search.Enabled = false;
                start.Enabled = false;
                detailShow_textBox.Clear();
                //load();

                //头
                /*head1_Byte = Encoding.Default.GetBytes(textBox4.Text);
                head2_Byte = Encoding.Default.GetBytes(textBox5.Text);
                head3_Byte = Encoding.Default.GetBytes(textBox6.Text);
                head4_Byte = Encoding.Default.GetBytes(textBox7.Text);
                head5_Byte = Encoding.Default.GetBytes(textBox8.Text);
                head6_Byte = Encoding.Default.GetBytes(textBox9.Text);*/

                //结果地址
                fileType = type_comboBox1.SelectedItem.ToString();
                resultFileAddress = fileOutPut_textBox3.Text + resultFileName_textBox.Text + fileType;
                fileReader1 = new FileStream(fileInPut_textBox1.Text, FileMode.Open, FileAccess.Read);
                fileReader2 = new FileStream(fileInPut_textBox2.Text, FileMode.Open, FileAccess.Read);
                resultWriter = new FileStream(resultFileAddress, FileMode.Create, FileAccess.Write);
                finalWriter = resultWriter; //new FileStream(@"C:\Users\Master\Desktop\finalWriter.bin", FileMode.Create, FileAccess.Write);
                if (fileType == ".txt")
                {
                    resultWriterForText = new StreamWriter(resultWriter, Encoding.UTF8);
                    resultWriterForText.AutoFlush = true;
                }

                Console.WriteLine(@"格        式：{0}", 0);
                detailShow_textBox.AppendText(string.Format(@"格        式：{0}", 0) + "\r\n");
                Console.WriteLine(@"厂 商 代 码 ：{0}", 0);
                detailShow_textBox.AppendText(string.Format(@"厂 商 代 码 ：{0}", 0) + "\r\n");
                Console.WriteLine(@"目 标 版 本 ：{0}", 0);
                detailShow_textBox.AppendText(string.Format(@"目 标 版 本 ：{0}", 0) + "\r\n");
                Console.WriteLine(@"升级包校验码：{0}", 0);
                detailShow_textBox.AppendText(string.Format(@"升级包校验码：{0}", 0) + "\r\n");
                Console.WriteLine(@"升级后校验码：{0}", 0);
                detailShow_textBox.AppendText(string.Format(@"升级后校验码：{0}", 0) + "\r\n");
                Console.WriteLine(@"保        留：{0}\r\n", 0);
                detailShow_textBox.AppendText(string.Format(@"保        留：{0}", 0) + "\r\n\r\n");

                //写入头
                if (fileType == ".bin")
                {
                    for (int i = 0; i < head1_Byte.Length; i++)
                        resultWriter.WriteByte(head1_Byte[i]);
                    for (int i = head1_Byte.Length; i < 4; i++)
                        resultWriter.WriteByte(0);

                    for (int i = 0; i < head2_Byte.Length; i++)
                        resultWriter.WriteByte(head2_Byte[i]);
                    for (int i = head2_Byte.Length; i < 4; i++)
                        resultWriter.WriteByte(0);

                    for (int i = 0; i < head3_Byte.Length; i++)
                        resultWriter.WriteByte(head3_Byte[i]);
                    for (int i = head3_Byte.Length; i < 2; i++)
                        resultWriter.WriteByte(0);

                    for (int i = 0; i < head4_Byte.Length; i++)
                        resultWriter.WriteByte(head4_Byte[i]);
                    for (int i = head4_Byte.Length; i < 4; i++)
                        resultWriter.WriteByte(0);

                    for (int i = 0; i < head5_Byte.Length; i++)
                        resultWriter.WriteByte(head5_Byte[i]);
                    for (int i = head5_Byte.Length; i < 4; i++)
                        resultWriter.WriteByte(0);

                    for (int i = 0; i < head6_Byte.Length; i++)
                        resultWriter.WriteByte(head6_Byte[i]);
                    for (int i = head6_Byte.Length; i < 14; i++)
                        resultWriter.WriteByte(0);
                }
                else if (fileType == ".txt")
                {
                    for (int i = 0; i < head1_Byte.Length; i++)
                        resultWriterForText.Write(head1_Byte[i].ToString("x2") + " ");
                    for (int i = head1_Byte.Length; i < 4; i++)
                        resultWriterForText.Write("00" + " ");

                    for (int i = 0; i < head2_Byte.Length; i++)
                        resultWriterForText.Write(head2_Byte[i].ToString("x2") + " ");
                    for (int i = head2_Byte.Length; i < 4; i++)
                        resultWriterForText.Write("00" + " ");

                    for (int i = 0; i < head3_Byte.Length; i++)
                        resultWriterForText.Write(head3_Byte[i].ToString("x2") + " ");
                    for (int i = head3_Byte.Length; i < 2; i++)
                        resultWriterForText.Write("00" + " ");

                    for (int i = 0; i < head4_Byte.Length; i++)
                        resultWriterForText.Write(head4_Byte[i].ToString("x2") + " ");
                    for (int i = head4_Byte.Length; i < 4; i++)
                        resultWriterForText.Write("00" + " ");

                    for (int i = 0; i < head5_Byte.Length; i++)
                        resultWriterForText.Write(head5_Byte[i].ToString("x2") + " ");
                    for (int i = head5_Byte.Length; i < 4; i++)
                        resultWriterForText.Write("00" + " ");

                    for (int i = 0; i < head6_Byte.Length; i++)
                        resultWriterForText.Write(head6_Byte[i].ToString("x2") + " ");
                    for (int i = head6_Byte.Length; i < 14; i++)
                        resultWriterForText.Write("00" + " ");

                    resultWriterForText.WriteLine();
                }

                dataLineListA = new List<DataLineStruct>();
                dataLineListB = new List<DataLineStruct>();
                dataLineListA.Clear();
                dataLineListB.Clear();
                play();
                WriteListToFile();
                start.Enabled = true;
                search.Enabled = true;
                fileReader1.Close();
                fileReader2.Close();
                resultWriter.Close();
                finalWriter.Close();
                detailShow_textBox.AppendText("\r\n");
                detailShow_textBox.AppendText("内容包数量：" + massageCount);
            }
        }

        private void load()
        {
            head1 = "0";
            head2 = "0";
            head3 = "0";
            head4 = "0";
            head5 = "0";
            head6 = "0";
            fileAdd1_ok = false;
            fileAdd2_ok = false;
            fileAdd3_ok = false;
            resultName_ok = false;
            head1_ok = false;
            head2_ok = false;
            head3_ok = false;
            head4_ok = false;
            head5_ok = false;
            head6_ok = false;
            head1_Byte = new byte[4];//格式
            head2_Byte = new byte[4];//厂商代码
            head3_Byte = new byte[2];//目标版本
            head4_Byte = new byte[4];//升级包校验码
            head5_Byte = new byte[4];//升级后校验码
            head6_Byte = new byte[14];//保留
        }

        private void play()
        {
            Console.WriteLine("Hello World!");

            if (fileReader1.Length > 0xFFFF)
                return;
            if (fileReader2.Length > 0xFFFF)
                return;

            long fileLength = Math.Max(fileReader1.Length, fileReader2.Length);
            if ((fileLength & 0xffffff00) == 0)
                fileAddressLength = 1;
            else if ((fileLength & 0xffff0000) == 0)
                fileAddressLength = 2;
            else if ((fileLength & 0xff000000) == 0)
                fileAddressLength = 3;
            else
                fileAddressLength = 4;

            //cursorRange = Convert.ToUInt32(para_textBox2.Text);
            if(checkTip == false)
            {
                if (fileType == ".bin")
                    resultWriter.WriteByte(fileAddressLength);
                else if (fileType == ".txt")
                {
                    resultWriterForText.Write(fileAddressLength.ToString("x2") + " ");
                    resultWriterForText.WriteLine();
                }
            }
            //resultWriterForText.Write(String.Format("{0:X2 }", fileAddressLength)); 
            //resultWriterForText.Write(String.Format("{0:X2 }", fileAddressLength)); 

            bool bigLoop = true;
            while (bigLoop)//步步跟进//////////////////////////////////////////////////////////*************一样循环
            {
                int a = 0; int b = 0;
                searchLoop = true;
                a = fileReader1.ReadByte();
                b = fileReader2.ReadByte();

                if ((a == -1) && (b == -1))//末尾退出//////////////------------------------如果相同区域共同到底
                    break;

                if (a != b)//路径断的时候
                {
                    if (a == -1)
                        fileReader2.Position--;
                    if (b == -1)
                        fileReader1.Position--;
                    //记录各个重要节点
                    Int64 locationTailA = fileReader1.Position - 2;//上一个连路径的末尾
                    Int64 locationTailB = fileReader2.Position - 2;//上一个连路径的末尾
                    Int64 locationHeadHoldA = fileReader1.Position - 1;//记录查找时断路径区域的开头
                    Int64 locationHeadHoldB = fileReader2.Position - 1;//记录查找时断路径区域的开头

                    Int64 locationRootA = locationHeadHoldA;
                    Int64 locationRootB = locationHeadHoldB;//用于渐进的游标底

                    byte[] rootA = new byte[judgeLength];
                    byte[] rootB = new byte[judgeLength];//对照数组，游标底数组
                    byte[] cursorA = new byte[judgeLength];
                    byte[] cursorB = new byte[judgeLength];//游标数组

                    Int64 locationCursorA = locationRootA;
                    Int64 locationCursorB = locationRootB;

                    //root
                    while (searchLoop)//////////////////////////////////////////////////////////*************root循环
                    {
                        fileReader1.Position = locationRootA;
                        fileReader2.Position = locationRootB;

                        int rootResultA = fileReader1.Read(rootA, 0, judgeLength);//读取、记录root
                        int rootResultB = fileReader2.Read(rootB, 0, judgeLength);

                        if ((rootResultA == 0) && (rootResultB == 0))//如果root到底
                        {
                            if(checkTip == true)
                                CheckRecordAndQuit(locationHeadHoldA, locationHeadHoldB, locationRootA, locationRootB, 2);
                            else
                                RecordAndQuit(locationHeadHoldA, locationHeadHoldB, locationRootA, locationRootB, 2);
                            bigLoop = false;
                            break;
                        }

                        Int64 activeA = 0; Int64 activeB = 0;

                        while ((Math.Max(activeA, activeB) < cursorRange) && (searchLoop == true))/////////*************游标循环
                        {
                            fileReader1.Position = locationRootA + activeA;
                            fileReader2.Position = locationRootB + activeB;
                            int resultA = fileReader1.Read(cursorA, 0, judgeLength);//得到cursor，等待比较
                            int resultB = fileReader2.Read(cursorB, 0, judgeLength);

                            if ((resultA == 0) && (resultB == 0))////////////////----------如果双到底
                            {
                                if (activeA > 0)
                                    locationRootB++;
                                if (activeB > 0)
                                    locationRootA++;
                                break;
                            }

                            bool AFind = ArrayEqul(cursorA, rootB, Math.Min(resultA, rootResultB));//删除
                            bool BFind = ArrayEqul(cursorB, rootA, Math.Min(resultB, rootResultA));//添加
                            if (activeA == 254)
                            {

                            }
                            if ((AFind == true) || (BFind == true))///////////---------如果找到路径（,）
                            {
                                if (AFind == true)
                                {
                                    locationRootA += activeA;//记录此时位置，即为下一个一样区域的开头
                                }

                                if (BFind == true)
                                {
                                    locationRootB += activeB;
                                }
                                //如果找到或者到末尾还未找到

                                if(checkTip == true)
                                    CheckRecordAndQuit(locationHeadHoldA, locationHeadHoldB, locationRootA, locationRootB, 1);//记录，做相应工作
                                else
                                    RecordAndQuit(locationHeadHoldA, locationHeadHoldB, locationRootA, locationRootB, 1);//记录，做相应工作

                                fileReader1.Position = locationRootA;//重置文件位置为一样区域开头，开始一样查找，找下一个不一样
                                fileReader2.Position = locationRootB;//文件重置查找位置

                                if (searchLoop == false)//说明找到了
                                    break;
                            }

                            if ((resultA > 0) && (rootResultA > 0))
                                activeA++;
                            if ((resultB > 0) && (rootResultB > 0))
                                activeB++;
                        }

                        if (Math.Max(activeA, activeB) >= cursorRange)
                        {
                            if ((activeA > 0) && (rootResultA > 0))
                                locationRootB++;
                            if ((activeB > 0) && (rootResultB > 0))
                                locationRootA++;
                        }
                        if (searchLoop == false)
                            break;
                    }
                }

                if (bigLoop == false)
                    break;
            }
        }

        private void RecordAndQuit(Int64 theLocationHeadHoldA, Int64 theLocationHeadHoldB, Int64 theLocationRootA, Int64 theLocationRootB, int type)
        {
            massageCount++;
            Int64 lengthA = theLocationRootA - theLocationHeadHoldA;
            Int64 lengthB = theLocationRootB - theLocationHeadHoldB; //int i = 0;
            Int64 theLengthA = lengthA;
            Int64 theLengthB = lengthB;
            Int64 theHoldA = theLocationHeadHoldA;

            if ((lengthA > 0) && (lengthB > 0))////////////////////////////////////////////////////////////////////////////////////////////////替换
            {
                byte[] res = new byte[lengthB];
                fileReader2.Position = theLocationHeadHoldB;
                fileReader2.Read(res, 0, (int)lengthB);
                Console.Write(@"从:{0:x}(包括)到:{1:x}(不包括)的{2}个元素替换为{3}个元素：", theLocationHeadHoldA, theLocationRootA, lengthA, lengthB);
                detailShow_textBox.AppendText(string.Format(@"从:{0:x}(包括)到:{1:x}(不包括)的{2}个元素替换为{3}个元素：", theLocationHeadHoldA, theLocationRootA, lengthA, lengthB));
                for (int i = 0; i < lengthB; i++)
                {
                    Console.Write(@" {0:x}", res[i]);
                    detailShow_textBox.AppendText(string.Format(@" {0:x}", res[i]));
                }
                Console.WriteLine(@" ");
                detailShow_textBox.AppendText("\r\n");
                //写入文件
                if ((theLengthA == theLengthB) && (theLengthA < 3))
                {
                    if ((theLengthA == 1) && (theLengthB == 1))
                    {
                        bool found = false;
                        int i;
                        for (i = 0; i < dataLineListA.Count; i++)//逐项寻找A匹配项
                        {
                            if (dataLineListA[i].commenValue == res[0])
                            {
                                dataLineListA[i].positionList.Add(theLocationHeadHoldA);
                                found = true;
                                break;
                            }
                        }
                        if (found == false)//说明该项不存在，创建
                        {
                            DataLineStruct tempData = new DataLineStruct();
                            tempData.commenValue = res[0];
                            tempData.bbSecondValue = 0;
                            tempData.positionList = new List<long>();
                            tempData.positionList.Add(theLocationHeadHoldA);
                            dataLineListA.Add(tempData);
                        }
                    }

                    else if ((theLengthA == 2) && (theLengthB == 2))
                    {
                        bool found = false;
                        int i;
                        for (i = 0; i < dataLineListB.Count; i++)
                        {
                            if (theLocationHeadHoldA == 58)
                            {

                            }
                            if ((dataLineListB[i].commenValue == res[0]) && (dataLineListB[i].bbSecondValue == res[1]))
                            {
                                dataLineListB[i].positionList.Add(theLocationHeadHoldA);
                                found = true;
                                break;
                            }
                        }
                        if (found == false)
                        {
                            DataLineStruct tempData = new DataLineStruct();
                            tempData.commenValue = res[0];
                            tempData.bbSecondValue = res[1];
                            tempData.positionList = new List<long>();
                            tempData.positionList.Add(theLocationHeadHoldA);
                            dataLineListB.Add(tempData);
                        }
                    }
                }
                else
                {
                    if (fileType == ".bin")
                    {
                        //for (int i = 0; i < 2; i++)
                        resultWriter.WriteByte(0xff);//小节头
                        resultWriter.WriteByte(0x01);//类型符
                        for (int i = 0; i < fileAddressLength; i++)
                        {
                            resultWriter.WriteByte((byte)(theLocationHeadHoldA & 0xff));//写入theLocationHeadHoldA
                            theLocationHeadHoldA >>= 8;
                        }
                        for (int i = 0; i < fileAddressLength; i++)
                        {
                            resultWriter.WriteByte((byte)(lengthA & 0xff));//写入被替换长度
                            lengthA >>= 8;
                        }
                        for (int i = 0; i < fileAddressLength; i++)
                        {
                            resultWriter.WriteByte((byte)(lengthB & 0xff));//写入替换长度
                            lengthB >>= 8;
                        }
                        for (int i = 0; i < theLengthB; i++)//写入替换元素
                            resultWriter.WriteByte(res[i]);
                    }
                    else if (fileType == ".txt")
                    {
                        //for (int i = 0; i < 2; i++)
                        resultWriterForText.Write(0xff.ToString("x2") + " ");//小节头
                        resultWriterForText.Write(0x01.ToString("x2") + " ");//类型符
                        for (int i = 0; i < fileAddressLength; i++)
                        {
                            resultWriterForText.Write((theLocationHeadHoldA & 0xff).ToString("x2") + " ");//写入theLocationHeadHoldA
                            theLocationHeadHoldA >>= 8;
                        }
                        for (int i = 0; i < fileAddressLength; i++)
                        {
                            resultWriterForText.Write((lengthA & 0xff).ToString("x2") + " ");//写入被替换长度
                            lengthA >>= 8;
                        }
                        for (int i = 0; i < fileAddressLength; i++)
                        {
                            resultWriterForText.Write((lengthB & 0xff).ToString("x2") + " ");//写入替换长度
                            lengthB >>= 8;
                        }
                        for (int i = 0; i < theLengthB; i++)//写入替换元素
                            resultWriterForText.Write(res[i].ToString("x2") + " ");
                        resultWriterForText.WriteLine();
                    }
                }
            }

            else if ((lengthA > 0) && (lengthB == 0))////////////////////////////////////////////////////////////////////////////////////////////删除
            {
                Console.WriteLine(@"删除:{0:x}（包括）到:{1:x}(不包括)后面长度为:{2}的元素", theLocationHeadHoldA, theLocationRootA, lengthA);
                detailShow_textBox.AppendText(string.Format(@"删除:{0:x}（包括）到:{1:x}(不包括)后面长度为:{2}的元素", theLocationHeadHoldA, theLocationRootA, lengthA) + "\r\n");
                {
                    //写入文件
                    if (fileType == ".bin")
                    {
                        //for (int i = 0; i < 2; i++)
                        resultWriter.WriteByte(0xff);//小节头
                        resultWriter.WriteByte(0x02);//类型符
                        for (int i = 0; i < fileAddressLength; i++)
                        {
                            resultWriter.WriteByte((byte)(theLocationHeadHoldA & 0xff));//写入theLocationHeadHoldA
                            theLocationHeadHoldA >>= 8;
                        }
                        for (int i = 0; i < fileAddressLength; i++)
                        {
                            resultWriter.WriteByte((byte)(lengthA & 0xff));//写入删除长度
                            lengthA >>= 8;
                        }
                    }
                    else if (fileType == ".txt")
                    {
                        //for (int i = 0; i < 2; i++)
                        resultWriterForText.Write(0xff.ToString("x2") + " ");//小节头
                        resultWriterForText.Write(0x02.ToString("x2") + " ");//类型符
                        for (int i = 0; i < fileAddressLength; i++)
                        {
                            resultWriterForText.Write((theLocationHeadHoldA & 0xff).ToString("x2") + " ");//写入theLocationHeadHoldA
                            theLocationHeadHoldA >>= 8;
                        }
                        for (int i = 0; i < fileAddressLength; i++)
                        {
                            resultWriterForText.Write((lengthA & 0xff).ToString("x2") + " ");//写入删除长度
                            lengthA >>= 8;
                        }
                        resultWriterForText.WriteLine();
                    }
                }
            }

            else if ((lengthA == 0) && (lengthB > 0))////////////////////////////////////////////////////////////////////////////////////////////插入
            {
                byte[] res = new byte[lengthB];
                fileReader2.Position = theLocationHeadHoldB;
                fileReader2.Read(res, 0, (int)lengthB);
                Console.Write(@"在{0:x}前增加数量为:{1}的元素：", theLocationHeadHoldA, lengthB);
                detailShow_textBox.AppendText(string.Format(@"在{0:x}前增加数量为:{1}的元素：", theLocationHeadHoldA, lengthB));
                for (int i = 0; i < lengthB; i++)
                {
                    Console.Write(@" {0:x}", res[i]);
                    detailShow_textBox.AppendText(string.Format(@" {0:x}", res[i]));
                }
                Console.WriteLine(@" ");
                detailShow_textBox.AppendText("\r\n");
                if (fileType == ".bin")
                {
                    //写入文件
                    //for (int i = 0; i < 2; i++)
                    resultWriter.WriteByte(0xff);//小节头
                    resultWriter.WriteByte(0x03);//类型符
                    for (int i = 0; i < fileAddressLength; i++)
                    {
                        resultWriter.WriteByte((byte)(theLocationHeadHoldA & 0xff));//写入theLocationHeadHoldA
                        theLocationHeadHoldA >>= 8;
                    }
                    for (int i = 0; i < fileAddressLength; i++)
                    {
                        resultWriter.WriteByte((byte)(lengthB & 0xff));//写入插入长度
                        lengthB >>= 8;
                    }
                    for (int i = 0; i < theLengthB; i++)//写入插入元素
                        resultWriter.WriteByte(res[i]);
                }
                else if (fileType == ".txt")
                {
                    //写入文件
                    //for (int i = 0; i < 2; i++)
                    resultWriterForText.Write(0xff.ToString("x2") + " ");//小节头
                    resultWriterForText.Write(0x03.ToString("x2") + " ");//类型符
                    for (int i = 0; i < fileAddressLength; i++)
                    {
                        resultWriterForText.Write((theLocationHeadHoldA & 0xff).ToString("x2") + " ");//写入theLocationHeadHoldA
                        theLocationHeadHoldA >>= 8;
                    }
                    for (int i = 0; i < fileAddressLength; i++)
                    {
                        resultWriterForText.Write((lengthB & 0xff).ToString("x2") + " ");//写入插入长度
                        lengthB >>= 8;
                    }
                    for (int i = 0; i < theLengthB; i++)//写入插入元素
                        resultWriterForText.Write(res[i].ToString("x2") + " ");
                    resultWriterForText.WriteLine();
                }
            }
            searchLoop = false;
        }

        static void WriteListToFile()
        {
            //finalWriter.WriteByte(fileAddressLength);

            if (fileType == ".bin")
            {
                //先写A链
                //finalWriter.WriteByte(0xaa);//a类标志头
                //finalWriter.WriteByte(0xaa);
                for (int i = 0; i < dataLineListA.Count; i++)//写入所有头值
                {
                    finalWriter.WriteByte(0xaa);
                    finalWriter.WriteByte(dataLineListA[i].commenValue);//写入头值

                    int positionCountA = dataLineListA[i].positionList.Count;//写入地址长度
                    for (int j = 0; j < fileAddressLength; j++)
                    {
                        finalWriter.WriteByte((byte)(positionCountA & 0xff));
                        positionCountA >>= 8;
                    }
                    for (int j = 0; j < dataLineListA[i].positionList.Count; j++)//写入所有地址
                    {
                        Int64 tempPosition = dataLineListA[i].positionList[j];
                        byte a = fileAddressLength;
                        while (a > 0)
                        {
                            finalWriter.WriteByte((byte)(tempPosition & 0xff));//左低右高
                            tempPosition >>= 8;
                            a--;
                        }
                    }
                }

                //再写B链
                //finalWriter.WriteByte(0xbb);//b类标志头
                //finalWriter.WriteByte(0xbb);
                for (int i = 0; i < dataLineListB.Count; i++)//写入所有头值
                {
                    finalWriter.WriteByte(0xbb);
                    finalWriter.WriteByte(dataLineListB[i].commenValue);//写入头值
                    finalWriter.WriteByte(dataLineListB[i].bbSecondValue);

                    int positionCountB = dataLineListB[i].positionList.Count;//写入地址长度
                    for (int j = 0; j < fileAddressLength; j++)
                    {
                        finalWriter.WriteByte((byte)(positionCountB & 0xff));
                        positionCountB >>= 8;
                    }
                    for (int j = 0; j < dataLineListB[i].positionList.Count; j++)//写入所有地址
                    {
                        Int64 tempPosition = dataLineListB[i].positionList[j];
                        byte b = fileAddressLength;
                        while (b > 0)
                        {
                            finalWriter.WriteByte((byte)(tempPosition & 0xff));//左低右高
                            tempPosition >>= 8;
                            b--;
                        }
                    }
                }
            }

            else if (fileType == ".txt")
            {
                resultWriterForText.WriteLine("aa aa ".ToString());//a类标志头

                for (int i = 0; i < dataLineListA.Count; i++)//写入所有头值
                {
                    //resultWriterForText.Write("aa ".ToString());
                    resultWriterForText.Write(dataLineListA[i].commenValue.ToString("x2") + " ");//写入头值

                    int positionCountA = dataLineListA[i].positionList.Count;//写入地址长度
                    for (int j = 0; j < fileAddressLength; j++)
                    {
                        resultWriterForText.Write((positionCountA & 0xff).ToString("x2") + " ");
                        positionCountA >>= 8;
                    }
                    for (int j = 0; j < dataLineListA[i].positionList.Count; j++)//写入所有地址
                    {
                        Int64 tempPosition = dataLineListA[i].positionList[j];
                        byte a = fileAddressLength;
                        while (a > 0)
                        {
                            resultWriterForText.Write((tempPosition & 0xff).ToString("x2") + " ");//左低右高
                            tempPosition >>= 8;
                            a--;
                        }
                    }
                    resultWriterForText.WriteLine();
                }

                resultWriterForText.WriteLine("bb bb ");//b类标志头
                for (int i = 0; i < dataLineListB.Count; i++)//写入所有头值
                {
                    //resultWriterForText.Write("bb ");
                    resultWriterForText.Write(dataLineListB[i].commenValue.ToString("x2") + " ");//写入头值
                    resultWriterForText.Write(dataLineListB[i].bbSecondValue.ToString("x2") + " ");

                    int positionCountB = dataLineListB[i].positionList.Count;//写入地址长度
                    for (int j = 0; j < fileAddressLength; j++)
                    {
                        resultWriterForText.Write((positionCountB & 0xff).ToString("x2") + " ");
                        positionCountB >>= 8;
                    }
                    for (int j = 0; j < dataLineListB[i].positionList.Count; j++)//写入所有地址
                    {
                        Int64 tempPosition = dataLineListB[i].positionList[j];
                        byte b = fileAddressLength;
                        while (b > 0)
                        {
                            resultWriterForText.Write((tempPosition & 0xff).ToString("x2") + " ");//左低右高
                            tempPosition >>= 8;
                            b--;
                        }
                    }
                    resultWriterForText.WriteLine();
                }
            }
            else
                Console.WriteLine(@"内部文件类型错误，尝试更改文件类型重新生成");
        }

        static bool ArrayEqul(byte[] a, byte[] b, int judgeCount)
        {
            if (judgeCount == 0)
                return false;
            bool boolTip = true;
            for (int i = 0; i < judgeCount; i++)
            {
                if (a[i] != b[i])
                    boolTip = false;
            }
            return boolTip;
        }


        //____________________________________________________________________________________________________解压

        static FileStream fileResultReader, fileBaseReader, fileWriter;
        static Int64 addressLength, tempLength;
        static List<byte> fileList;
        static int diff = 0;//这个参数非常重要，因为差异包里所有的地址描述都是以旧文件为准的，而在文件替换过程中会有增加和删除的部分，所有地址会有飘逸，而这个diff就是总的地址漂移量。
        static bool successTip = true;
        static string errorString;

        private void unzip_Click(object sender, EventArgs e)
        {
            Console.WriteLine("Hello World!");
            resultFileAddress = fileOutPut_textBox3.Text + resultFileName_textBox.Text + fileType;
            if(resultFileAddress == null)
            {
                unzipperLabel.Text = "压缩文件地址为空，请返回“生成压缩包”确认";
                return;
            }

            fileBaseReader = new FileStream(fileInPut_textBox1.Text, FileMode.Open, FileAccess.Read);//fileReader1;//new FileStream(@"C:\Users\Master\Desktop\upload\Test V17.bin", FileMode.Open, FileAccess.Read);
            fileResultReader = new FileStream(resultFileAddress, FileMode.Open, FileAccess.Read);//new FileStream(@"C:\Users\Master\Desktop\App1 Result.bin", FileMode.Open, FileAccess.Read);
            fileList = new List<byte>();
            BuildFileArray();
            fileResultReader.Position = 0;
            if (fileList.Count != fileBaseReader.Length)
            {
                unzipperLabel.Text = "转录list元素个数错误";
                Console.WriteLine(@"转录list元素个数错误");
                return;
            }
            fileBaseReader.Close();

            fileWriter = new FileStream(unzipper_textBox1.Text + unzipName_textBox4.Text, FileMode.Create, FileAccess.Write);
            BuiltFileList();

            fileResultReader.Position = 0;
            WriteToFile();
            fileResultReader.Close();
            fileWriter.Close();
            if (successTip == true)
                unzipperLabel.Text = "解压成功";
            else
                unzipperLabel.Text = errorString;
        }

        static void BuildFileArray()//读取tempFile
        {
            fileList.Clear();
            int temp = 0;//把old转录到tempFile中
            while ((temp = fileBaseReader.ReadByte()) != -1)
                fileList.Add((byte)temp);

            //修改tempFile
            fileResultReader.Position = 32;
            addressLength = fileResultReader.ReadByte();//获得地址长度

            {//第一次循环，替换单双元素
                int tempType = 0;
                while ((tempType = fileResultReader.ReadByte()) != -1)
                {
                    //先替换aa,bb开头的
                    if (tempType == 0xff)//先跳过ff部分
                    {
                        switch (fileResultReader.ReadByte())
                        {
                            case 0x01://替换
                                fileResultReader.Position += addressLength;//跳过地址
                                fileResultReader.Position += addressLength;//跳过被替换长度
                                tempLength = 0;//取长度（顺带跳过）
                                for (int i = 0; i < addressLength; i++)
                                    tempLength |= (Int64)((fileResultReader.ReadByte() & 0xff) << (8 * i));//读到
                                fileResultReader.Position += tempLength;//跳过元素长度
                                break;

                            case 0x02://删除
                                fileResultReader.Position += addressLength;//跳过地址
                                fileResultReader.Position += addressLength;//跳过被删除长度
                                break;

                            case 0x03://增加
                                fileResultReader.Position += addressLength;//跳过地址
                                tempLength = 0;//取增加长度（顺带跳过）
                                for (int i = 0; i < addressLength; i++)
                                    tempLength |= (Int64)((fileResultReader.ReadByte() & 0xff) << (8 * i));//读到
                                fileResultReader.Position += tempLength;//跳过元素长度
                                break;

                            default:
                                successTip = false;
                                errorString = "参数错误，退出1";
                                Console.WriteLine(@"参数错误，退出1");
                                return;
                        }
                    }

                    else if (tempType == 0xaa)//单元素替换
                    {
                        byte tempValueA = (byte)fileResultReader.ReadByte();//取主元素
                        Int64 positionCount = 0;//取地址个数
                        for (int i = 0; i < addressLength; i++)
                            positionCount |= (Int64)((fileResultReader.ReadByte() & 0xff) << (i * 8));
                        while (positionCount > 0)//取每个地址
                        {
                            Int64 tempPositionA = 0;
                            for (int i = 0; i < addressLength; i++)
                                tempPositionA |= (Int64)((fileResultReader.ReadByte() & 0xff) << (i * 8));
                            if (tempPositionA >= fileBaseReader.Length)
                            {
                                successTip = false;
                                errorString = "读取到错误长度1";
                                Console.WriteLine(@"读取到错误长度1");
                                return;
                            }
                            positionCount--;

                            fileList[(int)tempPositionA] = tempValueA;//写入更改值，替换源文件中值
                        }
                    }

                    else if (tempType == 0xbb)//双元素替换
                    {
                        byte tempValueB1 = (byte)fileResultReader.ReadByte();//取主元素
                        byte tempValueB2 = (byte)fileResultReader.ReadByte();//取主元素
                        Int64 positionCount = 0;//取地址个数
                        for (int i = 0; i < addressLength; i++)
                            positionCount |= (Int64)((fileResultReader.ReadByte() & 0xff) << (i * 8));
                        while (positionCount > 0)//取每个地址
                        {
                            Int64 tempPositionB = 0;
                            for (int i = 0; i < addressLength; i++)
                                tempPositionB |= (Int64)((fileResultReader.ReadByte() & 0xff) << (i * 8));
                            if (tempPositionB >= fileBaseReader.Length)
                            {
                                successTip = false;
                                errorString = "读取到错误长度2";
                                Console.WriteLine(@"读取到错误长度2");
                                return;
                            }
                            positionCount--;

                            fileList[(int)tempPositionB] = tempValueB1;//写入更改值，替换源文件中值
                            fileList[(int)tempPositionB + 1] = tempValueB2;//写入更改值，替换源文件中值
                        }
                    }

                    else
                    {
                        successTip = false;
                        errorString = "元素头搜索错误2";
                        Console.WriteLine(@"元素头搜索错误2");
                        return;
                    }
                }
            }
        }

        static void BuiltFileList()//把FRR里内容合并到result里
        {
            fileResultReader.Position = 33;//去文件头

            {//第二次循环，更改其他元素以及赋值到结果
                int tempType = 0;
                while ((tempType = fileResultReader.ReadByte()) != -1)
                {
                    if (tempType == 0xff)//更改ff部分
                    {
                        switch (fileResultReader.ReadByte())
                        {
                            case 0x01://替换
                                {
                                    int basePosition = 0;
                                    int baseLength = 0;
                                    int resultLentgh = 0;
                                    for (int i = 0; i < addressLength; i++)//被替换地址
                                        basePosition |= ((fileResultReader.ReadByte() & 0xff) << (8 * i));

                                    for (int i = 0; i < addressLength; i++)//被替换长度
                                        baseLength |= ((fileResultReader.ReadByte() & 0xff) << (8 * i));
                                    for (int i = 0; i < addressLength; i++)//替换长度
                                        resultLentgh |= ((fileResultReader.ReadByte() & 0xff) << (8 * i));

                                    basePosition += diff;
                                    fileList.RemoveRange(basePosition, baseLength);//先删除
                                    diff -= baseLength;

                                    byte[] temp = new byte[resultLentgh];
                                    for (int i = 0; i < resultLentgh; i++)//再添加
                                        temp[i] = (byte)fileResultReader.ReadByte();
                                    fileList.InsertRange(basePosition, temp);
                                    diff += resultLentgh;
                                }
                                break;

                            case 0x02://删除
                                {
                                    int basePosition = 0;
                                    int baseLength = 0;
                                    for (int i = 0; i < addressLength; i++)//被删除地址
                                        basePosition |= ((fileResultReader.ReadByte() & 0xff) << (8 * i));
                                    for (int i = 0; i < addressLength; i++)//被删除长度
                                        baseLength |= ((fileResultReader.ReadByte() & 0xff) << (8 * i));

                                    basePosition += diff;
                                    fileList.RemoveRange(basePosition, baseLength);//删除
                                    diff -= baseLength;
                                }
                                break;

                            case 0x03://增加
                                {
                                    int basePosition = 0;
                                    int resultLentgh = 0;
                                    for (int i = 0; i < addressLength; i++)//被增加地址
                                        basePosition |= ((fileResultReader.ReadByte() & 0xff) << (8 * i));
                                    basePosition += diff;
                                    for (int i = 0; i < addressLength; i++)//增加长度
                                        resultLentgh |= ((fileResultReader.ReadByte() & 0xff) << (8 * i));

                                    byte[] temp = new byte[resultLentgh];
                                    for (int i = 0; i < resultLentgh; i++)//添加新元素
                                        temp[i] = (byte)fileResultReader.ReadByte();
                                    fileList.InsertRange(basePosition, temp);
                                    diff += resultLentgh;


                                }
                                break;

                            default:
                                successTip = false;
                                errorString = "参数错误，退出1";
                                Console.WriteLine(@"参数错误，退出1");
                                return;
                        }
                    }

                    else if (tempType == 0xaa)//单元素替换
                    {
                        byte tempValueA = (byte)fileResultReader.ReadByte();//取主元素
                        Int64 positionCount = 0;//取地址个数
                        for (int i = 0; i < addressLength; i++)
                            positionCount |= (Int64)((fileResultReader.ReadByte() & 0xff) << (i * 8));

                        fileResultReader.Position += (positionCount * addressLength);//跳过地址
                    }

                    else if (tempType == 0xbb)//双元素替换
                    {
                        byte tempValueB1 = (byte)fileResultReader.ReadByte();//取主元素
                        byte tempValueB2 = (byte)fileResultReader.ReadByte();//取主元素
                        Int64 positionCount = 0;//取地址个数
                        for (int i = 0; i < addressLength; i++)
                            positionCount |= (Int64)((fileResultReader.ReadByte() & 0xff) << (i * 8));

                        fileResultReader.Position += (positionCount * addressLength);//跳过地址
                    }

                    else
                    {
                        successTip = false;
                        errorString = "元素头搜索错误3";
                        Console.WriteLine(@"元素头搜索错误3");
                        return;
                    }
                }

                if (fileResultReader.Position > fileResultReader.Length)
                {
                    successTip = false;
                    errorString = "错误3";
                    Console.WriteLine(@"错误3");
                }
            }
        }
        static void WriteToFile()
        {
            fileWriter.Position = 0;

            for (int i = 0; i < fileList.Count; i++)
                fileWriter.WriteByte(fileList[i]);
        }

        //________________________________________________________________________________验证


        private void search_textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void check_Click(object sender, EventArgs e)
        {
            checkDetail_textBox1.Clear();
            checkResult_textBox5.Clear();
            if (unzipName_textBox4.TextLength == 0)
                unzipName_textBox4.Text = resultFileName_textBox.Text;
            if(unzipName_textBox4.TextLength == 0)
            {
                errorProvider1.SetError(label13, "请输入名称");
                return;
            }
            unzipName = unzipName_textBox4.Text;

            if (fileAdd2_ok == true)
            {
                
                //结果地址
                fileType = type_comboBox1.SelectedItem.ToString();
                resultFileAddress = fileOutPut_textBox3.Text + unzipName + "_check" + fileType;
                fileReader1 = new FileStream(fileInPut_textBox2.Text, FileMode.Open, FileAccess.Read);
                fileReader2 = new FileStream(unzipper_textBox1.Text + unzipName_textBox4.Text, FileMode.Open, FileAccess.Read);
                //finalWriter = resultWriter; //new FileStream(@"C:\Users\Master\Desktop\finalWriter.bin", FileMode.Create, FileAccess.Write);
                if (fileType == ".txt")
                {
                    resultWriterForText = new StreamWriter(resultWriter, Encoding.UTF8);
                    resultWriterForText.AutoFlush = true;
                }

                Console.WriteLine(@"格        式：{0}", 0);
                checkDetail_textBox1.AppendText(string.Format(@"格        式：{0}", 0) + "\r\n");
                Console.WriteLine(@"厂 商 代 码 ：{0}", 0);
                checkDetail_textBox1.AppendText(string.Format(@"厂 商 代 码 ：{0}", 0) + "\r\n");
                Console.WriteLine(@"目 标 版 本 ：{0}", 0);
                checkDetail_textBox1.AppendText(string.Format(@"目 标 版 本 ：{0}", 0) + "\r\n");
                Console.WriteLine(@"升级包校验码：{0}", 0);
                checkDetail_textBox1.AppendText(string.Format(@"升级包校验码：{0}", 0) + "\r\n");
                Console.WriteLine(@"升级后校验码：{0}", 0);
                checkDetail_textBox1.AppendText(string.Format(@"升级后校验码：{0}", 0) + "\r\n");
                Console.WriteLine(@"保        留：{0}\r\n", 0);
                checkDetail_textBox1.AppendText(string.Format(@"保        留：{0}", 0) + "\r\n\r\n");

                //写入头

                checkTip = true;
                dataLineListA = new List<DataLineStruct>();
                dataLineListB = new List<DataLineStruct>();
                dataLineListA.Clear();
                dataLineListB.Clear();
                play();
                //AddSameElements();
                WriteListToFile();
                start.Enabled = true;
                search.Enabled = true;
                fileReader1.Close();
                fileReader2.Close();
                checkDetail_textBox1.AppendText("\r\n");
                checkDetail_textBox1.AppendText("内容包数量：" + massageCount);
                if (massageCount == 0)
                    checkResult_textBox5.Text = "无差异，验证正确";
                else
                    checkResult_textBox5.Text = "验证错误，请确认解压或验证过程中是否文件有变化，或验证差异原因";
                checkTip = false;
            }
            else
                errorProvider1.SetError(label7, "请选择地址");
        }

        static int ckeckMassageCount = 0;
        static bool checkSuccess = true;
        private void CheckRecordAndQuit(Int64 theLocationHeadHoldA, Int64 theLocationHeadHoldB, Int64 theLocationRootA, Int64 theLocationRootB, int type)
        {
            checkSuccess = false;
            ckeckMassageCount++;
            Int64 lengthA = theLocationRootA - theLocationHeadHoldA;
            Int64 lengthB = theLocationRootB - theLocationHeadHoldB; //int i = 0;
            Int64 theLengthA = lengthA;
            Int64 theLengthB = lengthB;
            Int64 theHoldA = theLocationHeadHoldA;

            if ((lengthA > 0) && (lengthB > 0))////////////////////////////////////////////////////////////////////////////////////////////////替换
            {
                byte[] res = new byte[lengthB];
                fileReader2.Position = theLocationHeadHoldB;
                fileReader2.Read(res, 0, (int)lengthB);
                Console.Write(@"从:{0:x}(包括)到:{1:x}(不包括)的{2}个元素替换为{3}个元素：", theLocationHeadHoldA, theLocationRootA, lengthA, lengthB);
                checkDetail_textBox1.AppendText(string.Format(@"从:{0:x}(包括)到:{1:x}(不包括)的{2}个元素替换为{3}个元素：", theLocationHeadHoldA, theLocationRootA, lengthA, lengthB));
                for (int i = 0; i < lengthB; i++)
                {
                    Console.Write(@" {0:x}", res[i]);
                    checkDetail_textBox1.AppendText(string.Format(@" {0:x}", res[i]));
                }
                Console.WriteLine(@" ");
                checkDetail_textBox1.AppendText("\r\n");
            }

            else if ((lengthA > 0) && (lengthB == 0))////////////////////////////////////////////////////////////////////////////////////////////删除
            {
                Console.WriteLine(@"删除:{0:x}（包括）到:{1:x}(不包括)后面长度为:{2}的元素", theLocationHeadHoldA, theLocationRootA, lengthA);
                checkDetail_textBox1.AppendText(string.Format(@"删除:{0:x}（包括）到:{1:x}(不包括)后面长度为:{2}的元素", theLocationHeadHoldA, theLocationRootA, lengthA) + "\r\n");
            }

            else if ((lengthA == 0) && (lengthB > 0))////////////////////////////////////////////////////////////////////////////////////////////插入
            {
                byte[] res = new byte[lengthB];
                fileReader2.Position = theLocationHeadHoldB;
                fileReader2.Read(res, 0, (int)lengthB);
                Console.Write(@"在{0:x}前增加数量为:{1}的元素：", theLocationHeadHoldA, lengthB);
                checkDetail_textBox1.AppendText(string.Format(@"在{0:x}前增加数量为:{1}的元素：", theLocationHeadHoldA, lengthB));
                for (int i = 0; i < lengthB; i++)
                {
                    Console.Write(@" {0:x}", res[i]);
                    checkDetail_textBox1.AppendText(string.Format(@" {0:x}", res[i]));
                }
                Console.WriteLine(@" ");
                checkDetail_textBox1.AppendText("\r\n");
            }
            searchLoop = false;
        }
    }
}
