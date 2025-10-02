using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

 

using Microsoft.FlightSimulator.SimConnect;
using System.Runtime.InteropServices;

using System.IO.Ports;


using System.Net.Sockets;

using System.Net;
using System.Threading;
using System.Globalization;

namespace G5_FSX
{
    public partial class Form1 : Form
    {

        SimConnect simconnect = null;
        uint WM_USER_SIMCONNECT = 0x402;

        bool isConnectedSimconnect = false;

        string responseString = "{}";
        static HttpListener listener;
        static bool serverRunning = true;

        public string currentButton = "x";

        enum REQUESTS
        {
            Request1
        }

        private enum DEFINITIONS
        {
            Struct1
        }


        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        struct Struct1
        {
            public double heading;
            public double pitch;
            public double roll;
            public double airspeed;
            public double baro_altitude;
            public double baro_adjust;
            public int autopilot_heading;
            public int autopilot_altitude;
            public int autopilot_speed;

            // autopilot state: heading, altitude, speed
            

            public bool autopilot_state;

            public bool flightDirector_state;
            public double flightDirector_pitch;
            public double flightDirector_roll;

            public double ground_speed;
            public double vertical_speed;

            public bool glideslope_active;
            public double glideslope_value;

            public bool localizer_active;
            public double localizer_value;


            public double hsi_cdi_needle;
            public bool hsi_cdi_needle_valid;

            public double hsi_bearing;
            public bool hsi_bearing_valid;



            public double air_temperature;



        };




        enum EVENT_ID
        {
            YOKE_PITCH,
            YOKE_ROLL
        }


        public enum NOTIFICATION_GROUPS
        {
            GROUP0
        }





        public Form1()
        {
            InitializeComponent();
            textBoxIP.Text = getIP();
            atualizaListaCOMs();
        }





        void Conectar()
        {

            try
            {

                simconnect = new SimConnect("SimConnect Example", this.Handle, WM_USER_SIMCONNECT, null, 0);

                simconnect.OnRecvOpen += new SimConnect.RecvOpenEventHandler(simconnect_OnRecvOpen);
                simconnect.OnRecvQuit += new SimConnect.RecvQuitEventHandler(simconnect_OnRecvQuit);
                simconnect.OnRecvException += new SimConnect.RecvExceptionEventHandler(simconnect_OnRecvException);
                simconnect.OnRecvSimobjectData += new SimConnect.RecvSimobjectDataEventHandler(simconnect_OnRecvSimobjectData);


                simconnect.AddToDataDefinition(DEFINITIONS.Struct1, "HEADING INDICATOR", "degrees", SIMCONNECT_DATATYPE.FLOAT64, 0, SimConnect.SIMCONNECT_UNUSED);
                simconnect.AddToDataDefinition(DEFINITIONS.Struct1, "PLANE PITCH DEGREES", "degrees", SIMCONNECT_DATATYPE.FLOAT64, 0, SimConnect.SIMCONNECT_UNUSED);
                simconnect.AddToDataDefinition(DEFINITIONS.Struct1, "PLANE BANK DEGREES", "degrees", SIMCONNECT_DATATYPE.FLOAT64, 0, SimConnect.SIMCONNECT_UNUSED);
                simconnect.AddToDataDefinition(DEFINITIONS.Struct1, "AIRSPEED INDICATED", "knots", SIMCONNECT_DATATYPE.FLOAT64, 0, SimConnect.SIMCONNECT_UNUSED);
                simconnect.AddToDataDefinition(DEFINITIONS.Struct1, "INDICATED ALTITUDE", "feet", SIMCONNECT_DATATYPE.FLOAT64, 0, SimConnect.SIMCONNECT_UNUSED);
                simconnect.AddToDataDefinition(DEFINITIONS.Struct1, "KOHLSMAN SETTING HG", "inHg", SIMCONNECT_DATATYPE.FLOAT64, 0, SimConnect.SIMCONNECT_UNUSED);
                simconnect.AddToDataDefinition(DEFINITIONS.Struct1, "AUTOPILOT HEADING LOCK DIR", "degrees", SIMCONNECT_DATATYPE.INT32, 0, SimConnect.SIMCONNECT_UNUSED);
                simconnect.AddToDataDefinition(DEFINITIONS.Struct1, "AUTOPILOT ALTITUDE LOCK VAR", "feet", SIMCONNECT_DATATYPE.INT32, 0, SimConnect.SIMCONNECT_UNUSED);
                simconnect.AddToDataDefinition(DEFINITIONS.Struct1, "AUTOPILOT AIRSPEED HOLD VAR", "knots", SIMCONNECT_DATATYPE.INT32, 0, SimConnect.SIMCONNECT_UNUSED);

                simconnect.AddToDataDefinition(DEFINITIONS.Struct1, "AUTOPILOT MASTER", "bool", SIMCONNECT_DATATYPE.INT32, 0, SimConnect.SIMCONNECT_UNUSED);
                simconnect.AddToDataDefinition(DEFINITIONS.Struct1, "AUTOPILOT FLIGHT DIRECTOR ACTIVE", "bool", SIMCONNECT_DATATYPE.INT32, 0, SimConnect.SIMCONNECT_UNUSED);


                simconnect.AddToDataDefinition(DEFINITIONS.Struct1, "AUTOPILOT FLIGHT DIRECTOR PITCH", "radians", SIMCONNECT_DATATYPE.FLOAT64, 0, SimConnect.SIMCONNECT_UNUSED);
                simconnect.AddToDataDefinition(DEFINITIONS.Struct1, "AUTOPILOT FLIGHT DIRECTOR BANK", "radians", SIMCONNECT_DATATYPE.FLOAT64, 0, SimConnect.SIMCONNECT_UNUSED);

                simconnect.AddToDataDefinition(DEFINITIONS.Struct1, "GROUND VELOCITY", "knots", SIMCONNECT_DATATYPE.FLOAT64, 0, SimConnect.SIMCONNECT_UNUSED);
                simconnect.AddToDataDefinition(DEFINITIONS.Struct1, "VERTICAL SPEED", "feet/second", SIMCONNECT_DATATYPE.FLOAT64, 0, SimConnect.SIMCONNECT_UNUSED);



                simconnect.AddToDataDefinition(DEFINITIONS.Struct1, "NAV HAS GLIDE SLOPE:1", "bool", SIMCONNECT_DATATYPE.INT32, 0, SimConnect.SIMCONNECT_UNUSED);
                simconnect.AddToDataDefinition(DEFINITIONS.Struct1, "NAV LOCALIZER:1", "degrees", SIMCONNECT_DATATYPE.FLOAT64, 0, SimConnect.SIMCONNECT_UNUSED);


                simconnect.AddToDataDefinition(DEFINITIONS.Struct1, "NAV HAS LOCALIZER:1", "bool", SIMCONNECT_DATATYPE.INT32, 0, SimConnect.SIMCONNECT_UNUSED);
                simconnect.AddToDataDefinition(DEFINITIONS.Struct1, "NAV LOCALIZER:1", "degrees", SIMCONNECT_DATATYPE.FLOAT64, 0, SimConnect.SIMCONNECT_UNUSED);



                simconnect.AddToDataDefinition(DEFINITIONS.Struct1, "HSI CDI NEEDLE", "number", SIMCONNECT_DATATYPE.FLOAT64, 0, SimConnect.SIMCONNECT_UNUSED);
                simconnect.AddToDataDefinition(DEFINITIONS.Struct1, "HSI CDI NEEDLE VALID", "bool", SIMCONNECT_DATATYPE.INT32, 0, SimConnect.SIMCONNECT_UNUSED);

                simconnect.AddToDataDefinition(DEFINITIONS.Struct1, "HSI BEARING", "degrees", SIMCONNECT_DATATYPE.FLOAT64, 0, SimConnect.SIMCONNECT_UNUSED);
                simconnect.AddToDataDefinition(DEFINITIONS.Struct1, "HSI BEARING VALID", "bool", SIMCONNECT_DATATYPE.INT32, 0, SimConnect.SIMCONNECT_UNUSED);



                simconnect.AddToDataDefinition(DEFINITIONS.Struct1, "TOTAL AIR TEMPERATURE", "celsius", SIMCONNECT_DATATYPE.FLOAT64, 0, SimConnect.SIMCONNECT_UNUSED);

                 
 

                simconnect.RegisterDataDefineStruct<Struct1>(DEFINITIONS.Struct1);
                simconnect.RequestDataOnSimObject(REQUESTS.Request1, DEFINITIONS.Struct1, SimConnect.SIMCONNECT_OBJECT_ID_USER, SIMCONNECT_PERIOD.VISUAL_FRAME, SIMCONNECT_DATA_REQUEST_FLAG.CHANGED, 0, 0, 0);


                // YOKE
                // Mapear Eventos
                simconnect.MapClientEventToSimEvent(EVENT_ID.YOKE_PITCH, "AXIS_ELEVATOR_SET"); // Sets elevator postion (-16383 - +16383)
                simconnect.AddClientEventToNotificationGroup(NOTIFICATION_GROUPS.GROUP0, EVENT_ID.YOKE_PITCH, false);

                simconnect.MapClientEventToSimEvent(EVENT_ID.YOKE_ROLL, "AXIS_AILERONS_SET"); // Sets aileron postion (-16383 - +16383)
                simconnect.AddClientEventToNotificationGroup(NOTIFICATION_GROUPS.GROUP0, EVENT_ID.YOKE_ROLL, false);


                simconnect.SetNotificationGroupPriority(NOTIFICATION_GROUPS.GROUP0, SimConnect.SIMCONNECT_GROUP_PRIORITY_HIGHEST);

                // Testar Envio de Eventos
                //simconnect.TransmitClientEvent(0, EVENT_ID.YOKE_PITCH, (uint)yokePitch, NOTIFICATION_GROUPS.GROUP0, SIMCONNECT_EVENT_FLAG.DEFAULT);
                // END - YOKE

            }
            catch (COMException ex)
            {
                Console.WriteLine("erro C.O.M: " + ex.Message);
                MessageBox.Show("Erro do Simconnect", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                Console.WriteLine("erro generico: " + ex.Message);
                MessageBox.Show("Erro desconhecido", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }




        private void simconnect_OnRecvOpen(SimConnect sender, SIMCONNECT_RECV_OPEN data)
        {
            Console.WriteLine("Conectado ao SimConnect.");
            isConnectedSimconnect = true;
            botaoConectar.Text = "Desconectar";

            textBoxIP.Enabled = textBoxPorta.Enabled = false;

            Thread httpThread = new Thread(StartHttpServer);
            httpThread.Start();
        }



        private void simconnect_OnRecvQuit(SimConnect sender, SIMCONNECT_RECV data)
        {
            Console.WriteLine("FSX:SimConnect foi fechado.");
            Desconectar();
        }



        private void simconnect_OnRecvException(SimConnect sender, SIMCONNECT_RECV_EXCEPTION data)
        {
            Console.WriteLine("Exception received: " + ((uint)data.dwException));
        }



        private void simconnect_OnRecvSimobjectData(SimConnect sender, SIMCONNECT_RECV_SIMOBJECT_DATA data)
        {
            if (data.dwRequestID == (uint)REQUESTS.Request1)
            {
                Struct1 struct1 = (Struct1)data.dwData[0];

                double pitch = -struct1.pitch;
                double roll = struct1.roll;
                double yaw = struct1.heading;//Math.Round(struct1.heading + 1);
                double altitude = struct1.baro_altitude;
                double baroAdjust = struct1.baro_adjust;
                double speedValue = struct1.airspeed;
                int autopilotHeading = struct1.autopilot_heading;
                int autopilotAltitude = struct1.autopilot_altitude;
                int autopilotSpeed = struct1.autopilot_speed;
                double verticalSpeed = struct1.vertical_speed * 60f;


                string jsonData = $"{{" +

                    $"\"currentButton\": \"{currentButton}\"," +

                    $"\"IMU\": [{pitch.ToString("F2", CultureInfo.InvariantCulture)}, {roll.ToString("F2", CultureInfo.InvariantCulture)}, {yaw.ToString("F2", CultureInfo.InvariantCulture)}]," +
                    $"\"speed\": {speedValue.ToString("F2", CultureInfo.InvariantCulture)}," +
                    $"\"altitude\": {altitude.ToString("F2", CultureInfo.InvariantCulture)}," +
                    $"\"baroAdjust\": {baroAdjust.ToString("F2", CultureInfo.InvariantCulture)}," +
                    $"\"autopilotHeading\": {autopilotHeading}," +
                    $"\"autopilotAltitude\": {autopilotAltitude}," +
                    $"\"autopilotSpeed\": {autopilotSpeed}," +

                    $"\"autopilotState\": {struct1.autopilot_state.ToString().ToLowerInvariant()}," +


                    $"\"flightDirectorState\": {struct1.flightDirector_state.ToString().ToLowerInvariant()}," +

                    //$"\"flightDirectorPitch\": {struct1.flightDirector_pitch.ToString("F2", CultureInfo.InvariantCulture)}," +
                    //$"\"flightDirectorRoll\": {struct1.flightDirector_roll.ToString("F2", CultureInfo.InvariantCulture)}," +
                    $"\"flightDirectorPitch\": {struct1.flightDirector_pitch.ToString(CultureInfo.InvariantCulture)}," +
                    $"\"flightDirectorRoll\": {struct1.flightDirector_roll.ToString(CultureInfo.InvariantCulture)}," +


                    $"\"groundSpeed\": {struct1.ground_speed.ToString("F2", CultureInfo.InvariantCulture)}," +
                    $"\"verticalSpeed\": {verticalSpeed.ToString("F2", CultureInfo.InvariantCulture)}," +


                    $"\"glideslope\": [{struct1.glideslope_active.ToString().ToLowerInvariant()}, {struct1.glideslope_value.ToString("F6", CultureInfo.InvariantCulture)}]," +
                    $"\"localizer\": [{struct1.localizer_active.ToString().ToLowerInvariant()}, {struct1.localizer_value.ToString("F6", CultureInfo.InvariantCulture)}]," +



                    $"\"hsiCdiNeedle\": {struct1.hsi_cdi_needle.ToString("F2", CultureInfo.InvariantCulture)}," +
                    $"\"hsiCdiNeedleValid\": {struct1.hsi_cdi_needle_valid.ToString().ToLowerInvariant()}," +

                    $"\"hsiBearingNeedle\": {struct1.hsi_bearing.ToString("F2", CultureInfo.InvariantCulture)}," +
                    $"\"hsiBearingNeedleValid\": {struct1.hsi_bearing_valid.ToString().ToLowerInvariant()}," +



                    $"\"airTemperature\": {struct1.air_temperature.ToString("F2", CultureInfo.InvariantCulture)}" +

                    "}";

                

                responseString = jsonData;

            }
        }



        void Desconectar()
        {
            if (simconnect != null)
            {
                simconnect.Dispose();
                simconnect = null;

                isConnectedSimconnect = false;

                if (botaoConectar.InvokeRequired)
                {
                    botaoConectar.Invoke(new Action(() => {
                        botaoConectar.Text = "Conectar";
                        textBoxIP.Enabled = textBoxPorta.Enabled = true;
                    }));
                }
                else
                {
                    botaoConectar.Text = "Conectar";
                    textBoxIP.Enabled = textBoxPorta.Enabled = true;
                }

                
            }

            StopHttpServer();

            desconectarSerial();

        }



        protected override void DefWndProc(ref Message m)
        {
            if (m.Msg == WM_USER_SIMCONNECT)
            {
                if (simconnect != null)
                {
                    simconnect.ReceiveMessage();
                }
            }
            else
            {
                base.DefWndProc(ref m);
            }
        }
















        private void atualizaListaCOMs()
        {
            int i = 0;
            bool quantDiferente = false;    //flag para sinalizar que a quantidade de portas mudou



            //se a quantidade de portas mudou
            if (comboBox1.Items.Count == SerialPort.GetPortNames().Length)
            {
                foreach (string s in SerialPort.GetPortNames())
                {
                    if (comboBox1.Items[i++].Equals(s) == false)
                    {
                        quantDiferente = true;
                    }
                }
            }
            else
            {
                quantDiferente = true;
            }

            //Se não foi detectado diferença
            if (quantDiferente == false)
            {
                return;                     //retorna
            }

            //limpa comboBox
            comboBox1.Items.Clear();

            //adiciona todas as COM diponíveis na lista
            foreach (string s in SerialPort.GetPortNames())
            {
                comboBox1.Items.Add(s);
            }

            if (comboBox1.Items.Count > 0)
            {
                //seleciona a primeira posição da lista
                comboBox1.SelectedIndex = 0;
            }


        }





        public void conectarSerial()
        {
            try
            {
                serialPort1.PortName = comboBox1.Items[comboBox1.SelectedIndex].ToString();
                serialPort1.Open();

            }
            catch
            {
                return;

            }
            if (serialPort1.IsOpen)
            {
                botaoConectarSerial.Text = "Desconectar";
                comboBox1.Enabled = false;
                timerCOM.Enabled = false;

            }
        }




        public void desconectarSerial()
        {
            try
            {
                serialPort1.Close();
                comboBox1.Enabled = true;
                botaoConectarSerial.Text = "Conectar";
                timerCOM.Enabled = true;
            }
            catch
            {
                return;
            }
        }



        private void botaoConectarSerial_Click(object sender, EventArgs e)
        {

            if (serialPort1.IsOpen == false)
            {
                conectarSerial();
            }
            else
            {
                desconectarSerial();
            }

        }



        String RxString;

        private void serialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            
            RxString = serialPort1.ReadExisting();
            Console.WriteLine($"PASS RECV: {RxString} é {currentButton}");
            Invoke(new EventHandler(trataDadoRecebido));
        }

        private void trataDadoRecebido(object sender, EventArgs e)
        {
            Console.WriteLine($"SERIAL: {RxString} é {currentButton}");
            currentButton = RxString.Replace("\r\n", String.Empty);
        }




        private void timerCOM_Tick(object sender, EventArgs e)
        {
            atualizaListaCOMs();
        }





        private void botaoConectar_Click(object sender, EventArgs e)
        {
            if (isConnectedSimconnect)
            {
                Desconectar();
                return;
            }

            bool ipValido = checarIP(textBoxIP.Text);
            bool portaValida = checarPorta(textBoxPorta.Text);

            if (!ipValido && !portaValida)
            {
                MessageBox.Show("IP e porta inválidos.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!ipValido)
            {
                MessageBox.Show("IP inválido.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!portaValida)
            {
                MessageBox.Show("Porta inválida.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Conectar();
        }



        bool checarIP(string ip)
        {
            return ip == String.Empty || ip == "localhost" || IPAddress.TryParse(ip, out _);
        }



        static bool checarPorta(string porta)
        {
            if (int.TryParse(porta, out int portNumber))
            {
                return portNumber >= 1 && portNumber <= 65535;
            }
            return false;
        }


        string getIP()
        {
            string localIP = string.Empty;

            // Obtém os endereços IP do host local
            foreach (IPAddress ipAddress in Dns.GetHostAddresses(Dns.GetHostName()))
            {
                // Filtra apenas os endereços IPv4
                if (ipAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    localIP = ipAddress.ToString();
                    break; // Pode haver mais de um IP, então escolhemos o primeiro
                }
            }

            return localIP;
        }



        /* HTTP */
        void StartHttpServer()
        {

            if (textBoxIP.Text == string.Empty)
            {
                if (textBoxIP.InvokeRequired)
                {
                    textBoxIP.Invoke(new Action(()=> {
                        textBoxIP.Text = "localhost";
                    }));
                }
            }

            listener = new HttpListener();
            listener.Prefixes.Add($"http://{textBoxIP.Text}:{textBoxPorta.Text}/");

            try
            {
                listener.Start();
            }
            catch (HttpListenerException)
            {
                MessageBox.Show("Acesso ao IP/PORTA foi negado\ntente executarcomo administrador ou verifique o firewall.", "Negado", MessageBoxButtons.OK, MessageBoxIcon.Error);
                listener = null;
                Desconectar();
                return;
            }

            Console.WriteLine("Servidor HTTP aguardando requisições...");


            // Fica aguardando requisições HTTP
            while (serverRunning)
            {
                try
                {
                    // Recebe uma requisição HTTP
                    HttpListenerContext context = listener.GetContext();
                    HttpListenerRequest request = context.Request;
                    HttpListenerResponse response = context.Response;


                    response.AddHeader("Access-Control-Allow-Origin", "*");  // Permitir qualquer origem
                    response.AddHeader("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE");  // Métodos permitidos
                    response.AddHeader("Access-Control-Allow-Headers", "Content-Type");  // Cabeçalhos permitidos

                    byte[] buffer = Encoding.UTF8.GetBytes(responseString);

                    // Define o tamanho do conteúdo e envia a resposta
                    response.ContentLength64 = buffer.Length;
                    response.OutputStream.Write(buffer, 0, buffer.Length);
                    response.OutputStream.Close();

                    //Console.WriteLine("Requisição recebida: " + request.Url);

                    if ( request.QueryString["buttonClear"] == "true" )
                    {
                        Console.WriteLine("Requisição recebida: " + request.Url);
                        currentButton = "x";
                    }


                    // YOKE
                    

                    try
                    {
                        int yokePitch = int.Parse(request.QueryString["z"]);
                        int yokeRoll = int.Parse(request.QueryString["x"]);

                        simconnect.TransmitClientEvent(0, EVENT_ID.YOKE_PITCH, (uint)yokePitch, NOTIFICATION_GROUPS.GROUP0, SIMCONNECT_EVENT_FLAG.DEFAULT);
                        simconnect.TransmitClientEvent(0, EVENT_ID.YOKE_ROLL, (uint)-yokeRoll, NOTIFICATION_GROUPS.GROUP0, SIMCONNECT_EVENT_FLAG.DEFAULT);
                    }
                    catch (Exception expected) {}

                    
                    // END - YOKE

                }
                catch (Exception ex)
                {
                    Console.WriteLine("Erro ao processar requisição: " + ex.Message);
                }
            }

            listener.Stop();

        }


        void StopHttpServer()
        {
            serverRunning = false;
        }

        /* END HTTP */





        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Desconectar();
        }




        


    }
}
