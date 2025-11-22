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
        // 1. Fecha emisión (8 dígitos): ddmmyyyy
        string fecha = fechaEmision.ToString("ddMMyyyy");
        
        // 2. Tipo comprobante (2 dígitos): 01=Factura
        string tipo = tipoComprobante.PadLeft(2, '0');
        
        // 3. RUC (13 dígitos)
        string rucFormateado = ruc.PadLeft(13, '0');
        
        // 4. Ambiente (1 dígito): 1=Pruebas, 2=Producción
        string amb = ambiente;
        
        // 5. Serie (6 dígitos): Establecimiento (3) + Punto Emisión (3)
        string serie = establecimiento.PadLeft(3, '0') + puntoEmision.PadLeft(3, '0');
        
        // 6. Número secuencial (9 dígitos)
        string sec = secuencial.PadLeft(9, '0');
        
        // 7. Código numérico (8 dígitos aleatorios)
        string codigoNumerico = GenerarCodigoNumerico();
        
        // 8. Tipo emisión (1 dígito): 1=Normal
        string tipoEmision = "1";
        
        // Construir los primeros 48 dígitos
        string clave48 = fecha + tipo + rucFormateado + amb + serie + sec + codigoNumerico + tipoEmision;
        
        // 9. Dígito verificador usando Módulo 11
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
