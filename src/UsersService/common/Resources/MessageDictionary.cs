using System.Collections.Generic;

public sealed class MessageService //sealed aseguramos que no puedan heredar de la clase
{
    private static readonly MessageService _instance = new(); //readonly asegura que solo se pueda asignar el valor una vez
    private readonly Dictionary<string, string> _messages;

    //Se crea un constructor privado y se crea el diccionario de mensajes
    private MessageService()
    {
        _messages = new Dictionary<string, string>
        {
            {"Productos200", "Productos obtenidos exitosamente." },
            {"Productoscreate200","Producto agregado exitosamente."},
            {"Productos404", "Productos no encontrados." },
            {"Producto200","Productos obtenidos exitosamente."},
            {"Producto404","Producto no encontrado."},
            {"ProductUpdate404","Producto no encontrado."},
            {"ProductUpdate200","Producto actualizado exitosamente."},
            {"ProductUpdate400","Datos inválidos en la solicitud."},
            {"ProductPartialUpdate404","Producto no encontrado."},
            {"ProductPartialUpdate200","Producto actualizado exitosamente."},
            {"DeleteProduct404","Producto no encontrado."},
            {"DeleteProduct200","Producto eliminado exitosamente."},
            {"DeleteProduct400","El producto ya está eliminado."},
            {"RestoreProduct404","Producto no encontrado."},
            {"RestoreProduct400","El producto no estaba marcado como eliminado."},
            {"RestoreProduct200","Producto restaurado exitosamente."},
            {"ProductsByType404","No se encontraron productos para el tipo especificado."},
            {"ProductsByType200","Productos obtenidos exitosamente."},
            {"ProductsStatus404","No se encontraron productos que coincidan con la búsqueda."},
            {"ProductsStatus200","Productos obtenidos exitosamente."},
            {"ProductsSearh404","No se encontraron productos que coincidan con la búsqueda."},
            {"ProductsSearh200","Productos encontrados exitosamente."},
            {"ProductsPrice404","No se encontraron productos en el rango de precios especificado."},
            {"ProductsPrice200","Productos obtenidos exitosamente."},
            {"UpdateImage404","Producto no encontrado."},
            {"UpdateImage200","Imagen del producto actualizada exitosamente."},
            {"DeactivateProduct404","Producto no encontrado."},
            {"DeactivateProduct400","El producto ya está desactivado."},
            {"DeactivateProduct200","Producto desactivado correctamente."},
            {"ActivateProduct404","Producto no encontrado."},
            {"ActivateProduct400","El producto ya está activado."},
            {"ActivateProduct200","Producto activado correctamente."},
            {"ErrorServidor", "Error interno del servidor." },
            {"controllerProductDTO400","Datos inválidos en la solicitud."},
            {"controllerGuidEmpty400","El ID del producto no puede ser vacío."},
            {"controllerIsNullOrEmpty400","La búsqueda no puede ser vacía."},
            {"controllerProductsPrice400","El rango de precios proporcionado no es válido."},
            {"controllerGetAll500","Ocurrió un error al obtener los productos."},
            {"controllerGetProduct500","Ocurrió un error al obtener el producto."},
            {"controllerPost500","Ocurrió un error inesperado al crear el producto."},
            {"controllerPut500","Ocurrió un error al actualizar el producto."},
            {"controllerPatch500","Ocurrió un error al actualizar el producto."},
            {"controllerDeleteProduct500","Ocurrió un error al eliminar el producto."},
            {"controllerRestoreProduct500","Ocurrió un error al restaurar el producto."},
            {"controllerGetProductsByType500","Ocurrió un error al obtener los productos."},
            {"controllerGetProductsStatus500","Ocurrió un error al obtener los productos por estado."},
            {"controllerGetProductsSearch500","Ocurrió un error al obtener los productos por búsqueda."},
            {"controllerGetProductsPrice500","Ocurrió un error al obtener los productos por rango de precios."},
            {"controllerUpdateImage500","Ocurrió un error al actualizar la imagen del producto."},
            {"controllerDeactivateProduct500","Error interno del servidor al desactivar el producto."},
            {"controllerActivateProduct500","Error interno del servidor al activar el producto."},
            {"controller400","Datos invalidos en la solicitud."},
            {"RestoreUserAsyncUser404","Usuario no encontrado."},
            {"RestoreUserAsyncUser400","El usuario no estaba eliminado."},
            {"controllerUser500","Error interno del servidor."},
            {"RestoreUserAsyncUser200","Usuario restaurado exitosamente."},
            {"ActivateUserUser404","Usuario no encontrado."},
            {"ActivateUserUser200","Usuario activado exitosamente."},
            {"ActivateUserUser400","El usuario ya estaba activo."},
            {"GetByIdAsyncUser404","Usuario no encontrado."},
            {"GetByIdAsyncUser200","Usuario encontrado."},
            {"controllerUser404","Usuario no encontrado."},
            {"DeleteAsyncUser200","Usuario eliminado exitosamente."},
            {"DeleteAsyncUser400","Usuario ya se encuentra eliminado."},
            {"getAllUser404","Usuarios no encontrados en la base de datos."},
            {"getAlluser200","Usuarios obtenidos exitosamente."},
            {"controllerPostUser","Datos inválidos."},
            {"AddAsyncUser201","Usuario creado exitosamente."},
            {"AddAsyncUser409","Nombre de usuario o email ya registrado."},
            {"AddAsyncUserEmail400","Email invalido."}, //Se agrego pero no esta espeficicado en el PDF
            {"DeactivateUserAsyncUser400","El usuario ya estaba activo."},
            {"DeactivateUserAsyncUser200","Usuario activado exitosamente."},
            {"UpdatePartialAsyncUser200","Usuario actualizado exitosamente."},
            {"UpdatePartialAsyncUser404","Usuario no encontrado."},
            {"UpdatePartialAsyncUser400","El usuario ya estaba activo."},

            {"UpdateAsyncUser400Deleted","Usuario se encuantra eliminado."},
            {"UpdateAsyncUser200","Usuario actualizado exitosamente."},
            {"UpdateAsyncUser404","Usuario no encontrado."},
            {"UpdateAsyncUser409","Conflicto (nombre de usuario o email ya registrado)."},
            {"UpdateAsyncUser400Active","El usuario esta desactivado."},

            {"VerifyEmailAsyncUser400","El email ya estaba verificado."},
            {"VerifyEmailAsyncUser200","Email verificado exitosamente."},
            {"ChangePasswordAsyncUser400","La contraseña actual no es correcta."},
            {"ChangePasswordAsyncUser400Size","La nueva contraseña debe tener al menos 8 caracteres y contener letras y números."},
            {"ChangePasswordAsyncUser400Confirmation","La nueva contraseña y la confirmación no coinciden."},
            {"ChangePasswordAsyncUser200","Contraseña cambiada exitosamente."},
            {"ChangePasswordAsyncUser400Current","La nueva contraseña no debe ser la misma que la actual."},
            {"RequestPasswordResetAsyncUser404","No se encontró una cuenta con ese email."},
            {"RequestPasswordResetAsyncUser400","El email no ha sido verificado."},
            {"RequestPasswordResetAsyncUser500","Error al enviar el correo de restablecimiento."},
            {"RequestPasswordResetAsyncUser200","Se ha enviado un correo con el enlace de restablecimiento."},

            {"ResetPasswordAsyncUser404","Los datos fallaron o el tiempo de espera se paso."},
            {"ResetPasswordAsyncUser400Size","Contraseña invalida, favor de verificar los datos la contraseña debe tener al menos 8 caracteres, contiene al menos un dígito, contiene al menos una letra."},
            {"ResetPasswordAsyncUser200","La contraseña se actualizo con exito."},
            {"ResetPasswordAsyncUserNull404","Expiro la verificacion o ya se uso"}



        };
    }

    public static MessageService Instance{
        get{
            return _instance;
        }
    } //La propiedas Instance es de solo lectura

    /// <summary>
    /// Busca un mensaje en el diccionario usando la clave especificada.
    /// Si la clave no existe, retorna "Mensaje no encontrado".
    /// </summary>
    /// <param name="key">Clave del mensaje a buscar.</param>
    /// <returns>El mensaje correspondiente a la clave o un mensaje por defecto si no se encuentra.</returns>
    public string GetMessage(string key)
    {
        return _messages.TryGetValue(key, out var message) ? message : "Mensaje no encontrado";
    }
}