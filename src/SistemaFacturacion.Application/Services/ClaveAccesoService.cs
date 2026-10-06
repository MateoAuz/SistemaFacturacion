using SistemaFacturacion.Application.Contracts;

namespace SistemaFacturacion.Application.Services;

public class ClaveAccesoService : IClaveAccesoService
{
    public string GenerarClaveAcceso(
        DateTime fechaEmision,
        string tipoComprobante,
        string ruc,
        string ambiente,
        string establecimiento,
        string puntoEmision,
        string secuencial)
    {
        string fecha = fechaEmision.ToString("ddMMyyyy");
        
        string tipo = tipoComprobante.PadLeft(2, '0');
        
        string rucFormateado = ruc.PadLeft(13, '0');
        
        string amb = ambiente;
        
        string serie = establecimiento.PadLeft(3, '0') + puntoEmision.PadLeft(3, '0');
        
        string sec = secuencial.PadLeft(9, '0');
        
        string codigoNumerico = GenerarCodigoNumerico();
        
        string tipoEmision = "1";
        
        string clave48 = fecha + tipo + rucFormateado + amb + serie + sec + codigoNumerico + tipoEmision;
        
        string digitoVerificador = CalcularDigitoVerificadorModulo11(clave48);
        
        return clave48 + digitoVerificador;
    }
    
    private string GenerarCodigoNumerico()
    {
        Random random = new Random();
        return random.Next(10000000, 99999999).ToString();
    }
        
    private string CalcularDigitoVerificadorModulo11(string clave48)
{
    int suma = 0;
    int factor = 2;
    
    // Recorrer de derecha a izquierda
    for (int i = clave48.Length - 1; i >= 0; i--)
    {
        int digito = int.Parse(clave48[i].ToString());
        suma += digito * factor;
        factor++;
        if (factor > 7) factor = 2;
    }
    
    int modulo = suma % 11;
    int resultado = 11 - modulo;
    
    if (resultado == 11) return "0";
    if (resultado == 10) return "1";
    return resultado.ToString();
}

}
