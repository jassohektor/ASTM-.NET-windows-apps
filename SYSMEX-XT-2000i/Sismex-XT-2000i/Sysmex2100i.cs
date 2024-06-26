using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;
using System.IO;
using System.Data.SqlClient;

namespace Sismex_xs_100i
{
    public partial class Sysmex2100i : Form
    {
        public Sysmex2100i()
        {
            InitializeComponent();          
        }

        string error = "";
        string muestra = string.Empty;
        int ex, ey;
        bool _existeInterfaz;
        bool arrastre;
        string strConection;
        string mensaje, character, texto; // = string.Empty;
        char ENQ = Convert.ToChar(5); //♣
        char ACK = Convert.ToChar(6); //♠
        char NAK = Convert.ToChar(21); //§
        char ETX = Convert.ToChar(3); //♥
        char EOT = Convert.ToChar(4); //♦
        char STX = Convert.ToChar(2); //☻
        char ETB = Convert.ToChar(23); //↨
        char LF = Convert.ToChar(10);  //◙
        char CR = Convert.ToChar(13); //♪

        private void port_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
              do
              {
                  char chrRecibido = '\'';

                  chrRecibido = Convert.ToChar(port.ReadChar());

                  if (chrRecibido.Equals(STX))
                      port.Write(ACK.ToString());

                  if (chrRecibido.Equals(STX) )
                  {
                       this.mensaje = this.texto = string.Empty;                      
                  }      



                   mensaje += chrRecibido.ToString();

                   switch (Convert.ToInt32(chrRecibido))
                   {
                       case 2:
                           character = "<STX>";
                           break;
                       case 3:
                           character = "<ETX>";
                           break;
                       case 4:
                           character = "<EOT>";
                           break;
                       case 5:
                           character = "<ENQ>";
                           break;
                       case 6:
                           character = "<ACK>";
                           break;
                       case 10:
                           character = "<LF>";
                           break;
                       case 13:
                           character = "<CR>";
                           break;
                       case 21:
                           character = "<NAK>";
                           break;
                       case 23:
                           character = "<ETB>";
                           break;
                       default:
                           character = "";
                           break;
                   }

                   if (character != string.Empty)
                   {
                       texto += chrRecibido.ToString();
                   }
                   else
                   {
                       texto += chrRecibido.ToString();
                   }

                   if (chrRecibido.Equals(ETX))
                   {
                       this.Invoke(new MethodInvoker(addResults));
                   }
              }
              while (!((port.BytesToRead == 0))); 
        }


        void addResults()
        {
            this.lstResultados.ForeColor = Color.LimeGreen;
            lstResultados.Items.Add(new ListViewItem(texto));
            obtainSample();
        }

        public static bool IsNumeric(object Expression)
        {
            bool isNum;
            double retNum;
            isNum = Double.TryParse(Convert.ToString(Expression), System.Globalization.NumberStyles.Any, System.Globalization.NumberFormatInfo.InvariantInfo, out retNum);
            return isNum;
        }

        public static String ntest(string sample)
        {
            for (int x = 0; x <= sample.Length - 1; x++)
            {
                if (!IsNumeric(sample[x].ToString()) & sample[x].ToString() != ".")
                {
                    sample = sample.Replace(sample[x].ToString(), "0");
                }
            }
            return sample;
        }

        void obtainSample()
        {
            try
            {
                string encabezado = mensaje.Replace(ETX.ToString(), "");
                encabezado = encabezado.Replace(STX.ToString(), "");
                if (encabezado.Substring(0, 3) == "D1U")
                {
                    muestra = encabezado.Substring(32, 15).Trim();

                    if (muestra == string.Empty) muestra = encabezado.Substring(71, 15).Trim();

                    string codigo = muestra.Substring(0, 2);

                    bool isNumeric;
                    double i;
                    isNumeric = double.TryParse(codigo, out i);

                    if (!isNumeric)
                    {
                        muestra = "00" + muestra.Substring(2);
                    }
                }
                if (encabezado.Substring(0, 3).Contains("D2U"))
                {
                    /* TENGO QUE AGREGAR ESTAS 2 PRUEBAS  IG#, IG% */
                    try
                    {
                        string ggato = encabezado.Substring(218, 2).Insert(0, "."); // 010 GRANULOCITOS INMADUROS #_
                        ggato = ntest(ggato);
                        decimal gato = Convert.ToDecimal(ggato.Replace("?","0"));
                        sendResult(muestra, "IG#", gato);

                        string gporc = encabezado.Substring(223, 2).Insert(1, "."); // 0020 GRANULOCITOS INMADUROS % nomas checa que si sean esas poiusiciones
                        gporc = ntest(gporc);
                        decimal porc = Convert.ToDecimal(gporc.Replace("?", "0"));
                        sendResult(muestra, "IG%", porc);
                    }
                    catch (Exception ex)
                    {
                        error = ex.Message.ToString();
                    }
                    finally
                    {
                    }
                    
                    encabezado = encabezado.Substring(32);
                    encabezado = encabezado.Substring(15);

                    try
                    {
                        string swbc = encabezado.Substring(0, 5).Insert(3, ".");
                        swbc = ntest(swbc);
                        decimal wbc = Convert.ToDecimal(swbc);
                        sendResult(muestra, "WBC", wbc);
                    }
                    catch (Exception ex)
                    {
                        error = ex.Message.ToString();
                    }
                    finally
                    {
                    }

                    encabezado = encabezado.Substring(6);
                    try
                    {
                        string srbc = encabezado.Substring(0, 4).Insert(2, ".");
                        srbc = ntest(srbc);
                        decimal rbc = Convert.ToDecimal(srbc);
                        sendResult(muestra, "RBC", rbc);
                    }
                    catch (Exception ex)
                    {
                        error = ex.Message.ToString();
                    }
                    finally
                    {
                    }


                    encabezado = encabezado.Substring(5);
                    try
                    {
                        string shgb = encabezado.Substring(0, 4).Insert(3, ".");
                        shgb = ntest(shgb);
                        decimal hgb = Convert.ToDecimal(shgb);
                        sendResult(muestra, "HGB", hgb);
                    }
                    catch (Exception ex)
                    {
                        error = ex.Message.ToString();
                    }
                    finally
                    {
                    }

                    encabezado = encabezado.Substring(5);
                    try
                    {
                        string shct = encabezado.Substring(0, 4).Insert(3, ".");
                        shct = ntest(shct);
                        decimal hct = Convert.ToDecimal(shct);
                        sendResult(muestra, "HCT", hct);
                    }
                    catch (Exception ex)
                    {
                        error = ex.Message.ToString();
                    }
                    finally
                    {
                    }


                    encabezado = encabezado.Substring(5);
                    try
                    {
                        string smvc = encabezado.Substring(0, 4).Insert(3, ".");
                        smvc = ntest(smvc);
                        decimal mvc = Convert.ToDecimal(smvc);
                        sendResult(muestra, "MCV", mvc);
                    }
                    catch (Exception ex)
                    {
                        error = ex.Message.ToString();
                    }
                    finally
                    {
                    }

                    encabezado = encabezado.Substring(5);
                    try
                    {
                        string smch = encabezado.Substring(0, 4).Insert(3, ".");
                        smch = ntest(smch);
                        decimal mch = Convert.ToDecimal(smch);
                        sendResult(muestra, "MCH", mch);
                    }
                    catch (Exception ex)
                    {
                        error = ex.Message.ToString();
                    }
                    finally
                    {
                    }

                    encabezado = encabezado.Substring(5);
                    try
                    {
                        string smchc = encabezado.Substring(0, 4).Insert(3, ".");
                        smchc = ntest(smchc);
                        decimal mchc = Convert.ToDecimal(smchc);
                        sendResult(muestra, "MCHC", mchc);
                    }
                    catch (Exception ex)
                    {
                        error = ex.Message.ToString();
                    }
                    finally
                    {
                    }

                    encabezado = encabezado.Substring(5);
                    try
                    {
                        string splt = encabezado.Substring(0, 4);
                        splt = ntest(splt);
                        decimal plt = Convert.ToDecimal(splt);
                        sendResult(muestra, "PLT", plt);
                    }
                    catch (Exception ex)
                    {
                        error = ex.Message.ToString();
                    }
                    finally
                    {
                    }

                    encabezado = encabezado.Substring(5);
                    try
                    {
                        string slymph_ = encabezado.Substring(0, 4).Insert(3, ".");
                        slymph_ = ntest(slymph_);
                        decimal lymph_ = Convert.ToDecimal(slymph_);
                        sendResult(muestra, "LYMPH%", lymph_);
                    }
                    catch (Exception ex)
                    {
                        error = ex.Message.ToString();
                    }
                    finally
                    {
                    }

                    encabezado = encabezado.Substring(5);
                    try
                    {
                        string smono_ = encabezado.Substring(0, 4).Insert(3, ".");
                        smono_ = ntest(smono_);
                        decimal mono_ = Convert.ToDecimal(smono_);
                        sendResult(muestra, "MONO%", mono_);
                    }
                    catch (Exception ex)
                    {
                        error = ex.Message.ToString();
                    }
                    finally
                    {
                    }

                    encabezado = encabezado.Substring(5);
                    try
                    {
                        string sneut_ = encabezado.Substring(0, 4).Insert(3, ".");
                        sneut_ = ntest(sneut_);
                        decimal neut_ = Convert.ToDecimal(sneut_);
                        sendResult(muestra, "NEUT%", neut_);
                    }
                    catch (Exception ex)
                    {
                        error = ex.Message.ToString();
                    }
                    finally
                    {
                    }

                    encabezado = encabezado.Substring(5);
                    try
                    {
                        string seo_ = encabezado.Substring(0, 4).Insert(3, ".");
                        seo_ = ntest(seo_);
                        decimal eo_ = Convert.ToDecimal(seo_);
                        sendResult(muestra, "EO%", eo_);
                    }
                    catch (Exception ex)
                    {
                        error = ex.Message.ToString();
                    }
                    finally
                    {
                    }

                    encabezado = encabezado.Substring(5);
                    try
                    {
                        string sbaso_ = encabezado.Substring(0, 4).Insert(3, ".");
                        sbaso_ = ntest(sbaso_);
                        decimal baso_ = Convert.ToDecimal(sbaso_);
                        sendResult(muestra, "BASO%", baso_);
                    }
                    catch (Exception ex)
                    {
                        error = ex.Message.ToString();
                    }
                    finally
                    {
                    }


                    encabezado = encabezado.Substring(6);
                    try
                    {
                        string _slymph = encabezado.Substring(0, 4).Insert(2, ".");
                        _slymph = ntest(_slymph);
                        decimal _lymph = Convert.ToDecimal(_slymph);
                        sendResult(muestra, "LYMPH#", _lymph);
                    }
                    catch (Exception ex)
                    {
                        error = ex.Message.ToString();
                    }
                    finally
                    {
                    }

                    encabezado = encabezado.Substring(6);
                    try
                    {
                        string _smono = encabezado.Substring(0, 4).Insert(2, ".");
                        _smono = ntest(_smono);
                        decimal _mono = Convert.ToDecimal(_smono);
                        sendResult(muestra, "MONO#", _mono);
                    }
                    catch (Exception ex)
                    {
                        error = ex.Message.ToString();
                    }
                    finally
                    {
                    }

                    encabezado = encabezado.Substring(6);
                    try
                    {
                        string _sneut = encabezado.Substring(0, 4).Insert(2, ".");
                        _sneut = ntest(_sneut);
                        decimal _neut = Convert.ToDecimal(_sneut);
                        sendResult(muestra, "NEUT#", _neut);
                    }
                    catch (Exception ex)
                    {
                        error = ex.Message.ToString();
                    }
                    finally
                    {
                    }

                    encabezado = encabezado.Substring(6);
                    try
                    {
                        string _seo = encabezado.Substring(0, 4).Insert(2, ".");
                        _seo = ntest(_seo);
                        decimal _eo = Convert.ToDecimal(_seo);
                        sendResult(muestra, "EO#", _eo);
                    }
                    catch (Exception ex)
                    {
                        error = ex.Message.ToString();
                    }
                    finally
                    {
                    }

                    encabezado = encabezado.Substring(6);
                    try
                    {
                        string _sbaso = encabezado.Substring(0, 4).Insert(2, ".");
                        _sbaso = ntest(_sbaso);
                        decimal _baso = Convert.ToDecimal(_sbaso);
                        sendResult(muestra, "BASO#", _baso);
                    }
                    catch (Exception ex)
                    {
                        error = ex.Message.ToString();
                    }
                    finally
                    {
                    }


                    encabezado = encabezado.Substring(6);
                    try
                    {
                        string _srwdcv = encabezado.Substring(0, 4).Insert(2, ".");
                        _srwdcv = ntest(_srwdcv);
                        decimal _rwdcv = Convert.ToDecimal(_srwdcv);
                        sendResult(muestra, "RDW-CV", _rwdcv);
                    }
                    catch (Exception ex)
                    {
                        error = ex.Message.ToString();
                    }
                    finally
                    {
                    }


                    encabezado = encabezado.Substring(5);
                    try
                    {
                        string _srwdsd = encabezado.Substring(0, 4).Insert(2, ".");
                        _srwdsd = ntest(_srwdsd);
                        decimal _rwdsd = Convert.ToDecimal(_srwdsd);
                        sendResult(muestra, "RDW-SD", _rwdsd);
                    }
                    catch (Exception ex)
                    {
                        error = ex.Message.ToString();
                    }
                    finally
                    {
                    }

                    encabezado = encabezado.Substring(8);
                    try
                    {
                        string _smvp = encabezado.Substring(0, 5).Insert(4, ".");
                        _smvp = ntest(_smvp);
                        decimal _mvp = Convert.ToDecimal(_smvp);
                        sendResult(muestra, "MPV", _mvp);
                    }
                    catch (Exception ex)
                    {
                        error = ex.Message.ToString();
                    }
                    finally
                    {
                    }


                    encabezado = encabezado.Substring(10);
                    try
                    {
                        string sret = "";
                        decimal ret = 0;

                        if (encabezado.Substring(0, 5).Trim() != string.Empty)
                        {
                            sret = encabezado.Substring(0, 5).Insert(3, ".");
                            sret = ntest(sret);
                            ret = Convert.ToDecimal(sret);
                        }
                        sendResult(muestra, "RET%", ret);
                    }
                    catch (Exception ex)
                    {
                        error = ex.Message.ToString();
                    }
                    finally
                    {
                    }


                    encabezado = encabezado.Substring(5);
                    try
                    {
                        string _sret = "";
                        decimal _ret = 0;
                        if (encabezado.Substring(0, 5).Trim() != string.Empty)
                        {
                            _sret = encabezado.Substring(0, 5).Insert(3, ".");
                            _sret = ntest(_sret);
                            _ret = Convert.ToDecimal(_sret);
                        }
                        sendResult(muestra, "RET#", _ret);
                    }
                    catch (Exception ex)
                    {
                        error = ex.Message.ToString();
                    }
                    finally
                    {
                    }

                    encabezado = encabezado.Substring(5);
                    try
                    {
                        string srif = string.Empty;
                        decimal irf = 0;
                        if (encabezado.Substring(0, 5).Trim() != "")
                        {
                            srif = encabezado.Substring(0, 5).Insert(4, ".");
                            srif = ntest(srif);
                            irf = Convert.ToDecimal(srif);
                        }
                        sendResult(muestra, "IRF", irf);
                    }
                    catch (Exception ex)
                    {
                        error = ex.Message.ToString();
                    }
                    finally
                    {
                    }

                    encabezado = encabezado.Substring(27);
                    try
                    {
                        string snrbc = string.Empty;
                        decimal nrbc = 0;

                        if (encabezado.Substring(0, 4).Trim() != string.Empty)
                        {
                            snrbc = encabezado.Substring(0, 4).Insert(3, ".");
                            snrbc = ntest(snrbc);
                            nrbc = Convert.ToDecimal(snrbc);
                        }
                        sendResult(muestra, "NRBC#", nrbc);
                    }
                    catch (Exception ex)
                    {
                        error = ex.Message.ToString();
                    }
                    finally
                    {
                    }

                    encabezado = encabezado.Substring(5);
                    try
                    {
                        string _snrbc = string.Empty;
                        decimal _nrbc = 0;
                        if (encabezado.Substring(0, 5).Trim() != string.Empty)
                        {
                            _snrbc = encabezado.Substring(0, 5).Insert(3, ".");
                            _snrbc = ntest(_snrbc);
                            _nrbc = Convert.ToDecimal(_snrbc);
                        }
                        sendResult(muestra, "NRBC%", _nrbc);
                    }
                    catch (Exception ex)
                    {
                        error = ex.Message.ToString();
                    }
                    finally
                    {
                    }
                }
            }
            catch (Exception er)
            {
                Console.Write(er.Message);
            }
             
        }


        void sendResult(string sample, string parameter, decimal result)
        {
            try
            {
                GetConnectionParameters();
                SqlConnection sqlConn = new SqlConnection(strConection);
                SqlCommand sqlCmd = new SqlCommand();
                sqlCmd.Connection = sqlConn;
                sqlCmd.CommandType = CommandType.StoredProcedure;
                sqlCmd.CommandTimeout = 60000;
                sqlCmd.CommandText = "sp_AddUserResult";
                sqlCmd.Parameters.AddWithValue("@parameter", parameter);
                sqlCmd.Parameters.AddWithValue("@system", _argumentos);
                sqlCmd.Parameters.AddWithValue("@sample", sample);
                sqlCmd.Parameters.AddWithValue("@value", result);

                sqlConn.Open();
                sqlCmd.ExecuteNonQuery();
            }
            catch (Exception er)
            {
                MessageBox.Show(er.Message);
            }
        }

        private void frmCobas_Shown(object sender, EventArgs e)
        {
            if (_argumentos != string.Empty)
            {
                loadConfig(_argumentos);
                if (!_existeInterfaz)
                {
                    MessageBox.Show("System Interface does not exists: " + _argumentos);
                    this.Close();
                }
                else
                {
                    setupPort();
                }
            }
        }


        #region Control Panel Events
        private void trayIcon_DoubleClick(object sender, EventArgs e)
        {
            Show();
            this.WindowState = FormWindowState.Normal;
        }

        private void ubtnOcultar_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void btnCopiar_Click(object sender, EventArgs e)
        {
            try
            {
                if (savDialog.ShowDialog() == DialogResult.OK)
                {
                    StreamWriter wr = new StreamWriter(savDialog.FileName, true);
                    string resultados = string.Empty;
                    foreach (ListViewItem item in lstResultados.Items)
                    {
                        resultados += item.Text;
                        resultados += Environment.NewLine;
                    }
                    wr.WriteLine(resultados);
                    wr.Flush();
                    wr.Close();
                }
            }
            catch (Exception er)
            {
                MessageBox.Show(er.Message);
            }
        }

        private void btnLimpiar_Click(object sender, EventArgs e)
        {
            this.lstResultados.Items.Clear();
        }

        private void utbnCerrar_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(this, "Are you sure to exit the application, no data will be received?", "Info", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                this.Close();
            }
        }


        #endregion

        #region Port Configuration

        static string _fileXML = Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath) + @"\db.xml";

        void GetConnectionParameters()
        {
            DataSet ds = new DataSet();
            COBAS.Cryptography cryp = new COBAS.Cryptography();
            ds.ReadXml(_fileXML);

            if (ds.Tables.Count > 0)
            {
                DataRow row = ds.Tables["DatabaseParameters"].Rows[0];
                strConection = "Data Source = " + row["Server"].ToString() + "; Initial Catalog =" + row["Database"].ToString() 
                       + "; User id = " + row["Username"].ToString() + "; Password = " + cryp.Decrypt(row["Password"].ToString());
            }
        }



        private void setupPort()
        {
            try
            {
                //port.PortName = "COM12";
                //port.BaudRate = SetPortBaudRate("9600");
                //port.Parity = Parity.None;
                //port.DataBits = 8;  //SetPortDataBits(this.utxtBitsDatos.Text);
                //port.StopBits = StopBits.One; //SetPortStopBits(this.utxtBitsParo.Text);//this.utxtBitsParo.Text);
                //port.Handshake = Handshake.None;// SetPortHandshake(this.utxtHandShaking.Text);
                //port.ReadTimeout = 10;
                //port.Open();


                port.PortName = this.utxtPuerto.Text;
                port.BaudRate = SetPortBaudRate(this.utxtVelocidad.Text);
                port.Parity = SetPortParity(this.utxtParidad.Text);
                port.DataBits = SetPortDataBits(this.utxtBitsDatos.Text);
                port.StopBits = SetPortStopBits(this.utxtBitsParo.Text);//this.utxtBitsParo.Text);
                port.Handshake = SetPortHandshake(this.utxtHandShaking.Text);
                port.ReadTimeout = 10;
                port.Open();

           //     RealizaSincronización();
            }
            catch (Exception er)
            {
                MessageBox.Show(er.Message);
                this.Close();
            }
        }

        void loadConfig(string argumentos)
        {
            try
            {
                GetConnectionParameters();
                SqlConnection sqlConn = new SqlConnection(strConection);
                SqlCommand sqlCmd = new SqlCommand();
                sqlCmd.Connection = sqlConn;
                sqlCmd.CommandType = CommandType.Text;
                sqlCmd.CommandText = @"SELECT * FROM tblChemistrySystem cs
                                        INNER JOIN tblSystemInterface si ON cs.Code = si.Code
                                        WHERE si.Code = '" + argumentos + "'"; //db

                sqlConn.Open();
                SqlDataReader rd = sqlCmd.ExecuteReader();

                while (rd.Read())
                {
                    this.utxtEquipo.Text = argumentos;
                    this.utxtEstatus.Text = rd["ConEquActivo"] == DBNull.Value ? "No Active" : "Active";
                    this.utxtPuerto.Text = "COM"+ rd["ConEquPuerto"].ToString();
                    this.utxtVelocidad.Text = rd["ConEquVelocidad"].ToString();
                    switch (rd["ConEquHand"].ToString())
                    {
                        case "0":
                            this.utxtHandShaking.Text = Handshake.None.ToString();
                            break;
                        case "1":
                            this.utxtHandShaking.Text = Handshake.RequestToSend.ToString();
                            break;
                        case "2":
                            this.utxtHandShaking.Text = Handshake.RequestToSendXOnXOff.ToString();
                            break;
                        case "3":
                            this.utxtHandShaking.Text = Handshake.XOnXOff.ToString();
                            break;
                    }
                    switch (rd["ConEquParidad"].ToString())
                    { 
                        case "N":
                            this.utxtParidad.Text = Parity.None.ToString();
                            break;
                    }
                 
                    this.utxtBitsDatos.Text = rd["ConEquBitDatos"].ToString();
                    this.utxtBitsParo.Text = rd["ConEquBitStop"].ToString().Substring(0, 1);
                    this.port.ParityReplace = rd["ConEquParReplace"].ToString().Trim() == string.Empty ? Convert.ToByte(0) : Convert.ToByte(rd["ConEquParReplace"].ToString());
                    this.port.RtsEnable = rd["ConEquRTSE"].ToString()  == "0"? false : true;
                    this.port.DiscardNull = rd["ConEquNulDiscard"].ToString() == "0" ? false : true;
                    _existeInterfaz = true;
                }
            }
            catch (SqlException ex)
            {
                throw new Exception(ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                //if (rd != null) rd.Close();
                //if (sqlConn != null) sqlConn.Close();
            }
        }

        public static SerialPort SetPortName(string portName)
        {

            return (SerialPort)Enum.Parse(typeof(SerialPort), portName); ;
        }

        public static int SetPortBaudRate(string baudRate)
        {
            return int.Parse(baudRate);
        }

        public static Parity SetPortParity(string parity)
        {
            if (parity.Trim() == string.Empty)
            {
                return Parity.None;
            }
            else
            {
                return (Parity)Enum.Parse(typeof(Parity), parity);
            }
        }

        public static int SetPortDataBits(string dataBits)
        {
            return int.Parse(dataBits);
        }

        public static string PortCom(string puerto)
        {
            return "COM " + puerto;
        }

        public static StopBits SetPortStopBits(string stopBits)
        {

            decimal bits = Convert.ToInt32(stopBits);

            return (StopBits)Enum.Parse(typeof(StopBits), bits.ToString());
        }

        public static Handshake SetPortHandshake(string handshake)
        {
            return (Handshake)Enum.Parse(typeof(Handshake), handshake);
        }

        #endregion

        #region Events to move form
        private void frm_MouseDown(object sender, MouseEventArgs e)
        {
            ex = e.X;
            ey = e.Y;
            this.arrastre = true;
        }

        private void frm_MouseUp(object sender, MouseEventArgs e)
        {
            this.arrastre = false;
        }

        private void frm_MouseMove(object sender, MouseEventArgs e)
        {
            if (arrastre) this.Location = this.PointToScreen(new Point(MousePosition.X - this.Location.X - ex, MousePosition.Y - this.Location.Y - ey));
        }
        #endregion 
  
    }
}