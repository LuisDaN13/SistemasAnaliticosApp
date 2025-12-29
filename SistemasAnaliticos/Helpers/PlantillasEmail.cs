namespace SistemasAnaliticos.Helpers
{
    public static class PlantillasEmail
    {
        public static string ConfirmacionEmpleado(string nombreEmpleado, string tipoPermiso)
        {
            return $@"
            <!DOCTYPE html>
            <html>
            <body style='margin: 0; padding: 0; font-family: -apple-system, BlinkMacSystemFont, \'Segoe UI\', Roboto, \'Helvetica Neue\', Arial, sans-serif; background-color: #e8e8e8;'>
                <table role='presentation' cellpadding='0' cellspacing='0' border='0' width='100%' style='background-color: #e8e8e8;'>
                    <tr>
                        <td align='center' style='padding: 40px 20px;'>
                            <table role='presentation' cellpadding='0' cellspacing='0' border='0' width='600' style='background-color: #ffffff; max-width: 600px;'>
                                <tr>
                                    <td style='padding: 40px 40px 30px 40px; border-top: 4px solid #B7041A;'></td>
                                </tr>
                                <tr>
                                    <td style='padding: 0 40px 20px 40px;'>
                                        <p style='margin: 0 0 20px 0; font-size: 16px; line-height: 1.5; color: #333333;'>
                                            A quien corresponda, {nombreEmpleado}
                                        </p>
                                        <p style='margin: 0 0 20px 0; font-size: 16px; line-height: 1.6; color: #555555;'>
                                            Mediante la presente notificación por correo electrónico, se confirma la creación de un permiso del tipo {tipoPermiso}, el cual ha sido registrado correctamente en el sistema.
                                        </p>
                                        <p style='margin: 0 0 30px 0; font-size: 16px; line-height: 1.6; color: #555555;'>
                                            Si deseas observar más información, ingresa al sistema de Recursos Humanos para consultar el detalle del permiso.
                                        </p>
                                        <table role='presentation' cellpadding='0' cellspacing='0' border='0'>
                                            <tr>
                                                <td style='border-radius: 24px; border: 1px solid #333333;'>
                                                    <a href='#' style='display: inline-block; padding: 12px 28px; font-size: 15px; color: #333333; text-decoration: none; font-weight: 500;'>
                                                        Ir ahora
                                                    </a>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                                <tr>
                                    <td style='padding: 30px 40px 20px 40px;'>
                                        <p style='margin: 0 0 10px 0; font-size: 16px; line-height: 1.5; color: #555555;'>
                                            Gracias por tu colaboración, 
                                        </p>
                                        <p style='margin: 0; font-size: 16px; line-height: 1.5; color: #555555;'>
                                            Sistemas Analitícos
                                        </p>
                                    </td>
                                </tr>
                                <tr>
                                    <td style='padding-bottom: 40px; border-bottom: 4px solid #B7041A;'></td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
            </body>
            </html>";
        }

        public static string NotificacionJefatura(string nombreEmpleado, string tipoPermiso, string nombreJefe)
        {
            return $@"
            <!DOCTYPE html>
            <html>
            <body style='margin: 0; padding: 0; font-family: -apple-system, BlinkMacSystemFont, \'Segoe UI\', Roboto, \'Helvetica Neue\', Arial, sans-serif; background-color: #e8e8e8;'>
                <table role='presentation' cellpadding='0' cellspacing='0' border='0' width='100%' style='background-color: #e8e8e8;'>
                    <tr>
                        <td align='center' style='padding: 40px 20px;'>
                            <table role='presentation' cellpadding='0' cellspacing='0' border='0' width='600' style='background-color: #ffffff; max-width: 600px;'>
                                <tr>
                                    <td style='padding: 40px 40px 30px 40px; border-top: 4px solid #B7041A;'></td>
                                </tr>
                                <tr>
                                    <td style='padding: 0 40px 20px 40px;'>
                                        <p style='margin: 0 0 20px 0; font-size: 16px; line-height: 1.5; color: #333333;'>
                                            A quien corresponda de jefatura, {nombreJefe}
                                        </p>
                                        <p style='margin: 0 0 20px 0; font-size: 16px; line-height: 1.6; color: #555555;'>
                                            Mediante la presente notificación, se informa que el empleado {nombreEmpleado} ha solicitado un permiso del tipo {tipoPermiso} y actualmente se encuentra pendiente de aprobación por su parte.
                                        </p>
                                        <p style='margin: 0 0 30px 0; font-size: 16px; line-height: 1.6; color: #555555;'>
                                            Si deseas observar más información, ingresa al sistema de Recursos Humanos para consultar el detalle del permiso.
                                        </p>
                                        <table role='presentation' cellpadding='0' cellspacing='0' border='0'>
                                            <tr>
                                                <td style='border-radius: 24px; border: 1px solid #333333;'>
                                                    <a href='#' style='display: inline-block; padding: 12px 28px; font-size: 15px; color: #333333; text-decoration: none; font-weight: 500;'>
                                                        Ir ahora
                                                    </a>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                                <tr>
                                    <td style='padding: 30px 40px 20px 40px;'>
                                        <p style='margin: 0 0 10px 0; font-size: 16px; line-height: 1.5; color: #555555;'>
                                            Gracias por tu colaboración, 
                                        </p>
                                        <p style='margin: 0; font-size: 16px; line-height: 1.5; color: #555555;'>
                                            Sistemas Analitícos
                                        </p>
                                    </td>
                                </tr>
                                <tr>
                                    <td style='padding-bottom: 40px; border-bottom: 4px solid #B7041A;'></td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
            </body>
            </html>";
        }
    }
}