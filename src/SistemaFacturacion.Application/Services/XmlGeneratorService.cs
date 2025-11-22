using System.Xml;
using System.Xml.Serialization;
using System.Text;
using SistemaFacturacion.Application.Contracts;
using SistemaFacturacion.Domain.Entities;
using SistemaFacturacion.Domain.Entities.XmlModels;

namespace SistemaFacturacion.Application.Services;

public class XmlGeneratorService : IXmlGeneratorService
{
    private readonly IClaveAccesoService _claveAccesoService;
    private readonly IFacturaRepository _facturaRepository;
    private readonly IConfiguracionRepository _configuracionRepository;
    private readonly IComprobanteElectronicoRepository _comprobanteRepository;
    
    public XmlGeneratorService(
        IClaveAccesoService claveAccesoService,
        IFacturaRepository facturaRepository,
        IConfiguracionRepository configuracionRepository,
        IComprobanteElectronicoRepository comprobanteRepository)
    {
        _claveAccesoService = claveAccesoService;
        _facturaRepository = facturaRepository;
        _configuracionRepository = configuracionRepository;
        _comprobanteRepository = comprobanteRepository;
    }
    
    public async Task<string> GenerarXmlFactura(int idFactura, CancellationToken ct = default)
    {
        // 1. Obtener la factura con todas sus relaciones
        var factura = await _facturaRepository.GetByIdAsync(
            idFactura, 
            includePagos: true, 
            includeDetalles: true, 
            includeCliente: true, 
            ct);
            
        if (factura == null)
            throw new Exception($"Factura con ID {idFactura} no encontrada");
            
        if (factura.Cliente == null)
            throw new Exception("La factura no tiene un cliente asociado");
            
        if (factura.Detalles == null || !factura.Detalles.Any())
            throw new Exception("La factura no tiene detalles");
        
        // 2. Obtener configuración de empresa
        var config = await _configuracionRepository.GetConfiguracionAsync(ct);
            
        if (config == null)
            throw new Exception("No hay configuración de empresa registrada");
        
        // 3. Validar configuración requerida
        if (string.IsNullOrEmpty(config.Ruc) || 
            string.IsNullOrEmpty(config.RazonSocial) ||
            string.IsNullOrEmpty(config.Establecimiento) ||
            string.IsNullOrEmpty(config.PuntoEmision))
        {
            throw new Exception("Configuración de empresa incompleta. Verifique RUC, Razón Social, Establecimiento y Punto de Emisión");
        }
        
        // 4. Extraer número secuencial del número de factura (formato: 001-001-000000001)
        var partes = factura.NumeroFactura.Split('-');
        if (partes.Length != 3)
            throw new Exception("Formato de número de factura inválido. Debe ser: 001-001-000000001");
            
        string establecimiento = partes[0];
        string puntoEmision = partes[1];
        string secuencial = partes[2];
        
        // 5. Generar clave de acceso
        string claveAcceso = _claveAccesoService.GenerarClaveAcceso(
            factura.FechaEmision,
            "01", // 01 = Factura
            config.Ruc,
            config.Ambiente.ToString(),
            establecimiento,
            puntoEmision,
            secuencial
        );
        
        // 6. Construir el objeto FacturaXml
        var facturaXml = new FacturaXml
        {
            InfoTributaria = ConstruirInfoTributaria(factura, config, claveAcceso, establecimiento, puntoEmision, secuencial),
            InfoFactura = ConstruirInfoFactura(factura, config),
            Detalles = ConstruirDetalles(factura.Detalles.ToList()),
            InfoAdicional = ConstruirInfoAdicional(factura)
        };
        
        // 7. Serializar a XML
        string xmlGenerado = SerializarAXml(facturaXml);
        
        // 8. Verificar si ya existe un comprobante electrónico
        var comprobanteExistente = await _comprobanteRepository.GetByFacturaIdAsync(idFactura, ct);
        
        if (comprobanteExistente != null)
        {
            // Actualizar el existente
            comprobanteExistente.ClaveAcceso = claveAcceso;
            comprobanteExistente.XmlGenerado = xmlGenerado;
            comprobanteExistente.EstadoEnvio = "NO_ENVIADO";
            await _comprobanteRepository.UpdateAsync(comprobanteExistente, ct);
        }
        else
        {
            // Crear uno nuevo
            var comprobante = new ComprobanteElectronico
            {
                IdFactura = idFactura,
                ClaveAcceso = claveAcceso,
                XmlGenerado = xmlGenerado,
                EstadoEnvio = "NO_ENVIADO"
            };
            await _comprobanteRepository.AddAsync(comprobante, ct);
        }
        
        return xmlGenerado;
    }
    
    private InfoTributaria ConstruirInfoTributaria(
        Factura factura,
        ConfiguracionEmpresa config,
        string claveAcceso,
        string establecimiento,
        string puntoEmision,
        string secuencial)
    {
        return new InfoTributaria
        {
            Ambiente = config.Ambiente.ToString(),
            TipoEmision = "1",
            RazonSocial = config.RazonSocial,
            NombreComercial = config.NombreComercial ?? config.RazonSocial,
            Ruc = config.Ruc,
            ClaveAcceso = claveAcceso,
            CodDoc = "01",
            Estab = establecimiento,
            PtoEmi = puntoEmision,
            Secuencial = secuencial,
            DirMatriz = config.DireccionMatriz ?? "Matriz"
        };
    }
    
    private InfoFactura ConstruirInfoFactura(Factura factura, ConfiguracionEmpresa config)
    {
        // Determinar tipo de identificación (04=RUC, 05=Cédula)
        string tipoIdComprador = factura.Cliente.TipoIdentificacion == "RUC" ? "04" : "05";
        
        return new InfoFactura
        {
            FechaEmision = factura.FechaEmision.ToString("dd/MM/yyyy"),
            DirEstablecimiento = config.DireccionMatriz ?? "Matriz",
            ContribuyenteEspecial = null,
            ObligadoContabilidad = config.ObligadoContabilidad ?? "NO",
            TipoIdentificacionComprador = tipoIdComprador,
            GuiaRemision = null,
            RazonSocialComprador = $"{factura.Cliente.Nombres} {factura.Cliente.Apellidos}".Trim(),
            IdentificacionComprador = factura.Cliente.Identificacion.Trim(),
            TotalSinImpuestos = factura.Subtotal.ToString("F2"),
            TotalDescuento = "0.00",
            TotalConImpuestos = ConstruirTotalImpuestos(factura),
            Propina = "0.00",
            ImporteTotal = factura.Total.ToString("F2"),
            Moneda = "DOLAR",
            Pagos = ConstruirFormasPago(factura)
        };
    }
    
    private List<TotalImpuesto> ConstruirTotalImpuestos(Factura factura)
    {
        // IVA 15% (ajusta según la tarifa vigente en Ecuador)
        return new List<TotalImpuesto>
        {
            new TotalImpuesto
            {
                Codigo = "2", // 2 = IVA
                CodigoPorcentaje = "3", // 3 = 15% (verificar tabla SRI actualizada)
                BaseImponible = factura.Subtotal.ToString("F2"),
                Valor = factura.Iva.ToString("F2")
            }
        };
    }
    
    private List<FormaPago> ConstruirFormasPago(Factura factura)
{
    var pagos = new List<FormaPago>();
    
    if (factura.Pagos != null && factura.Pagos.Any())
    {
        foreach (var pago in factura.Pagos.Where(p => p.Estado == "REGISTRADO"))
        {
            pagos.Add(new FormaPago
            {
                FormaPagoCode = MapearMetodoPagoASri(pago.MetodoPago),
                Total = pago.Monto.ToString("F2"), // Siempre 2 decimales
                Plazo = null,
                UnidadTiempo = null
            });
        }
    }
    
    // Si no hay pagos, agregar uno por defecto
    if (!pagos.Any())
    {
        pagos.Add(new FormaPago
        {
            FormaPagoCode = "01", // Sin utilización del sistema financiero
            Total = factura.Total.ToString("F2"),
            Plazo = null,
            UnidadTiempo = null
        });
    }
    
    return pagos;
}

private string MapearMetodoPagoASri(string metodoPago)
{
    // Códigos oficiales del SRI (Tabla 24)
    return metodoPago?.ToUpper() switch
    {
        "EFECTIVO" => "01", // Sin utilización del sistema financiero
        "CHEQUE" => "02", // Cheque propio
        "TRANSFERENCIA" => "17", // Transferencia - Depósito en cuenta
        "TARJETA_DEBITO" => "19", // Tarjeta de débito
        "TARJETA DEBITO" => "19",
        "TARJETA_CREDITO" => "20", // Tarjeta de crédito nacional
        "TARJETA CREDITO" => "20",
        "TARJETA" => "20",
        _ => "01"
    };
}

    
    private List<DetalleXml> ConstruirDetalles(List<DetalleFactura> detalles)
    {
        return detalles.Select(d => new DetalleXml
        {
            CodigoPrincipal = d.Producto?.Codigo ?? "SIN_CODIGO",
            Descripcion = d.Producto?.Nombre ?? "Producto",
            Cantidad = d.Cantidad.ToString("F2"),
            PrecioUnitario = d.PrecioUnitario.ToString("F6"),
            Descuento = "0.00",
            PrecioTotalSinImpuesto = d.TotalLinea.ToString("F2"),
            Impuestos = new List<ImpuestoDetalle>
            {
                new ImpuestoDetalle
                {
                    Codigo = "2", // IVA
                    CodigoPorcentaje = "3", // 15%
                    Tarifa = "15",
                    BaseImponible = d.TotalLinea.ToString("F2"),
                    Valor = (d.TotalLinea * 0.15m).ToString("F2")
                }
            }
        }).ToList();
    }
    
    private List<CampoAdicional>? ConstruirInfoAdicional(Factura factura)
    {
        var campos = new List<CampoAdicional>();
        
        if (!string.IsNullOrEmpty(factura.Cliente?.Correo))
        {
            campos.Add(new CampoAdicional
            {
                Nombre = "Email",
                Valor = factura.Cliente.Correo
            });
        }
        
        if (!string.IsNullOrEmpty(factura.Cliente?.Telefono))
        {
            campos.Add(new CampoAdicional
            {
                Nombre = "Telefono",
                Valor = factura.Cliente.Telefono
            });
        }
        
        return campos.Count > 0 ? campos : null;
    }
    
    public string SerializarAXml<T>(T objeto)
{
    var xmlSerializer = new XmlSerializer(typeof(T));
    var settings = new XmlWriterSettings
    {
        Indent = true,
        IndentChars = "  ",
        Encoding = new UTF8Encoding(false), // Sin BOM
        OmitXmlDeclaration = false
    };
    using var stringWriter = new Utf8StringWriter(); // ← ESTA CLASE
    using var xmlWriter = XmlWriter.Create(stringWriter, settings);
    var namespaces = new XmlSerializerNamespaces();
    namespaces.Add("", "");
    xmlSerializer.Serialize(xmlWriter, objeto, namespaces);
    return stringWriter.ToString();
}

}
