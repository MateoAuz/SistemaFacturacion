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
        
        var config = await _configuracionRepository.GetConfiguracionAsync(ct);
            
        if (config == null)
            throw new Exception("No hay configuración de empresa registrada");
        
        if (string.IsNullOrEmpty(config.Ruc) || 
            string.IsNullOrEmpty(config.RazonSocial) ||
            string.IsNullOrEmpty(config.Establecimiento) ||
            string.IsNullOrEmpty(config.PuntoEmision))
        {
            throw new Exception("Configuración de empresa incompleta. Verifique RUC, Razón Social, Establecimiento y Punto de Emisión");
        }
        
        var partes = factura.NumeroFactura.Split('-');
        if (partes.Length != 3)
            throw new Exception("Formato de número de factura inválido. Debe ser: 001-001-000000001");
            
        string establecimiento = partes[0];
        string puntoEmision = partes[1];
        string secuencial = partes[2];
        
        string claveAcceso = _claveAccesoService.GenerarClaveAcceso(
            factura.FechaEmision,
            "01", 
            config.Ruc,
            config.Ambiente.ToString(),
            establecimiento,
            puntoEmision,
            secuencial
        );
        
        var facturaXml = new FacturaXml
        {
            InfoTributaria = ConstruirInfoTributaria(factura, config, claveAcceso, establecimiento, puntoEmision, secuencial),
            InfoFactura = ConstruirInfoFactura(factura, config),
            Detalles = ConstruirDetalles(factura.Detalles.ToList()),
            InfoAdicional = ConstruirInfoAdicional(factura)
        };
        
        string xmlGenerado = SerializarAXml(facturaXml);
        
        var comprobanteExistente = await _comprobanteRepository.GetByFacturaIdAsync(idFactura, ct);
        
        if (comprobanteExistente != null)
        {
            comprobanteExistente.ClaveAcceso = claveAcceso;
            comprobanteExistente.XmlGenerado = xmlGenerado;
            comprobanteExistente.EstadoEnvio = "NO_ENVIADO";
            await _comprobanteRepository.UpdateAsync(comprobanteExistente, ct);
        }
        else
        {
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
        string tipoIdComprador = factura.Cliente.TipoIdentificacion == "RUC" ? "04" : "05";
        
        return new InfoFactura
        {
            FechaEmision = factura.FechaEmision.ToString("dd/MM/yyyy"),
            DirEstablecimiento = config.DireccionMatriz ?? "Matriz",
            ContribuyenteEspecial = null,
            ObligadoContabilidad = config.ObligadoContabilidad ?? "NO",
            DireccionComprador = factura.Cliente.Direccion.Trim(),
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
        return new List<TotalImpuesto>
        {
            new TotalImpuesto
            {
                Codigo = "2", 
                CodigoPorcentaje = "4", 
                BaseImponible = factura.Subtotal.ToString("F2"),
                Tarifa = "15",
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
                Total = pago.Monto.ToString("F2"), 
                Plazo = null,
                UnidadTiempo = null
            });
        }
    }
    
    if (!pagos.Any())
    {
        pagos.Add(new FormaPago
        {
            FormaPagoCode = "01", 
            Total = factura.Total.ToString("F2"),
            Plazo = null,
            UnidadTiempo = null
        });
    }
    
    return pagos;
}

private string MapearMetodoPagoASri(string metodoPago)
{
    return metodoPago?.ToUpper() switch
    {
        "EFECTIVO" => "01", 
        "CHEQUE" => "02", 
        "TRANSFERENCIA" => "17", 
        "TARJETA_DEBITO" => "19", 
        "TARJETA DEBITO" => "19",
        "TARJETA_CREDITO" => "20", 
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
                    Codigo = "2", 
                    CodigoPorcentaje = "4", 
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
    private string SerializarAXml(FacturaXml facturaXml)
{
    var namespaces = new XmlSerializerNamespaces();


    var serializer = new XmlSerializer(typeof(FacturaXml));
    
    var settings = new XmlWriterSettings
    {
        Encoding = new UTF8Encoding(false),
        Indent = true,
        IndentChars = "  ",
        OmitXmlDeclaration = false
    };

    using var stringWriter = new Utf8StringWriter();
    using var xmlWriter = XmlWriter.Create(stringWriter, settings);
    
    serializer.Serialize(xmlWriter, facturaXml, namespaces);
    
    return stringWriter.ToString();
}

    public string SerializarAXml<T>(T objeto)
{
    var xmlSerializer = new XmlSerializer(typeof(T));
    var settings = new XmlWriterSettings
    {
        Indent = true,
        IndentChars = "  ",
        Encoding = new UTF8Encoding(false),
        OmitXmlDeclaration = false
    };
    using var stringWriter = new Utf8StringWriter(); 
    using var xmlWriter = XmlWriter.Create(stringWriter, settings);
    var namespaces = new XmlSerializerNamespaces();
    namespaces.Add("", "");
    xmlSerializer.Serialize(xmlWriter, objeto, namespaces);
    return stringWriter.ToString();
}

}

public class Utf8StringWriter : StringWriter
{
    public override Encoding Encoding => Encoding.UTF8;
}

