using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.InteropServices;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;

using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Diagnostics;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Runtime.Serialization.Formatters.Binary;

namespace rzkclock_ws
{
    public class met_MarcasHuellas
    {
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
        [DllImport("plcommpro.dll", EntryPoint = "DeleteDeviceData")]
        public static extern int DeleteDeviceData(IntPtr h, string tablename, string data, string options);
        [DllImport("plcommpro.dll", EntryPoint = "SetDeviceData")]
        public static extern int SetDeviceData(IntPtr h, string tablename, string data, string options);

        //Config por ambiente
        String ambienteServicio = "";
        mainConnRFC connSAP = new mainConnRFC();
        /*
        public void main_MarcasHuellas()
        {
            //Config por ambiente
            ambienteServicio = Properties.Settings.Default.SAP_WS_DEV_ZK;
            func_SapRfc func_SAPRFC = new func_SapRfc(ambienteServicio);
        }

        //--------------------------- SINCRONIZAR RELOJ -----------------------------------------------------
        // --------------------------------------------------------------------------------------------------
        public int SincronizacionReloj(string ip, string port, int dwMachineNumber)
        {
            Boolean banderaActualizo = false;
            DataTable info_RelojSAP = null;
            info_RelojSAP    = connSAP.obtenerItemRelojSAP(ip);
            if (info_RelojSAP != null && info_RelojSAP.Rows.Count == 1)
            {
                zkemkeeper.CZKEMClass axCZKEM1 = new zkemkeeper.CZKEMClass();
                # region Sincronizar hora Dispositivo SDK Plcommpro
                string str = "protocol=TCP,ipaddress=" + ip + ",port=" + port + ",timeout=2000,passwd=";// string de conexion
                IntPtr cone = IntPtr.Zero; // variable de conexion
                cone = Connect(str); // realiza la conexion
                if (cone.ToInt32() != 0) // si la conexion fue exitosa continua con el  SDK plcommpro
                {
                    int ret = 0; // variable de retorno
                    DateTime dt = DateTime.Now; // fecha y hora actual
                    int date = ((dt.Year - 2000) * 12 * 31 + (dt.Month - 1) * 31 + (dt.Day - 1)) * (24 * 60 * 60) + dt.Hour * 60 * 60 + dt.Minute * 60 + dt.Second; // convertir el la fecha en formato dispocitivo
                    string items = ("DateTime=" + date.ToString());// crear el parametro para el dispositivo
                    ret = SetDeviceParam(cone, items); // enviar el parametro
                    if (ret == 0) // si el retorno fue = exitoso
                    {
                        Disconnect(cone); // se desonecta el sistema
                        return 1;
                    }
                    else  // sino envie error
                    {
                        Disconnect(cone);
                        return -1;
                    }
                }
                #endregion
                else
                {
                    int idwErrorCode = 0;
                    //DataRow reg_RelojSAP = null;
                    //reg_RelojSAP = info_RelojSAP.Rows[0];
                    // HACE LA CONEXION CON EL RELOJ Y SE TOMA COMO PARAMETRO LAS IPS DE LA LISTA 
                    if (axCZKEM1.Connect_Net(ip, Convert.ToInt32(port)))
                    {
                        if (axCZKEM1.SetDeviceTime(dwMachineNumber))
                        {
                            //ACTUALIZA LA HORA DEL RELOJ 
                            axCZKEM1.RefreshData(dwMachineNumber);
                            string ip_return = "";
                            axCZKEM1.GetDeviceIP(dwMachineNumber, ip_return);
                            banderaActualizo = true;
                        }
                    }

                    //DESHABILITA EL MODO LECTURA/ESCRITURA EN EL RELOJ
                    axCZKEM1.EnableDevice(dwMachineNumber, true);
                    axCZKEM1.Disconnect();
                }
                //VERIFICA ESTADO
                if (banderaActualizo) { return 1; }
                // else axCZKEM1.GetLastError(ref idwErrorCode);

            }
            //salida Error
            Console.WriteLine("No existe la IP indicada en SAP.");
            return -1;
        }

        //-------------------- BORRA LAS MARCAS ALMACENADAS EN UN RELOJ ESPECIFICO --------------------------
        // --------------------------------------------------------------------------------------------------
        public int BorrarMarcasReloj(string ip, string port, int dwMachineNumber)
        {
            //Config por ambiente
            String ambienteServicio = Properties.Settings.Default.SAP_WS_DEV_ZK;
            func_SapRfc connSAP = new func_SapRfc(ambienteServicio);
            Boolean banderaElimino = false;
            string str = "protocol=TCP,ipaddress=" + ip + ",port=" + port + ",timeout=2000,passwd=";
            IntPtr cone = IntPtr.Zero; // variable de conexion
            cone = Connect(str); // realiza la conexion
            #region Metodo plcomm
            if (cone.ToInt32() != 0) // si la conexion fue exitosa continua con el  SDK plcommpro
            {
                int ret = 0;
                string tablename = "transaction";
                string filtro = "";
                string options = "";
                if (IntPtr.Zero != cone)
                {
                    ret = DeleteDeviceData(cone, tablename, filtro, options);
                    Disconnect(cone);
                    if (ret == 0)
                    { return 1; }
                    else { return -1; }
                }
            }
            #endregion
            else
            {
                #region Dispositivos con SDK zkemkeeper
                DataTable info_RelojSAP = connSAP.obtenerItemRelojSAP(ip);
                if (info_RelojSAP != null && info_RelojSAP.Rows.Count == 1)
                {
                    zkemkeeper.CZKEMClass axCZKEM1 = new zkemkeeper.CZKEMClass();
                    DataRow reg_RelojSAP = info_RelojSAP.Rows[0];
                    // HACE LA CONEXION CON EL RELOJ Y SE TOMA COMO PARAMETRO LAS IPS DE LA LISTA 
                    if ((reg_RelojSAP["ESTADO"].ToString() == "X") &&
                            (axCZKEM1.Connect_Net(ip, Convert.ToInt32(port))))
                    {
                        //disable the device
                        axCZKEM1.EnableDevice(dwMachineNumber, false);
                        //BORRA LOS DATOS DEL RELOJ 
                        if (axCZKEM1.ClearGLog(dwMachineNumber))
                        {
                            banderaElimino = true;
                            axCZKEM1.RefreshData(dwMachineNumber);
                        }
                        else
                        {
                            int idwErrorCode = 0;
                            axCZKEM1.GetLastError(ref idwErrorCode);
                        }
                    }
                    //enable the device
                    axCZKEM1.EnableDevice(dwMachineNumber, true);
                    axCZKEM1.Disconnect();

                    //VERIFICA ESTADO
                    if (banderaElimino) { return 1; }
                }
                #endregion
            }

            //salida Error
            Console.WriteLine("No existe la IP indicada en SAP.");
            return -1;
        }

        //-------------------- BORRA LAS MARCAS ALMACENADAS EN UN RELOJ ESPECIFICO --------------------------
        // --------------------------------------------------------------------------------------------------
        public int BorrarUsuarioReloj(string tipo, string ip, string port, string dwEnrollNumber)
        {
            //Config por ambiente
            String ambienteServicio = Properties.Settings.Default.SAP_WS_DEV_ZK;
            func_SapRfc connSAP = new func_SapRfc(ambienteServicio);

            string str = "protocol=TCP,ipaddress=" + ip + ",port=" + port + ",timeout=2000,passwd=";
            IntPtr cone = IntPtr.Zero; // variable de conexion
            cone = Connect(str); // realiza la conexion
            #region Metodo plcomm
            if (cone.ToInt32() != 0) // si la conexion fue exitosa continua con el  SDK plcommpro
            {
                if (IntPtr.Zero != cone)
                {
                    int ret = 0;
                    string devtablename = "user";
                    string data = "";

                    if (tipo.Equals("MULTIPLE"))
                    {

                        String item = "\r\n";
                        string[] ids = dwEnrollNumber.Split(',');
                        foreach (string id in ids)
                        {
                            data = data + "Pin=" + id + item;
                        }
                    }
                    else
                        data = "Pin=" + dwEnrollNumber;

                    string options = "";
                    ret = DeleteDeviceData(cone, devtablename, data, options);
                    //limpia las huellas asociadas al usuario por borrar
                    devtablename = "templatev10";
                    DeleteDeviceData(cone, devtablename, data, options);
                    //limpia los horarios de marca del usuario por borrar
                    devtablename = "userauthorize";
                    DeleteDeviceData(cone, devtablename, data, options);

                    Disconnect(cone);
                    if (ret == 0)
                    { return 1; }
                    else { return -1; }
                }
            }
            #endregion
            else
            {
                #region enviar empleado zkemkeeper
                zkemkeeper.CZKEMClass axCZKEM1 = new zkemkeeper.CZKEMClass();
                // HACE LA CONEXION CON EL RELOJ Y SE TOMA COMO PARAMETRO LAS IPS DE LA LISTA 
                if (axCZKEM1.Connect_Net(ip, Convert.ToInt32(port)))
                {
                    //Variables para borrar usuario
                    int idwErrorCode = 0;
                    int dwMachineNumber = 1;
                    int dwEMachineNumber = 0;
                    int dwBackupNumber = 12;
                    Boolean estadoCarga = false;

                    //HABILITA EL MODO LECTURA/ESCRITURA EN EL RELOJ
                    axCZKEM1.EnableDevice(dwMachineNumber, false);
                    if (tipo.Equals("MULTIPLE"))
                    {
                        string[] ids = dwEnrollNumber.Split(',');
                        foreach (string id in ids)
                        {
                            estadoCarga = (axCZKEM1.DeleteEnrollData(dwMachineNumber, System.Convert.ToInt32(id),
                                                dwEMachineNumber, dwBackupNumber));
                        }
                    }
                    else
                    {
                        DataTable info_RelojSAP = connSAP.obtenerItemRelojSAP(ip);
                        DataRow reg_RelojSAP = info_RelojSAP.Rows[0];
                        DataTable info_EmpleadoSAP = connSAP.obtenerItemEmpleadoSAP(dwEnrollNumber, reg_RelojSAP["BDEGR"].ToString());
                        if (info_EmpleadoSAP != null && info_EmpleadoSAP.Rows.Count == 1)
                        {
                            estadoCarga = (axCZKEM1.DeleteEnrollData(dwMachineNumber,
                                            System.Convert.ToInt32(info_EmpleadoSAP.Rows[0]["PERNR"].ToString()),
                                            dwEMachineNumber, dwBackupNumber));
                        }
                    }

                    //Verifica la conexion exitosa con el reloj;
                    if (estadoCarga)
                    {
                        axCZKEM1.BatchUpdate(dwMachineNumber);//SUBE LA INFORMACION AL RELOJ 
                        axCZKEM1.RefreshData(dwMachineNumber);//ACTUALIZA LA INFO EN EL RELOJ 
                        axCZKEM1.EnableDevice(dwMachineNumber, true);
                        axCZKEM1.Disconnect();
                        return 1;
                    }
                }
                else
                {
                    Console.WriteLine("No existe empleado relacionado al codigo recibido.");
                    return -1;

                }
                //salida Error
                Console.WriteLine("No existe la IP indicada en SAP.");
                return -1;
                #endregion
            }
            //salida Error
            return -1;
        }

        //---------------- SERV EXTRAE PERSONAS CONTENIDAS EN SAP Y LAS ENVIA A RELOJ -----------------------
        // --------------------------------------------------------------------------------------------------
        public int EnviarEmpleado_SAPtoReloj(string ip, string port, string dwEnrollNumber)
        {
            //Config por ambiente
            String ambienteServicio = Properties.Settings.Default.SAP_WS_DEV_ZK;
            func_SapRfc connSAP = new func_SapRfc(ambienteServicio);
            DataTable info_RelojSAP = connSAP.obtenerItemRelojSAP(ip);

            string str = "protocol=TCP,ipaddress=" + ip + ",port=" + port + ",timeout=2000,passwd=";
            IntPtr cone = IntPtr.Zero; // variable de conexion
            cone = Connect(str); // realiza la conexion
            #region plcomm
            if (cone.ToInt32() != 0) // si la conexion fue exitosa continua con el  SDK plcommpro
            {
                int ret = 0;
                if (info_RelojSAP != null && info_RelojSAP.Rows.Count == 1)
                {
                    DataRow reg_RelojSAP = info_RelojSAP.Rows[0];
                    DataTable info_EmpleadoSAP = connSAP.obtenerItemEmpleadoSAP(dwEnrollNumber, reg_RelojSAP["BDEGR"].ToString());
                    if (info_EmpleadoSAP != null && info_EmpleadoSAP.Rows.Count == 1)
                    {
                        string data = "", options = "", devtablename = "";
                        //Asignación del horario para puerta por usuario
                        devtablename = "timeZone";
                        //Primero limpia el registro actual y despues agrega el Horario
                        data = "TimezoneId=1";
                        DeleteDeviceData(cone, devtablename, data, options);
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
                        int ret2 = SetDeviceData(cone, devtablename, data, options);

                        //Crea los datos del usuario
                        devtablename = "user";
                        string nombreCarga = info_EmpleadoSAP.Rows[0]["CNAME"].ToString().Replace('_', ' ');
                        data = "CardNo=" + info_EmpleadoSAP.Rows[0]["PERNR"].ToString() +
                                      "\tUID=" + info_EmpleadoSAP.Rows[0]["PERNR"].ToString() +
                                      "\tPin=" + info_EmpleadoSAP.Rows[0]["PERNR"].ToString() +
                                      "\tName=" + nombreCarga +
                                      "\tPassword=1\tGroup=0\tPrivilege=0";
                        ret = SetDeviceData(cone, devtablename, data, options);

                        //Permisos en puerta para nuevo usuario
                        devtablename = "userauthorize";
                        data = "Pin=" + info_EmpleadoSAP.Rows[0]["PERNR"].ToString() +
                            //data = "Pin=1691" +
                                "\tAuthorizeTimezoneId=1\tAuthorizeDoorId=1";
                        int ret3 = SetDeviceData(cone, devtablename, data, options);

                        Disconnect(cone);
                        if (ret == 0) { return 1; }
                        else { return -1; }

                    }
                }
                Disconnect(cone);
            }
            #endregion
            else
            {
                #region enviar empleado zkemkeeper

                if (info_RelojSAP != null && info_RelojSAP.Rows.Count == 1)
                {
                    DataRow reg_RelojSAP = info_RelojSAP.Rows[0];
                    DataTable info_EmpleadoSAP = connSAP.obtenerItemEmpleadoSAP(dwEnrollNumber, reg_RelojSAP["BDEGR"].ToString());
                    if (info_EmpleadoSAP != null && info_EmpleadoSAP.Rows.Count == 1)
                    {
                        int iUpdateFlag = 1;
                        int dwMachineNumber = 1;
                        zkemkeeper.CZKEMClass axCZKEM1 = new zkemkeeper.CZKEMClass();
                        // HACE LA CONEXION CON EL RELOJ Y SE TOMA COMO PARAMETRO LAS IPS DE LA LISTA 
                        if (axCZKEM1.Connect_Net(ip, Convert.ToInt32(port)))
                        {
                            int idwErrorCode = 0;
                            //HABILITA EL MODO LECTURA/ESCRITURA EN EL RELOJ
                            axCZKEM1.EnableDevice(dwMachineNumber, false);
                            //HACE ESPACIO EN EL RELOJ PARA LOS NUEVOS DATOS
                            if (axCZKEM1.BeginBatchUpdate(dwMachineNumber, iUpdateFlag))
                            {
                                //Variables para nuevo usuario
                                int user_autorizacion = 0;
                                string user_clave = dwEnrollNumber;
                                bool user_actividad = false;
                                string nombreCarga = info_EmpleadoSAP.Rows[0]["CNAME"].ToString().Replace('_', ' ');

                                if (info_EmpleadoSAP.Rows[0]["ENABLE"].ToString() == "X") { user_actividad = true; }
                                Boolean estadoCarga = false;
                                string tipoProceso = reg_RelojSAP["INFO"].ToString().Contains("U580") ||
                                                        reg_RelojSAP["INFO"].ToString().Contains("UF11")
                                                            ? "basico" : "estandar";
                                switch (tipoProceso)
                                {
                                    case "basico":
                                        if (reg_RelojSAP["INFO"].ToString().Contains("UF11"))
                                        {
                                            string[] name = nombreCarga.Split(' ');
                                            nombreCarga = name.Length > 1 ? name[2] + " " + name[0] :
                                                                          name[name.Length - 1] + " " + name[0];
                                        }

                                        estadoCarga = (axCZKEM1.SetUserInfo(dwMachineNumber,
                                                        System.Convert.ToInt32(info_EmpleadoSAP.Rows[0]["PERNR"].ToString()),
                                                        nombreCarga, info_EmpleadoSAP.Rows[0]["PERNR"].ToString(),
                                                        user_autorizacion, user_actividad));
                                        break;

                                    case "estandar":
                                        estadoCarga = (axCZKEM1.SSR_SetUserInfo(dwMachineNumber, info_EmpleadoSAP.Rows[0]["PERNR"].ToString(),
                                                            nombreCarga, info_EmpleadoSAP.Rows[0]["PERNR"].ToString(),
                                                            user_autorizacion, user_actividad));
                                        break;
                                }

                                axCZKEM1.BatchUpdate(dwMachineNumber);//SUBE LA INFORMACION AL RELOJ 
                                axCZKEM1.RefreshData(dwMachineNumber);//ACTUALIZA LA INFO EN EL RELOJ 
                                axCZKEM1.EnableDevice(dwMachineNumber, true);
                                axCZKEM1.Disconnect();
                                return 1;
                            }
                            else
                            {
                                axCZKEM1.GetLastError(ref idwErrorCode);
                                axCZKEM1.EnableDevice(dwMachineNumber, true);
                                axCZKEM1.Disconnect();
                                Console.WriteLine("No se puede generar espacio en el reloj.");
                                return -1;
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("No existe empleado relacionado al codigo recibido.");
                        return -1;
                    }
                }
                //salida Error
                Console.WriteLine("No existe la IP indicada en SAP.");
                return -1;
                #endregion
            }
            return -1;
        }

        //---------------- SERV EXTRAE HUELLAS CONTENIDAS EN SAP Y LAS ENVIA A RELOJ ------------------------
        // --------------------------------------------------------------------------------------------------
        public int EnviarHuellaEmpleado_SAPtoReloj(string ip, string port, string dwEnrollNumber)
        {
            //Config por ambiente
            String ambienteServicio = Properties.Settings.Default.SAP_WS_DEV_ZK;
            func_SapRfc connSAP = new func_SapRfc(ambienteServicio);

            DataTable info_RelojSAP = connSAP.obtenerItemRelojSAP(ip);
            if (info_RelojSAP != null && info_RelojSAP.Rows.Count == 1)
            {
                DataRow reg_RelojSAP = info_RelojSAP.Rows[0];
                DataTable info_EmpleadoSAP = connSAP.obtenerItemEmpleadoSAP(dwEnrollNumber, reg_RelojSAP["BDEGR"].ToString());
                DataTable info_EmpleadoHuellasSAP = connSAP.obtenerItemHuellasEmpleadoSAP(dwEnrollNumber, "");

                if (info_EmpleadoSAP != null && info_EmpleadoSAP.Rows.Count == 1 &&
                    info_EmpleadoHuellasSAP != null && info_EmpleadoHuellasSAP.Rows.Count > 0)
                {
                    DataTable resultadoregistro = new DataTable();
                    resultadoregistro.Columns.Add("DEDO");
                    resultadoregistro.Columns.Add("HUELLA");
                    resultadoregistro.Columns.Add("PERNR");
                    resultadoregistro.Columns.Add("IP");
                    resultadoregistro.Columns.Add("FINDEX");

                    //Castea cada huella en el reg tipo usuario
                    foreach (DataRow usuarios_ip in info_EmpleadoSAP.Rows)
                    {
                        foreach (DataRow huellas in info_EmpleadoHuellasSAP.Rows)
                        {
                            if (usuarios_ip["PERNR"].ToString().Equals(huellas["PERNR"].ToString()))
                            {
                                DataRow filaHuella_SAP = resultadoregistro.NewRow();
                                filaHuella_SAP["DEDO"] = huellas["DEDO"];
                                filaHuella_SAP["HUELLA"] = huellas["HUELLA"];
                                filaHuella_SAP["PERNR"] = huellas["PERNR"];
                                filaHuella_SAP["IP"] = huellas["IP"];
                                filaHuella_SAP["FINDEX"] = huellas["FINDEX"];
                                resultadoregistro.Rows.Add(filaHuella_SAP);
                            }
                        }
                    }

                    //Carga las huellas si se encuentran
                    if (resultadoregistro.Rows.Count > 0)
                    {
                        int ret = -1;
                        string str = "protocol=TCP,ipaddress=" + ip + ",port=" + port + ",timeout=2000,passwd=";
                        IntPtr cone = IntPtr.Zero; // variable de conexion
                        cone = Connect(str); // realiza la conexion
                        #region Enviar Huellas con SDK Plcomm
                        if (cone.ToInt32() != 0) // si la conexion fue exitosa continua con el  SDK plcommpro
                        {
                            for (int count_huellas = 0; count_huellas < resultadoregistro.Rows.Count; count_huellas++)
                            {
                                DataRow lasHuellasWS = resultadoregistro.Rows[count_huellas];
                                if (lasHuellasWS["DEDO"].ToString() == "IZ") lasHuellasWS["FINDEX"] = "1";
                                string devtablename = "templatev10";
                                Byte valid = 3;
                                
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
                                
                                string data = "UID=" + lasHuellasWS["PERNR"].ToString() +
                                              "\tPin=" + lasHuellasWS["PERNR"].ToString() +
                                              "\tValid=" + valid.ToString() +
                                              "\tFingerID=" + lasHuellasWS["FINDEX"].ToString() +
                                              "\tTemplate=" + lasHuellasWS["HUELLA"].ToString();
                                string options = "";
                                ret = SetDeviceData(cone, devtablename, data, options);
                            }

                            //Finaliza conexion con dispo y envia ultima respuesta
                            Disconnect(cone);
                            if (ret == 0) { return 1; }
                            else { return -1; }
                        }
                        #endregion
                        else
                        {
                            #region enviar huellas con sdk zkemkeeper
                            zkemkeeper.CZKEMClass axCZKEM1 = new zkemkeeper.CZKEMClass();
                            // HACE LA CONEXION CON EL RELOJ Y SE TOMA COMO PARAMETRO LAS IPS DE LA LISTA 
                            if (axCZKEM1.Connect_Net(ip, Convert.ToInt32(port)))
                            {
                                Boolean estadoEnvio = true;
                                int dwMachineNumber = 1;
                                for (int count_huellas = 0; count_huellas < resultadoregistro.Rows.Count; count_huellas++)
                                {
                                    DataRow lasHuellasWS = resultadoregistro.Rows[count_huellas];
                                    int idwErrorCode = 0;
                                    int iUpdateFlag = 1;
                                    int iFlag = 1;

                                    // HABILITA EL RELOJ PARA LECTURA/ESCRITURA
                                    axCZKEM1.EnableDevice(dwMachineNumber, false);
                                    //HACE ESPACIO EN EL RELOJ PARA LOS NUEVOS DATOS
                                    if (axCZKEM1.BeginBatchUpdate(dwMachineNumber, iUpdateFlag))
                                    {
                                        if (lasHuellasWS["DEDO"].ToString() == "IZ") lasHuellasWS["FINDEX"] = "1";
                                        //EJECUTA PROCESO PARA CARGA DE DATOS
                                        if (axCZKEM1.SetUserTmpExStr(dwMachineNumber, lasHuellasWS["PERNR"].ToString(),
                                                        Convert.ToInt32(lasHuellasWS["FINDEX"].ToString()), iFlag,
                                                        lasHuellasWS["HUELLA"].ToString()))
                                        {
                                            axCZKEM1.BatchUpdate(dwMachineNumber);//SUBE LA INFORMACION AL RELOJ 
                                            axCZKEM1.RefreshData(dwMachineNumber);//ACTUALIZA LA INFO EN EL RELOJ 
                                        }
                                        else
                                        {
                                            axCZKEM1.GetLastError(ref idwErrorCode);
                                            Console.WriteLine("No se puede generar espacio para la huella-" +
                                                    lasHuellasWS["FINDEX"].ToString());
                                        }
                                    }
                                    else
                                    {
                                        axCZKEM1.GetLastError(ref idwErrorCode);
                                        Console.WriteLine("No se puede generar espacio en la huella.");
                                        estadoEnvio = false;
                                    }
                                }

                                axCZKEM1.EnableDevice(dwMachineNumber, true);
                                axCZKEM1.Disconnect();
                                //Valor estado general actualizacion
                                return (estadoEnvio) ? 1 : -1;
                            }
                            #endregion
                        }
                    }
                }
                else
                {
                    Console.WriteLine("No existe empleado/huella relacionado al codigo recibido.");
                    return -1;
                }
            }
            //salida Error
            Console.WriteLine("No existe la IP indicada en SAP.");
            return -1;
        }

        //Metodo que consulta los empleados registrados en un relog y retorna una lista
        public string getlistEmpleado_fromReloj(string ip, string port, int dwMachineNumber, string dwEnrollNumber)
        {
            bool huella = false;

            //Config por ambiente
            String ambienteServicio = Properties.Settings.Default.SAP_WS_DEV_ZK;
            func_SapRfc connSAP = new func_SapRfc(ambienteServicio);

            //Traer los datos del relog
            DataTable info_RelojSAP = connSAP.obtenerItemRelojSAP(ip);
            string str = "protocol=TCP,ipaddress=" + ip + ",port=" + port + ",timeout=2000,passwd=";// string de conexion
            IntPtr cone = IntPtr.Zero; // variable de conexion
            cone = Connect(str); // realiza la conexion
            #region Metodo plcomm
            if (cone.ToInt32() != 0) // si la conexion fue exitosa continua con el  SDK plcommpro
            {
                DataTable resultadoregistro = new DataTable();
                resultadoregistro.Columns.Add("PERNR");
                resultadoregistro.Columns.Add("DEDO");
                resultadoregistro.Columns.Add("HUELLA");
                int retPerso = 0, ret = 0;
                int BUFFERSIZE = 10 * 1024 * 1024;
                byte[] buffer = new byte[BUFFERSIZE];

                string devtablename = "user";
                string data = "Pin\tName";
                string options = "";
                string devdatfilter = "";
                if (dwEnrollNumber != "") { devdatfilter = "Pin=" + dwEnrollNumber.ToString(); }
                retPerso = GetDeviceData(cone, ref buffer[0], BUFFERSIZE, devtablename, data, devdatfilter, options);
                if (retPerso > 0) // si el retorno fue = exitoso
                {
                    List<string> datos_arrayPerso = normaliza_buffer(buffer);
                    devtablename = "templatev10";
                    data = "Pin\tFingerID\tTemplate";
                    if (dwEnrollNumber != "") { devdatfilter = "Pin=" + dwEnrollNumber.ToString(); }
                    ret = GetDeviceData(cone, ref buffer[0], BUFFERSIZE, devtablename, data, devdatfilter, options);

                    int iPos = 2, indexPerso = 0, indexPos = 0;
                    List<string> datos_arrayHuellas = normaliza_buffer(buffer);

                    while (indexPerso < retPerso)
                    {
                        DataRow row = resultadoregistro.NewRow();
                        row["PERNR"] = datos_arrayPerso[iPos];
                        for (int i = 3; i < datos_arrayHuellas.Count; i += 3)
                        {
                            if (datos_arrayPerso[indexPos + 2] == datos_arrayHuellas[i])
                            {
                                row["DEDO"] = datos_arrayHuellas[i + 1];
                                row["HUELLA"] = "X";//datos_arrayHuellas[i + 2];
                                break;
                            }
                        }
                        //Agrega el reg
                        resultadoregistro.Rows.Add(row);
                        indexPerso = indexPerso + 1;
                        indexPos = indexPos + 2;
                        iPos = iPos + 2;
                    }

                    Disconnect(cone); // se desonecta el sistema
                    return getlistempleado_ToString(resultadoregistro);
                }
                else  // sino envie error
                {
                    Disconnect(cone);
                    return "Error en el SDK";
                }

            }
            #endregion
            else
            {
                #region Relojes con Zkemkeeper sdk
                if (info_RelojSAP != null && info_RelojSAP.Rows.Count == 1)
                {
                    DataRow reg_RelojSAP = info_RelojSAP.Rows[0];
                    zkemkeeper.CZKEMClass axCZKEM1 = new zkemkeeper.CZKEMClass();
                    // HACE LA CONEXION CON EL RELOJ Y SE TOMA COMO PARAMETRO LAS IPS DE LA LISTA 
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

                            //Estrucutra para almacenar los datos
                            DataTable resultadoregistro = new DataTable();
                            resultadoregistro.Columns.Add("PERNR");
                            resultadoregistro.Columns.Add("DEDO");
                            resultadoregistro.Columns.Add("HUELLA");

                            // FILTRO: SI NO SE PONE ID DE USUARIO, EL WEBSERVICE DEL RELOJ RETORNA
                            //  TODAS LAS HUELLAS QUE TIENEN TODOS LOS USUARIOS 
                            if (dwEnrollNumber == "")
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
                                switch (tipoProceso)
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
                                                        DataRow fila = getREmpleado(resultadoregistro.NewRow(), dwEnrollNumberAUX.ToString(), dwFingerIndex, TmpData);

                                                        //Agrega cada reg para el envio
                                                        resultadoregistro.Rows.Add(fila);
                                                    }
                                                }
                                            }

                                            //No tiene huella, se agrega al menos una vez
                                            if (huella == false)
                                            {
                                                DataRow fila = getREmpleado(resultadoregistro.NewRow(), dwEnrollNumberAUX.ToString(), -1, "");

                                                //Agrega cada reg para el envio
                                                resultadoregistro.Rows.Add(fila);
                                            }
                                        }
                                        break;

                                    case "estandar":
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
                    } //Conexion con dispositivo
                    else
                    {
                        //enable the device
                        axCZKEM1.EnableDevice(dwMachineNumber, true);
                        axCZKEM1.Disconnect();
                        //salida Error
                        String msj = "<MSG>No se ha establecido conexión con el dispositivo.</MSG>";
                        Console.WriteLine(msj);
                        return msj;
                    }
                }
                else
                {
                    //salida Error
                    String msj = "<MSG>No existe la IP indicada en SAP.</MSG>";
                    Console.WriteLine(msj);
                    return msj;
                }
                #endregion
            }
            return "<MSG>No se obtuvo información del dispositivo.</MSG>";
        }
        //Retorna String de empleados
        private string getlistempleado_ToString(DataTable data)
        {
            String result = "";

            //Recorrer registros
            for (int row = 0; row < data.Rows.Count; row++)
            {
                if (row > 0)
                {
                    result = result + ",";
                }
                result = result + "{" + "\"PERNR\":\"" + data.Rows[row]["PERNR"] + "\",\"DEDO\":\"" + data.Rows[row]["DEDO"] +
                         "\",\"HUELLA\":\"" + data.Rows[row]["HUELLA"] + "\"}";

            }

            result = "[" + result + "]";
            return result;
        }

        private DataRow getREmpleado(DataRow fila, string dwEnrollNumber, int dwFingerIndex, string TmpData)
        {

            //Cast de cada valor en su campo
            fila["PERNR"] = dwEnrollNumber.ToString();

            if (dwFingerIndex > 0 && dwFingerIndex < 5)
            {
                fila["DEDO"] = "D" + (dwFingerIndex + 1).ToString();
            }
            else if (dwFingerIndex >= 5)
            {
                fila["DEDO"] = "I" + (dwFingerIndex - 4).ToString();
            }

            fila["HUELLA"] = TmpData.Length > 0 ? "X" : ""; //Si hay huella se pasa X
            return fila;
        }

        //---------------------- SERV EXTRAE HUELLAS DE RELOJ Y LAS CARGA EN SAP ----------------------------
        // --------------------------------------------------------------------------------------------------
        public int Extraer_HuellasReloj(string ip, string port, int dwMachineNumber, string dwEnrollNumber)
        {
            //Config por ambiente
            String ambienteServicio = Properties.Settings.Default.SAP_WS_DEV_ZK;
            func_SapRfc connSAP = new func_SapRfc(ambienteServicio);

            DataTable info_RelojSAP = connSAP.obtenerItemRelojSAP(ip);
            if (info_RelojSAP != null && info_RelojSAP.Rows.Count == 1)
            {
                //Estrucutra carga en SAP
                DataTable resultadoregistro = new DataTable();
                resultadoregistro.Columns.Add("PERNR");
                resultadoregistro.Columns.Add("DEDO");
                resultadoregistro.Columns.Add("HUELLA");

                DataRow reg_RelojSAP = info_RelojSAP.Rows[0];

                int ret;
                string str = "protocol=TCP,ipaddress=" + ip + ",port=" + port + ",timeout=2000,passwd=";
                IntPtr cone = IntPtr.Zero; // variable de conexion
                cone = Connect(str); // realiza la conexion
                #region Metodo plcomm
                if (cone.ToInt32() != 0) // si la conexion fue exitosa continua con el  SDK plcommpro
                {
                    int BUFFERSIZE = 10 * 1024 * 1024;
                    byte[] buffer = new byte[BUFFERSIZE];
                    string devtablename = "templatev10";
                    string data = "UID\tFingerID\tTemplate";
                    string devdatfilter = "";
                    if (dwEnrollNumber != "") { devdatfilter = "UID=" + dwEnrollNumber.ToString(); }
                    string options = "";
                    ret = GetDeviceData(cone, ref buffer[0], BUFFERSIZE, devtablename, data, devdatfilter, options);
                    if (ret == 0) // si el retorno fue = exitoso
                    {
                        List<string> datos_array = normaliza_buffer(buffer);
                        for (int i = 3; i < datos_array.Count; i += 3)
                        {
                            DataRow row = resultadoregistro.NewRow();
                            row["PERNR"] = datos_array[i];
                            row["DEDO"] = datos_array[i + 1];
                            row["HUELLA"] = datos_array[i + 2];
                            resultadoregistro.Rows.Add(row);

                        }

                        Disconnect(cone); // se desonecta el sistema
                        return connSAP.servicioSAP_CargaHuellasDeReloj(resultadoregistro);
                    }
                    else  // sino envie error
                    {
                        Disconnect(cone);
                        return -1;
                    }

                }
                #endregion
                else
                {
                    #region Zkemkeeper
                    zkemkeeper.CZKEMClass axCZKEM1 = new zkemkeeper.CZKEMClass();
                    // HACE LA CONEXION CON EL RELOJ Y SE TOMA COMO PARAMETRO LAS IPS DE LA LISTA 
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
                            if (dwEnrollNumber == "")
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
                                switch (tipoProceso)
                                {
                                    case "basico":
                                        while (axCZKEM1.GetAllUserInfo(dwMachineNumber, ref dwEnrollNumberAUX, ref Name, ref Password, ref Privilege, ref Enabled))
                                        {
                                            for (dwFingerIndex = 0; dwFingerIndex < 10; dwFingerIndex++)
                                            {
                                                //TOMA LAS HUELLAS COMO STRINGS Y SU RESPECTIVO TAMANO
                                                if (axCZKEM1.GetUserTmpExStr(dwMachineNumber, dwEnrollNumberAUX.ToString(), dwFingerIndex, out iFlag, out TmpData, out TmpLength))
                                                {
                                                    DataRow fila = resultadoregistro.NewRow();
                                                    //Cast de cada valor en su campo
                                                    fila["PERNR"] = dwEnrollNumberAUX.ToString();
                                                    fila["DEDO"] = dwFingerIndex.ToString();
                                                    fila["HUELLA"] = TmpData;
                                                    //Agrega cada reg para el envio
                                                    resultadoregistro.Rows.Add(fila);
                                                }
                                            }
                                        }
                                        break;

                                    case "estandar":
                                        while (axCZKEM1.SSR_GetAllUserInfo(dwMachineNumber, out dwEnrollNumber, out Name, out Password, out Privilege, out Enabled)) //TOMA LAS HUELLAS EN EL RELOJ
                                        {
                                            for (dwFingerIndex = 0; dwFingerIndex < 10; dwFingerIndex++)
                                            {
                                                //TOMA LAS HUELLAS COMO STRINGS Y SU RESPECTIVO TAMANO
                                                if (axCZKEM1.GetUserTmpExStr(dwMachineNumber, dwEnrollNumber, dwFingerIndex, out iFlag, out TmpData, out TmpLength))//TOMA LAS HUELLAS COMO STRINGS Y SU RESPECTIVO TAMANO
                                                {
                                                    DataRow fila = resultadoregistro.NewRow();
                                                    //Cast de cada valor en su campo
                                                    fila["PERNR"] = dwEnrollNumber.ToString();
                                                    fila["DEDO"] = dwFingerIndex.ToString();
                                                    fila["HUELLA"] = TmpData;
                                                    //Agrega cada reg para el envio
                                                    resultadoregistro.Rows.Add(fila);
                                                }
                                            }
                                        }
                                        break;
                                }
                            }
                            else
                            {
                                //SE HACE LA BUSQUEDA POR EL USUARIO DIGITADO 
                                resultadoregistro.Clear();
                                for (dwFingerIndex = 0; dwFingerIndex < 10; dwFingerIndex++)
                                {
                                    if (axCZKEM1.GetUserTmpExStr(dwMachineNumber, dwEnrollNumber, dwFingerIndex, out iFlag, out TmpData, out TmpLength))//(axCZKEM1.GetUserTmpExStr(dwMachineNumber, dwEnrollNumber, dwFingerIndex, out iFlag, out TmpData, out TmpLength))//TOMA LAS HUELLAS COMO STRINGS Y SU RESPECTIVO TAMANO
                                    {
                                        DataRow fila = resultadoregistro.NewRow();
                                        //Cast de cada valor en su campo
                                        fila["PERNR"] = dwEnrollNumber.ToString();
                                        fila["DEDO"] = dwFingerIndex.ToString();
                                        fila["HUELLA"] = TmpData;
                                        //Agrega cada reg para el envio
                                        resultadoregistro.Rows.Add(fila);
                                    }
                                }
                            }
                        }

                        //DESHABILITA PARA LECTURA/ ESCRITURA EL RELOJ
                        axCZKEM1.EnableDevice(dwMachineNumber, true);
                        axCZKEM1.Disconnect();
                        if (resultadoregistro.Rows.Count > 0)
                        {
                            //SI HAY DATOS LOS AGREGA EN SAP
                            return connSAP.servicioSAP_CargaHuellasDeReloj(resultadoregistro);
                        }
                    }

                    #endregion
                }
            }
            //salida Error
            Console.WriteLine("No existe la IP indicada en SAP.");
            return -1;
        }

        //-------------------- SERV EXTRAE MARCAS Y GENERA TABLA PARA CARGA EN SAP --------------------------
        // --------------------------------------------------------------------------------------------------
        public int Extraer_MarcasReloj(string ip, string port, int dwMachineNumber)
        {
            zkemkeeper.CZKEMClass axCZKEM1 = new zkemkeeper.CZKEMClass();
            //Config por ambiente
            String ambienteServicio = Properties.Settings.Default.SAP_WS_DEV_ZK;
            func_SapRfc connSAP = new func_SapRfc(ambienteServicio);

            DataTable info_RelojSAP = connSAP.obtenerItemRelojSAP(ip);
            if (info_RelojSAP != null && info_RelojSAP.Rows.Count == 1)
            {
                int dwEnrollNumber2 = 0, dwReserved = 0, dwVerifyMode = 0, dwInOutMode = 0;
                int dwWorkCode = 0, dwYear = 0, dwMonth = 0, dwDay = 0, dwHour = 0, dwMinute = 0, dwSecond = 0;
                string dwEnrollNumberAux = "0", mes, dia, ano, hora, minuto, segundo;

                DataTable resultadoregistro = new DataTable();
                resultadoregistro.Columns.Add("FECHA");
                resultadoregistro.Columns.Add("HORA");
                resultadoregistro.Columns.Add("IP");
                resultadoregistro.Columns.Add("PERNR");
                resultadoregistro.Columns.Add("MODO");
                resultadoregistro.Columns.Add("TIPO");
                DataRow reg_RelojSAP = info_RelojSAP.Rows[0];
                // validacion para plcomm
                int ret;
                string str = "protocol=TCP,ipaddress=" + ip + ",port=" + port + ",timeout=2000,passwd=";
                IntPtr cone = IntPtr.Zero; // variable de conexion
                cone = Connect(str); // realiza la conexion
                #region Metodo plcomm
                if (cone.ToInt32() != 0) // si la conexion fue exitosa continua con el  SDK plcommpro
                {
                    int BUFFERSIZE = 10 * 1024 * 1024;
                    byte[] buffer = new byte[BUFFERSIZE];
                    string devtablename = "transaction";
                    string data = "Pin\tVerified\tInOutState\tTime_second";
                    string devdatfilter = "";
                    string options = "";
                    ret = GetDeviceData(cone, ref buffer[0], BUFFERSIZE, devtablename, data, devdatfilter, options);
                    if (ret > 0) // si el retorno fue = exitoso
                    {
                        List<string> datos_array = normaliza_buffer(buffer);
                        for (int i = 4; i < datos_array.Count; i += 4)
                        {
                            if (Convert.ToInt32(datos_array[i]) > 0)
                            {
                                DataRow row = resultadoregistro.NewRow();
                                row["PERNR"] = datos_array[i];
                                row["TIPO"] = datos_array[i + 1];
                                row["MODO"] = datos_array[i + 2];
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
                                resultadoregistro.Rows.Add(row);
                            }
                        }
                        Disconnect(cone); // se desonecta el sistema
                        return connSAP.servicioSAP_CargaMarcasDeReloj(resultadoregistro);
                    }
                    else  // sino envie error
                    {
                        Disconnect(cone);
                        return -1;
                    }
                }
                #endregion
                else
                {
                    #region zkemkeeper
                    if ((reg_RelojSAP["ESTADO"].ToString() == "X") &&
                            (axCZKEM1.Connect_Net(ip, Convert.ToInt32(port))))
                    {
                        if (axCZKEM1.SetDeviceTime(dwMachineNumber))
                        {
                            //HABILITA EL MODO LECTURA/ESCRITURA EN EL RELOJ
                            axCZKEM1.EnableDevice(dwMachineNumber, false);
                            //LEER TODAS LAS MARCAS EN EL RELOJ
                            if (axCZKEM1.ReadGeneralLogData(dwMachineNumber))
                            {

                                DataRow fila = resultadoregistro.NewRow();
                                string tipoProceso = reg_RelojSAP["INFO"].ToString().Contains("U580") ||
                                                         reg_RelojSAP["INFO"].ToString().Contains("UF11")
                                                            ? "basico" : "estandar";
                                switch (tipoProceso)
                                {
                                    case "basico":
                                        while (axCZKEM1.GetGeneralExtLogData(dwMachineNumber, ref dwEnrollNumber2, ref dwVerifyMode, ref dwInOutMode,
                                            ref dwYear, ref dwMonth, ref dwDay, ref dwHour, ref dwMinute, ref dwSecond, ref dwWorkCode, ref dwReserved))
                                        {
                                            fila = resultadoregistro.NewRow();
                                            //parser de la fecha
                                            if (dwMonth <= 9) mes = "0" + dwMonth; else mes = dwMonth.ToString();
                                            if (dwDay <= 9) dia = "0" + dwDay; else dia = dwDay.ToString();
                                            if (dwYear <= 9) ano = "0" + dwYear; else ano = dwYear.ToString();
                                            if (dwHour <= 9) hora = "0" + dwHour; else hora = dwHour.ToString();
                                            if (dwMinute <= 9) minuto = "0" + dwMinute; else minuto = dwMinute.ToString();
                                            if (dwSecond <= 9) segundo = "0" + dwSecond; else segundo = dwSecond.ToString();
                                            //Cast de cada valor en su campo
                                            fila["FECHA"] = ano + "-" + mes + "-" + dia;
                                            fila["HORA"] = hora + ":" + minuto + ":" + segundo;
                                            fila["IP"] = reg_RelojSAP["IP"].ToString();
                                            fila["PERNR"] = dwEnrollNumber2.ToString();
                                            fila["MODO"] = dwInOutMode.ToString();
                                            fila["TIPO"] = dwVerifyMode.ToString();
                                            //Agrega el reg para el envio
                                            resultadoregistro.Rows.Add(fila);
                                        }
                                        break;

                                    case "estandar":
                                        while (axCZKEM1.SSR_GetGeneralLogData(dwMachineNumber, out dwEnrollNumberAux, out dwVerifyMode,
                                            out dwInOutMode, out dwYear, out dwMonth, out dwDay, out dwHour, out dwMinute, out dwSecond, ref dwWorkCode))
                                        {
                                            fila = resultadoregistro.NewRow();
                                            //parser de la fecha
                                            if (dwMonth <= 9) mes = "0" + dwMonth; else mes = dwMonth.ToString();
                                            if (dwDay <= 9) dia = "0" + dwDay; else dia = dwDay.ToString();
                                            if (dwYear <= 9) ano = "0" + dwYear; else ano = dwYear.ToString();
                                            if (dwHour <= 9) hora = "0" + dwHour; else hora = dwHour.ToString();
                                            if (dwMinute <= 9) minuto = "0" + dwMinute; else minuto = dwMinute.ToString();
                                            if (dwSecond <= 9) segundo = "0" + dwSecond; else segundo = dwSecond.ToString();
                                            //Cast de cada valor en su campo
                                            fila["FECHA"] = ano + "-" + mes + "-" + dia;
                                            fila["HORA"] = hora + ":" + minuto + ":" + segundo;
                                            fila["IP"] = reg_RelojSAP["IP"].ToString();
                                            fila["PERNR"] = dwEnrollNumberAux.ToString();
                                            fila["MODO"] = dwInOutMode.ToString();
                                            fila["TIPO"] = dwVerifyMode.ToString();
                                            //Agrega el reg para el envio
                                            resultadoregistro.Rows.Add(fila);
                                        }
                                        break;
                                }

                                //DESHABILITA EL MODO LECTURA/ESCRITURA EN EL RELOJ
                                axCZKEM1.EnableDevice(dwMachineNumber, true);
                                axCZKEM1.Disconnect();
                                //ENVIO SAP
                                if (resultadoregistro.Rows.Count > 0)
                                {
                                    Console.WriteLine("Inicia envio informacion marcas a SAP.");
                                    return connSAP.servicioSAP_CargaMarcasDeReloj(resultadoregistro);
                                }
                            }
                            else
                            {
                                if (axCZKEM1.ReadAllGLogData(dwMachineNumber))
                                {
                                    Console.WriteLine("Entra lectura alterna.");
                                }
                                //int idwErrorCode = 0;
                                //axCZKEM1.GetLastError(ref idwErrorCode);
                                //DESHABILITA EL MODO LECTURA/ESCRITURA EN EL RELOJ
                                axCZKEM1.EnableDevice(dwMachineNumber, true);
                                axCZKEM1.Disconnect();
                                return 1;
                            }
                        }
                    }
                    #endregion
                }
            }
            //salida Error

            //DESHABILITA EL MODO LECTURA/ESCRITURA EN EL RELOJ
            axCZKEM1.EnableDevice(dwMachineNumber, true);
            axCZKEM1.Disconnect();
            Console.WriteLine("No existe la IP indicada en SAP.");
            return -1;
        }

        private List<string> normaliza_buffer(byte[] buffer)
        {
            string datos = Encoding.Default.GetString(buffer);
            datos = datos.Replace("\r\n", ",");
            List<string> datos2 = datos.Split(',').ToList();
            datos2.RemoveAt(datos2.Count - 1);
            return datos2;
        }
        */
    }
}
