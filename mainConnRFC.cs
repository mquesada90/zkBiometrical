using System;
using System.Data;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Net;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using System.Collections;
 
namespace rzkclock_ws
{
    public class mainConnRFC
    {
        /* Variables globales */
        private UtilConstantes.sys_opServicios idServActual;

        //private List<String> data = new List<String>();
        private String msg_ex_error = "";
        private String ambiente_ZK = "";
        private Boolean estadoServicio = false;

        //Variables conexions WS
        private Boolean estadoConn = false;
        private String puertoReloj = UtilConstantes.infoBIO_PUERTO;
        private System.Net.NetworkCredential Crendentials;
        private ff.dev.sap.rfc.ZK_WEBSERVICES_RELOJ_SN ins_ws_ff_dev;   //FF-DEV
        //private ff.qa.sap.rfc.ZK_WEBSERVICES_RELOJ_SN ins_ws_ff_qa;   //RFC QAS
        //private ff.prd.sap.rfc.ZK_WEBSERVICES_RELOJ_SN ins_ws_ff_prd; //RFC PRD

        // Funcion carga parametros inciales
        public void ini_mainConnRFC()
        {
            ambiente_ZK = Properties.Settings.Default.ambiente_ZK;


            String puertoReloj = UtilConstantes.infoBIO_PUERTO;
            String user_ZK = Properties.Settings.Default.user_SAP_WS;
            String pass_ZK = Properties.Settings.Default.pass_SAP_WS;
        }


        public void func_SapRfc(String opAmbiente)
        {
            //se renueva la instancia de conexion a SAP
            ins_ws_ff_dev = new rzkclock_ws.ff.dev.sap.rfc.ZK_WEBSERVICES_RELOJ_SN();
            String wsSAP_url = "", wsSAP_usuario = "", wsSAP_clave = "";

            switch (opAmbiente)
            {
                case "DEV":
                    estadoServicio = true;
                    wsSAP_url = Properties.Settings.Default.ambiente_ZK;
                    wsSAP_usuario = Properties.Settings.Default.user_SAP_WS.Length == 0 ? UtilConstantes.DEV_SAPws_user :
                                                                                    Properties.Settings.Default.user_SAP_WS;
                    wsSAP_clave = Properties.Settings.Default.pass_SAP_WS.Length == 0 ?  UtilConstantes.DEV_SAPws_pass :
                                                                                    Properties.Settings.Default.pass_SAP_WS;
                    break;
                case "QAS":
                    estadoServicio = true;
                    wsSAP_url = Properties.Settings.Default.SAP_WS_QAS_ZK;
                    wsSAP_usuario = Properties.Settings.Default.user_SAP_WS;
                    wsSAP_clave = Properties.Settings.Default.pass_SAP_WS;
                    break;
                case "PRD":
                    estadoServicio = true;
                    wsSAP_url = Properties.Settings.Default.SAP_WS_PRD_ZK;
                    wsSAP_url = wsSAP_url.Length == 0 ? UtilConstantes.PRD_SapUrl_respaldo : wsSAP_url;
                    wsSAP_usuario = Properties.Settings.Default.user_SAP_WS;
                    wsSAP_clave = Properties.Settings.Default.pass_SAP_WS;
                    break;
            }

            if (estadoServicio)
            {
                try
                {
                    Crendentials = new System.Net.NetworkCredential(wsSAP_usuario, wsSAP_clave);
                    ins_ws_ff_dev.Url = wsSAP_url;
                    ins_ws_ff_dev.Credentials = Crendentials;
                }
                catch (Exception E)
                {
                    estadoServicio = false;
                    Console.WriteLine(E.ToString());
                }
            }
        }

        // Metodo para buscar informacion de un reloj
        public DataTable obtenerItemRelojSAP(UtilConstantes.sys_ambientes id_ambiente, string ip)
        {
            if (estadoServicio)
            {
                DataTable Lista_Relojes_Final = new DataTable();
                Lista_Relojes_Final.Columns.Add("BDEGR");
                Lista_Relojes_Final.Columns.Add("ESTADO");
                Lista_Relojes_Final.Columns.Add("INFO");
                Lista_Relojes_Final.Columns.Add("IP");
                Lista_Relojes_Final.Columns.Add("LAND");
                Lista_Relojes_Final.Columns.Add("MANDT");
                DataRow fila = Lista_Relojes_Final.NewRow();

                switch (id_ambiente)
                {
                    case UtilConstantes.sys_ambientes.DEV:
                        ff.dev.sap.rfc.ZktReloj[] L_Reloj = new ff.dev.sap.rfc.ZktReloj[1];
                        ff.dev.sap.rfc.ZkListaRelojes ListaRelojes = new ff.dev.sap.rfc.ZkListaRelojes();
                        ff.dev.sap.rfc.ZkListaRelojesResponse ListaRelojesResponse = new ff.dev.sap.rfc.ZkListaRelojesResponse();

                        L_Reloj[0] = new ff.dev.sap.rfc.ZktReloj();
                        ListaRelojes.Relojes = L_Reloj;
                        //Ejecuta consulta en SAP
                        ListaRelojesResponse = ins_ws_ff_dev.ZkListaRelojes(ListaRelojes);
                        L_Reloj = ListaRelojesResponse.Relojes;
                        for (int i = 0; i < L_Reloj.Length; i++)
                        {
                            if (ip == "ALL" || ip == L_Reloj[i].Ip)
                            {
                                fila["BDEGR"] = L_Reloj[i].Bdegr;
                                fila["ESTADO"] = L_Reloj[i].Estado;
                                fila["INFO"] = L_Reloj[i].Info;
                                fila["IP"] = L_Reloj[i].Ip;
                                fila["LAND"] = L_Reloj[i].Land;
                                fila["MANDT"] = L_Reloj[i].Mandt;
                                Lista_Relojes_Final.Rows.Add(fila);
                            }
                        }
                        break;

                    case UtilConstantes.sys_ambientes.QA:

                        break;
                    case UtilConstantes.sys_ambientes.PRD:

                        break;
                }

                //Respuesta total de relojes
                return Lista_Relojes_Final;
            }
            else
            {
                Console.WriteLine("Error en obtencion datos Reloj de SAP.");
                return null;
            }
        }

        // Metodo para buscar informacion de un empleado
        public DataTable obtenerItemEmpleadoSAP(string cod_Empleado, string ip_GrupoReloj)
        {
            if (estadoServicio)
            {
                ff.dev.sap.rfc.ZksStatusEmple[] L_Empleados = new ff.dev.sap.rfc.ZksStatusEmple[1];
                ff.dev.sap.rfc.ZkDistrEmpleados Empleados = new ff.dev.sap.rfc.ZkDistrEmpleados();
                ff.dev.sap.rfc.ZkDistrEmpleadosResponse EmpleadosResponse;

                L_Empleados[0] = new rzkclock_ws.ff.dev.sap.rfc.ZksStatusEmple();
                Empleados.Info = L_Empleados;
                Empleados.Empleado = cod_Empleado;
                Empleados.Reloj = ip_GrupoReloj;
                //Ejecuta consulta en SAP
                EmpleadosResponse = ins_ws_ff_dev.ZkDistrEmpleados(Empleados);
                L_Empleados = EmpleadosResponse.Info;

                DataTable ListaEmpleadoResponse = new DataTable();
                ListaEmpleadoResponse.Columns.Add("BDEGR");
                ListaEmpleadoResponse.Columns.Add("IP");
                ListaEmpleadoResponse.Columns.Add("PERNR");
                ListaEmpleadoResponse.Columns.Add("ENABLE");
                ListaEmpleadoResponse.Columns.Add("CNAME");

                DataRow fila = ListaEmpleadoResponse.NewRow();
                for (int i = 1; i < L_Empleados.Length; i++)
                {
                    fila["BDEGR"] = L_Empleados[i].Bdegr.ToString();
                    fila["IP"] = L_Empleados[i].Ip.ToString();
                    fila["PERNR"] = L_Empleados[i].Pernr.ToString().TrimStart('0');
                    fila["ENABLE"] = L_Empleados[i].Enable.ToString();
                    fila["CNAME"] = L_Empleados[i].Cname.ToString();
                    ListaEmpleadoResponse.Rows.Add(fila);
                }
                //Respuesta total de usuarios
                return ListaEmpleadoResponse;
            }
            else
            {
                Console.WriteLine("Error en obtencion datos Empleados de SAP.");
                return null;
            }
        }

        // Metodo para buscar informacion de una huella
        public DataTable obtenerItemHuellasEmpleadoSAP(string cod_Empleado, string ip_GrupoReloj)
        {
            if (estadoServicio)
            {
                ff.dev.sap.rfc.ZksHuellas[] L_Huellas = new rzkclock_ws.ff.dev.sap.rfc.ZksHuellas[1];
                ff.dev.sap.rfc.ZkEnviarHuellas Huellas = new rzkclock_ws.ff.dev.sap.rfc.ZkEnviarHuellas();
                ff.dev.sap.rfc.ZkEnviarHuellasResponse HuellasResponse;

                Huellas.Empleado= cod_Empleado;
                Huellas.Reloj  = ip_GrupoReloj;
                Huellas.Huellas = L_Huellas;
                //Ejecuta consulta en SAP
                HuellasResponse = ins_ws_ff_dev.ZkEnviarHuellas(Huellas);
                L_Huellas = HuellasResponse.Huellas;

                DataTable ListaHuellasEmpleadoResponse = new DataTable();
                ListaHuellasEmpleadoResponse.Columns.Add("DEDO");
                ListaHuellasEmpleadoResponse.Columns.Add("HUELLA");
                ListaHuellasEmpleadoResponse.Columns.Add("PERNR");
                ListaHuellasEmpleadoResponse.Columns.Add("IP");
                ListaHuellasEmpleadoResponse.Columns.Add("FINDEX");

                for (int i = 0; i < L_Huellas.Length; i++)
                {
                    DataRow fila = ListaHuellasEmpleadoResponse.NewRow();
                    fila["DEDO"]  = L_Huellas[i].Dedo;
                    fila["HUELLA"]= L_Huellas[i].Huella;
                    fila["PERNR"] = L_Huellas[i].Pernr.TrimStart('0');
                    fila["IP"]    = HuellasResponse.Ip;

                    switch (fila["DEDO"].ToString())
                    {
                        case "": case "D1": fila["FINDEX"] = 0; break;
                        case "D2": fila["FINDEX"] = 1; break;
                        case "D3": fila["FINDEX"] = 2; break;
                        case "D4": fila["FINDEX"] = 3; break;
                        case "D5": fila["FINDEX"] = 4; break;
                        case "I1": fila["FINDEX"] = 5; break;
                        case "I2": fila["FINDEX"] = 6; break;
                        case "I3": fila["FINDEX"] = 7; break;
                        case "I4": fila["FINDEX"] = 8; break;
                        case "I5": fila["FINDEX"] = 9; break;
                    }
                    //Add huela por enviar
                    ListaHuellasEmpleadoResponse.Rows.Add(fila);
                }
                //Respuesta total de huellas
                return ListaHuellasEmpleadoResponse;
            }
            else
            {
                Console.WriteLine("Error en obtencion datos Huellas de SAP.");
                return null;
            }
        }


        // Metodos intermedios para envio de informacion de MARCAS_RELOJ para SAP, TABLA=MARCAS
        public int servicioSAP_CargaMarcasDeReloj(DataTable ZKS_MARCAS)
        {
            if (estadoServicio)
            {
                ff.dev.sap.rfc.ZkMarcasEmpleados MarcasEmpleados = new rzkclock_ws.ff.dev.sap.rfc.ZkMarcasEmpleados();
                ff.dev.sap.rfc.ZkMarcasEmpleadosResponse MarcasEmpleadosResponse = new rzkclock_ws.ff.dev.sap.rfc.ZkMarcasEmpleadosResponse();
                ff.dev.sap.rfc.ZksMarcas[] MarcarArreglo = new rzkclock_ws.ff.dev.sap.rfc.ZksMarcas[ZKS_MARCAS.Rows.Count];

                for (int i = 0; i < MarcarArreglo.Length; i++)
                {
                    DataRow MKZ = ZKS_MARCAS.Rows[i];
                    MarcarArreglo[i] = new rzkclock_ws.ff.dev.sap.rfc.ZksMarcas();
                    //Agrega cada dato por enviar
                    MarcarArreglo[i].Fecha = MKZ["FECHA"].ToString();
                    MarcarArreglo[i].Hora = MKZ["HORA"].ToString();
                    MarcarArreglo[i].Ip = MKZ["IP"].ToString();
                    MarcarArreglo[i].Pernr = MKZ["PERNR"].ToString();
                    MarcarArreglo[i].Modo = MKZ["MODO"].ToString();
                    MarcarArreglo[i].Tipo = MKZ["TIPO"].ToString();
                }
                //Hace la carga y envio final
                MarcasEmpleados.Marcas = MarcarArreglo;
                MarcasEmpleadosResponse = ins_ws_ff_dev.ZkMarcasEmpleados(MarcasEmpleados);
                return Convert.ToInt32(MarcasEmpleadosResponse.IdocNum);
            }
            else
            {
                Console.WriteLine("Error en configuracion de datos, durante envio datos MARCAS para SAP.");
            }
            return -1;
        }

        // Metodos intermedios para envio de informacion de HUELLAS_RELOJ para SAP, TABLA=HUELLAS
        public int servicioSAP_CargaHuellasDeReloj(DataTable ZKS_Huellas)
        {
            if (estadoServicio)
            {
                ff.dev.sap.rfc.ZkGuardarHuellas GHuellas = new rzkclock_ws.ff.dev.sap.rfc.ZkGuardarHuellas();
                ff.dev.sap.rfc.ZkGuardarHuellasResponse GHuellasResponse = new rzkclock_ws.ff.dev.sap.rfc.ZkGuardarHuellasResponse();
                ff.dev.sap.rfc.ZksHuellas[] ZKGHuellas = new rzkclock_ws.ff.dev.sap.rfc.ZksHuellas[ZKS_Huellas.Rows.Count];

                for (int i = 0; i < ZKGHuellas.Length; i++)
                {
                    DataRow ZKGH = ZKS_Huellas.Rows[i];
                    ZKGHuellas[i] = new rzkclock_ws.ff.dev.sap.rfc.ZksHuellas();
                    ZKGHuellas[i].Pernr = ZKGH["PERNR"].ToString();
                    ZKGHuellas[i].Huella = ZKGH["HUELLA"].ToString();
                    //Agrega cada dato por enviar
                    if (ZKGH["DEDO"].ToString() == "0") { ZKGHuellas[i].Dedo = "D1";  }
                    if (ZKGH["DEDO"].ToString() == "1") { ZKGHuellas[i].Dedo = "D2";  }
                    if (ZKGH["DEDO"].ToString() == "2") { ZKGHuellas[i].Dedo = "D3";  }
                    if (ZKGH["DEDO"].ToString() == "3") { ZKGHuellas[i].Dedo = "D4";  }
                    if (ZKGH["DEDO"].ToString() == "4") { ZKGHuellas[i].Dedo = "D5";  }
                    if (ZKGH["DEDO"].ToString() == "5") { ZKGHuellas[i].Dedo = "I1";  }
                    if (ZKGH["DEDO"].ToString() == "6") { ZKGHuellas[i].Dedo = "I2";  }
                    if (ZKGH["DEDO"].ToString() == "7") { ZKGHuellas[i].Dedo = "I3";  }
                    if (ZKGH["DEDO"].ToString() == "8") { ZKGHuellas[i].Dedo = "I4";  }
                    if (ZKGH["DEDO"].ToString() == "9") { ZKGHuellas[i].Dedo = "I5";  }
                }
                //Hace la carga y envio final
                GHuellas.Huellas = ZKGHuellas;
                GHuellasResponse = ins_ws_ff_dev.ZkGuardarHuellas(GHuellas);
                return 1;
            }
            else
            {
                Console.WriteLine("Error en configuracion de datos, durante envio datos HUELLAS para SAP.");
            }
            return -1;
        }



    }
}

