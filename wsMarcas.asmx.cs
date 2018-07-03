using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Xml.Linq;
using System.Collections.Generic;

//Mejora, LOG DE ACCIONES
namespace rzkclock_ws
{
    /// <summary>
    /// Summary description for wsMarcasHuellas
    /// Servicios de comunicacion y logica para desarrollo con relojes de Marcas de ZK
    /// </summary>
    [WebService(Namespace = "http://charlatan")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class wsMarcasHuellas : System.Web.Services.WebService
    {
        // Genera el ciclo de intentos para los servicios de comunicacion con los ZK 
        private string generarCicloProceso(UtilConstantes.sys_opServicios idServicio, String ip, String puerto, String tipo, String usuData)
        {
            String msgRetorno = "";
            Boolean estadoServ = false;
            metZKHuellas insMetodos = new metZKHuellas();
            switch (idServicio)
            {
                case UtilConstantes.sys_opServicios.GET_TEMPLATE:
                    msgRetorno = insMetodos.control_servicios(UtilConstantes.sys_opServicios.GET_TEMPLATE, ip, puerto, tipo, usuData);
                    if (!msgRetorno.Contains("<INCIDENTE>"))
                        estadoServ = true;
                    break;
                case UtilConstantes.sys_opServicios.PING:
                    msgRetorno = insMetodos.control_servicios(UtilConstantes.sys_opServicios.PING, ip, puerto, tipo, usuData);
                    if (!msgRetorno.Contains("<INCIDENTE>"))
                        estadoServ = true;
                    break;
                case UtilConstantes.sys_opServicios.LIST_USU:
                    msgRetorno = insMetodos.control_servicios(UtilConstantes.sys_opServicios.LIST_USU, ip, puerto, tipo, usuData);
                    if (!msgRetorno.Contains("<INCIDENTE>"))
                    {
                        estadoServ = true;
                    }
                    break;
                case UtilConstantes.sys_opServicios.OBT_MARC:
                    msgRetorno = insMetodos.control_servicios(UtilConstantes.sys_opServicios.OBT_MARC, ip, puerto, tipo, usuData);
                    if (msgRetorno.Contains("<INCIDENTE>"))
                    {
                        estadoServ = true;
                    }
                    break;
                case UtilConstantes.sys_opServicios.ENV_USU:
                    usuData = usuData.Length > 0 ? usuData : usuData.TrimStart(new Char[] { '0' }); //Condicion_SAP
                    msgRetorno = insMetodos.control_servicios(UtilConstantes.sys_opServicios.ENV_USU, ip, puerto, tipo, usuData);
                    if (msgRetorno.Contains("<INCIDENTE>"))
                    { 
                        estadoServ = true;
                    }
                    break;
                case UtilConstantes.sys_opServicios.BOR_USU:
                    break;

                case UtilConstantes.sys_opServicios.ENV_HUE:
                    usuData = usuData.Length > 0 ? usuData : usuData.TrimStart(new Char[] { '0' }); //Condicion_SAP
                    msgRetorno = insMetodos.control_servicios(UtilConstantes.sys_opServicios.ENV_HUE, ip, puerto, tipo, usuData);
                    if (msgRetorno.Contains("<INCIDENTE>"))
                    {
                        estadoServ = true;
                    }
                    break;
                case UtilConstantes.sys_opServicios.OBT_HUE:
                    //usuData = usuData.TrimStart(new Char[] { '0' }); //Condicion_SAP
                    msgRetorno = insMetodos.control_servicios(UtilConstantes.sys_opServicios.OBT_HUE, ip, puerto, tipo, usuData);
                    if (msgRetorno.Contains("<ERROR>"))
                    {
                        estadoServ = true;
                    }
                    break;

                case UtilConstantes.sys_opServicios.ESTADO:
                    break;
                case UtilConstantes.sys_opServicios.CARGA_MAS:
                    break;
            }
            
            if (estadoServ)
                return msgRetorno; 
         
        return msgRetorno;
        }

        // Retorna en pantalla la configuracion actual del sistema
        /*[WebMethod]
        public string WS_NEW_TOKEN(UtilConstantes.sys_opServicios idServicio, String inParametros)
        {
            switch (idServicio)
            {
                case UtilConstantes.sys_opServicios.PING:
                    break;
                default: return "Revisar parametros entrada, ninguna accion ejecutada.";
            }
            return null;
        }*/
        // Retorna en pantalla la configuracion actual del sistema
        [WebMethod]
        public string Ejecutar_Serv_Auto(string p_ip, string p_puerto, string p_tipo)
        {
            String msg = "";

            msg = sUSB_ZFinger_id_X();
            string[] data = msg.Split(new[] { "FID = " }, StringSplitOptions.None);

            if (data.Length == 2)
            {
                msg = sBio_SetHuella_id5(p_ip, p_puerto, p_tipo, data[1] );
            }

            return msg;
        }

        /** --------------------------------------------------------------------------   **/
        /** --------------------------------------------------------------------------   **/
        /** --------------------------------------------------------------------------   **/
        //Realiza un conectar y generar nueva huella con dedos entrantes en ZK4500
        [WebMethod]
        public String sUSB_ZFinger_id_X()
        {
            String msgObtZK = "";
            msgObtZK = generarCicloProceso(UtilConstantes.sys_opServicios.GET_TEMPLATE, null, null, null, null);
            if (!msgObtZK.Contains("<ERROR>"))
            {

            }
            return msgObtZK;
        }
        //Realiza un ping a la IP y actualiza la hora del dispositivo
        [WebMethod]
        public String sBio_CPing_id1(string p_ip, string p_puerto, string p_tipo)
        {
            String msgConnZK = "";
            if (p_ip == null || p_ip.Length == 0)
                msgConnZK = "<INCIDENTE> Error en proceso, debe indicar una IP para ejecutar el servicio indicado.";
            else{
                msgConnZK = generarCicloProceso(UtilConstantes.sys_opServicios.PING, p_ip, p_puerto, p_tipo, null);
                if (msgConnZK.Contains("<INCIDENTE>"))
                {
                }
            }
            return msgConnZK;
        }
        //Realiza una conexion a la IP y trae la lista de usuarios/huellas
        [WebMethod]
        public String sBio_GetUserL_id2(string p_ip, string p_puerto, string p_tipo, string dwEnrollNumber)
        {
            String msgLUsuZK = "";
            if (p_ip == null || p_ip.Length == 0)
                msgLUsuZK = "<INCIDENTE> Error en proceso, debe indicar una IP para ejecutar el servicio indicado.";
            else
            {
                msgLUsuZK = generarCicloProceso(UtilConstantes.sys_opServicios.LIST_USU, p_ip, p_puerto, p_tipo, dwEnrollNumber);
                if (!msgLUsuZK.Contains("<ERROR>"))
                {
                }
            }
            return msgLUsuZK;
        }
        //Realiza una conexion a la IP y trae las marcas existentes
        [WebMethod]
        public String sBio_GetLog_id3(string p_ip, string p_puerto, string p_tipo, string dwEnrollNumber)
        {
            String msgObtZK = "";
            if (p_ip == null || p_ip.Length == 0)
                msgObtZK = "<INCIDENTE> Error en proceso, debe indicar una IP para ejecutar el servicio indicado.";
            else
            {
                msgObtZK = generarCicloProceso(UtilConstantes.sys_opServicios.OBT_MARC, p_ip, p_puerto, p_tipo, null);
                if (!msgObtZK.Contains("<ERROR>"))
                {
                }
            }
            return msgObtZK;
        }
        //Realiza una conexion a la IP y genera en el Biometrico un registro de nuevo usuario
        [WebMethod]
        public String sBio_SetUsuario_id4(string p_ip, string p_puerto, string p_tipo, string dwEnrollNumber)
        {
            String msgSetUsu = "";
            if (p_ip == null || p_ip.Length == 0)
                msgSetUsu = "<INC IDENTE> Error en proceso, debe indicar una IP para ejecutar el servicio indicado.";
            else
            {
                msgSetUsu = generarCicloProceso(UtilConstantes.sys_opServicios.ENV_USU, p_ip, p_puerto, p_tipo, dwEnrollNumber);
                if (!msgSetUsu.Contains("<ERROR>"))
                {
                }
            }
            return msgSetUsu;
        }
        //Realiza una conexion a la IP y genera en el Biometrico un registro de nueva huella
        [WebMethod]
        public String sBio_SetHuella_id5(string p_ip, string p_puerto, string p_tipo, string dwEnrollNumber)
        {
            String msgSetHue = "";
            if (p_ip == null || p_ip.Length == 0)
                msgSetHue = "<INCIDENTE> Error en proceso, debe indicar una IP para ejecutar el servicio indicado.";
            else
            {
                msgSetHue = generarCicloProceso(UtilConstantes.sys_opServicios.ENV_HUE, p_ip, p_puerto, p_tipo, dwEnrollNumber);
                if (!msgSetHue.Contains("<ERROR>"))
                {
                }
            }
            return msgSetHue;
        }








        // Retorna en pantalla la configuracion actual del sistema
        [WebMethod]
        public string AInformacion_Config()
        {
            String ambiente_ZK = Properties.Settings.Default.SAP_WS_DEV_ZK;
            String user_ZK = Properties.Settings.Default.user_SAP_WS;
            String pass_ZK = Properties.Settings.Default.pass_SAP_WS;
            String url_SAP_wS = "";
            switch (ambiente_ZK)
            {
                case "DEV":
                    url_SAP_wS = Properties.Settings.Default.ambiente_ZK;
                    break;
                case "QAS":
                    url_SAP_wS = Properties.Settings.Default.SAP_WS_QAS_ZK;
                    break;
                case "PRD":
                    url_SAP_wS = Properties.Settings.Default.SAP_WS_PRD_ZK;
                    break;
            }
            //Datos devueltos
            String response = "Ambiente: " + ambiente_ZK + ". {Usuario SAP: " + user_ZK + " / Clave SAP: " + pass_ZK + "}";
            response += " URL_SERVICIO_SAP_REFERENCIADO: " + url_SAP_wS;
            return response;
        }
        //Actualiza parametros para cambiar entre ambientes 
        [WebMethod]
        public string parametrizacion_AMBIENTE(string tipoAmbiente)
        {
            String response = "Actualizacion del ambiente ";
            if (tipoAmbiente != null && tipoAmbiente.Length > 0)
            {
                UtilParametrizacion metActualizacion = new UtilParametrizacion();

                if (metActualizacion.cambiarAmbiente_wsZK(tipoAmbiente) == 1)
                    response += tipoAmbiente + ", se completo de forma exitosa. Activo";
                else
                    response += tipoAmbiente + ", no se logro completar. Inactivo";
            }
            else
            {
                response = "False, no se pudo actualizar el ambiente.";
            }
            return response;
        }
        //Actualiza parametros para conexion con SAP
        [WebMethod]
        public string parametrizacion_CREDENCIALES(string tipoAmbiente, string user, string pass)
        {
            String response = "Actualizacion de credenciales para ";
            if (tipoAmbiente != null && user != null && pass != null &&
                tipoAmbiente.Length > 0 && user.Length > 0 && pass.Length > 0)
            {
                UtilParametrizacion metActualizacion = new UtilParametrizacion();

                if (metActualizacion.cambiarCredencialesConexion_wsSAP(tipoAmbiente, user, pass) == 1)
                    response += tipoAmbiente + ", se completo de forma exitosa.";
                else
                    response += tipoAmbiente + ", no se logro completar.";
            }
            else
            {
                response = "False, no se pudo actualizar las credenciales del usuario.";
            }
            return response;
        }
        //Actualiza parametros para RFC WS SAP 
        [WebMethod]
        public string parametrizacion_RFC(string tipoAmbiente, string nuevo_SAPURL)
        {
            String response = "Actualizacion de servicio SAP para ";
            if (tipoAmbiente != null && nuevo_SAPURL != null &&
                tipoAmbiente.Length > 0 && nuevo_SAPURL.Length > 0)
            {
                UtilParametrizacion metActualizacion = new UtilParametrizacion();
                if (metActualizacion.cambiarRFCSAP_wsZK(tipoAmbiente, nuevo_SAPURL) == 1)
                    response += tipoAmbiente + ", se completo de forma exitosa.";
                else
                    response += tipoAmbiente + ", no se logro completar.";
            }
            else
            {
                response = "False, no se pudo actualizar la URL del WS en SAP para ZK.";
            }
            return response;
        }
        

        /*
        //Realiza funcion botonera, carga Reloj con datos de USUARIOS provinientes de SAP
        [WebMethod]
        public bool sReloj_EnviarInformacion_id1(string ip, string dwEnrollNumber)
        {
            Boolean respSAP_WS = false;
            if (ip != null && ip.Length > 0 && dwEnrollNumber != null && dwEnrollNumber.Length > 0)
            {
                String msgEnviarUsuario = generarCicloProceso(UtilConstantes.sys_opServicios.ENV_USU, ip, dwEnrollNumber);
                if(msgEnviarUsuario.Contains("<ERROR>")){

                }else{
                    respSAP_WS = true;
                }

                met_MarcasHuellas insMet = new met_MarcasHuellas();
                String puertoReloj = UtilConstantes.infoBIO_PUERTO;
                Boolean estadoEnvio = false;
                for (int i = 0; i < cantIntentos; i++)
                {
                    try
                    {
                        dwEnrollNumber = dwEnrollNumber.TrimStart(new Char[] { '0' });
                        int respEnvio = insMet.EnviarEmpleado_SAPtoReloj(ip, puertoReloj, dwEnrollNumber);
                        if (respEnvio == 1) { estadoEnvio = true; break; }
                        System.Threading.Thread.Sleep(tiempoEntreIntentos);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Ex del dispo:" + ex.ToString());
                        System.Threading.Thread.Sleep(tiempoEntreIntentos);
                    }
                }


                return respSAP_WS;
            }
            //Salida ERROR
            else
                return false;
        }

        //Realiza funcion botonera, borra marcas de reloj
        [WebMethod]
        public bool sReloj_BorrarMarcas_id6(string ip)
        {
            if (ip != null && ip.Length > 0)
            {
                Boolean estadoBorrado = false;
                for (int i = 0; i < cantIntentos; i++)
                {
                    met_MarcasHuellas insMet = new met_MarcasHuellas();
                    String puertoReloj = UtilConstantes.infoBIO_PUERTO;
                    try
                    {
                        int estado = insMet.BorrarMarcasReloj(ip, puertoReloj, 1);
                        if (estado == 1) { estadoBorrado = true; break; }
                        System.Threading.Thread.Sleep(tiempoEntreIntentos);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Ex del dispo:" + ex.ToString());
                        System.Threading.Thread.Sleep(tiempoEntreIntentos);
                    }
                }
                return estadoBorrado;
            }
            else
                //Salida Error
                return false;
        }

        //Realiza funcion botonera, carga Reloj con datos de HUELLAS provinientes de SAP
        [WebMethod]
        public bool sReloj_EnviarHuella_id4(string ip, string dwEnrollNumber)
        {
            if (ip != null && ip.Length > 0 && dwEnrollNumber != null && dwEnrollNumber.Length > 0)
            {
                met_MarcasHuellas insMet = new met_MarcasHuellas();
                String puertoReloj = UtilConstantes.infoBIO_PUERTO;

                Boolean estadoEnvio = false;
                for (int i = 0; i < cantIntentos; i++)
                {
                    try
                    {
                        dwEnrollNumber = dwEnrollNumber.TrimStart(new Char[] { '0' });
                        int respEnvio = insMet.EnviarHuellaEmpleado_SAPtoReloj(ip, puertoReloj, dwEnrollNumber);
                        if (respEnvio == 1) { estadoEnvio = true; break; }
                        System.Threading.Thread.Sleep(tiempoEntreIntentos);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Ex del dispo:" + ex.ToString());
                        System.Threading.Thread.Sleep(tiempoEntreIntentos);
                    }
                }
                return estadoEnvio;
            }
            else
                return false; //salida error
        }

        //Realiza funcion botonera, extrae datos de huellas/empleados de un reloj cargando las mismas en SAP
        [WebMethod]
        public bool sReloj_ExtraerHuellas_DeReloj_id5(string ip, string dwEnrollNumber)
        {
            if (ip != null && ip.Length > 0)
            {
                met_MarcasHuellas insMet = new met_MarcasHuellas();
                String puertoReloj = UtilConstantes.infoBIO_PUERTO;
                if (dwEnrollNumber == "_") { dwEnrollNumber = ""; }

                Boolean estadoExtraer = false;
                for (int i = 0; i < cantIntentos; i++)
                {
                    try
                    {
                        dwEnrollNumber = dwEnrollNumber.TrimStart(new Char[] { '0' });
                        int respExtraer = insMet.Extraer_HuellasReloj(ip, puertoReloj, 1, dwEnrollNumber);
                        if (respExtraer == 1) { estadoExtraer = true; break; }
                        System.Threading.Thread.Sleep(tiempoEntreIntentos);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Ex del dispo:" + ex.ToString());
                        System.Threading.Thread.Sleep(tiempoEntreIntentos);
                    }
                }
                return estadoExtraer;
            }
            else
                return false; //salida error
        }

        //Realiza funcion botonera, extrae las marcas de un reloj cargando las mismas en SAP
        [WebMethod]
        public string sReloj_ExtraerMarcas_DeReloj_id2(string ip, string boli) //boli es DUMMY enviado por SAP
        {
            if (ip != null && ip.Length > 0)
            {
                met_MarcasHuellas insMet = new met_MarcasHuellas();
                String puertoReloj = UtilConstantes.infoBIO_PUERTO;

                for (int i = 0; i < cantIntentos; i++)
                {
                    try
                    {
                        int idocRetorno = insMet.Extraer_MarcasReloj(ip, puertoReloj, 1);
                        if (idocRetorno != -1 && idocRetorno != 0)
                        {
                            //Se genero un idoc correcto o 1=no hay marcas por leer
                            if (idocRetorno > 1)
                            {
                                //Se limpian las marcas de ese reloj
                                insMet.BorrarMarcasReloj(ip, puertoReloj, 1);
                                return "Idoc: " + idocRetorno.ToString();
                            }
                            else if (idocRetorno == 1)
                            {
                                Console.WriteLine("No hay marcas por extraer para intento:" + i);
                                System.Threading.Thread.Sleep(tiempoEntreIntentos);
                                break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Ex del dispo:" + ex.ToString());
                        System.Threading.Thread.Sleep(tiempoEntreIntentos);
                    }
                }
            }
            //Caso Error
            return "Error";
        }

        //Realiza funcion botonera, extrae los usuarios de un reloj 
        [WebMethod]
        public string sEmpleado_fromReloj_id7(string ip, string pernr)
        {
            if (ip != null && ip.Length > 0)
            {
                if (pernr == "-")
                {
                    pernr = "";
                }

                met_MarcasHuellas insMet = new met_MarcasHuellas();
                String puertoReloj = UtilConstantes.infoBIO_PUERTO;

                String listaConsulta = "";
                pernr = pernr.TrimStart(new Char[] { '0' });
                listaConsulta = insMet.getlistEmpleado_fromReloj(ip, puertoReloj, 1, pernr);
                if (listaConsulta.Length > 0)
                {
                    return listaConsulta;
                }
            }
            //Caso Error
            return "<MSG>Indique la ip del dispositivo.</MSG>";
        }

        //Realiza funcion botonera, borra usuarios especificos en reloj indicado
        [WebMethod]
        public bool sReloj_BorrarUsuarios_id8(string tipo, string ip, string dwEnrollNumber)
        {
            if (ip != null && ip.Length > 0)
            {
            Boolean estadoBorrado = false;
            dwEnrollNumber = dwEnrollNumber.TrimStart(new Char[] { '0' });
            int estado = insMet.BorrarUsuarioReloj(tipo, ip, puertoReloj, dwEnrollNumber);
            if (estado == 1) { estadoBorrado = true; break; }
            return estadoBorrado;
            }
            else
                //Salida Error
                return false;
        }

        [WebMethod]
        public string zSapPrueba_Ping_EX(string ip)
        {
            if (ip != null && ip.Length > 0)
            {
                String msgConn = "";
                for (int i = 0; i < cantIntentos; i++)
                {
                    met_MarcasHuellas insMet = new met_MarcasHuellas();
                    String puertoReloj = UtilConstantes.infoBIO_PUERTO;
                    try
                    {
                        int estado = insMet.SincronizacionReloj(ip, puertoReloj, 1);
                        if (estado == 1) { msgConn = "Proceso ejecutado exitosamente."; break; }
                        System.Threading.Thread.Sleep(tiempoEntreIntentos);
                        return msgConn;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Ex del servicio:" + ex.ToString());
                        System.Threading.Thread.Sleep(tiempoEntreIntentos);
                        return ex.ToString();
                    }
                }
                return msgConn;
            }
            else
                //Salida Error
                return "Error";
        }

        [WebMethod]
        public string zCargaMasiva_TXT(string ip, string tipo, string txtPath)
        {
            if ((ip != null && ip.Length > 0) && (txtPath != null && txtPath.Length > 0))
            {
                Boolean banderaError = false;
                String codUsu, msgConn = "", msgError = "";
                string infoUsuarios = System.IO.File.ReadAllText(@txtPath);
                string[] lporCargar = infoUsuarios.Split(',');

                int respEnvio = 0;
                String puertoReloj = UtilConstantes.infoBIO_PUERTO;
                met_MarcasHuellas insMet = new met_MarcasHuellas();

                foreach (string id in lporCargar)
                {

                    codUsu = id.Replace("\r\n", "").Replace("\r", "").Replace("\n", "");
                    for (int i = 0; i < cantIntentos; i++)
                    {
                        try
                        {
                            banderaError = false;
                            switch (tipo)
                            {
                                case "USUARIO":
                                    respEnvio = insMet.EnviarEmpleado_SAPtoReloj(ip, puertoReloj, codUsu);
                                    if (respEnvio == 1)
                                    {
                                        msgConn = msgConn + "Envio de usu: " + codUsu + "es " + "correcto. | ";
                                        i = 100;
                                        break;
                                    }
                                    banderaError = true;
                                    System.Threading.Thread.Sleep(tiempoEntreIntentos);
                                    break;

                                case "HUELLA":
                                    respEnvio = insMet.EnviarHuellaEmpleado_SAPtoReloj(ip, puertoReloj, codUsu);
                                    if (respEnvio == 1)
                                    {
                                        msgConn = msgConn + "Envio de hue: " + codUsu + "es " + "correcto. | ";
                                        i = 100;
                                        break;
                                    }
                                    banderaError = true;
                                    System.Threading.Thread.Sleep(tiempoEntreIntentos);
                                    break;
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Ex del servicio:" + ex.ToString());
                            System.Threading.Thread.Sleep(tiempoEntreIntentos);
                            return ex.ToString();
                        }
                    }

                    if (banderaError == true)
                    {
                        banderaError = false;
                        msgError = msgError + "Error envio de: " + codUsu + ". | ";
                    }
                }

                //Estado general del envio
                if (msgError != "")
                    return msgError;
                else
                    return msgConn;
            }
            else
                //Salida Error
                return "Error";
        }
          
         */
    }
}

