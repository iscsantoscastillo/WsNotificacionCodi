using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;

namespace WsNotificacionCodi
{
    /// <summary>
    /// Summary description for NotificacionCodi
    /// </summary>
    [WebService(Namespace = "http://notificacion.macropay.mx/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class NotificacionCodi : System.Web.Services.WebService
    {
        const string PREFIJO_USUARIO_WS = "8"; //A7859gHy (contrasenia usada)
        const char LETRA_DIVISORA = '$';
        /// <summary>
        ///
        /// </summary>
        [WebMethod]
        public ObjRespuesta Notify(ObjPeticion objPeticion)
        {
            //Referencia //campo "des" de peticion de QR? o campo "ref" de 7 caracteres
            BaseDatos.RutaLog = Server.MapPath("~/log4net.config");
            
            try
            {
                string[] arregloUsuario = new string[3];
                arregloUsuario = objPeticion.Usuario.Split(LETRA_DIVISORA);//prefijo
                
                if (arregloUsuario[0] == PREFIJO_USUARIO_WS)
                {
                    if (arregloUsuario.Count() >= 2)
                    {
                        if (BaseDatos.Antenticar(arregloUsuario[1], objPeticion.Contrasenia))
                        {
                            string folioRegistro = "";
                            folioRegistro = BaseDatos.RegistrarPago(objPeticion);
                            

                            if (folioRegistro != "")
                            {
                                BaseDatos.GenerarAbono(objPeticion, folioRegistro);
                                
                                ObjRespuesta objRespuesta = new ObjRespuesta();
                                objRespuesta.Error = "0";
                                objRespuesta.Folio = folioRegistro;
                                objRespuesta.Message = "";
                                return objRespuesta;
                            }
                            else
                            {
                                ObjRespuesta objRespuesta = new ObjRespuesta();
                                objRespuesta.Error = "1";
                                objRespuesta.Folio = "0";
                                objRespuesta.Message = "Error de petición";
                                return objRespuesta;
                            }
                        }
                        else
                        {
                            ObjRespuesta objRespuesta = new ObjRespuesta();
                            objRespuesta.Error = "1";
                            objRespuesta.Folio = "0";
                            objRespuesta.Message = "Error de petición";
                            return objRespuesta;
                        }
                    }
                    else
                    {
                        ObjRespuesta objRespuesta = new ObjRespuesta();
                        objRespuesta.Error = "1";
                        objRespuesta.Folio = "0";
                        objRespuesta.Message = "Error de petición";
                        return objRespuesta;
                    }
                }
                else
                {
                    ObjRespuesta objRespuesta = new ObjRespuesta();
                    objRespuesta.Error = "1";
                    objRespuesta.Folio = "0";
                    objRespuesta.Message = "Error de petición";
                    return objRespuesta;
                }
            }
            catch (Exception ex)
            {                
                ObjRespuesta objRespuesta = new ObjRespuesta();
                objRespuesta.Error = "1";
                objRespuesta.Folio = "0";
                objRespuesta.Message = "Error de petición";
                return objRespuesta;
            }
        }
    }
}
