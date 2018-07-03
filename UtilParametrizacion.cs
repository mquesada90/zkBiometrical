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

namespace rzkclock_ws
{
    public class UtilParametrizacion
    {

        //Actualiza entre los parametros configurables, el tipo de ambiente del dispositivo.
        public int cambiarAmbiente_wsZK(string opID)
        {
            String tipoAmbiente="", wsSAP_usuario="", wsSAP_clave="";
            switch (opID)
            {
                case "DEV":
                    tipoAmbiente = "DEV";
                    wsSAP_usuario = UtilConstantes.DEV_SAPws_user;
                    wsSAP_clave = UtilConstantes.DEV_SAPws_pass;
                    break;

                case "QAS":
                    tipoAmbiente = "QAS";
                    wsSAP_usuario = UtilConstantes.QAS_SAPws_user;
                    wsSAP_clave = UtilConstantes.QAS_SAPws_pass;
                    break;

                case "PRD":
                    tipoAmbiente = "PRD";
                    wsSAP_usuario = UtilConstantes.PRD_SAPws_user;
                    wsSAP_clave = UtilConstantes.PRD_SAPws_pass;
                    break;
            }

            //Guarda el nuevo dato
            if (tipoAmbiente.Length > 0)
            {
                Properties.Settings.Default["ambiente_ZK"] = tipoAmbiente;
                Properties.Settings.Default["user_SAP_WS"] = wsSAP_usuario;
                Properties.Settings.Default["pass_SAP_WS"] = wsSAP_clave;
                Properties.Settings.Default.Save();
                return 1;
            }
            else{
                return -1;
            }
        }

        //Actualiza entre los parametros de usuario
        public int cambiarCredencialesConexion_wsSAP(string opID, string user, string pass)
        {
            String ambienteActivo = (String)Properties.Settings.Default.PropertyValues["ambiente_ZK"].PropertyValue;
            if (ambienteActivo=="PRD" || ambienteActivo=="QAS" || ambienteActivo=="DEV")
            {
                Properties.Settings.Default["user_SAP_WS"] = user;
                Properties.Settings.Default["pass_SAP_WS"] = pass;
                Properties.Settings.Default.Save();
                return 1;
            }else{
                return -1;
            }
        }

        //Actualiza el valor asociado al SERVICIO INVOCADO EN SAP.
        public int cambiarRFCSAP_wsZK(string opID, string url_nuevoSAP_WS)
        {
            String tipoAmbiente = "";
            switch (opID)
            {
                case "DEV":
                    tipoAmbiente = "DEV";
                    Properties.Settings.Default["SAP_WS_DEV_ZK"] = url_nuevoSAP_WS;
                    break;

                case "QAS":
                    tipoAmbiente = "QAS";
                    Properties.Settings.Default["SAP_WS_QAS_ZK"] = url_nuevoSAP_WS;
                    break;

                case "PRD":
                    tipoAmbiente = "PRD";
                    Properties.Settings.Default["SAP_WS_PRD_ZK"] = url_nuevoSAP_WS;
                    break;
            }

            //Guarda el nuevo valor
            if (tipoAmbiente.Length > 0)
            {
                Properties.Settings.Default.Save();
                return 1;
            }
            else
            {
                return -1;
            }
        }



    }
}
