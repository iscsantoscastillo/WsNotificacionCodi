using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace WsNotificacionCodi
{
    public class ObjPeticion
    {
        public string Referencia { get; set; }
        public string Banco      { get; set; }
        public string FechaHora  { get; set; }
        public string Descripcion{ get; set; }
        public string Usuario    { get; set; }
        public string Contrasenia{ get; set; }
        public string Monto      { get; set; }
        public string TipoPago   { get; set; }
        public string Folio      { get; set; }
        public string Sucursal   { get; set; }

        public ObjPeticion()
        {
            Referencia ="";
            Banco      ="";
            FechaHora  ="";
            Descripcion="";
            Usuario    ="";
            Contrasenia="";
            Monto      ="";
            TipoPago   ="";
            Folio      ="";
            Sucursal   ="";
        }
    }
}