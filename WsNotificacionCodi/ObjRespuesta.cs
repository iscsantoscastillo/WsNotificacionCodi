using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WsNotificacionCodi
{
    public class ObjRespuesta
    {
        public string Folio { get; set; }
        public string Error { get; set; }
        public string Message { get; set; }
        public ObjRespuesta()
        {
            Folio = "";
            Error = "";
            Message = "";
        }
    }
}