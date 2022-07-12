// See https://aka.ms/new-console-template for more information
using FEL_ADO.REPOSITORIO;

//Console.WriteLine("Hello, World!");

#region Instacias
Validaciones oValidar = new Validaciones();
#endregion
oValidar.ValidacionDatos(args);


ListaArgumentos MisArgumentos = new ListaArgumentos();

var Argumentos = new ListaArgumentos()
{
    Servidor = args[0],
    DataBaseEmpresa = args[1],
    DataBaseFEL = args[2],
    Usuario = args[3],
    clave = args[4],
    Id_Empresa = args[5],
    Id_Documento = args[6],
    Tipo_Transaccion = args[7],
    Establecimiento = args[8]

};


if (Argumentos.Tipo_Transaccion == "C")
{

    oValidar.VerificarDatosEmpresa(Argumentos);
    oValidar.ExisteElDocumentoCertificacion(Argumentos);
    Console.WriteLine("Si el Proceso llego hasta aqui, Algo falló al intentar Certificar el Documento");



}

if (Argumentos.Tipo_Transaccion == "A")
{
    oValidar.VerificarDatosEmpresa(Argumentos);
    oValidar.ExisteElDocumentoAnulacion(Argumentos);
    Console.WriteLine("Si el Proceso llego hasta aqui, Algo falló al intentar Anular el Documento");
}