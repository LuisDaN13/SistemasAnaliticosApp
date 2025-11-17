namespace SistemasAnaliticos.Services
{
    public class FechaLargaService : IFechaLargaService
    {
        public string FechaEnPalabras(DateOnly fecha)
        {
            string[] dias = { "", "uno", "dos", "tres", "cuatro", "cinco", "seis", "siete", "ocho",
        "nueve", "diez", "once", "doce", "trece", "catorce", "quince", "dieciséis",
        "diecisiete", "dieciocho", "diecinueve", "veinte", "veintiuno", "veintidós",
        "veintitrés", "veinticuatro", "veinticinco", "veintiséis", "veintisiete",
        "veintiocho", "veintinueve", "treinta", "treinta y uno" };

            string[] meses = { "enero", "febrero", "marzo", "abril", "mayo", "junio",
        "julio", "agosto", "setiembre", "octubre", "noviembre", "diciembre"
        };

            string dia = dias[fecha.Day];
            string mes = meses[fecha.Month - 1];
            string año = NumeroEnPalabras(fecha.Year);

            return $"{dia} días del mes de {mes} del {año}";
        }

        public string NumeroEnPalabras(int numero)
        {
            var unidades = new[] { "", "uno", "dos", "tres", "cuatro", "cinco", "seis", "siete", "ocho", "nueve" };
            var decenas = new[] { "", "diez", "veinte", "treinta", "cuarenta", "cincuenta", "sesenta", "setenta", "ochenta", "noventa" };
            var especiales = new[] { "diez", "once", "doce", "trece", "catorce", "quince", "dieciséis", "diecisiete", "dieciocho", "diecinueve" };

            if (numero < 10) return unidades[numero];
            if (numero < 20) return especiales[numero - 10];
            if (numero < 100)
            {
                int d = numero / 10;
                int u = numero % 10;
                return u == 0 ? decenas[d] : $"{decenas[d]} y {unidades[u]}";
            }
            if (numero < 1000)
            {
                int c = numero / 100;
                int resto = numero % 100;
                string centenas =
                    c == 1 ? "ciento" :
                    c == 5 ? "quinientos" :
                    c == 7 ? "setecientos" :
                    c == 9 ? "novecientos" :
                    $"{unidades[c]}cientos";

                return resto == 0 ? $"{centenas}" : $"{centenas} {NumeroEnPalabras(resto)}";
            }
            if (numero < 2000)
                return $"mil {NumeroEnPalabras(numero % 1000)}".Trim();

            if (numero < 1_000_000)
            {
                int miles = numero / 1000;
                int resto = numero % 1000;

                string milesTxt = miles == 1 ? "mil" : $"{NumeroEnPalabras(miles)} mil";

                return resto == 0 ? milesTxt : $"{milesTxt} {NumeroEnPalabras(resto)}";
            }

            return numero.ToString();
        }

        public string SalarioEnPalabras(int numero)
        {
            string numeroTerminado;

            if (numero == 0) return "cero";
            if (numero < 0) return "menos " + NumeroEnPalabras(Math.Abs(numero));

            var unidades = new[] { "", "uno", "dos", "tres", "cuatro", "cinco", "seis", "siete", "ocho", "nueve" };
            var decenas = new[] { "", "diez", "veinte", "treinta", "cuarenta", "cincuenta", "sesenta", "setenta", "ochenta", "noventa" };
            var especiales = new[] { "diez", "once", "doce", "trece", "catorce", "quince", "dieciséis", "diecisiete", "dieciocho", "diecinueve" };

            // Para números pequeños (0-999)
            string ConvertirMenorMil(int n)
            {
                if (n == 0) return "";

                if (n < 10) return unidades[n];
                if (n < 20) return especiales[n - 10];
                if (n < 100)
                {
                    int d = (int)(n / 10);
                    int u = n % 10;
                    return u == 0 ? decenas[d] : $"{decenas[d]} y {unidades[u]}";
                }

                int c = (int)(n / 100);
                int resto = n % 100;
                string centenas =
                    c == 1 && resto == 0 ? "cien" :
                    c == 1 ? "ciento" :
                    c == 5 ? "quinientos" :
                    c == 7 ? "setecientos" :
                    c == 9 ? "novecientos" :
                    $"{unidades[c]}cientos";

                return resto == 0 ? centenas : $"{centenas} {ConvertirMenorMil(resto)}";
            }

            // Para números grandes
            if (numero < 1000)
                return ConvertirMenorMil(numero);

            if (numero < 1000000)
            {
                int miles = numero / 1000;
                int resto = numero % 1000;

                string milesTexto;
                if (miles == 1)
                    milesTexto = "mil";
                else
                    milesTexto = $"{ConvertirMenorMil(miles)} mil";

                if (resto == 0)
                    return milesTexto;

                // Si el resto es menor a 100, usa "y" para conectar
                return resto < 100 ? $"{milesTexto} y {ConvertirMenorMil(resto)}"
                                  : $"{milesTexto} {ConvertirMenorMil(resto)}";
            }

            if (numero < 1000000000)
            {
                int millones = numero / 1000000;
                int resto = numero % 1000000;

                string millonesTexto;
                if (millones == 1)
                    millonesTexto = "un millón";
                else
                    millonesTexto = $"{ConvertirMenorMil(millones)} millones";

                if (resto == 0)
                    return millonesTexto;

                return $"{millonesTexto} {NumeroEnPalabras(resto)}";
            }

            return numero.ToString(); // Para números muy grandes
        }
    }
}
