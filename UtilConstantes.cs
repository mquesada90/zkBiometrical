using System;
using System.Data;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Web.Script.Serialization;
using System.Reflection;

namespace rzkclock_ws
{
    public class UtilConstantes
    {
        //SISTEMA
        private static String INST_TEST     = "";
        private static String TI_LISTA_IP   = "";
        private static String TI_URL_ORI    = "";
        private string curDir = @"C:\ZKTeco\Biometrics\Log";

        private static String WS_DEV_ZK = "";
        private static String WS_QAS_ZK = "";
        private static String WS_PRD_ZK = "";

        private static String WS_USR_DEV = "scsleandro";
        private static String WS_PSS_DEV = "FloridaS2016";
        private static String WS_USR_QA = "";
        private static String WS_PSS_QA = "";
        private static String WS_USR_PRD = "";
        private static String WS_PSS_PRD = "";

        //DATOS PREDEFINIDOS EN BIOMETRICOS
        public static String infoBIO_IP = "192.168.1.201";
        public static String infoBIO_PUERTO = "4370";

        //OP ZKBIOMETRICOS
        //static ArrayList<String> elements = new ArrayList<String>();
        //ArrayList listOfObjects = new ArrayList();
        public enum sys_opServicios
        {
            GET_TEMPLATE,
            ESTADO, CARGA_MAS,
            //Servicios sap
            PING,       ENV_USU,
            ENV_HUE,    OBT_HUE,
            OBT_MARC,   BOR_USU,
            BOR_MARC,   LIST_USU   
        };
        public enum sys_ambientes
        {
            DEV, QA, PRD
        };


        //CONEXIONES PREDEFINIDAS
        //DEV
        public static String DEV_SAP_ID = "01";
        public static String DEV_SAP_LENGUAJE = "ES";
        public static String DEV_SAP_SYSTEM = "DEH";
        public static String DEV_SAP_CLIENTE = "200";
        public static String DEV_SAP_SYSNUMBER = "11";
        public static String DEV_SAP_IP = "172.23.5.11";
        public static String DEV_SAPws_user = "scsleandro";
        public static String DEV_SAPws_pass = "FloridaS2016";
        public static String DEV_SapUrl_respaldo = "";
        //QAS
        public static String QAS_SAP_ID = "02";
        public static String QAS_SAP_LENGUAJE = "ES";
        public static String QAS_SAP_SYSTEM = "QAS";
        public static String QAS_SAP_CLIENTE = "700";
        public static String QAS_SAP_SYSNUMBER = "20";
        public static String QAS_SAP_IP = "192.168.0.22";
        public static String QAS_SAPws_user = "SLEANDRO";
        public static String QAS_SAPws_pass = "";
        public static String QAS_SapUrl_respaldo = "";
        //PRD
        public static String PRD_SAP_ID = "03";
        public static String PRD_SAP_LENGUAJE = "ES";
        public static String PRD_SAP_SYSTEM = "GEP";
        public static String PRD_SAP_CLIENTE = "900";
        public static String PRD_SAP_SYSNUMBER = "20";
        public static String PRD_SAP_IP = "192.168.0.23";
        public static String PRD_SAPws_user = "zinterfase";
        public static String PRD_SAPws_pass = "";
        public static String PRD_SapUrl_respaldo = "";

        public void generarBitacoraZK(String tipo, String msg)
        {
            String logName = tipo == "ERROR" ? "ErrorLog.txt" : "LogInfo.txt";
            String pathTXT = curDir + @"\" + logName; ;
            String fecha_hora = getDateTime();
            msg = fecha_hora + " - " + msg;

            if (System.IO.File.Exists(pathTXT))
            {
                System.IO.StreamWriter file = new System.IO.StreamWriter(pathTXT, true);
                file.WriteLine(msg);
                file.Close();
            }
            else
            {
                System.IO.Directory.CreateDirectory(pathTXT);
                //pathTXT = pathTXT + @"\" + logName;
                using (System.IO.FileStream fs = System.IO.File.Create(pathTXT))
                {
                    fs.Close();
                    System.IO.StreamWriter file = new System.IO.StreamWriter(pathTXT, true);
                    file.WriteLine(msg);
                    file.Close();
                }
            }
        }

        private string getDateTime(){
          
          String msgValor = "";
            DateTime serverTime = DateTime.Now;
            //Time
            String hou = serverTime.Hour.ToString();
            hou = hou.Length == 1 ? "0" + hou : hou;
            String min = serverTime.Minute.ToString();
            min = min.Length == 1 ? "0" + min : min;
            String sec = serverTime.Second.ToString();
            sec = sec.Length == 1 ? "0" + sec : sec;
            //Date
            String year = serverTime.Year.ToString();
            String mont = serverTime.Month.ToString();
            mont = mont.Length == 1 ? "0" + mont : mont;
            String day = serverTime.Day.ToString();
            
            msgValor = "[" + day + "/" + mont + "/" + year + " " + hou + ":" + min + ":" + sec + "]";
            
          return msgValor;
        }
        private bool validIPFormat(string ipString)
        {
            if (ipString == null || ipString.Length == 0)
            {
                return false;
            }
            string[] splitValues = ipString.Split('.');
            if (splitValues.Length != 4)
            {
                return false;
            }
            return true;
        }
        public object dataTableToJSON(DataTable table)
        {
            var list = new List<Dictionary<string, object>>();

            foreach (DataRow row in table.Rows)
            {
                var dict = new Dictionary<string, object>();

                foreach (DataColumn col in table.Columns)
                {
                    dict[col.ColumnName] = (Convert.ToString(row[col]));
                }
                list.Add(dict);
            }
            JavaScriptSerializer serializer = new JavaScriptSerializer();

            return serializer.Serialize(list);
        }
        public List<string> normaliza_buffer(byte[] buffer)
        {
            string datos = Encoding.Default.GetString(buffer);
            datos = datos.Replace("\r\n", ",");
            List<string> datos2 = datos.Split(',').ToList();
            datos2.RemoveAt(datos2.Count - 1);
            return datos2;
        }
        
        public DataRow getUserInfo(DataRow fila, string dwEnrollNumber, int dwFingerIndex, string TmpData)
        {
            //Cast de cada valor en su campo
            fila["PERNR"] = dwEnrollNumber.ToString();

            /* Codigo en SAP para indentificar los dedos
            "0"  = "D1"
            "1"  = "D2"
            "2"  = "D3"
            "3"  = "D4"
            "4"  = "D5"
            "5"  = "I1"
            "6"  = "I2"
            "7"  = "I3
            "8"  = "I4"
            "9"  = "I5"*/

            if (dwFingerIndex > 0 && dwFingerIndex < 5)
            {
                fila["DEDO"] = "D" + (dwFingerIndex + 1).ToString();
            }
            else if (dwFingerIndex >= 5)
            {
                fila["DEDO"] = "I" + (dwFingerIndex - 4).ToString();
            }
            //Si hay huella se pasa X
            fila["HUELLA"] = TmpData.Length > 0 ? "X" : ""; 
            return fila;
        }
       
    }
}

