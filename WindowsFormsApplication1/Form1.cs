using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text.RegularExpressions;
using MySql.Data.MySqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic.FileIO;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        String finalclass = "";
        decimal maxvalue = 0;
        List<string> updatedb = new List<string>();
        static List<string> GetCombination(List<string> list)
        {

            List<string> newlist = new List<string>();
            double count = Math.Pow(2, list.Count);
            for (int i = 1; i <= count - 1; i++)
            {
                string values = "";
                string str = Convert.ToString(i, 2).PadLeft(list.Count, '0');
                for (int j = 0; j < str.Length; j++)
                {
                    if (str[j] == '1')
                    {
                        values = values + list[j] + ",";
                        values = Regex.Replace(values, @"\t|\n|\r", "");
                    }
                }
                newlist.Add(values);
            }
            return newlist;
        }
        private static DataTable GetData(string sqlCommand)
        {
            MySqlConnection dbConnection = new MySqlConnection("server=localhost;uid=root;pwd=root;database=nullvalues;");
            dbConnection.Open();
            MySqlCommand command = new MySqlCommand(sqlCommand,dbConnection );
            MySqlDataAdapter adapter = new MySqlDataAdapter();
            adapter.SelectCommand = command;

            DataTable table = new DataTable();
            table.Locale = System.Globalization.CultureInfo.InvariantCulture;
            adapter.Fill(table);

            return table;
        }
        static IEnumerable<string> SortByLength(IEnumerable<string> e)
        {
            var sorted = from s in e
                         orderby s.Length descending
                         select s;
            return sorted;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFile1 = new OpenFileDialog();


            openFile1.Filter = "Comma Seperated Files|*.csv";


            if (openFile1.ShowDialog() == System.Windows.Forms.DialogResult.OK)


                richTextBox1.LoadFile(openFile1.FileName,
                RichTextBoxStreamType.PlainText);
            int firstVisibleChar = richTextBox1.GetCharIndexFromPosition(new Point(0, 0));
            int lineIndex = richTextBox1.GetLineFromCharIndex(firstVisibleChar);
            string firstVisibleLine = richTextBox1.Lines[lineIndex];
            String[] ColoumnName = firstVisibleLine.Split(',');
            string last = ColoumnName[ColoumnName.Length - 1];

            using (MySqlConnection dbConnection = new MySqlConnection("server=localhost;uid=root;pwd=root;database=nullvalues;"))
            {
                
                dbConnection.Open();
                string tabledrop = "drop table if exists table1";
                MySqlCommand cmd3 = new MySqlCommand(tabledrop, dbConnection);
                cmd3.ExecuteNonQuery(); 
                string sql = "CREATE TABLE table1(abc varchar(40))";
                MySqlCommand cmd = new MySqlCommand(sql, dbConnection);
                cmd.ExecuteNonQuery();
                string drop = "ALTER TABLE table1 CHANGE COLUMN abc " + ColoumnName[0].Replace(" ", "_") + " varchar(40)";
                MySqlCommand cmd1 = new MySqlCommand(drop, dbConnection);
                cmd1.ExecuteNonQuery();
                for (int i = 1; i < ColoumnName.Length; i++)
                {
                    string alter = "ALTER TABLE table1 ADD COLUMN " + ColoumnName[i].Replace(" ", "_") + "  varchar(40)";
                    MySqlCommand cmd2 = new MySqlCommand(alter, dbConnection);
                    cmd2.ExecuteNonQuery();
                }

                MySqlBulkLoader s = new MySqlBulkLoader(dbConnection);

                s.TableName = "table1";
                s.FieldTerminator = ",";
                s.LineTerminator = "\n";
                s.FileName = openFile1.FileName;
                s.NumberOfLinesToSkip = 1;
                try
                {
                    int count = s.Load();
                    Console.WriteLine(count + " lines uploaded.");
                    dbConnection.Close();

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
            
            //dataGridView1.Dock = DockStyle.Fill;
            dataGridView1.AutoGenerateColumns = true;
            dataGridView1.DataSource = GetData("Select * From table1");
            dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCellsExceptHeaders;
           // this.table1TableAdapter.Fill(this.nullvaluesDataSet.table1);
        }
        //rpocessing the values in the db for null values
        private void button2_Click(object sender, EventArgs e)
        {
            string classheader = "";
            int testrow = 0, testcol = 0;
            for (int col = 0; col < dataGridView1.Columns.Count; col++)
            {
                for (int rows = 0; rows < dataGridView1.Rows.Count - 1; rows++)
                {
                    if (dataGridView1.Rows[rows].Cells[col].Value.ToString() == "" || dataGridView1.Rows[rows].Cells[col].Value.ToString() == "\r")
                    {
                        testrow = rows; testcol = col;
                        textBox1.Text = rows + " " + col;
                        classheader = dataGridView1.Columns[col].HeaderText.ToString();


                        //textBox1.AppendText(dataGridView1.Rows[rows].Cells[col].Value.ToString());
                        string[] nullindex = textBox1.Text.Split();
                        List<string> newlist = new List<string>();
                        List<string> ColumnList = new List<string>();
                        foreach (DataGridViewColumn column in dataGridView1.Columns)
                        {
                            if (column.Index == Convert.ToInt32(nullindex[1]))
                                continue;
                            
                            if (dataGridView1.Rows[Convert.ToInt32(nullindex[0])].Cells[column.Index].Value.ToString() == "")
                                continue;
                            if (dataGridView1.Rows[Convert.ToInt32(nullindex[0])].Cells[column.Index].Value.ToString() == "\r")
                                continue;
                            newlist.Add(column.HeaderText);
                        }

                        ColumnList = GetCombination(newlist);
                        ColumnList = SortByLength(ColumnList).ToList();

                        //richTextBox1.Text = string.Join(Environment.NewLine, SortByLength(ColumnList));


                        List<String> ValuesList = new List<string>();
                        List<String> ValuesListCombinations = new List<string>();
                        String Values = "";
                        for (int i = 0; i < dataGridView1.Columns.Count; i++)
                        {
                            if (i == Convert.ToInt32(nullindex[1]))
                                continue;
                            if (dataGridView1.Rows[Convert.ToInt32(nullindex[0])].Cells[i].Value.ToString() == "")
                                continue;
                            if (dataGridView1.Rows[Convert.ToInt32(nullindex[0])].Cells[i].Value.ToString() == "\r")
                                continue;
                            ValuesList.Add(dataGridView1.Rows[Convert.ToInt32(nullindex[0])].Cells[i].Value.ToString());
                            // Values = Values + dataGridView1.Rows[Convert.ToInt32(nullindex[0])].Cells[i].Value.ToString();
                        }
                        ValuesListCombinations = GetCombination(ValuesList);
                        ValuesListCombinations = SortByLength(ValuesListCombinations).ToList();

                        //richTextBox2.Text = string.Join(Environment.NewLine, ValuesListCombinations);
                        for (int i = 0; i < ValuesListCombinations.Count; i++)
                        {
                            
                            string[] colm = ColumnList[i].Split(',');
                            colm = colm.Where(x => !string.IsNullOrEmpty(x)).ToArray();
                            string[] val = ValuesListCombinations[i].Split(',');
                            val = val.Where(x => !string.IsNullOrEmpty(x)).ToArray();
                            List<string> querycol = new List<string>();
                            List<string> queryval = new List<string>();
                            for (int j = 0; j < val.Length; j++)
                            {
                                querycol.Add(colm[j]);
                                queryval.Add(val[j]);
                            }
                            List<string> colheader=new List<string>() ;
                            for (int k = 0; k < dataGridView1.Columns.Count; k++)
                                 colheader.Add(dataGridView1.Columns[k].HeaderText.ToString());

                            String rand = "";
                            String notnull = "";
                            for(int k=0;k<colheader.Count-1;k++)
                                notnull=notnull+colheader[k]+"!=' ' AND ";
                            notnull = notnull + colheader[colheader.Count - 1]+"!=' '";
                               
                            for (int j = 0; j < queryval.Count; j++)
                                rand = rand + querycol[j] + "= '" + queryval[j] + "' AND ";

                            using (MySqlConnection dbConnection = new MySqlConnection("server=localhost;uid=root;pwd=root;database=nullvalues;"))
                            {
                                List<String> difclass = new List<string>();
                                dbConnection.Open();
                                string trim=dataGridView1.Columns[dataGridView1.Columns.Count - 1].HeaderText.ToString();
                                string removenextline = "update table1 set "+trim+" =TRIM(trailing '\r' from "+trim+")";
                                MySqlCommand cmd = new MySqlCommand(removenextline, dbConnection);
                                cmd.ExecuteNonQuery();
                                string count = "select count('" + classheader + "') from table1 where " + rand + " "+notnull+"";
                                MySqlCommand cmd2 = new MySqlCommand(count, dbConnection);
                                MySqlDataReader rdr = cmd2.ExecuteReader();
                                int totalcount = 0;
                                while (rdr.Read())
                                {
                                    totalcount = Convert.ToInt32(rdr[0]);
                                }
                                rdr.Close();

                                //string test= "select Column_1,count(Column_1) from table1 where Column_2='b' AND Column_3='c' AND Column_4='d' AND Column_1!=' ' group by Column_1";
                                string findclass = "select " + classheader + ",count('" + classheader + "') from table1 where " + rand + " " + notnull + " group by " + classheader + "";
                                MySqlCommand cmd1 = new MySqlCommand(findclass, dbConnection);
                                rdr = cmd1.ExecuteReader();
                                

                                KeyValuePair<string, decimal> dic1 = new KeyValuePair<string, decimal>();
                                Dictionary<String, decimal> newdic = new Dictionary<string, decimal>();
                                while (rdr.Read())
                                {
                                    newdic[Convert.ToString(rdr[0])] = Convert.ToDecimal(rdr[1]) / totalcount * 100;

                                }
                                dic1 = newdic.FirstOrDefault(x => x.Value == newdic.Values.Max());
                                if (dic1.Value > maxvalue)
                                {
                                    finalclass = dic1.Key;
                                    maxvalue = dic1.Value;
                                }
                            }
                        }
 
                       textBox2.AppendText(finalclass + " - " + maxvalue.ToString()+" ");
                        String query = null;
                        foreach (DataGridViewColumn Column in dataGridView1.Columns)
                        { if (Column.HeaderText == classheader)
                                continue;
                            if (dataGridView1.Rows[Convert.ToInt32(nullindex[0])].Cells[Column.Index].Value.ToString() == "")
                                continue; 
                            query = query + " AND " + Column.HeaderText + "='" + dataGridView1.Rows[Convert.ToInt32(nullindex[0])].Cells[Column.Index].Value.ToString() + "'";
                            query = Regex.Replace(query, @"\t|\n|\r", "");
                        }
                        updatedb.Add("update table1 set " + classheader + "='" + finalclass + "' where " + classheader + "=' '" + query);
                       finalclass = null;
                       maxvalue = 0;
                    }
                    
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            using (MySqlConnection dbConnection = new MySqlConnection("server=localhost;uid=root;pwd=root;database=nullvalues;"))
            {
                dbConnection.Open();
                for (int i = 0; i < updatedb.Count; i++)
                {
                    MySqlCommand cmd = new MySqlCommand(updatedb[i], dbConnection);
                    cmd.ExecuteNonQuery();
                }
                dataGridView1.AutoGenerateColumns = true;
                dataGridView1.DataSource = GetData("Select * From table1");
                dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCellsExceptHeaders;
                dataGridView1.Refresh();
            }
        }
    }
}
     
