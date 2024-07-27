using CapaDatos;
using CapaModelo;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace VentasWeb.Controllers
{
    public class VentaController : Controller
    {
        private static Usuario SesionUsuario;
        // GET: Venta
        public ActionResult Crear()
        {
            SesionUsuario = (Usuario)Session["Usuario"];
            return View();
        }

        // GET: Venta
        public ActionResult Consultar()
        {
            return View();
        }

        public ActionResult Documento(int IdVenta = 0)
        {

            Venta oVenta = CD_Venta.Instancia.ObtenerDetalleVenta(IdVenta);



            NumberFormatInfo formato = new CultureInfo("es-CL").NumberFormat;
            formato.CurrencyGroupSeparator = ".";
            formato.CurrencyDecimalSeparator = ",";  // Asegúrate de que el separador decimal sea correcto
            formato.CurrencySymbol = "$";  // Si deseas incluir el símbolo de moneda

            if (oVenta == null)
                oVenta = new Venta();
            else {

                oVenta.oListaDetalleVenta = (from dv in oVenta.oListaDetalleVenta
                                             select new DetalleVenta()
                                             {
                                                 Cantidad = dv.Cantidad,
                                                 NombreProducto = dv.NombreProducto,
                                                 PrecioUnidad = dv.PrecioUnidad / 100, 
                                                 TextoPrecioUnidad = (dv.PrecioUnidad / 100).ToString("C", formato),
                                                 ImporteTotal = dv.ImporteTotal / 100,  
                                                 TextoImporteTotal = (dv.ImporteTotal / 100).ToString("C", formato)
                                             }).ToList();

                oVenta.TextoImporteRecibido = (oVenta.ImporteRecibido / 100).ToString("C", formato);
                oVenta.TextoImporteCambio = (oVenta.ImporteCambio / 100).ToString("C", formato);
                oVenta.TextoTotalCosto = (oVenta.TotalCosto / 100).ToString("C", formato);
            }
               

            return View(oVenta);
        }


        public JsonResult Obtener(string codigo, string fechainicio, string fechafin, string numerodocumento, string nombres)
        {
            List<Venta> lista = CD_Venta.Instancia.ObtenerListaVenta(codigo, Convert.ToDateTime(fechainicio), Convert.ToDateTime(fechafin), numerodocumento, nombres);


            if (lista == null)
                lista = new List<Venta>();

            return Json(new { data = lista }, JsonRequestBehavior.AllowGet);
        }


        public JsonResult ObtenerUsuario()
        {
            Usuario rptUsuario = CD_Usuario.Instancia.ObtenerDetalleUsuario(SesionUsuario.IdUsuario);
            return Json(rptUsuario, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ObtenerProductoPorTienda(int IdTienda)
        {

            List<ProductoTienda> oListaProductoTienda = CD_ProductoTienda.Instancia.ObtenerProductoTienda();
            oListaProductoTienda = oListaProductoTienda.Where(x => x.oTienda.IdTienda == IdTienda && x.Stock > 0).ToList();


            return Json(new { data = oListaProductoTienda }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult ControlarStock(int idproducto, int idtienda, int cantidad, bool restar)
        {
            bool respuesta = CD_ProductoTienda.Instancia.ControlarStock(idproducto, idtienda, cantidad, restar);
            return Json(new { resultado = respuesta }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Guardar(string xml)
        {
            xml = xml.Replace("!idusuario¡", SesionUsuario.IdUsuario.ToString());
            int Respuesta = 0;
            Respuesta = CD_Venta.Instancia.RegistrarVenta(xml);
            if (Respuesta != 0)
                return Json(new { estado = true, valor = Respuesta.ToString() }, JsonRequestBehavior.AllowGet);
            else
                return Json(new { estado = false, valor = "" }, JsonRequestBehavior.AllowGet);
        }

    }
}