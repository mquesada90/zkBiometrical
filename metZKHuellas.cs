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
    public class metZKHuellas
    {
        //Config por ambiente
        private String sistemaServicio = "";
        private String ambienteServicio = "";
        private Boolean estadoSyst = false;
        private Boolean estadoParam = false;
        private mainConnRFC connSAP = null;

        //Instacias de clases Glob
        func_SapRfc func_SAPRFC = null;
        UtilConstantes metUtil = null;
        metSDK insSDK = null;
        String msgRet = "";

        public void ini_InsZK()
        {
            //Config por ambiente
            sistemaServicio = Properties.Settings.Default.sistema_ZK;
            ambienteServicio = Properties.Settings.Default.ambiente_ZK;
            //estadoSyst = generarParametrosSist(sistemaServicio, ambienteServicio);
            //estadoParam = generarParametrosZK(sistemaServicio, ambienteServicio);

            //func_SAPRFC = new func_SapRfc(ambienteServicio);
            insSDK = new metSDK();
            metUtil = new UtilConstantes();
        }
        public string control_servicios(UtilConstantes.sys_opServicios idServicio, String ip, String puerto, String tipo, String usuData) {

            //Iniciar instancias para clases ZK
            ini_InsZK();

            switch (idServicio)
            {
                case UtilConstantes.sys_opServicios.GET_TEMPLATE:
                    metUtil.generarBitacoraZK("INFO", "Inicia proceso de captura de huella con ZK4500.");
                    msgRet = insSDK.sdk_EnRoll();
                    if (msgRet.Contains("<ERROR>"))
                    {
                        string[] msg = msgRet.Split(new[] { "<ERROR>" }, StringSplitOptions.None);
                        if (msg.Length == 2)
                        {
                            msgRet = "<INCIDENTE> " + msg[1];
                            metUtil.generarBitacoraZK("INFO", "Ex en biometrioco, " + msg[1]);
                        }
                    }
                    break;
                case UtilConstantes.sys_opServicios.PING:
                    metUtil.generarBitacoraZK("INFO", "Inicia proceso para revisar conexión del ZK con la IP " + ip);
                    msgRet = insSDK.sdk_Ping(ip, puerto, tipo);
                    if (msgRet.Contains("<ERROR>"))
                    {
                        string[] msg = msgRet.Split(new[] { "<ERROR>" }, StringSplitOptions.None);
                        if (msg.Length == 2)
                        {
                            msgRet = "<INCIDENTE> " + msg[1];
                            metUtil.generarBitacoraZK("INFO", "Ex en biometrioco, " + msg[1]);
                        }
                    }
                    break;
                case UtilConstantes.sys_opServicios.LIST_USU:
                    metUtil.generarBitacoraZK("INFO", "Inicia proceso para obtener Lista de Usuarios/Huellas del ZK con la IP " + ip);
                    msgRet = insSDK.sdk_LUsuarios(ip, puerto, tipo, usuData);
                    if (msgRet.Contains("<ERROR>"))
                    {
                        string[] msg = msgRet.Split(new[] { "<ERROR>" }, StringSplitOptions.None);
                        if (msg.Length == 2)
                        {
                            msgRet = "<INCIDENTE> " + msg[1];
                            metUtil.generarBitacoraZK("INFO", "Ex en biometrioco, " + msg[1]);
                        }
                    }
                    break;
                case UtilConstantes.sys_opServicios.OBT_MARC:
                    metUtil.generarBitacoraZK("INFO", "Inicia proceso para obtener Lista de Usuarios/Marcas del ZK con la IP " + ip);
                    msgRet = insSDK.sdk_MarcasZK(ip, puerto, tipo, usuData);
                    if (msgRet.Contains("<ERROR>"))
                    {
                        string[] msg = msgRet.Split(new[] { "<ERROR>" }, StringSplitOptions.None);
                        if (msg.Length == 2)
                        {
                            msgRet = "<INCIDENTE> " + msg[1];
                            metUtil.generarBitacoraZK("INFO", "Ex en biometrioco, " + msg[1]);
                        }
                    }
                    break;
                case UtilConstantes.sys_opServicios.ENV_USU:
                    metUtil.generarBitacoraZK("INFO", "Inicia proceso para Enviar Usuarios al ZK con la IP " + ip);
                    msgRet = insSDK.sdk_Add_UsuariosZK(ip, puerto, tipo, usuData);
                    if (msgRet.Contains("<ERROR>"))
                    {
                        string[] msg = msgRet.Split(new[] { "<ERROR>" }, StringSplitOptions.None);
                        if (msg.Length == 2)
                        {
                            msgRet = "<INCIDENTE> " + msg[1];
                            metUtil.generarBitacoraZK("INFO", "Ex en biometrioco, " + msg[1]);
                        }
                    }
                    break;
                case UtilConstantes.sys_opServicios.ENV_HUE:
                    metUtil.generarBitacoraZK("INFO", "Inicia proceso para Enviar Huellas al ZK con la IP " + ip);
                    msgRet = insSDK.sdk_Add_HuellasZK(ip, puerto, tipo, usuData);
                    if (msgRet.Contains("<ERROR>"))
                    {
                        string[] msg = msgRet.Split(new[] { "<ERROR>" }, StringSplitOptions.None);
                        if (msg.Length == 2)
                        {
                            msgRet = "<INCIDENTE> " + msg[1];
                            metUtil.generarBitacoraZK("INFO", "Ex en biometrioco, " + msg[1]);
                        }
                    }
                    break;
            }
            return msgRet;
        }

        private Boolean generarParametrosSist(String sist, String amb)
        {
            Boolean estadoR = false;
            return estadoR;
        }   
        private Boolean generarParametrosZK(String sist, String amb)
        {
            Boolean estadoR = false;

            connSAP = new mainConnRFC();
            return estadoR;
        }







    }
}
