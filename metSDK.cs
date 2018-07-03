using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Runtime.InteropServices;
using libzkfpcsharp;
using System.Threading;
using System.Web.UI.MobileControls;
using System.IO;
using System.Drawing;

namespace rzkclock_ws
{
    public class metSDK
    {
        // ZK-F16
        # region Lib F16
        [DllImport("C:\\WINDOWS\\system32\\plcommpro.dll", EntryPoint = "Connect")]
        public static extern IntPtr Connect(string Parameters);
        [DllImport("plcommpro.dll", EntryPoint = "Disconnect")]
        public static extern void Disconnect(IntPtr h);
        [DllImport("plcommpro.dll", EntryPoint = "GetDeviceParam")]
        public static extern int GetDeviceParam(IntPtr h, ref byte buffer, int buffersize, string itemvalues);
        [DllImport("plcommpro.dll", EntryPoint = "SetDeviceParam")]
        public static extern int SetDeviceParam(IntPtr h, string itemvalues);
        [DllImport("plcommpro.dll", EntryPoint = "PullLastError")]
        public static extern int PullLastError();
        [DllImport("plcommpro.dll", EntryPoint = "GetDeviceData")]
        public static extern int GetDeviceData(IntPtr h, ref byte buffer, int buffersize, string tablename, string filename, string filter, string options);
        [DllImport("plcommpro.dll", EntryPoint= "DeleteDeviceData")]
        public static extern int DeleteDeviceData(IntPtr h, string tablename, string data, string options);
        [DllImport("plcommpro.dll", EntryPoint = "SetDeviceData")]
        public static extern int SetDeviceData(IntPtr h, string tablename, string data, string options);
        # endregion
        // ZK-4500
        # region Lib zk4500
        zkfp fpInstance = new zkfp();
        IntPtr FormHandle = IntPtr.Zero;
        bool bIsTimeToDie = false;
        bool esNuevoRegistro = false;

        bool bIdentify = true;
        byte[] FPBuffer;
        int RegisterCount = 0;
        const int REGISTER_FINGER_COUNT = 3;

        byte[][] RegTmps = new byte[3][];
        byte[] RegTmp = new byte[2048];
        byte[] CapTmp = new byte[2048];
        int cbCapTmp = 2048;
        int cbRegTmp = 0;
        int iFid = 1;

        const int MESSAGE_CAPTURED_OK = 0x0400 + 6;
        string msgEstado_EnRoll = "";
        [DllImport("user32.dll", EntryPoint = "SendMessageA")]
        public static extern int SendMessage(IntPtr hwnd, int wMsg, IntPtr wParam, IntPtr lParam);
        # endregion

        // Confing Globales para ZK
        private int cantIntentos = 6;
        private int tiempoEntreIntentos = 5000;
        private UtilConstantes utilM = null;

        public metSDK()
        {
            utilM = new UtilConstantes();
        }

        // Funcion para obtener nueva huella desde ZK4500
        public string sdk_EnRoll()
        {
            String msgResp = "";
            Boolean flagGet = false;
            for (int i = 0; i < cantIntentos; i++)
            {
                try
                {
                    # region SDK ZK-4500
                    int ret = zkfp.ZKFP_ERR_OK;
                    if ((ret = fpInstance.Initialize()) == zkfp.ZKFP_ERR_OK)
                    {
                        utilM.generarBitacoraZK("INFO", "En proceso de captura, se detecta zk4500 en USB. I=" + i);
                        int nCount = fpInstance.GetDeviceCount();
                        if ((nCount > 0) && (zkfp.ZKFP_ERR_OK == (ret = fpInstance.OpenDevice(0))))
                        {
                            for (int findex = 0; findex < 3; findex++)
                            {
                                RegTmps[findex] = new byte[2048];
                            }
                            FPBuffer = new byte[fpInstance.imageWidth * fpInstance.imageHeight];
                            Thread captureThread = new Thread(new ThreadStart(GenerarCapture));
                            captureThread.IsBackground = true;
                            captureThread.Start();
                            bIsTimeToDie = false;
                            utilM.generarBitacoraZK("INFO", "En proceso de captura, se inicia hilo para evento entrada de huella. I=" + i);
                            while (!bIsTimeToDie)
                            {
                                //espere TimeOut o Entrada de 3 Huellas para generar Template
                            }
                            flagGet = true;
                            msgResp = msgEstado_EnRoll;
                            utilM.generarBitacoraZK("INFO", "En proceso de captura, finaliza evento. I=" + i + " EX: " + msgResp);
                        }
                        else
                        {
                            msgResp = "Incidente en proceso de captura, imposible iniciar captura para biometrico ZK4500. Resp_SDK = " + ret;
                            utilM.generarBitacoraZK("ERROR", msgResp + " I=" + i);
                            msgResp = "<ERROR>" + msgResp;
                        }
                        fpInstance.CloseDevice();
                        fpInstance.Finalize();
                    }
                    else
                    {
                        fpInstance.Finalize();
                        msgResp = "Incidente en proceso de captura, imposible detectar ZK4500. Resp_SDK = " + ret + ".";
                        utilM.generarBitacoraZK("ERROR", msgResp + " I=" + i);
                        msgResp = "<ERROR>" + msgResp;
                    }
                    # endregion
                }
                catch (Exception ex)
                {
                    flagGet = false;
                    msgResp = "Error crítico en capa del sdk, Ex generada: " + ex.ToString();
                    utilM.generarBitacoraZK("ERROR", msgResp);
                }
                # region SDK
                /*
                //TIPO F16
                # region Sincronizar hora Dispositivo SDK Plcommpro
                string str = "protocol=TCP,ipaddress=" + ip + ",port=" + port + ",timeout=2000,passwd=";// string de conexion
                IntPtr cone = IntPtr.Zero; // variable de conexion
                cone = Connect(str); // realiza la conexion
                if (cone.ToInt32() != 0) // si la conexion fue exitosa continua con el  SDK plcommpro
                {
                    int retPerso = 0, ret = 0;
                    int BUFFERSIZE = 10 * 1024 * 1024;
                    byte[] buffer = new byte[BUFFERSIZE];

                    string devtablename = "user";
                    string data = "Pin\tName";
                    string options = "";
                    string devdatfilter = "";
                    if (usuData != "") { devdatfilter = "Pin=" + usuData.ToString(); }
                    retPerso = GetDeviceData(cone, ref buffer[0], BUFFERSIZE, devtablename, data, devdatfilter, options);
                    if (retPerso > 0) // si el retorno fue = exitoso
                    {
                        List<string> info_arrayPerso = utilM.normaliza_buffer(buffer);
                        devtablename = "templatev10";
                        data = "Pin\tFingerID\tTemplate";
                        if (usuData != "") { devdatfilter = "Pin=" + usuData.ToString(); }
                        ret = GetDeviceData(cone, ref buffer[0], BUFFERSIZE, devtablename, data, devdatfilter, options);

                        int iPos = 2, indexPerso = 0, indexPos = 0;
                        List<string> datos_arrayHuellas = utilM.normaliza_buffer(buffer);
                        while (indexPerso < retPerso)
                        {
                            DataRow row = zkLUsuarios.NewRow();
                            row["PERNR"] = datos_arrayHuellas[iPos];
                            for (int pos = 3; pos < datos_arrayHuellas.Count; pos += 3)
                            {
                                if (datos_arrayHuellas[indexPos + 2] == datos_arrayHuellas[pos])
                                {
                                    row["DEDO"] = datos_arrayHuellas[i + pos];
                                    row["HUELLA"] = "X";//datos_arrayHuellas[pos + 2];
                                    break;
                                }
                            }
                            //Agrega el reg
                            zkLUsuarios.Rows.Add(row);
                            indexPerso = indexPerso + 1;
                            indexPos = indexPos + 2;
                            iPos = iPos + 2;
                        }
                        msgResp = utilM.DataTableToJSON(zkLUsuarios).ToString();
                        flagUpdate = true;
                    }
                    else  // sino envie error
                    {
                        msgResp = "Error en búsqueda, no se encuentra el usuario solicitado";
                        utilM.generarBitacora_info(msgResp);
                    }
                    Disconnect(cone);
                }
                # endregion
                //u580-INA01-TIPO F11
                # region Sincronizar hora Dispositivo SDK WINDOWS
                if (axCZKEM1.Connect_Net(ip, Convert.ToInt32(port)))
                {
                    if (axCZKEM1.SetDeviceTime(dwMachineNumber))
                    {
                        //HABILITA EL MODO LECTURA/ESCRITURA EN EL RELOJ
                        axCZKEM1.EnableDevice(dwMachineNumber, false);
                        // LEE LOS USUARIOS Y LA INFORMACION EN EL RELOJ
                        axCZKEM1.ReadAllUserID(dwMachineNumber);
                        //LEE LAS HUELLAS DE CADA USUARIO DEL RELOJ
                        axCZKEM1.ReadAllTemplate(dwMachineNumber);

                        int iFlag = 1;
                        int dwFingerIndex = 0;
                        string TmpData = "";
                        int TmpLength = 0;


                        // FILTRO: SI NO SE PONE ID DE USUARIO, EL WEBSERVICE DEL RELOJ RETORNA
                        //  TODAS LAS HUELLAS QUE TIENEN TODOS LOS USUARIOS 
                        if (usuData == "")
                        {
                            //Parametros varios
                            string Name = "";
                            string Password = "";
                            bool Enabled = true;
                            int Privilege = 0;
                            int dwEnrollNumberAUX = 0;

                            //SE TOMA LAS HUELLAS EN EL RELOJ
                            string tipoProceso = reg_RelojSAP["INFO"].ToString().Contains("U580") ||
                                                    reg_RelojSAP["INFO"].ToString().Contains("UF11")
                                                        ? "basico" : "estandar";
                            switch ("basico")
                            {
                                case "basico":
                                    while (axCZKEM1.GetAllUserInfo(dwMachineNumber, ref dwEnrollNumberAUX, ref Name, ref Password, ref Privilege, ref Enabled))
                                    {
                                        huella = false;//Limpiar variable
                                        for (dwFingerIndex = 0; dwFingerIndex < 10; dwFingerIndex++)
                                        {
                                            //TOMA LAS HUELLAS COMO STRINGS Y SU RESPECTIVO TAMANO
                                            if (axCZKEM1.GetUserTmpExStr(dwMachineNumber, dwEnrollNumberAUX.ToString(), dwFingerIndex, out iFlag, out TmpData, out TmpLength))
                                            {
                                                if (TmpData.Length > 0) //Tiene huella
                                                {
                                                    huella = true;
                                                    DataRow fila = utilM.getUserInfo(zkLUsuarios.NewRow(), dwEnrollNumberAUX.ToString(), dwFingerIndex, TmpData);
                                                    //Agrega cada reg para el envio
                                                    zkLUsuarios.Rows.Add(fila);
                                                }
                                            }
                                        }
                                        //No tiene huella, se agrega al menos una vez
                                        if (huella == false)
                                        {
                                            DataRow fila = utilM.getUserInfo(zkLUsuarios.NewRow(), dwEnrollNumberAUX.ToString(), -1, "");
                                            //Agrega cada reg para el envio
                                            zkLUsuarios.Rows.Add(fila);
                                        }
                                        flagUpdate = true;
                                    }
                                    break;

                                case "estandar":
                                    /*
                                    while (axCZKEM1.SSR_GetAllUserInfo(dwMachineNumber, out dwEnrollNumber, out Name, out Password, out Privilege, out Enabled)) //TOMA LAS HUELLAS EN EL RELOJ
                                    {
                                        huella = false;

                                        for (dwFingerIndex = 0; dwFingerIndex < 10; dwFingerIndex++)
                                        {
                                            //TOMA LAS HUELLAS COMO STRINGS Y SU RESPECTIVO TAMANO
                                            if (axCZKEM1.GetUserTmpExStr(dwMachineNumber, dwEnrollNumber, dwFingerIndex, out iFlag, out TmpData, out TmpLength))//TOMA LAS HUELLAS COMO STRINGS Y SU RESPECTIVO TAMANO
                                            {

                                                if (TmpData.Length > 0) //Tiene huella
                                                {
                                                    huella = true;
                                                    DataRow fila = getREmpleado(resultadoregistro.NewRow(), dwEnrollNumber, dwFingerIndex, TmpData);
                                                    //Agrega cada reg para el envio
                                                    resultadoregistro.Rows.Add(fila);
                                                }
                                            }
                                        }

                                        //No tiene huella, se agrega al menos una vez
                                        if (huella == false)
                                        {
                                            DataRow fila = getREmpleado(resultadoregistro.NewRow(), dwEnrollNumber, -1, "");

                                            //Agrega cada reg para el envio
                                            resultadoregistro.Rows.Add(fila);
                                        }

                                    }
                                    break;
                            }
                        }
                        /*
                        else
                        {
                            //SE HACE LA BUSQUEDA POR EL USUARIO DIGITADO 
                            resultadoregistro.Clear();
                            huella = false;

                            for (dwFingerIndex = 0; dwFingerIndex < 10; dwFingerIndex++)
                            {
                                if (axCZKEM1.GetUserTmpExStr(dwMachineNumber, dwEnrollNumber, dwFingerIndex, out iFlag, out TmpData, out TmpLength))//(axCZKEM1.GetUserTmpExStr(dwMachineNumber, dwEnrollNumber, dwFingerIndex, out iFlag, out TmpData, out TmpLength))//TOMA LAS HUELLAS COMO STRINGS Y SU RESPECTIVO TAMANO
                                {
                                    if (TmpData.Length > 0) //Si tiene huella
                                    {
                                        huella = true;

                                        DataRow fila = getREmpleado(resultadoregistro.NewRow(), dwEnrollNumber, dwFingerIndex, TmpData);

                                        //Agrega cada reg para el envio
                                        resultadoregistro.Rows.Add(fila);
                                    }
                                }
                            }

                            //No tiene huella, se agrega al menos una vez
                            if (huella == false)
                            {
                                DataRow fila = getREmpleado(resultadoregistro.NewRow(), dwEnrollNumber.ToString(), -1, "");

                                //Agrega cada reg para el envio
                                resultadoregistro.Rows.Add(fila);
                            }
                        }

                        //enable the device
                        axCZKEM1.EnableDevice(dwMachineNumber, true);
                        axCZKEM1.Disconnect();
                        return getlistempleado_ToString(resultadoregistro);
                    
                    }
                }
                else
                {
                    msgResp = "Error en búsqueda, no se encuentra el usuario solicitado";
                    utilM.generarBitacora_info(msgResp);
                    //enable the device
                    axCZKEM1.EnableDevice(dwMachineNumber, true);
                    axCZKEM1.Disconnect();
                }
            }
            catch (Exception ex)
            {
                flagUpdate = false;
                msgResp = "Error crítico en capa del sdk, Ex generada: " + ex.ToString();
                Console.WriteLine("Ex en capa sdk:" + ex.ToString());
                utilM.generarBitacoraZK("ERROR", msgResp);
            }
            */
                # endregion
                // Evalua para cualqueir zk
                if (flagGet)
                    return msgResp;
            }
            //Salida para reportar ultimo error generado
            return msgResp;
        }
        //Hilo Captura entradas en ZK4500, en caso de error no activa DefWndProc
        private void GenerarCapture()
        {
            while (!bIsTimeToDie)
            {
                cbCapTmp = 2048;
                int ret = fpInstance.AcquireFingerprint(FPBuffer, CapTmp, ref cbCapTmp);
                if (ret == zkfp.ZKFP_ERR_OK)
                {
                    esNuevoRegistro = true;
                    utilM.generarBitacoraZK("INFO", "En proceso de captura, detecta entrada de nueva huella en ZK4500.");
                    SendMessage(FormHandle, MESSAGE_CAPTURED_OK, IntPtr.Zero, IntPtr.Zero);
                    msgEstado_EnRoll = DefWndProc(1030);
                    if (msgEstado_EnRoll != "")
                    {
                        bIsTimeToDie = true;
                    }
                }
                Thread.Sleep(200);
                esNuevoRegistro = false;
            }
        }
        //Evento para nuevo template de huella correcto generado en zk4500
        protected string DefWndProc(int tipoM)
        {
            string msgResp = "";
            switch (tipoM)
            {
                # region MetCaptura cuando template nueva huella es correcta
                case MESSAGE_CAPTURED_OK:
                    {
                        //MemoryStream ms = new MemoryStream();
                        //BitmapFormat.GetBitmap(FPBuffer, fpInstance.imageWidth, fpInstance.imageHeight, ref ms);
                        //Bitmap bmp = new Bitmap(ms);
                        //this.picFPImg.Image = bmp;
                        if (esNuevoRegistro)
                        {
                            int ret = zkfp.ZKFP_ERR_OK;
                            int fid = 0, score = 0;
                            ret = fpInstance.Identify(CapTmp, ref fid, ref score);
                            if (zkfp.ZKFP_ERR_OK == ret)
                            {
                                msgResp = "El dedo ya esta contenido en la instancia del equipo ZK4500. HuellaID = " + fid;
                                utilM.generarBitacoraZK("ERROR", msgResp);
                                return "<ERROR> " + msgResp;
                            }
                            if (RegisterCount > 0 && fpInstance.Match(CapTmp, RegTmps[RegisterCount - 1]) <= 0)
                            {
                                msgResp = "Se detectan huellas combinadas para generar template. ";
                                utilM.generarBitacoraZK("ERROR", "Incidente en proceso de captura, " + msgResp + "Resp_SDK = " + ret);
                                return "<ERROR> " + msgResp + "Use el mismo dedo para enrolar y reintente.";
                            }
                            Array.Copy(CapTmp, RegTmps[RegisterCount], cbCapTmp);
                            RegisterCount++;
                            utilM.generarBitacoraZK("INFO", "En proceso de captura, detecta nueva entrada para template #" + RegisterCount);
                            if (RegisterCount >= REGISTER_FINGER_COUNT)
                            {
                                RegisterCount = 0;
                                if (zkfp.ZKFP_ERR_OK == (ret = fpInstance.
                                                    GenerateRegTemplate(RegTmps[0], RegTmps[1], RegTmps[2], RegTmp, ref cbRegTmp)) &&
                                        zkfp.ZKFP_ERR_OK == (ret = fpInstance.AddRegTemplate(iFid, RegTmp)))
                                {
                                    iFid++;
                                    string result = System.Convert.ToBase64String(RegTmp);
                                    msgResp = "Proceso Enroll completado exitosamente. FID = " + result;
                                    utilM.generarBitacoraZK("INFO", "En proceso de captura, finaliza correctamente en generar y agregar huella.");
                                    utilM.generarBitacoraZK("INFO", "En proceso de captura, String huella generado: " + result);
                                }
                                else
                                {
                                    msgResp = "Se detectan huellas combinadas para generar template. Favor reintente. CodError = " + ret;
                                    utilM.generarBitacoraZK("ERROR", "Incidente en proceso de captura, " + msgResp);
                                    return "<ERROR> " + msgResp;
                                }
                            }
                            else
                            {
                                return "";
                            }
                        }
                        # region EvaluarHuella Entrante contra memoria del ZK4500
                        /*
                else
                {
                    if (cbRegTmp <= 0 || false)
                    {
                        msgResp = "Please register your finger first!";
                        return msgResp;
                    }
                    if (bIdentify || true)
                    {
                        int ret = zkfp.ZKFP_ERR_OK;
                        int fid = 0, score = 0;
                        ret = fpInstance.Identify(CapTmp, ref fid, ref score);
                        if (zkfp.ZKFP_ERR_OK == ret)
                        {
                            msgResp = "Identify succ, fid= " + fid + ",score=" + score + "!";
                            return msgResp;
                        }
                        else
                        {
                            msgResp = "Identify fail, ret= " + ret;
                            return msgResp;
                        }
                    }
                    else
                    {
                        int ret = fpInstance.Match(CapTmp, RegTmp);
                        if (0 < ret)
                        {
                            msgResp = "Match finger succ, score=" + ret + "!";
                            return msgResp;
                        }
                        else
                        {
                            msgResp = "Match finger fail, ret= " + ret;
                            return msgResp;
                        }
                    }
                }
                */
                        # endregion
                    }
                    break;
                # endregion
                default:
                    DefWndProc(tipoM);
                    break;
            }
            return msgResp;
        }

        // Funcion para evaluar comunicacion entre el SDK en servidor con la IP del biometrico
        public string sdk_Ping(string ip, string puerto, string tipo)
        {
            String msgResp = "";
            Boolean flagConn = false;
            for (int i = 0; i < cantIntentos; i++)
            {
                try{
                    switch (tipo)
                    {
                        case "F16":
                            //TIPO F16
                            # region Sincronizar hora Dispositivo SDK Plcommpro
                            string str = "protocol=TCP,ipaddress=" + ip + ",port=" + puerto + ",timeout=2000,passwd=";
                            IntPtr cone = IntPtr.Zero; // variable de conexion
                            cone = Connect(str);     // realiza la conexion
                            if (cone.ToInt32() != 0) // si la conexion fue exitosa continua con el  SDK plcommpro
                            {
                                utilM.generarBitacoraZK("INFO", "En proceso de conexión, se detecta la IP "+ ip +" con el SDK Plcommpro. I=" + i);
                                int ret = 0;
                                DateTime dt = DateTime.Now; // fecha y hora actual
                                int date = ((dt.Year - 2000) * 12 * 31 + (dt.Month - 1) * 31 + (dt.Day - 1))   // convertir el la fecha en formato dispocitivo
                                             * (24 * 60 * 60) + dt.Hour * 60 * 60 + dt.Minute * 60 + dt.Second; 
                                string items = ("DateTime=" + date.ToString());// crear el parametro para el dispositivo
                                ret = SetDeviceParam(cone, items); // enviar el parametro
                                if (ret == 0) // si el retorno fue = exitoso
                                {
                                    flagConn = true;
                                    Disconnect(cone); // se desonecta el sistema
                                    msgResp = "Proceso Ping de conexión completado exitosamente con la IP " + ip;
                                    utilM.generarBitacoraZK("INFO", "En proceso de conexión, se logra actualizar la hora del ZK con la IP " + ip + ". I=" + i);
                                }
                                else
                                {
                                    Disconnect(cone);
                                    msgResp = "Incidente en proceso de conexión, fallo al actualizar fecha/hora en la IP " + ip + ". Resp_SDK = " + ret + ".";
                                    utilM.generarBitacoraZK("ERROR", msgResp + " I=" + i);
                                    msgResp = "<ERROR>" + msgResp;
                                }
                            }
                            else
                            {
                                msgResp = "Incidente en proceso de conexión, fallo al conectar la IP " + ip + ". Resp_SDK = " + cone + ".";
                                utilM.generarBitacoraZK("ERROR", msgResp + " I=" + i);
                                msgResp = "<ERROR>" + msgResp; 
                            }
                            #endregion
                            break;
                        case "U580":
                        case "F11":
                        case "INA-01":
                            //F11-U580-INA01
                            # region Sincronizar Dispositivo SDK ZKEMKEEPER
                            int idwErrorCode = 0;
                            int dwMachineNumber = 1;
                            zkemkeeper.CZKEMClass axCZKEM1 = new zkemkeeper.CZKEMClass();
                            // HACE LA CONEXION CON EL RELOJ Y SE TOMA COMO PARAMETRO LAS IPS DE LA LISTA 
                            if (axCZKEM1.Connect_Net(ip, Convert.ToInt32(puerto)))
                            {
                                utilM.generarBitacoraZK("INFO", "En proceso de conexión, se detecta el Ping con la IP " + ip + " con el SDK Zkemkeeper. I=" + i);
                                //CONECTA Y ACTUALIZA LA HORA DEL RELOJ 
                                if (axCZKEM1.SetDeviceTime(dwMachineNumber))
                                {
                                    flagConn = true;
                                    axCZKEM1.RefreshData(dwMachineNumber);
                                    axCZKEM1.GetDeviceIP(dwMachineNumber, "");
                                    msgResp = "Proceso Ping de conexión completado exitosamente con la IP " + ip;
                                    utilM.generarBitacoraZK("INFO", "En proceso de conexión, se logra actualizar la hora del ZK con la IP " + ip +". I=" + i);
                                }
                                else
                                {
                                    axCZKEM1.GetLastError(ref idwErrorCode);
                                    msgResp = "Incidente en proceso de conexión, fallo al actualizar fecha/hora en la IP " + ip + ". Resp_SDK = " + idwErrorCode + ".";
                                    utilM.generarBitacoraZK("ERROR", msgResp + " I=" + i);
                                    msgResp = "<ERROR>" + msgResp;
                                }
                            }
                            else
                            {
                                axCZKEM1.GetLastError(ref idwErrorCode);
                                msgResp = "Incidente en proceso de conexión, fallo al conectar la IP " + ip + ". Resp_SDK = " + idwErrorCode + ".";
                                utilM.generarBitacoraZK("ERROR", msgResp + " I=" + i);
                                msgResp = "<ERROR>" + msgResp; 
                            }
                            //DESHABILITA EL MODO LECTURA/ESCRITURA EN EL RELOJ
                            axCZKEM1.EnableDevice(dwMachineNumber, true);
                            axCZKEM1.Disconnect();
                            #endregion
                            break;
                    }
                }
                catch (Exception ex)
                {
                    flagConn = false;
                    msgResp = "Error crítico en capa del sdk, Ex generada: " + ex.ToString();
                    Console.WriteLine("Ex en capa :" + ex.ToString());
                    utilM.generarBitacoraZK("ERROR", msgResp);
                }
                // Evalua para cualqueir zk
                if (flagConn)
                    return msgResp;

                System.Threading.Thread.Sleep(tiempoEntreIntentos);
            }
            //Salida para reportar ultimo error generado
            return msgResp;
        }
        // Funcion para obtener lista de Usuarios contenidos en la IP con el biometrico indicado
        public string sdk_LUsuarios(string ip, string puerto, string tipo, string usuData)
        {
            String msgResp = "";
            Boolean flagGet = false;
            //Estructuras para Usuarios/Huellas encontradas
            DataTable zkLUsuarios = new DataTable();
            zkLUsuarios.Columns.Add("PERNR");
            zkLUsuarios.Columns.Add("DEDO");
            zkLUsuarios.Columns.Add("HUELLA");
            for (int i = 0; i < cantIntentos; i++)
            {
                try{
                    switch (tipo)
                    {
                        case "F16":
                            //TIPO F16
                            # region Sincronizar hora Dispositivo SDK Plcommpro
                            string str = "protocol=TCP,ipaddress=" + ip + ",port=" + puerto + ",timeout=2000,passwd=";
                            IntPtr cone = IntPtr.Zero; // variable de conexion
                            cone = Connect(str);     // realiza la conexion
                            if (cone.ToInt32() != 0) // si la conexion fue exitosa continua con el  SDK plcommpro
                            {
                                utilM.generarBitacoraZK("INFO", "En proceso de Listar Usuarios/Huellas, se detecta la IP " + ip + " con el SDK Plcommpro. I=" + i);
                                int ret = 0;
                                int BUFFERSIZE = 10 * 1024 * 1024;
                                byte[] buffer = new byte[BUFFERSIZE];
                                string options = "";
                                string devdatfilter = "";
                                string devtablename = "user";
                                string data = "Pin\tName";
                                if (usuData != "") { devdatfilter = "Pin=" + usuData.ToString(); }

                                ret = GetDeviceData(cone, ref buffer[0], BUFFERSIZE, devtablename, data, devdatfilter, options);
                                if (ret > 0) // si el retorno fue = exitoso
                                {
                                    List<string> linfo_ZKUsu = utilM.normaliza_buffer(buffer);
                                    devtablename = "templatev10";
                                    data = "Pin\tFingerID\tTemplate";
                                    if (usuData != "") { devdatfilter = "Pin=" + usuData.ToString(); }
                                    utilM.generarBitacoraZK("INFO", "En proceso de Listar Usuarios/Huellas, se obtiene Usuarios por " + linfo_ZKUsu.Count + " reg en la IP " + ip + ". I=" + i);

                                    ret = GetDeviceData(cone, ref buffer[0], BUFFERSIZE, devtablename, data, devdatfilter, options);
                                    //int iPos = 2, indexPerso = 0, indexPos = 0;
                                    List<string> linfo_ZKHuellas = utilM.normaliza_buffer(buffer);
                                    utilM.generarBitacoraZK("INFO", "En proceso de Listar Usuarios/Huellas, se obtiene Huellas por " + linfo_ZKHuellas.Count + " reg en la IP " + ip + ". I=" + i);

                                    DataRow row;
                                    List<String> lindexElim = new List<string>();
                                    for (int indexU = 2; indexU < linfo_ZKUsu.Count; indexU += 2)
                                    {
                                        for (int indexH = 3; indexH < linfo_ZKHuellas.Count; indexH += 3)
                                        {
                                            if (linfo_ZKUsu[indexU].ToString() == linfo_ZKHuellas[indexH].ToString())
                                            {
                                                row = zkLUsuarios.NewRow();
                                                row["PERNR"] = linfo_ZKUsu[indexU];
                                                row["DEDO"] = linfo_ZKHuellas[indexH + 1];
                                                row["HUELLA"] = "X";    //datos_arrayHuellas[i + 2]; 
                                                utilM.generarBitacoraZK("INFO", "En proceso de Listar Usuarios/Huellas, se determina usuario " + row["PERNR"] +
                                                                            ", con la huella del dedo " + row["DEDO"] + ",esto en la IP " + ip + ". I=" + i);

                                                zkLUsuarios.Rows.Add(row);
                                                lindexElim.Add(indexH.ToString());
                                                flagGet = true;
                                            }
                                        }
                                        if (lindexElim.Count() > 0)
                                        {
                                            foreach (String indexE in lindexElim)
                                            {
                                                linfo_ZKHuellas.RemoveAt(Convert.ToInt16(indexE));
                                            }
                                            lindexElim.Clear();
                                        }
                                    }
                                    if (flagGet)
                                    {
                                        string result = utilM.dataTableToJSON(zkLUsuarios).ToString();
                                        msgResp = "Proceso Listar Usuarios/Huellas completado exitosamente. JSON = " + result;
                                        utilM.generarBitacoraZK("INFO", "En proceso Listar Usuarios/Huellas, finaliza correctamente en obtener datos.");
                                        utilM.generarBitacoraZK("INFO", "En proceso Listar Usuarios/Huellas, JSON Info generado: " + result);
                                    }
                                }
                                else
                                {
                                    msgResp = "Incidente en proceso de Listar Usuarios/Huellas, no hay info de Usuarios en la IP " + ip + ". Resp_SDK = " + ret + ".";
                                    utilM.generarBitacoraZK("ERROR", msgResp + " I=" + i);
                                    msgResp = "<ERROR>" + msgResp;
                                }
                                Disconnect(cone);
                            }
                            else
                            {
                                msgResp = "Incidente en proceso Listar Usuarios/Huellas, fallo al conectar la IP " + ip + ". Resp_SDK = " + cone + ".";
                                utilM.generarBitacoraZK("ERROR", msgResp + " I=" + i);
                                msgResp = "<ERROR>" + msgResp;
                            }
                            #endregion
                            break;
                        case "U580":
                        case "F11":
                        case "INA-01":
                            //F11-U580-INA01
                            # region Sincronizar Dispositivo SDK ZKEMKEEPER
                            bool zkResponse = false;
                            int idwErrorCode = 0;
                            int dwMachineNumber = 1;
                            zkemkeeper.CZKEMClass axCZKEM1 = new zkemkeeper.CZKEMClass();
                            // HACE LA CONEXION CON EL RELOJ Y SE TOMA COMO PARAMETRO LAS IPS DE LA LISTA 
                            if (axCZKEM1.Connect_Net(ip, Convert.ToInt32(puerto)))
                            {
                                utilM.generarBitacoraZK("INFO", "En proceso de Listar Usuarios/Huellas, se detecta conexión con la IP " + ip + " con el SDK Zkemkeeper. I=" + i);
                                //CONECTA Y ACTUALIZA LA HORA DEL RELOJ 
                                if (axCZKEM1.SetDeviceTime(dwMachineNumber))
                                {
                                    //HABILITA EL MODO LECTURA/ESCRITURA EN EL RELOJ
                                    axCZKEM1.EnableDevice(dwMachineNumber, false);
                                    // LEE LOS USUARIOS Y LA INFORMACION EN EL RELOJ
                                    zkResponse = axCZKEM1.ReadAllUserID(dwMachineNumber);
                                    utilM.generarBitacoraZK("INFO", "En proceso de Listar Usuarios/Huellas, se obtiene Usuarios de la IP " + ip + ". Resp_SDK = " + zkResponse + ". I=" + i);
                                    //LEE LAS HUELLAS DE CADA USUARIO DEL RELOJ
                                    zkResponse = axCZKEM1.ReadAllTemplate(dwMachineNumber);
                                    utilM.generarBitacoraZK("INFO", "En proceso de Listar Usuarios/Huellas, se obtiene Huellas de la IP " + ip + ". Resp_SDK = " + zkResponse + ". I=" + i);
                                    //Parametros varios zk
                                    int iFlag = 1;
                                    int dwFingerIndex = 0;
                                    string dwEnrollNumber = usuData, TmpData = "";
                                    int TmpLength = 0;
                                    string Name = "";
                                    string Password = "";
                                    bool Enabled = true, flagHuella = false;
                                    int Privilege = 0;
                                    int dwEnrollNumberAUX = 0;
                                    DataRow filaHuella = null;
                                    //SE TOMA LAS HUELLAS EN EL RELOJ
                                    //string tipoProceso = reg_RelojSAP["INFO"].ToString().Contains("U580") ||    reg_RelojSAP["INFO"].ToString().Contains("UF11")    ? "basico" : "estandar";
                                    switch (tipo)
                                    {
                                        case "F11":
                                            while (axCZKEM1.GetAllUserInfo(dwMachineNumber, ref dwEnrollNumberAUX, ref Name, ref Password, ref Privilege, ref Enabled))
                                            {
                                                flagGet = true;
                                                flagHuella = false;//Limpiar variable
                                                for (dwFingerIndex = 0; dwFingerIndex < 10; dwFingerIndex++)
                                                {
                                                    //TOMA LAS HUELLAS COMO STRINGS Y SU RESPECTIVO TAMANO
                                                    if (axCZKEM1.GetUserTmpExStr(dwMachineNumber, dwEnrollNumberAUX.ToString(), dwFingerIndex, out iFlag, out TmpData, out TmpLength))
                                                    {
                                                        if (TmpData.Length > 0) //Tiene huella
                                                        {
                                                            flagHuella = true;
                                                            DataRow fila = utilM.getUserInfo(zkLUsuarios.NewRow(), dwEnrollNumberAUX.ToString(), dwFingerIndex, TmpData);
                                                            //Agrega cada reg para el envio
                                                            zkLUsuarios.Rows.Add(fila);
                                                        }
                                                    }
                                                }
                                                //Si No tiene huella, se agrega al menos una vez y vacio en indicador de huellas existentes en ZK
                                                if (flagHuella == false)
                                                {
                                                    filaHuella = utilM.getUserInfo(zkLUsuarios.NewRow(), dwEnrollNumberAUX.ToString(), -1, "");
                                                    zkLUsuarios.Rows.Add(filaHuella);
                                                }
                                            }
                                            break;
                                        case "U580":
                                        case "INA-01":
                                            while (axCZKEM1.SSR_GetAllUserInfo(dwMachineNumber, out dwEnrollNumber, out Name, out Password, out Privilege, out Enabled)) //TOMA LAS HUELLAS EN EL RELOJ
                                            {
                                                flagGet = true;
                                                flagHuella = false;//Limpiar variable
                                                for (dwFingerIndex = 0; dwFingerIndex < 10; dwFingerIndex++)
                                                {
                                                    //TOMA LAS HUELLAS COMO STRINGS Y SU RESPECTIVO TAMANO
                                                    if (axCZKEM1.GetUserTmpExStr(dwMachineNumber, dwEnrollNumber, dwFingerIndex, out iFlag, out TmpData, out TmpLength))//TOMA LAS HUELLAS COMO STRINGS Y SU RESPECTIVO TAMANO
                                                    {
                                                        if (TmpData.Length > 0) //Tiene huella
                                                        {
                                                            flagHuella = true;
                                                            filaHuella = utilM.getUserInfo(zkLUsuarios.NewRow(), dwEnrollNumber, dwFingerIndex, TmpData);
                                                            //Agrega cada reg para el envio
                                                            zkLUsuarios.Rows.Add(filaHuella);
                                                        }
                                                    }
                                                }
                                                //No tiene huella, se agrega al menos una vez
                                                if (flagHuella == false)
                                                {
                                                    DataRow fila = utilM.getUserInfo(zkLUsuarios.NewRow(), dwEnrollNumber, -1, "");
                                                    //Agrega cada reg para el envio
                                                    zkLUsuarios.Rows.Add(fila);
                                                }
                                            }
                                            break;
                                    }
                                    # region MultiHuella
                                    /*
                                    else
                                    {   
                                          //SE HACE LA BUSQUEDA POR EL USUARIO DIGITADO 
                                          resultadoregistro.Clear();
                                          huella = false;
                                          for (dwFingerIndex = 0; dwFingerIndex < 10; dwFingerIndex++)
                                          {
                                              if (axCZKEM1.GetUserTmpExStr(dwMachineNumber, dwEnrollNumber, dwFingerIndex, out iFlag, out TmpData, out TmpLength))//(axCZKEM1.GetUserTmpExStr(dwMachineNumber, dwEnrollNumber, dwFingerIndex, out iFlag, out TmpData, out TmpLength))//TOMA LAS HUELLAS COMO STRINGS Y SU RESPECTIVO TAMANO
                                              {
                                                  if (TmpData.Length > 0) //Si tiene huella
                                                  {
                                                      huella = true;

                                                      DataRow fila = getREmpleado(resultadoregistro.NewRow(), dwEnrollNumber, dwFingerIndex, TmpData);

                                                      //Agrega cada reg para el envio
                                                      resultadoregistro.Rows.Add(fila);
                                                  }}}
                                          //No tiene huella, se agrega al menos una vez
                                          if (huella == false)
                                          {
                                              DataRow fila = getREmpleado(resultadoregistro.NewRow(), dwEnrollNumber.ToString(), -1, "");
                                              //Agrega cada reg para el envio
                                              resultadoregistro.Rows.Add(fila);
                                          }
                                      }
                                      //enable the device
                                      axCZKEM1.EnableDevice(dwMachineNumber, true);
                                      axCZKEM1.Disconnect();
                                      return getlistempleado_ToString(resultadoregistro);
                                    }
                                */
                                #endregion
                                if (zkLUsuarios.Rows.Count > 0)
                                {
                                    flagHuella = true;
                                    string result = utilM.dataTableToJSON(zkLUsuarios).ToString();
                                    msgResp = "Proceso Listar Usuarios/Huellas completado exitosamente. JSON = " + result;
                                    utilM.generarBitacoraZK("INFO", "En proceso de Listar Usuarios/Huellas, finaliza correctamente en obtener y generar.");
                                    utilM.generarBitacoraZK("INFO", "En proceso de Listar Usuarios/Huellas, JSON Usu/Huellas: " + result);
                                }
                                else{
                                    flagGet = true;
                                    axCZKEM1.GetLastError(ref idwErrorCode);
                                    msgResp = "Incidente en proceso de Listar Usuarios/Huellas, no hay info de Usuarios en la IP " + ip + ". Resp_SDK = " + idwErrorCode + ".";
                                    utilM.generarBitacoraZK("ERROR", msgResp + " I=" + i);
                                    msgResp = "<ERROR>" + msgResp;
                                    }
                                }else{
                                    axCZKEM1.GetLastError(ref idwErrorCode);
                                    msgResp = "Incidente en proceso Listar Usuarios/Huellas, fallo al activar el ZK de la IP " + ip + ". Resp_SDK = " + idwErrorCode + ".";
                                    utilM.generarBitacoraZK("ERROR", msgResp + " I=" + i);
                                    msgResp = "<ERROR>" + msgResp;
                                }
                            }
                            else
                            {
                                axCZKEM1.GetLastError(ref idwErrorCode);
                                msgResp = "Incidente en proceso Listar Usuarios/Huellas, fallo al conectar la IP " + ip + ". Resp_SDK = " + idwErrorCode + ".";
                                utilM.generarBitacoraZK("ERROR", msgResp + " I=" + i);
                                msgResp = "<ERROR>" + msgResp;
                            }
                            //DESHABILITA EL MODO LECTURA/ESCRITURA EN EL RELOJ
                            axCZKEM1.EnableDevice(dwMachineNumber, true);
                            axCZKEM1.Disconnect();
                            #endregion
                            break;
                    }
                }
                catch (Exception ex)
                {
                    flagGet = false;
                    msgResp = "Error crítico en capa del sdk, Ex generada: " + ex.ToString();
                    Console.WriteLine("Ex en capa :" + ex.ToString());
                }

                // Evalua para cualqueir zk
                if (flagGet)
                    return msgResp;
            }
            //Salida para reportar ultimo error generado
            return msgResp;
        }
        // Funcion para obtener lista de MarcasLog en la IP con el biometrico indicado
        public string sdk_MarcasZK(string ip, string puerto, string tipo, string usuData)
        {
            String msgResp = "";
            Boolean flagGet = false;
            //Estructuras para Usuarios/Huellas encontradas
            DataTable zkLMarcas = new DataTable();
            zkLMarcas.Columns.Add("FECHA");
            zkLMarcas.Columns.Add("HORA");
            zkLMarcas.Columns.Add("IP");
            zkLMarcas.Columns.Add("PERNR");
            zkLMarcas.Columns.Add("MODO");
            zkLMarcas.Columns.Add("TIPO");
            for (int i = 0; i < cantIntentos; i++)
            {
                try
                {
                    switch (tipo)
                    {
                        case "F16":
                            //TIPO F16
                            # region Sincronizar hora Dispositivo SDK Plcommpro
                            string str = "protocol=TCP,ipaddress=" + ip + ",port=" + puerto + ",timeout=2000,passwd=";
                            IntPtr cone = IntPtr.Zero; // variable de conexion
                            cone = Connect(str);     // realiza la conexion
                            if (cone.ToInt32() != 0) // si la conexion fue exitosa continua con el  SDK plcommpro
                            {
                                utilM.generarBitacoraZK("INFO", "En proceso de Extraer Marcas, se detecta la IP " + ip + " con el SDK Plcommpro. I=" + i);
                                int ret = 0;
                                int BUFFERSIZE = 10 * 1024 * 1024;
                                byte[] buffer = new byte[BUFFERSIZE];
                                string devtablename = "transaction";
                                string data = "Pin\tVerified\tInOutState\tTime_second";
                                string devdatfilter = "";
                                string options = "";
                                ret = GetDeviceData(cone, ref buffer[0], BUFFERSIZE, devtablename, data, devdatfilter, options);
                                if (ret > 0) // si el retorno fue = exitoso
                                {
                                    List<string> datos_array = utilM.normaliza_buffer(buffer);
                                    string mes, dia, ano, hora, minuto, segundo;
                                    for (int indexM = 4; indexM < datos_array.Count; indexM += 4)
                                    {
                                        if (Convert.ToInt32(datos_array[i]) > 0)
                                        {
                                            DataRow row = zkLMarcas.NewRow();
                                            row["PERNR"] = datos_array[indexM];
                                            row["TIPO"] = datos_array[indexM + 1];
                                            row["MODO"] = datos_array[indexM + 2];
                                            row["IP"] = ip;
                                            int date = Convert.ToInt32(datos_array[i + 3]);
                                            if ((date / 2678400) % 12 + 1 <= 9) mes = "0" + Convert.ToString((date / 2678400) % 12 + 1); else mes = Convert.ToString((date / 2678400) % 12 + 1);
                                            if ((date / 86400) % 31 + 1 <= 9) dia = "0" + Convert.ToString((date / 86400) % 31 + 1); else dia = Convert.ToString((date / 86400) % 31 + 1);
                                            if ((date / 32140800) + 2000 <= 9) ano = "0" + Convert.ToString((date / 32140800) + 2000); else ano = Convert.ToString((date / 32140800) + 2000);
                                            if ((date / 3600) % 24 <= 9) hora = "0" + Convert.ToString((date / 3600) % 24); else hora = Convert.ToString((date / 3600) % 24);
                                            if ((date / 60) % 60 <= 9) minuto = "0" + Convert.ToString((date / 60) % 60); else minuto = Convert.ToString((date / 60) % 60);
                                            if (date % 60 <= 9) segundo = "0" + Convert.ToString(date % 60); else segundo = Convert.ToString(date % 60);
                                            //Cast de cada valor en su campo
                                            row["FECHA"] = ano + "-" + mes + "-" + dia;
                                            row["HORA"] = hora + ":" + minuto + ":" + segundo;
                                            zkLMarcas.Rows.Add(row);
                                        }
                                    }
                                }
                                else
                                {
                                    msgResp = "Incidente en proceso Extraer Marcas, no hay marcas en la IP " + ip + ". Resp_SDK = " + cone + ".";
                                    utilM.generarBitacoraZK("ERROR", msgResp + " I=" + i);
                                    utilM.generarBitacoraZK("INFO", msgResp + " I=" + i);
                                    msgResp = "<ERROR>" + msgResp;
                                }
                                Disconnect(cone); // se desonecta el sistema
                            }
                            else
                            {
                                msgResp = "Incidente en proceso Extraer Marcas, fallo al conectar la IP " + ip + ". Resp_SDK = " + cone + ".";
                                utilM.generarBitacoraZK("ERROR", msgResp + " I=" + i);
                                msgResp = "<ERROR>" + msgResp;
                            }
                            #endregion
                            break;
                        case "U580":
                        case "F11":
                        case "INA-01":
                            //F11-U580-INA01
                            # region Sincronizar Dispositivo SDK ZKEMKEEPER
                            bool zkResponse = false;
                            int idwErrorCode = 0;
                            int dwMachineNumber = 1;
                            zkemkeeper.CZKEMClass axCZKEM1 = new zkemkeeper.CZKEMClass();
                            // HACE LA CONEXION CON EL RELOJ Y SE TOMA COMO PARAMETRO LAS IPS DE LA LISTA 
                            if (axCZKEM1.Connect_Net(ip, Convert.ToInt32(puerto)))
                            {
                                utilM.generarBitacoraZK("INFO", "En proceso de Extraer Marcas, se detecta conexión con la IP " + ip + " con el SDK Zkemkeeper. I=" + i);
                                //CONECTA Y ACTUALIZA LA HORA DEL RELOJ 
                                if (axCZKEM1.SetDeviceTime(dwMachineNumber))
                                {
                                    //HABILITA EL MODO LECTURA/ESCRITURA EN EL RELOJ
                                    axCZKEM1.EnableDevice(dwMachineNumber, false);
                                    // LEE LOS USUARIOS Y LA INFORMACION EN EL RELOJ
                                    if (axCZKEM1.ReadGeneralLogData(dwMachineNumber))
                                    {
                                        utilM.generarBitacoraZK("INFO", "En proceso de Extraer Marcas, se obtiene Log de la IP " + ip + ". Resp_SDK = " + zkResponse + ". I=" + i);
                                        int dwEnrollNumber2 = 0, dwReserved = 0, dwVerifyMode = 0, dwInOutMode = 0;
                                        int dwWorkCode = 0, dwYear = 0, dwMonth = 0, dwDay = 0, dwHour = 0, dwMinute = 0, dwSecond = 0;
                                        string dwEnrollNumberAux = "0", mes, dia, ano, hora, minuto, segundo;
                                        DataRow filaMarca = null;
                                        switch (tipo)
                                        {
                                            case "F11":
                                                while (axCZKEM1.SSR_GetGeneralLogData(dwMachineNumber, out dwEnrollNumberAux, out dwVerifyMode,
                                                    out dwInOutMode, out dwYear, out dwMonth, out dwDay, out dwHour, out dwMinute, out dwSecond, ref dwWorkCode))
                                                {
                                                    filaMarca = zkLMarcas.NewRow();
                                                    //parser de la fecha
                                                    if (dwMonth <= 9) mes = "0" + dwMonth; else mes = dwMonth.ToString();
                                                    if (dwDay <= 9) dia = "0" + dwDay; else dia = dwDay.ToString();
                                                    if (dwYear <= 9) ano = "0" + dwYear; else ano = dwYear.ToString();
                                                    if (dwHour <= 9) hora = "0" + dwHour; else hora = dwHour.ToString();
                                                    if (dwMinute <= 9) minuto = "0" + dwMinute; else minuto = dwMinute.ToString();
                                                    if (dwSecond <= 9) segundo = "0" + dwSecond; else segundo = dwSecond.ToString();
                                                    //Cast de cada valor en su campo
                                                    filaMarca["FECHA"] = ano + "-" + mes + "-" + dia;
                                                    filaMarca["HORA"] = hora + ":" + minuto + ":" + segundo;
                                                    filaMarca["IP"] = ip;
                                                    filaMarca["PERNR"] = dwEnrollNumberAux.ToString();
                                                    filaMarca["MODO"] = dwInOutMode.ToString();
                                                    filaMarca["TIPO"] = dwVerifyMode.ToString();
                                                    //Agrega el reg para el envio
                                                    zkLMarcas.Rows.Add(filaMarca);
                                                }
                                                break;
                                            case "U580":
                                            case "INA-01":
                                                while (axCZKEM1.GetGeneralExtLogData(dwMachineNumber, ref dwEnrollNumber2, ref dwVerifyMode, ref dwInOutMode,
                                                    ref dwYear, ref dwMonth, ref dwDay, ref dwHour, ref dwMinute, ref dwSecond, ref dwWorkCode, ref dwReserved))
                                                {
                                                    filaMarca = zkLMarcas.NewRow();
                                                    //parser de la fecha
                                                    if (dwMonth <= 9) mes = "0" + dwMonth; else mes = dwMonth.ToString();
                                                    if (dwDay <= 9) dia = "0" + dwDay; else dia = dwDay.ToString();
                                                    if (dwYear <= 9) ano = "0" + dwYear; else ano = dwYear.ToString();
                                                    if (dwHour <= 9) hora = "0" + dwHour; else hora = dwHour.ToString();
                                                    if (dwMinute <= 9) minuto = "0" + dwMinute; else minuto = dwMinute.ToString();
                                                    if (dwSecond <= 9) segundo = "0" + dwSecond; else segundo = dwSecond.ToString();
                                                    //Cast de cada valor en su campo
                                                    filaMarca["FECHA"] = ano + "-" + mes + "-" + dia;
                                                    filaMarca["HORA"] = hora + ":" + minuto + ":" + segundo;
                                                    filaMarca["IP"] = ip;
                                                    filaMarca["PERNR"] = dwEnrollNumber2.ToString();
                                                    filaMarca["MODO"] = dwInOutMode.ToString();
                                                    filaMarca["TIPO"] = dwVerifyMode.ToString();
                                                    //Agrega el reg para el envio
                                                    zkLMarcas.Rows.Add(filaMarca);
                                                }
                                                break;
                                        }
                                    }
                                    else
                                    {
                                        flagGet = true;
                                        axCZKEM1.GetLastError(ref idwErrorCode);
                                        msgResp = "Incidente en proceso Listar Usuarios/Marcas, no hay marcas en el ZK de la IP " + ip + ". Resp_SDK = " + idwErrorCode + ".";
                                        utilM.generarBitacoraZK("ERROR", msgResp + " I=" + i);
                                        msgResp = "<ERROR>" + msgResp;
                                    }
                                }else{
                                    axCZKEM1.GetLastError(ref idwErrorCode);
                                    msgResp = "Incidente en proceso Listar Usuarios/Marcas, fallo al activar el ZK de la IP " + ip + ". Resp_SDK = " + idwErrorCode + ".";
                                    utilM.generarBitacoraZK("ERROR", msgResp + " I=" + i);
                                    msgResp = "<ERROR>" + msgResp;
                               }
                            }
                            else
                            {
                                axCZKEM1.GetLastError(ref idwErrorCode);
                                msgResp = "Incidente en proceso Listar Usuarios/Marcas, fallo al conectar la IP " + ip + ". Resp_SDK = " + idwErrorCode + ".";
                                utilM.generarBitacoraZK("ERROR", msgResp + " I=" + i);
                                msgResp = "<ERROR>" + msgResp;
                            }
                            //DESHABILITA EL MODO LECTURA/ESCRITURA EN EL RELOJ
                            axCZKEM1.EnableDevice(dwMachineNumber, true);
                            axCZKEM1.Disconnect();
                            #endregion
                            break;
                    }
                    if (zkLMarcas.Rows.Count > 0)
                    {
                        flagGet = true;
                        string result = utilM.dataTableToJSON(zkLMarcas).ToString();
                        msgResp = "Proceso Extraer Marcas completado exitosamente. JSON = " + result;
                        utilM.generarBitacoraZK("INFO", "En proceso Extraer Marcas, finaliza correctamente en obtener datos.");
                        utilM.generarBitacoraZK("INFO", "En proceso Extraer Marcas, JSON Info generado: " + result);
                    }
                }
                catch (Exception ex)
                {
                    flagGet = false;
                    msgResp = "Error crítico en capa del sdk, Ex generada: " + ex.ToString();
                    Console.WriteLine("Ex en capa :" + ex.ToString());
                }

                // Evalua para cualqueir zk
                if (flagGet)
                    return msgResp;
            }
            return msgResp;
        }
        // Funcion para crear y agregar Reg Tipo Usuarios en la IP con el biometrico indicado
        public string sdk_Add_UsuariosZK(string ip, string puerto, string tipo, string usuData)
        {
            String msgResp = "";
            Boolean flagSet = false;

            String cnum = "69";
            String uid = "69";
            String pin = "69";
            Boolean est = true;
            String name = "Marin_come_banano"; 

            for (int i = 0; i < cantIntentos; i++)
            {
                try
                {
                    switch (tipo)
                    {
                        case "F16":
                            //TIPO F16
                            # region Sincronizar hora Dispositivo SDK Plcommpro
                            string str = "protocol=TCP,ipaddress=" + ip + ",port=" + puerto + ",timeout=2000,passwd=";
                            IntPtr cone = IntPtr.Zero; // variable de conexion
                            cone = Connect(str);     // realiza la conexion
                            if (cone.ToInt32() != 0) // si la conexion fue exitosa continua con el  SDK plcommpro
                            {
                                utilM.generarBitacoraZK("INFO", "En proceso de Enviar Usuario, se detecta la IP " + ip + " con el SDK Plcommpro. I=" + i);
                                int ret1 = 0, ret2 = 0, ret3 = 0;
                                string data = "", options = "", devtablename = "";
                                //Asignación del horario para puerta por usuario
                                devtablename = "timeZone";
                                //Primero limpia el registro actual y despues agrega el Horario
                                data = "TimezoneId=1";
                                DeleteDeviceData(cone, devtablename, data, options);
                                utilM.generarBitacoraZK("INFO", "En proceso de Enviar Usuario, se limpia el TZ de la IP " + ip + " con el SDK Plcommpro. I=" + i);
                                data = "TimezoneId=1" +
                                        "\tSunTime1=0\tSunTime2=2359\tSunTime3=0" +
                                        "\tMonTime1=0\tMonTime2=2359\tMonTime3=0" +
                                        "\tTueTime1=0\tTueTime2=2359\tTueTime3=0" +
                                        "\tWedTime1=0\tWedTime2=2359\tWedTime3=0" +
                                        "\tThuTime1=0\tThuTime2=2359\tThuTime3=0" +
                                        "\tFriTime1=0\tFriTime2=2359\tFriTime3=0" +
                                        "\tSatTime1=0\tSatTime2=2359\tSatTime3=0" +
                                        "\tHol1Time1=0\tHol1Time2=2359\tHol1Time3=0" +
                                        "\tHol2Time1=0\tHol2Time2=2359\tHol2Time3=0" +
                                        "\tHol3Time1=0\tHol3Time2=2359\tHol3Time3=0";
                                ret1 = SetDeviceData(cone, devtablename, data, options);
                                utilM.generarBitacoraZK("INFO", "En proceso de Enviar Usuario, se crea el TZ de la IP " + ip + " con el SDK Plcommpro. I=" + i);
                                //Crea los datos del usuario -> OBTENER USUARIOS POR ENVIAR
                                //string nombreCarga = info_EmpleadoSAP.Rows[0]["CNAME"].ToString().Replace('_', ' ');
                                data = "CardNo=" + cnum + //info_EmpleadoSAP.Rows[0]["PERNR"].ToString() +
                                        "\tUID=" + uid +  //info_EmpleadoSAP.Rows[0]["PERNR"].ToString() +
                                        "\tPin=" + pin +  //info_EmpleadoSAP.Rows[0]["PERNR"].ToString() +
                                        "\tName=" + name +
                                        "\tPassword=0000\tGroup=0\tPrivilege=0";
                                ret2 = SetDeviceData(cone, devtablename, data, options);
                                utilM.generarBitacoraZK("INFO", "En proceso de Enviar Usuario, se crea el Usuario de codigo "+ uid + " en la IP " + ip + " con el SDK Plcommpro. I=" + i);

                                //Permisos en puerta para nuevo usuario
                                devtablename = "userauthorize";
                                data = "Pin=" + uid + //info_EmpleadoSAP.Rows[0]["PERNR"].ToString() +
                                        //data = "Pin=1691" +
                                        "\tAuthorizeTimezoneId=1\tAuthorizeDoorId=1";
                                ret3 = SetDeviceData(cone, devtablename, data, options);
                                utilM.generarBitacoraZK("INFO", "En proceso de Enviar Usuario, se da permiso al Usuario de codigo " + uid + " en la IP " + ip + " con el SDK Plcommpro. I=" + i);

                                if(ret1 == 0 && ret2 == 0 && ret3 ==0){
                                    flagSet = true;
                                    msgResp = "Proceso Enviar Usuario completado exitosamente. El usuario de código "+ uid + " se crea correctamente en la IP "+ ip;
                                    utilM.generarBitacoraZK("INFO", "En proceso Enviar Usuario, finaliza correctamente al crear el registro del código " + uid + " en la IP "+ ip);
                                }else{
                                    msgResp = "Incidente en proceso Enviar Usuario, Ex no esperada en la " + ip + " para el usuario "+ uid;
                                    msgResp = msgResp + ". Resp_SDK SetDevice = " + ret1 + ", Resp_SDK SetData = " + ret2 + ", Resp_SDK PermData = " + ret3 + ".";
                                    utilM.generarBitacoraZK("ERROR", msgResp + " I=" + i);
                                    utilM.generarBitacoraZK("INFO", msgResp + " I=" + i);
                                    msgResp = "<ERROR>" + msgResp;
                                }
                                Disconnect(cone); // se desonecta el sistema
                            }
                            else
                            {
                                msgResp = "Incidente en proceso Enviar Usuario, fallo al conectar la IP " + ip + ". Resp_SDK = " + cone + ".";
                                utilM.generarBitacoraZK("ERROR", msgResp + " I=" + i);
                                msgResp = "<ERROR>" + msgResp;
                            }
                            #endregion
                            break;
                        case "U580":
                        case "F11":
                        case "INA-01":
                            //F11-U580-INA01
                            # region Sincronizar Dispositivo SDK ZKEMKEEPER
                            int idwErrorCode = 0;
                            int dwMachineNumber = 1;
                            int iUpdateFlag = 1;
                            zkemkeeper.CZKEMClass axCZKEM1 = new zkemkeeper.CZKEMClass();
                            // HACE LA CONEXION CON EL RELOJ Y SE TOMA COMO PARAMETRO LAS IPS DE LA LISTA 
                            if (axCZKEM1.Connect_Net(ip, Convert.ToInt32(puerto)))
                            {
                                utilM.generarBitacoraZK("INFO", "En proceso Enviar Usuario, se detecta conexión con la IP " + ip + " con el SDK Zkemkeeper. I=" + i);
                                //CONECTA Y ACTUALIZA LA HORA DEL RELOJ 
                                if (axCZKEM1.SetDeviceTime(dwMachineNumber))
                                {
                                    //HABILITA EL MODO LECTURA/ESCRITURA EN EL RELOJ
                                    axCZKEM1.EnableDevice(dwMachineNumber, false);
                                    //HACE ESPACIO EN EL RELOJ PARA LOS NUEVOS DATOS
                                    if (axCZKEM1.BeginBatchUpdate(dwMachineNumber, iUpdateFlag))
                                    {
                                        //Variables para nuevo usuario
                                        int user_autorizacion = 0;
                                        string user_clave = pin; //dwEnrollNumber
                                        bool user_actividad = false;
                                        string nombreCarga = name; //info_EmpleadoSAP.Rows[0]["CNAME"].ToString().Replace('_', ' ');
                                        user_actividad = est;      //if (info_EmpleadoSAP.Rows[0]["ENABLE"].ToString() == "X") { user_actividad = true; }
                                        switch (tipo)
                                        {
                                            case "F11":
                                                flagSet = axCZKEM1.SetUserInfo(dwMachineNumber,
                                                           System.Convert.ToInt32(uid), //System.Convert.ToInt32(info_EmpleadoSAP.Rows[0]["PERNR"].ToString()),
                                                           name, uid,                   //nombreCarga, info_EmpleadoSAP.Rows[0]["PERNR"].ToString(),
                                                           user_autorizacion, user_actividad);
                                                break;
                                            case "U580":
                                            case "INA-01":
                                                flagSet = axCZKEM1.SSR_SetUserInfo(dwMachineNumber,
                                                            uid,                        //info_EmpleadoSAP.Rows[0]["PERNR"].ToString(),
                                                            name, uid,                  //info_EmpleadoSAP.Rows[0]["PERNR"].ToString(),
                                                            user_autorizacion, user_actividad);
                                                break;
                                        }
                                        if (flagSet)
                                        {
                                            msgResp = "Proceso Enviar Usuario completado exitosamente. El usuario de código "+ uid + " se crea correctamente en la IP "+ ip;
                                            utilM.generarBitacoraZK("INFO", "En proceso Enviar Usuario, finaliza correctamente al crear el registro del código " + uid + " en la IP "+ ip);
                                        }else{
                                            axCZKEM1.GetLastError(ref idwErrorCode);
                                            msgResp = "Incidente en proceso Enviar Usuario, Ex no esperada en la " + ip + " para el usuario "+ uid + ". Resp_SDK = " + idwErrorCode + ".";
                                            utilM.generarBitacoraZK("ERROR", msgResp + " I=" + i);
                                            utilM.generarBitacoraZK("INFO", msgResp + " I=" + i);
                                            msgResp = "<ERROR>" + msgResp;
                                        }
                                    }
                                    else
                                    {
                                        axCZKEM1.GetLastError(ref idwErrorCode);
                                        msgResp = "Incidente en proceso Enviar Usuario, no se creo el registro de ZK de la IP " + ip + ". Resp_SDK = " + idwErrorCode + ".";
                                        utilM.generarBitacoraZK("ERROR", msgResp + " I=" + i);
                                        msgResp = "<ERROR>" + msgResp;
                                    }
                                }
                                else
                                {
                                    axCZKEM1.GetLastError(ref idwErrorCode);
                                    msgResp = "Incidente en proceso Enviar Usuario, fallo al activar el ZK de la IP " + ip + ". Resp_SDK = " + idwErrorCode + ".";
                                    utilM.generarBitacoraZK("ERROR", msgResp + " I=" + i);
                                    msgResp = "<ERROR>" + msgResp;
                                }
                                // Apaga el equipo
                                axCZKEM1.BatchUpdate(dwMachineNumber);//SUBE LA INFORMACION AL RELOJ 
                                axCZKEM1.RefreshData(dwMachineNumber);//ACTUALIZA LA INFO EN EL RELOJ 
                                axCZKEM1.EnableDevice(dwMachineNumber, true);
                                axCZKEM1.Disconnect();
                            }
                            else
                            {
                                axCZKEM1.GetLastError(ref idwErrorCode);
                                msgResp = "Incidente en proceso Enviar Usuario, fallo al conectar la IP " + ip + ". Resp_SDK = " + idwErrorCode + ".";
                                utilM.generarBitacoraZK("ERROR", msgResp + " I=" + i);
                                msgResp = "<ERROR>" + msgResp;
                            }
                            #endregion
                            break;
                    }
                }
                catch (Exception ex)
                {
                    flagSet = false;
                    msgResp = "Error crítico en capa del sdk, Ex generada: " + ex.ToString();
                    Console.WriteLine("Ex en capa :" + ex.ToString());
                }

                // Evalua para cualqueir zk
                if (flagSet)
                    return msgResp;
            }
            return msgResp;
        }
        // Funcion para crear y agregar Reg Tipo Huella en la IP con el biometrico indicado
        public string sdk_Add_HuellasZK(string ip, string puerto, string tipo, string usuData)
        {
            String msgResp = "";
            Boolean flagSet = false;
            String uid = "1869504878";
            String dedo = "1";

            for (int i = 0; i < cantIntentos; i++)
            {
                try
                {
                    switch (tipo)
                    {
                        case "F16":
                            //TIPO F16
                            # region Sincronizar hora Dispositivo SDK Plcommpro
                            string str = "protocol=TCP,ipaddress=" + ip + ",port=" + puerto + ",timeout=2000,passwd=";
                            IntPtr cone = IntPtr.Zero; // variable de conexion
                            cone = Connect(str);       // realiza la conexion
                            if (cone.ToInt32() != 0)   // si la conexion fue exitosa continua con el  SDK plcommpro
                            {
                                int ret = 0;
                                string devtablename = "templatev10";
                                Byte valid = 3;
                                utilM.generarBitacoraZK("INFO", "En proceso de Enviar Huella, se detecta la IP " + ip + " con el SDK Plcommpro. I=" + i);
   
                                //if (lasHuellasWS["DEDO"].ToString() == "D1") { lasHuellasWS["FINDEX"] = 10; }
                                //if (lasHuellasWS["DEDO"].ToString() == "D2") { lasHuellasWS["FINDEX"] = 11; }
                                //if (lasHuellasWS["DEDO"].ToString() == "D3") { lasHuellasWS["FINDEX"] = 12; }
                                //if (lasHuellasWS["DEDO"].ToString() == "D4") { lasHuellasWS["FINDEX"] = 13; }
                                //if (lasHuellasWS["DEDO"].ToString() == "D5") { lasHuellasWS["FINDEX"] = 14; }
                                //if (lasHuellasWS["DEDO"].ToString() == "I1") { lasHuellasWS["FINDEX"] = 15; }
                                //if (lasHuellasWS["DEDO"].ToString() == "I2") { lasHuellasWS["FINDEX"] = 16; }
                                //if (lasHuellasWS["DEDO"].ToString() == "I3") { lasHuellasWS["FINDEX"] = 17; }
                                //if (lasHuellasWS["DEDO"].ToString() == "I4") { lasHuellasWS["FINDEX"] = 18; }
                                //if (lasHuellasWS["DEDO"].ToString() == "I5") { lasHuellasWS["FINDEX"] = 19; }
                                
                                string data = "UID=" +       uid + //   lasHuellasWS["PERNR"].ToString() +
                                              "\tPin=" +     uid + //   lasHuellasWS["PERNR"].ToString() +
                                              "\tValid=" +      valid.ToString() +
                                              "\tFingerID=" +   dedo +   //lasHuellasWS["FINDEX"].ToString() +
                                              "\tTemplate=" +   "" ;   //lasHuellasWS["HUELLA"].ToString();
                                string options = "";
                                utilM.generarBitacoraZK("INFO", "En proceso de Enviar Huella, se crea el Usuario de codigo " + uid + "con la huella " + dedo + " en la IP " + ip + " con el SDK Plcommpro. I=" + i);
                                ret = SetDeviceData(cone, devtablename, data, options);
                                if (ret == 0)
                                {
                                    flagSet = true;
                                    msgResp = "Proceso Enviar Huella completado exitosamente. El usuario de código " + uid + " se genera con una huella " + dedo + "en la IP " + ip;
                                    utilM.generarBitacoraZK("INFO", "En proceso Enviar Huella, finaliza correctamente al crear el registro del código " + uid + "con la huella " + dedo + " en la IP " + ip);
                                }
                                else
                                {
                                    msgResp = "Incidente en proceso Enviar Huella, Ex no esperada en la " + ip + " para el usuario " + uid + " con la huella " + dedo + ". Resp_SDK = " + ret;
                                    utilM.generarBitacoraZK("ERROR", msgResp + " I=" + i);
                                    utilM.generarBitacoraZK("INFO", msgResp + " I=" + i);
                                    msgResp = "<ERROR>" + msgResp;
                                }
                                Disconnect(cone); // se desonecta el sistema
                            }
                            else
                            {
                                msgResp = "Incidente en proceso Enviar Usuario, fallo al conectar la IP " + ip + ". Resp_SDK = " + cone + ".";
                                utilM.generarBitacoraZK("ERROR", msgResp + " I=" + i);
                                msgResp = "<ERROR>" + msgResp;
                            }
                            #endregion
                            break;
                        case "U580":
                        case "F11":
                        case "INA-01":
                            //F11-U580-INA01
                            # region Sincronizar Dispositivo SDK ZKEMKEEPER
                            int idwErrorCode = 0;
                            int dwMachineNumber = 1;
                            int iUpdateFlag = 1;
                            zkemkeeper.CZKEMClass axCZKEM1 = new zkemkeeper.CZKEMClass();
                            // HACE LA CONEXION CON EL RELOJ Y SE TOMA COMO PARAMETRO LAS IPS DE LA LISTA 
                            if (axCZKEM1.Connect_Net(ip, Convert.ToInt32(puerto)))
                            {
                                utilM.generarBitacoraZK("INFO", "En proceso Enviar Huella, se detecta conexión con la IP " + ip + " con el SDK Zkemkeeper. I=" + i);
                                //CONECTA Y ACTUALIZA LA HORA DEL RELOJ 
                                if (axCZKEM1.SetDeviceTime(dwMachineNumber))
                                {
                                    //HABILITA EL MODO LECTURA/ESCRITURA EN EL RELOJ
                                    axCZKEM1.EnableDevice(dwMachineNumber, false);
                                    //HACE ESPACIO EN EL RELOJ PARA LOS NUEVOS DATOS
                                    if (axCZKEM1.BeginBatchUpdate(dwMachineNumber, iUpdateFlag))
                                    {
                                        flagSet = axCZKEM1.SetUserTmpExStr(dwMachineNumber, uid, //lasHuellasWS["PERNR"].ToString(),
                                                        System.Convert.ToInt32(dedo), //Convert.ToInt32(lasHuellasWS["FINDEX"].ToString()),
                                                       iUpdateFlag,
                                                       usuData);  //lasHuellasWS["HUELLA"].ToString()))
                                        if (flagSet)
                                        {
                                            msgResp = "Proceso Enviar Huella completado exitosamente. El usuario de código "+ uid + " se genera con una huella "+ dedo + "en la IP "+ ip;
                                            utilM.generarBitacoraZK("INFO", "En proceso Enviar Huella, finaliza correctamente al crear el registro del código " + uid + "con la huella " + dedo +" en la IP "+ ip);
                                        }else{
                                            axCZKEM1.GetLastError(ref idwErrorCode);
                                            msgResp = "Incidente en proceso Enviar Huella, Ex no esperada en la " + ip + " para el usuario "+ uid + " y la huella " + dedo + ". Resp_SDK = " + idwErrorCode + ".";
                                            utilM.generarBitacoraZK("ERROR", msgResp + " I=" + i);
                                            utilM.generarBitacoraZK("INFO", msgResp + " I=" + i);
                                            msgResp = "<ERROR>" + msgResp;
                                        }
                                    }
                                    else
                                    {
                                        axCZKEM1.GetLastError(ref idwErrorCode);
                                        msgResp = "Incidente en proceso Enviar Usuario, no se creo el registro de ZK de la IP " + ip + ". Resp_SDK = " + idwErrorCode + ".";
                                        utilM.generarBitacoraZK("ERROR", msgResp + " I=" + i);
                                        msgResp = "<ERROR>" + msgResp;
                                    }
                                }
                                else
                                {
                                    axCZKEM1.GetLastError(ref idwErrorCode);
                                    msgResp = "Incidente en proceso Enviar Usuario, fallo al activar el ZK de la IP " + ip + ". Resp_SDK = " + idwErrorCode + ".";
                                    utilM.generarBitacoraZK("ERROR", msgResp + " I=" + i);
                                    msgResp = "<ERROR>" + msgResp;
                                }
                                // Apaga el equipo
                                axCZKEM1.BatchUpdate(dwMachineNumber);//SUBE LA INFORMACION AL RELOJ 
                                axCZKEM1.RefreshData(dwMachineNumber);//ACTUALIZA LA INFO EN EL RELOJ 
                                axCZKEM1.EnableDevice(dwMachineNumber, true);
                                axCZKEM1.Disconnect();
                            }
                            else
                            {
                                axCZKEM1.GetLastError(ref idwErrorCode);
                                msgResp = "Incidente en proceso Enviar Usuario, fallo al conectar la IP " + ip + ". Resp_SDK = " + idwErrorCode + ".";
                                utilM.generarBitacoraZK("ERROR", msgResp + " I=" + i);
                                msgResp = "<ERROR>" + msgResp;
                            }
                            #endregion
                            break;
                    }
                }
                catch (Exception ex)
                {
                    flagSet = false;
                    msgResp = "Error crítico en capa del sdk, Ex generada: " + ex.ToString();
                    Console.WriteLine("Ex en capa :" + ex.ToString());
                }

                // Evalua para cualqueir zk
                if (flagSet)
                    return msgResp;
            }
            return msgResp;
        }

        // Funcion para buscar y traer Reg Tipo Huella en la IP con el biometrico indicado
        public string sdk_Get_HuellasZK(string ip, string puerto, string tipo, string usuData)
        {
            String msgResp = "";
            Boolean flagGet = false;
            String uid = "6969";
            String dedo = "1";

            for (int i = 0; i < cantIntentos; i++)
            {
                try
                {
                    switch (tipo)
                    {
                        case "F16":
                            //TIPO F16
                            # region Sincronizar hora Dispositivo SDK Plcommpro
                            string str = "protocol=TCP,ipaddress=" + ip + ",port=" + puerto + ",timeout=2000,passwd=";
                            IntPtr cone = IntPtr.Zero; // variable de conexion
                            cone = Connect(str);       // realiza la conexion
                            if (cone.ToInt32() != 0)   // si la conexion fue exitosa continua con el  SDK plcommpro
                            {
                                int ret = 0;
                                string devtablename = "templatev10";
                                Byte valid = 3;
                                utilM.generarBitacoraZK("INFO", "En proceso de Enviar Huella, se detecta la IP " + ip + " con el SDK Plcommpro. I=" + i);

                                //if (lasHuellasWS["DEDO"].ToString() == "D1") { lasHuellasWS["FINDEX"] = 10; }
                                //if (lasHuellasWS["DEDO"].ToString() == "D2") { lasHuellasWS["FINDEX"] = 11; }
                                //if (lasHuellasWS["DEDO"].ToString() == "D3") { lasHuellasWS["FINDEX"] = 12; }
                                //if (lasHuellasWS["DEDO"].ToString() == "D4") { lasHuellasWS["FINDEX"] = 13; }
                                //if (lasHuellasWS["DEDO"].ToString() == "D5") { lasHuellasWS["FINDEX"] = 14; }
                                //if (lasHuellasWS["DEDO"].ToString() == "I1") { lasHuellasWS["FINDEX"] = 15; }
                                //if (lasHuellasWS["DEDO"].ToString() == "I2") { lasHuellasWS["FINDEX"] = 16; }
                                //if (lasHuellasWS["DEDO"].ToString() == "I3") { lasHuellasWS["FINDEX"] = 17; }
                                //if (lasHuellasWS["DEDO"].ToString() == "I4") { lasHuellasWS["FINDEX"] = 18; }
                                //if (lasHuellasWS["DEDO"].ToString() == "I5") { lasHuellasWS["FINDEX"] = 19; }

                                string data = "UID=" + uid + //   lasHuellasWS["PERNR"].ToString() +
                                              "\tPin=" + uid + //   lasHuellasWS["PERNR"].ToString() +
                                              "\tValid=" + valid.ToString() +
                                              "\tFingerID=" + dedo +   //lasHuellasWS["FINDEX"].ToString() +
                                              "\tTemplate=" + "";   //lasHuellasWS["HUELLA"].ToString();
                                string options = "";
                                utilM.generarBitacoraZK("INFO", "En proceso de Enviar Huella, se crea el Usuario de codigo " + uid + "con la huella " + dedo + " en la IP " + ip + " con el SDK Plcommpro. I=" + i);
                                ret = SetDeviceData(cone, devtablename, data, options);
                                if (ret == 0)
                                {
                                    flagGet = true;
                                    msgResp = "Proceso Enviar Huella completado exitosamente. El usuario de código " + uid + " se genera con una huella " + dedo + "en la IP " + ip;
                                    utilM.generarBitacoraZK("INFO", "En proceso Enviar Huella, finaliza correctamente al crear el registro del código " + uid + "con la huella " + dedo + " en la IP " + ip);
                                }
                                else
                                {
                                    msgResp = "Incidente en proceso Enviar Huella, Ex no esperada en la " + ip + " para el usuario " + uid + " con la huella " + dedo + ". Resp_SDK = " + ret;
                                    utilM.generarBitacoraZK("ERROR", msgResp + " I=" + i);
                                    utilM.generarBitacoraZK("INFO", msgResp + " I=" + i);
                                    msgResp = "<ERROR>" + msgResp;
                                }
                                Disconnect(cone); // se desonecta el sistema
                            }
                            else
                            {
                                msgResp = "Incidente en proceso Enviar Usuario, fallo al conectar la IP " + ip + ". Resp_SDK = " + cone + ".";
                                utilM.generarBitacoraZK("ERROR", msgResp + " I=" + i);
                                msgResp = "<ERROR>" + msgResp;
                            }
                            #endregion
                            break;
                        case "U580":
                        case "F11":
                        case "INA-01":
                            //F11-U580-INA01
                            # region Sincronizar Dispositivo SDK ZKEMKEEPER
                            int idwErrorCode = 0;
                            int dwMachineNumber = 1;
                            int iUpdateFlag = 1;
                            zkemkeeper.CZKEMClass axCZKEM1 = new zkemkeeper.CZKEMClass();
                            // HACE LA CONEXION CON EL RELOJ Y SE TOMA COMO PARAMETRO LAS IPS DE LA LISTA 
                            if (axCZKEM1.Connect_Net(ip, Convert.ToInt32(puerto)))
                            {
                                utilM.generarBitacoraZK("INFO", "En proceso Enviar Huella, se detecta conexión con la IP " + ip + " con el SDK Zkemkeeper. I=" + i);
                                //CONECTA Y ACTUALIZA LA HORA DEL RELOJ 
                                if (axCZKEM1.SetDeviceTime(dwMachineNumber))
                                {
                                    //HABILITA EL MODO LECTURA/ESCRITURA EN EL RELOJ
                                    axCZKEM1.EnableDevice(dwMachineNumber, false);
                                    //HACE ESPACIO EN EL RELOJ PARA LOS NUEVOS DATOS
                                    if (axCZKEM1.BeginBatchUpdate(dwMachineNumber, iUpdateFlag))
                                    {
                                        flagGet = axCZKEM1.SetUserTmpExStr(dwMachineNumber, uid, //lasHuellasWS["PERNR"].ToString(),
                                                        System.Convert.ToInt32(dedo), //Convert.ToInt32(lasHuellasWS["FINDEX"].ToString()),
                                                       iUpdateFlag,
                                                       usuData );  //lasHuellasWS["HUELLA"].ToString()))
                                        if (flagGet)
                                        {
                                            msgResp = "Proceso Enviar Huella completado exitosamente. El usuario de código " + uid + " se genera con una huella " + dedo + "en la IP " + ip;
                                            utilM.generarBitacoraZK("INFO", "En proceso Enviar Huella, finaliza correctamente al crear el registro del código " + uid + "con la huella " + dedo + " en la IP " + ip);
                                        }
                                        else
                                        {
                                            axCZKEM1.GetLastError(ref idwErrorCode);
                                            msgResp = "Incidente en proceso Enviar Huella, Ex no esperada en la " + ip + " para el usuario " + uid + " y la huella " + dedo + ". Resp_SDK = " + idwErrorCode + ".";
                                            utilM.generarBitacoraZK("ERROR", msgResp + " I=" + i);
                                            utilM.generarBitacoraZK("INFO", msgResp + " I=" + i);
                                            msgResp = "<ERROR>" + msgResp;
                                        }
                                    }
                                    else
                                    {
                                        axCZKEM1.GetLastError(ref idwErrorCode);
                                        msgResp = "Incidente en proceso Enviar Usuario, no se creo el registro de ZK de la IP " + ip + ". Resp_SDK = " + idwErrorCode + ".";
                                        utilM.generarBitacoraZK("ERROR", msgResp + " I=" + i);
                                        msgResp = "<ERROR>" + msgResp;
                                    }
                                }
                                else
                                {
                                    axCZKEM1.GetLastError(ref idwErrorCode);
                                    msgResp = "Incidente en proceso Enviar Usuario, fallo al activar el ZK de la IP " + ip + ". Resp_SDK = " + idwErrorCode + ".";
                                    utilM.generarBitacoraZK("ERROR", msgResp + " I=" + i);
                                    msgResp = "<ERROR>" + msgResp;
                                }
                                // Apaga el equipo
                                axCZKEM1.BatchUpdate(dwMachineNumber);//SUBE LA INFORMACION AL RELOJ 
                                axCZKEM1.RefreshData(dwMachineNumber);//ACTUALIZA LA INFO EN EL RELOJ 
                                axCZKEM1.EnableDevice(dwMachineNumber, true);
                                axCZKEM1.Disconnect();
                            }
                            else
                            {
                                axCZKEM1.GetLastError(ref idwErrorCode);
                                msgResp = "Incidente en proceso Enviar Usuario, fallo al conectar la IP " + ip + ". Resp_SDK = " + idwErrorCode + ".";
                                utilM.generarBitacoraZK("ERROR", msgResp + " I=" + i);
                                msgResp = "<ERROR>" + msgResp;
                            }
                            #endregion
                            break;
                    }
                }
                catch (Exception ex)
                {
                    flagGet = false;
                    msgResp = "Error crítico en capa del sdk, Ex generada: " + ex.ToString();
                    Console.WriteLine("Ex en capa :" + ex.ToString());
                }

                // Evalua para cualqueir zk
                if (flagGet)
                    return msgResp;
            }
            return msgResp;
        }


        public string sdk_AddUsuario(string ip)
        {
            bool huella = false;

            String msgResp = "";
            Boolean flagUpdate = false;
            for (int i = 0; i < cantIntentos; i++)
            {
                try
                {

                }
                catch (Exception ex)
                {
                    flagUpdate = false;
                    msgResp = "Error crítico en capa del sdk, Ex generada: " + ex.ToString();
                    Console.WriteLine("Ex en capa :" + ex.ToString());
                }

                // Evalua para cualqueir zk
                if (flagUpdate)
                    return msgResp;
            }
            //Salida para reportar ultimo error generado
            return msgResp;
        }
    }
}